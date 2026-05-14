using RabbitMQ.Client;
using Shared.Events;
using System.Text;
using System.Text.Json;

namespace OrderApi.Messaging.RabbitMQ.Producers;

public class NotificationProducer
{
    private readonly IConfiguration _configuration;

    public NotificationProducer(
        IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task PublishAsync(
        StockUpdatedNotificationEvent notification)
    {
        var factory =
            new ConnectionFactory()
            {
                HostName =
                    _configuration["RabbitMQ:Host"]
                    ?? throw new Exception(
                        "RabbitMQ Host not configured")
            };

        using var connection =
            await factory.CreateConnectionAsync();

        using var channel =
            await connection.CreateChannelAsync();

        await channel.QueueDeclareAsync(
            queue: "notification-queue",

            durable: false,

            exclusive: false,

            autoDelete: false,

            arguments: null);

        var json =
            JsonSerializer.Serialize(
                notification);

        var body =
            Encoding.UTF8.GetBytes(json);

        await channel.BasicPublishAsync(
            exchange: "",

            routingKey:
                "notification-queue",

            body: body);

        Console.WriteLine(
            "NOTIFICATION PUBLISHED");
    }
}