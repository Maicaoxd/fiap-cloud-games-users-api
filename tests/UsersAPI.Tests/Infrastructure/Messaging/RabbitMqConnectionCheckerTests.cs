using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Options;
using UsersAPI.Health;
using UsersAPI.Infrastructure.Messaging;

namespace UsersAPI.Tests.Infrastructure.Messaging;

public sealed class RabbitMqConnectionCheckerTests
{
    [Fact]
    public async Task CanConnectAsync_WhenTcpPortIsAcceptingConnections_ReturnsTrue()
    {
        var listener = new TcpListener(IPAddress.Loopback, 0);

        try
        {
            listener.Start();
            var endpoint = (IPEndPoint)listener.LocalEndpoint;
            var checker = new RabbitMqConnectionChecker(Options.Create(new RabbitMqOptions
            {
                Host = IPAddress.Loopback.ToString(),
                Port = endpoint.Port
            }));

            var acceptTask = listener.AcceptTcpClientAsync();

            var result = await checker.CanConnectAsync();

            result.ShouldBeTrue();
            using var acceptedClient = await acceptTask.WaitAsync(TimeSpan.FromSeconds(2));
        }
        finally
        {
            listener.Stop();
        }
    }
}
