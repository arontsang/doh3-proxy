using System.Buffers;
using System.Net.Sockets;
using Microsoft.Extensions.Hosting;
using Nito.AsyncEx;

namespace DohProxy;

public class UdpListener(Resolver resolver) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var udp = new UdpClient(5353);
        while (!stoppingToken.IsCancellationRequested)
        {
            var request = await udp.ReceiveAsync(stoppingToken);
            _ = Task.Factory.Run(() => DoWork(request, udp, stoppingToken));
        }
    }

    private async Task DoWork(UdpReceiveResult request, UdpClient udp, CancellationToken stoppingToken)
    {
        using var response = MemoryPool<byte>.Shared.Rent(1024 * 8);
        var responseSize = await resolver.Resolve(request.Buffer.AsMemory(), response.Memory, stoppingToken);

        if (responseSize == -1)
            return;

        if (responseSize >= 512)
        {
            Span<byte> payload = stackalloc byte[4 + 8];
            response.Memory.Span[0..3].CopyTo(payload);
            payload[2] = (byte)(payload[2] | 0x02);
            // ReSharper disable once AccessToDisposedClosure
            udp.Send(payload, request.RemoteEndPoint);
            return;
        }

        // ReSharper disable once AccessToDisposedClosure
        await udp.SendAsync(response.Memory[..responseSize], request.RemoteEndPoint, stoppingToken);
    }
}