using DohProxy;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Nito.AsyncEx;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<UdpListener>();
builder.Services.AddHostedService<TcpDnsListener>();
builder.Services.AddSingleton<Resolver>();
builder.Services.AddHttpClient(string.Empty, client =>
{
    client.DefaultRequestVersion = new Version(3, 0);
    client.DefaultVersionPolicy = HttpVersionPolicy.RequestVersionExact;
});

AsyncContext.Run(async () =>
{
    using var host = builder.Build();
    await host.RunAsync();
});