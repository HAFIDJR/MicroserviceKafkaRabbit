using Microsoft.AspNetCore.Mvc;
using ProductApi.ProductServices;
using Shared;

namespace ProductApi.Controllers
{
    [ApiController]
    [Route("[controller]")]

    public class ProductController (IProductService productService) : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> AddProduct (Product product)
        {
            await productService.AddProduct(product);
            return Created();
        }

        [HttpDelete("{id:int}")]
        public async Task <IActionResult> DeleteProduct (int id)
        {
            await productService.DeleteProduct(id);

            return NoContent();
        }
    }
}