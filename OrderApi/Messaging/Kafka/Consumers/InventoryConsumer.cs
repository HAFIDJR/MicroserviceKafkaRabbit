using System.Text.Json;
using Confluent.Kafka;
using Shared.Events;

namespace InventoryApi.BackgroundServices;

public class InventoryConsumer(
    IConfiguration configuration,
    ILogger<InventoryConsumer> logger
) : BackgroundService
{
    private readonly Dictionary<int, int> _stocks = new()
    {
        { 1, 100 },
        { 2, 50 }
    };

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        await Task.Yield();

        var config = new ConsumerConfig
        {
            BootstrapServers =
                configuration["Kafka:BootstrapServers"]
                ?? throw new Exception("Kafka BootstrapServers Not Found"),

            GroupId = "inventory-group",
            AutoOffsetReset = AutoOffsetReset.Earliest
        };

        using var consumer =
            new ConsumerBuilder<string, string>(config).Build();

        consumer.Subscribe("order-created-topic");

        logger.LogInformation("Inventory Consumer Started");

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var result = consumer.Consume(
                    TimeSpan.FromSeconds(1));

                if (result is null)
                    continue;

                logger.LogInformation(
                    "Order Event Received : {Message}",
                    result.Message.Value);

                var order = JsonSerializer.Deserialize<OrderCreatedEvent>(
                    result.Message.Value);

                if (order is null)
                {
                    logger.LogWarning("Deserialize Failed");
                    continue;
                }

                if (!_stocks.ContainsKey(order.ProductId))
                {
                    logger.LogWarning(
                        "Product Not Found : {ProductId}",
                        order.ProductId);

                    continue;
                }

                _stocks[order.ProductId] -= order.Quantity;

                logger.LogInformation(
                    """
                    ======================
                    STOCK UPDATED
                    Product : {ProductId}
                    Remaining : {Stock}
                    ======================
                    """,
                    order.ProductId,
                    _stocks[order.ProductId]);
            }
        }
        catch (ConsumeException ex)
        {
            logger.LogError(ex, "Kafka Consume Error");
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("Inventory Consumer Stopped");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Inventory Consumer Error");
        }
        finally
        {
            consumer.Close();
        }
    }
}