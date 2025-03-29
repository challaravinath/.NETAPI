using Microsoft.EntityFrameworkCore;

using ProductApi.Data;
using ProductApi.Models;
using ProductApi.Repositories.Interfaces;

namespace ProductApi.Repositories
{

    public class ProductRepository : IProductRepository
    {
        private readonly ApplicationDbContext _context;
        ILogger<ProductRepository> _logger;

        public ProductRepository(ApplicationDbContext context, ILogger<ProductRepository> logger)
        {
            _context = context;
            _logger = logger;
        }
        public async Task<Product> AddProductAsync(Product product)
        {
            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            return product;
        }

        public async Task DeleteProductAsync(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null) 
            {
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<Product?> GetProductByIdAsync(int id)
        {
            return await _context.Products
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<PagedResult<Product>> GetProductsPagedAsync(int pageNumber, int pageSize)
        {
            using (_logger.BeginScope("Getting paged products: Page {PageNumber}, Size {PageSize}", pageNumber, pageSize))
            {
                try
                {
                    pageNumber = pageNumber < 1 ? 1 : pageNumber;
                    pageSize = pageSize > 100 ? 100 : pageSize;
                    pageSize = pageSize < 1 ? 10 : pageSize;

                    _logger.LogDebug("Querying database for product count");
                    var totalCount = await _context.Products.CountAsync();

                    _logger.LogDebug("Retrieving products for page {PageNumber}", pageNumber);
                    var items = await _context.Products
                        .AsNoTracking()
                        .OrderBy(p => p.Id)
                        .Skip((pageNumber - 1) * pageSize)
                        .Take(pageSize)
                        .ToListAsync();

                    _logger.LogInformation("Retrieved {ItemCount} products from total {TotalCount}", items.Count, totalCount);

                    return new PagedResult<Product>
                    {
                        Items = items,
                        PageNumber = pageNumber,
                        PageSize = pageSize,
                        TotalCount = totalCount
                    };
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error retrieving paged products");
                    throw;
                }
            }
        }
        public async Task<Product> UpdateProductAsync(Product product)
        {
            _context.Entry(product).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return product;
        }
    }
}