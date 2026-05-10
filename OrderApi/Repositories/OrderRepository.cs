using Shared;

namespace OrderApi.Repositories;

public class OrderRepository
{
    private readonly List<Order> _orders = [];

    public void Add(Order order)
    {
        _orders.Add(order);
    }

    public List<Order> GetAll()
    {
        return _orders;
    }
}