using Microsoft.AspNetCore.Mvc;
using ProductApi.Extensions;
using ProductApi.Models;
using ProductApi.Repositories.Interfaces;

using Microsoft.AspNetCore.Mvc;
using ProductApi.Helpers;

namespace ProductApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : Controller
    {
        private readonly IProductRepository _repository;
        public ProductsController(IProductRepository repository)
        {
            _repository = repository;
        }
        // GET: api/products?page=2&pageSize=10
        //[HttpGet]
        //public async Task<ActionResult<PagedResult<Product>>> GetProducts(
        //    [FromQuery] int page = 1,
        //    [FromQuery] int pageSize = 10)
        //{
        //    var pagedResult = await _repository.GetProductsPagedAsync(page, pageSize);

        //    // Add pagination headers
        //    Response.AppendPaginationHeaders(pagedResult);

        //    return Ok(pagedResult);
        //}
        [HttpGet]
        public async Task<ActionResult<PagedResponse<Product>>> GetProducts( 
            [FromQuery] int page = 1,[FromQuery] int pageSize = 10)
        {
            var pagedResult = await _repository.GetProductsPagedAsync(page, pageSize);

            var response = new PagedResponse<Product>
            {
                Data = pagedResult.Items,
                PageNumber = page,
                PageSize = pageSize,
                TotalCount = pagedResult.TotalCount,
                TotalPages = pagedResult.TotalPages
            };

            return Ok(response); // ← middleware injects headers
        }


        // GET: api/products/5
        [HttpGet("{id}")]
        [ResponseCache(Duration = 60)]
        public async Task<ActionResult<Product>> GetProduct(int id)
        {
            var product = await _repository.GetProductByIdAsync(id);

            if (product == null)
            {
                return NotFound();
            }

            return Ok(product);
        }

        // POST: api/products
        [HttpPost]
        public async Task<ActionResult<Product>> CreateProduct(Product product)
        {
            await _repository.AddProductAsync(product);

            return CreatedAtAction(
                nameof(GetProduct),
                new { id = product.Id },
                product);
        }
        // PUT: api/products/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(int id, Product product)
        {
            if (id != product.Id)
            {
                return BadRequest();
            }

            var existingProduct = await _repository.GetProductByIdAsync(id);
            if (existingProduct == null)
            {
                return NotFound();
            }

            await _repository.UpdateProductAsync(product);

            return NoContent();
        }
        // DELETE: api/products/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _repository.GetProductByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            await _repository.DeleteProductAsync(id);

            return NoContent();
        }


    }
}
