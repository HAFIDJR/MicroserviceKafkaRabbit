using OrderApi.Kafka.Producers;
using OrderApi.Repositories;
using Shared;
using Shared.Events;

namespace OrderApi.Services;

public class OrderService(OrderRepository orderRepository,
    ProductCacheRepository productRepository,
    OrderProducer producer)
{
    public async Task AddOrder(Order order)
    {
        var product = productRepository.GetById(order.ProductId);

        if (product is null)
        {
            throw new Exception("Product Not Found");
        }

        orderRepository.Add(order);

        await producer.PublishOrderCreated(
            new OrderCreatedEvent
            {
                OrderId = order.Id,
                ProductId = order.ProductId,
                Quantity = order.Quantity,
                Price = product.Price
            }
        );
    }

    public List<OrderSummary> GetOrderSummaries()
    {
        return orderRepository
            .GetAll()
            .Select(order =>
            {
                var product =
                    productRepository
                        .GetById(order.ProductId);

                return new OrderSummary
                {
                    OrderId = order.Id,
                    ProductId = order.ProductId,
                    ProductName = product?.Name ?? "",
                    ProductPrice = product?.Price ?? 0,
                    OrderedQuantity = order.Quantity
                };

            }).ToList();
    }
}