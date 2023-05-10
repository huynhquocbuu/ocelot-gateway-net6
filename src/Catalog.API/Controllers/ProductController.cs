using Catalog.API.Models;
using Microsoft.AspNetCore.Mvc;

namespace Catalog.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        // GET: api/Products
        [HttpGet("GetAll")]
        public List<Product> GetProducts()
        {
            return new List<Product>()
            {
                new Product { 
                    Id = 1,
                    AvailableStock = 1,
                    Description = "Product 1 Desc",
                    Name = "Product 1",
                    Price = 100,
                    RestockThreshold = 1
                },
                new Product {
                    Id = 2,
                    AvailableStock = 2,
                    Description = "Product 2 Desc",
                    Name = "Product 2",
                    Price = 200,
                    RestockThreshold = 2
                }
            };
        }

        // GET: api/Products/5
        [HttpGet("{id}")]
        public Product GetProduct(int id)
        {
            return new Product
            {
                Id = 1,
                AvailableStock = 1,
                Description = "Product 1 Desc",
                Name = "Product 1",
                Price = 100,
                RestockThreshold = 1
            };
        }

        // PUT: api/Products/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("Edit/{id}")]
        public NoContentResult PutProduct(int id, Product product)
        {
            return NoContent();
        }

        [HttpPost("Add")]
        public Product PostProduct(Product product)
        {
            return product;
        }
    }
}
