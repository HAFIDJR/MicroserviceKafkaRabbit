using System.Text.Json;
using Confluent.Kafka;
using OrderApi.Repositories;
using Shared;

namespace OrderApi.Kafka.Consumers;

public class ProductConsumer(
    IConfiguration configuration,
    ProductCacheRepository repository,
    ILogger<ProductConsumer> logger
) : BackgroundService
{
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _ = Task.Run(() => RunConsumer(stoppingToken), stoppingToken);
        return Task.CompletedTask;
    }

    private void RunConsumer(CancellationToken stoppingToken)
    {
        var config = new ConsumerConfig
        {
            BootstrapServers = configuration["Kafka:BootstrapServers"],
            GroupId = "order-group",
            AutoOffsetReset = AutoOffsetReset.Earliest
        };

        using var consumer = new ConsumerBuilder<string, string>(config).Build();

        consumer.Subscribe("product-created-topic");

        logger.LogInformation("Product Consumer Started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var response = consumer.Consume(TimeSpan.FromSeconds(1));

                if (response is null)
                {
                    Thread.Sleep(100);
                    continue;
                }

                logger.LogInformation("Message Received : {Message}", response.Message.Value);

                var product = JsonSerializer.Deserialize<Product>(response.Message.Value);

                if (product is not null)
                {
                    repository.Add(product);

                    logger.LogInformation("Product Cached : {Name}", product.Name);
                }
            }
            catch (ConsumeException ex)
            {
                logger.LogError(ex, "Kafka Consume Error");
                Thread.Sleep(1000);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Kafka Error");
                Thread.Sleep(1000);
            }
        }

        consumer.Close();
    }
}