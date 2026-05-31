using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ProBeacon.Application.Common.Interfaces;
using ProBeacon.Application.Common.Models;
using RabbitMQ.Client;

namespace ProBeacon.Infrastructure.Messaging;

public class RabbitMqEmailPublisher : IEmailJobPublisher, IAsyncDisposable
{
    private readonly RabbitMqOptions _opts;
    private readonly ILogger<RabbitMqEmailPublisher> _logger;
    private IConnection? _connection;
    private readonly SemaphoreSlim _lock = new(1, 1);

    public RabbitMqEmailPublisher(IOptions<RabbitMqOptions> opts, ILogger<RabbitMqEmailPublisher> logger)
    {
        _opts = opts.Value;
        _logger = logger;
    }

    public async Task PublishAsync(EmailJob job, CancellationToken cancellationToken = default)
    {
        var connection = await GetConnectionAsync(cancellationToken);
        await using var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);

        await channel.QueueDeclareAsync(
            queue: _opts.EmailQueue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            cancellationToken: cancellationToken);

        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(job));

        var props = new BasicProperties { Persistent = true };

        await channel.BasicPublishAsync(
            exchange: string.Empty,
            routingKey: _opts.EmailQueue,
            mandatory: false,
            basicProperties: props,
            body: body,
            cancellationToken: cancellationToken);

        _logger.LogDebug("Email job queued for {To}", job.To);
    }

    private async Task<IConnection> GetConnectionAsync(CancellationToken ct)
    {
        if (_connection is { IsOpen: true })
            return _connection;

        await _lock.WaitAsync(ct);
        try
        {
            if (_connection is { IsOpen: true })
                return _connection;

            var factory = new ConnectionFactory
            {
                HostName = _opts.Host,
                Port = _opts.Port,
                UserName = _opts.Username,
                Password = _opts.Password,
                VirtualHost = _opts.VirtualHost,
                AutomaticRecoveryEnabled = true,
            };

            _connection = await factory.CreateConnectionAsync(cancellationToken: ct);
            return _connection;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_connection is not null)
            await _connection.DisposeAsync();
    }
}
