using Microsoft.AspNetCore.Mvc;
using OrderApi.OrderServices;
using Shared;

namespace OrderApi.Controllers
{
    [ApiController]
    [Route("[controller]")]

    public class OrderController(IOrderService orderService) :ControllerBase
    {
        [HttpGet("start-consuming-service")]
        public async Task <IActionResult> StartService()
        {
            await orderService.StartConsumingService();
            return NoContent();
        }

        [HttpGet("get-product")]
        public IActionResult GetProducts()
        {
            var products = orderService.GetProducts();
            return Ok(products);
        }

        [HttpPost("add-order")]
        public IActionResult AddOrder(Order order)
        {
            orderService.AddOrder(order);
            return Ok("Ordefer Placed");
        }

        [HttpGet("order-summary")]
        public IActionResult GetOrdersSummary()=>Ok(orderService.GetOrderSummaries());

        
    }
}