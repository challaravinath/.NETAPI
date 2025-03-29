using Microsoft.AspNetCore.Mvc;
using ProductApi.Extensions;
using ProductApi.Models;
using ProductApi.Repositories.Interfaces;

using Microsoft.AspNetCore.Mvc;
using ProductApi.Helpers;
using ProductApi.Repositories;

namespace ProductApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : Controller
    {
        private readonly IProductRepository _repository;
        ILogger<ProductRepository> _logger;
        public ProductsController(IProductRepository repository, ILogger<ProductRepository> logger)
        {
            _repository = repository;
            _logger = logger;
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
            using (_logger.BeginScope("API Request GET Products page {Page}, size {PageSize}", page, pageSize))
            {
                try
                {
                    _logger.LogInformation("Processing request for products page {Page}", page);

                    var pagedResult = await _repository.GetProductsPagedAsync(page, pageSize);
                    var response = new PagedResponse<Product>
                    {
                        Data = pagedResult.Items,
                        PageNumber = page,
                        PageSize = pageSize,
                        TotalCount = pagedResult.TotalCount,
                        TotalPages = pagedResult.TotalPages
                    };

                    _logger.LogInformation("Successfully retrieved {Count} products", pagedResult.Items.Count);

                    return Ok(response);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing GET Products request");
                    throw; // Will be caught by the exception middleware
                }
            }
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
