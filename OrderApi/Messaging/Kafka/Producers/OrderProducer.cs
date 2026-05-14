using System.Text.Json;
using Confluent.Kafka;
using Shared.Events;

namespace OrderApi.Kafka.Producers;

public class OrderProducer(
    IProducer<string, string> producer

)
{
    public async Task PublishOrderCreated(
        OrderCreatedEvent orderEvent
    )
    {
        await producer.ProduceAsync(
            "order-created-topic",
            new Message<string, string>
            {
                Key = orderEvent.OrderId.ToString(),
                Value = JsonSerializer.Serialize(orderEvent)
            }
        );
    }
}