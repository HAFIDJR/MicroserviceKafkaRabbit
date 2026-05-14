using RabbitMQ.Client;
using RabbitMQ.Client.Events;

using Shared.Events;

using System.Text;
using System.Text.Json;

namespace OrderApi.Messaging.RabbitMQ.Consumers;

public class NotificationConsumer(
    IConfiguration configuration,
    ILogger<NotificationConsumer> logger
) : BackgroundService
{
    private IConnection? _connection;

    private IChannel? _channel;

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        var factory =
            new ConnectionFactory()
            {
                HostName =
                    configuration["RabbitMQ:Host"]
                    ?? throw new Exception(
                        "RabbitMQ Host Not Found")
            };

        // ==========================
        // Retry Until RabbitMQ Ready
        // ==========================

        while(!stoppingToken
            .IsCancellationRequested)
        {
            try
            {
                logger.LogInformation(
                    "Connecting To RabbitMQ...");

                _connection =
                    await factory
                        .CreateConnectionAsync(
                            stoppingToken);

                _channel =
                    await _connection
                        .CreateChannelAsync(
                            cancellationToken:
                            stoppingToken);

                logger.LogInformation(
                    "RabbitMQ Connected");

                break;
            }
            catch(Exception ex)
            {
                logger.LogWarning(
                    ex,
                    """
                    RabbitMQ Not Ready
                    Retrying In 5 Seconds...
                    """);

                await Task.Delay(
                    5000,
                    stoppingToken);
            }
        }

        if(_channel is null)
        {
            logger.LogError(
                "RabbitMQ Channel Failed");

            return;
        }

        // ==========================
        // Queue Declare
        // ==========================

        await _channel.QueueDeclareAsync(
            queue: "notification-queue",

            durable: false,

            exclusive: false,

            autoDelete: false,

            arguments: null,

            cancellationToken:
            stoppingToken);

        logger.LogInformation(
            "Notification Queue Ready");

        // ==========================
        // Consumer
        // ==========================

        var consumer =
            new AsyncEventingBasicConsumer(
                _channel);

        consumer.ReceivedAsync += async (
            model,
            ea) =>
        {
            try
            {
                var body =
                    ea.Body.ToArray();

                var json =
                    Encoding.UTF8
                        .GetString(body);

                var notification =
                    JsonSerializer.Deserialize
                    <StockUpdatedNotificationEvent>(
                        json);

                if(notification is null)
                {
                    logger.LogWarning(
                        """
                        Notification Deserialize Failed
                        """);

                    return;
                }

                logger.LogInformation(
                    """
                    ==========================
                    NOTIFICATION RECEIVED
                    ProductId : {ProductId}
                    RemainingStock : {Stock}
                    Message : {Message}
                    ==========================
                    """,

                    notification.ProductId,

                    notification.RemainingStock,

                    notification.Message);

                await Task.CompletedTask;
            }
            catch(Exception ex)
            {
                logger.LogError(
                    ex,
                    "Notification Consumer Error");
            }
        };

        // ==========================
        // Start Consume
        // ==========================

        await _channel.BasicConsumeAsync(
            queue: "notification-queue",

            autoAck: true,

            consumer: consumer,

            cancellationToken:
            stoppingToken);

        logger.LogInformation(
            "Notification Consumer Started");

        // ==========================
        // Keep BackgroundService Alive
        // ==========================

        try
        {
            await Task.Delay(
                Timeout.Infinite,
                stoppingToken);
        }
        catch(OperationCanceledException)
        {
            logger.LogInformation(
                "Notification Consumer Stopped");
        }
    }

    public override async Task StopAsync(
        CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Closing RabbitMQ Connection");

        if(_channel is not null)
        {
            await _channel.DisposeAsync();
        }

        if(_connection is not null)
        {
            await _connection.DisposeAsync();
        }

        await base.StopAsync(
            cancellationToken);
    }
}