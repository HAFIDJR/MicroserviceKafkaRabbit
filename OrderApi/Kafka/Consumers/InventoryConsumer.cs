using System.Text.Json;
using Confluent.Kafka;
using Shared.Events;

namespace InventoryApi.BackgroundServices;

public class InventoryConsumer(
    IConfiguration configuration,
    ILogger<InventoryConsumer> logger
) : BackgroundService
{
    private readonly Dictionary<int, int> _stocks =
        new()
        {
            {1,100},
            {2,50}
        };

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        var config = new ConsumerConfig
        {
            BootstrapServers =
                configuration["Kafka:BootstrapServers"],

            GroupId = "inventory-group",

            AutoOffsetReset =
                AutoOffsetReset.Earliest
        };

        using var consumer =
            new ConsumerBuilder<string,string>(
                config).Build();

        consumer.Subscribe(
            "order-created-topic");

        logger.LogInformation(
            "Inventory Consumer Started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var result =
                    consumer.Consume(
                        TimeSpan.FromSeconds(1));

                if(result is null)
                    continue;

                var order =
                    JsonSerializer.Deserialize
                    <OrderCreatedEvent>(
                        result.Message.Value);

                if(order is not null)
                {
                    if(!_stocks.ContainsKey(
                        order.ProductId))
                    {
                        logger.LogWarning(
                            "Product Not Found");

                        continue;
                    }

                    _stocks[order.ProductId] -=
                        order.Quantity;

                    logger.LogInformation(
                        """
                        STOCK UPDATED
                        Product : {ProductId}
                        Remaining : {Stock}
                        """,
                        order.ProductId,
                        _stocks[order.ProductId]);
                }
            }
            catch(Exception ex)
            {
                logger.LogError(
                    ex,
                    "Inventory Consumer Error");
            }

            await Task.Delay(
                100,
                stoppingToken);
        }

        consumer.Close();
    }
}