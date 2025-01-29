using System.Buffers;
using System.Buffers.Binary;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Hosting;
using Nito.AsyncEx;

namespace DohProxy;

public class TcpDnsListener(Resolver resolver) : BackgroundService
{
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var listener = new TcpListener(IPAddress.Any, PortSelector.Port);
        listener.Start();
        while (!stoppingToken.IsCancellationRequested)
        {
            var client = await listener.AcceptTcpClientAsync(stoppingToken);
            _ = Task.Factory.Run(() => DoWork(client, stoppingToken));
        }
    }
    
    private async Task DoWork(TcpClient client, CancellationToken stoppingToken)
    {
        await using var clientStream = client.GetStream();
        ushort length = 0;
        unsafe
        {
            var lengthSpan = MemoryMarshal.Cast<ushort, byte>(MemoryMarshal.CreateSpan(ref length, 1));
            clientStream.ReadExactly(lengthSpan);
            if (BitConverter.IsLittleEndian)
            {
                length = BinaryPrimitives.ReverseEndianness(length);
            }
        }
                
        using var requestBuffer = MemoryPool<byte>.Shared.Rent(length);
        await clientStream.ReadExactlyAsync(requestBuffer.Memory[..length], stoppingToken);
        var responseBuffer = MemoryPool<byte>.Shared.Rent(8 * 1024);
        var responseLength = await resolver.Resolve(requestBuffer.Memory[..length], responseBuffer.Memory, stoppingToken);

        unsafe
        {
            length = (ushort)responseLength;
            if (BitConverter.IsLittleEndian)
            {
                length = BinaryPrimitives.ReverseEndianness(length);
            }
            var lengthSpan = MemoryMarshal.Cast<ushort, byte>(MemoryMarshal.CreateSpan(ref length, 1));
            clientStream.Write(lengthSpan);
        }
                
        await clientStream.WriteAsync(responseBuffer.Memory[..responseLength], stoppingToken);
    }
}