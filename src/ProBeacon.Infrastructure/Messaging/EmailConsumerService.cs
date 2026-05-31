using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ProBeacon.Application.Common.Interfaces;
using ProBeacon.Application.Common.Models;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace ProBeacon.Infrastructure.Messaging;

public class EmailConsumerService(
    IServiceScopeFactory scopeFactory,
    IOptions<RabbitMqOptions> opts,
    ILogger<EmailConsumerService> logger)
    : BackgroundService
{
    private readonly RabbitMqOptions _opts = opts.Value;
    private IConnection? _connection;
    private IChannel? _channel;

    private const string RetryHeader = "x-retry-count";
    private const int MaxRetries = 3;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await ConnectAsync(stoppingToken);

        stoppingToken.Register(() => logger.LogInformation("Email consumer stopping."));
        await Task.Delay(Timeout.Infinite, stoppingToken).ConfigureAwait(ConfigureAwaitOptions.SuppressThrowing);
    }

    private async Task ConnectAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            try
            {
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
                _channel = await _connection.CreateChannelAsync(cancellationToken: ct);

                await _channel.QueueDeclareAsync(
                    queue: _opts.EmailQueue,
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    cancellationToken: ct);

                // process one message at a time
                await _channel.BasicQosAsync(prefetchSize: 0, prefetchCount: 1, global: false, cancellationToken: ct);

                var consumer = new AsyncEventingBasicConsumer(_channel);
                consumer.ReceivedAsync += OnMessageReceivedAsync;

                await _channel.BasicConsumeAsync(
                    queue: _opts.EmailQueue,
                    autoAck: false,
                    consumer: consumer,
                    cancellationToken: ct);

                logger.LogInformation("Email consumer connected to RabbitMQ, listening on '{Queue}'.", _opts.EmailQueue);
                return;
            }
            catch (Exception ex) when (!ct.IsCancellationRequested)
            {
                logger.LogWarning(ex, "Could not connect to RabbitMQ, retrying in 10s.");
                await Task.Delay(TimeSpan.FromSeconds(10), ct).ConfigureAwait(ConfigureAwaitOptions.SuppressThrowing);
            }
        }
    }

    private async Task OnMessageReceivedAsync(object sender, BasicDeliverEventArgs ea)
    {
        EmailJob? job = null;
        try
        {
            var json = Encoding.UTF8.GetString(ea.Body.Span);
            job = JsonSerializer.Deserialize<EmailJob>(json);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to deserialize email job — discarding.");
            await _channel!.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: false);
            return;
        }

        if (job is null)
        {
            await _channel!.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: false);
            return;
        }

        try
        {
            using var scope = scopeFactory.CreateScope();
            var emailSender = scope.ServiceProvider.GetRequiredService<IEmailSender>();
            await emailSender.SendAsync(job.TenantId, job.To, job.Subject, job.HtmlBody);
            await _channel!.BasicAckAsync(ea.DeliveryTag, multiple: false);
        }
        catch (Exception ex)
        {
            var retries = GetRetryCount(ea.BasicProperties);
            logger.LogWarning(ex, "Failed to send email to {To}, attempt {Attempt}/{Max}.", job.To, retries + 1, MaxRetries);

            if (retries < MaxRetries)
                await RequeuWithRetryCountAsync(ea, retries + 1, job);
            else
            {
                logger.LogError("Email to {To} exceeded max retries — discarding.", job.To);
                await _channel!.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: false);
            }
        }
    }

    private async Task RequeuWithRetryCountAsync(BasicDeliverEventArgs ea, int retryCount, EmailJob job)
    {
        // ack the original, republish with incremented retry header
        await _channel!.BasicAckAsync(ea.DeliveryTag, multiple: false);

        var props = new BasicProperties
        {
            Persistent = true,
            Headers = new Dictionary<string, object?> { [RetryHeader] = retryCount }
        };

        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(job));

        await _channel.BasicPublishAsync(
            exchange: string.Empty,
            routingKey: _opts.EmailQueue,
            mandatory: false,
            basicProperties: props,
            body: body);
    }

    private static int GetRetryCount(IReadOnlyBasicProperties props)
    {
        if (props.Headers is not null && props.Headers.TryGetValue(RetryHeader, out var val))
            return val is int i ? i : 0;
        return 0;
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await base.StopAsync(cancellationToken);
        if (_channel is not null) await _channel.DisposeAsync();
        if (_connection is not null) await _connection.DisposeAsync();
    }
}
