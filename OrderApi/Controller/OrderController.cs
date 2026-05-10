using Microsoft.AspNetCore.Mvc;
using OrderApi.OrderServices;
using OrderApi.Services;
using Shared;

namespace OrderApi.Controllers
{
    [ApiController]
    [Route("[controller]")]

    public class OrderController(OrderService orderService) :ControllerBase
    {
        
    [HttpPost("orders")]
    public async Task<IActionResult> AddOrder(
        Order order)
    {
        await orderService.AddOrder(order);

        return Ok("Order Placed");
    }

    [HttpGet("order-summaries")]
    public IActionResult GetOrderSummaries()
    {
        return Ok(
            orderService.GetOrderSummaries());
    }

        
    }
}