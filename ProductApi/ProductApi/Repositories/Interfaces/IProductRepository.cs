using ProductApi.Models;

namespace ProductApi.Repositories.Interfaces
{
    public interface IProductRepository
    {
        Task<PagedResult<Product>> GetProductsPagedAsync(int pageNumber, int pageSize);
        Task<Product?> GetProductByIdAsync(int id);
        Task<Product> AddProductAsync(Product product);
        Task<Product> UpdateProductAsync(Product product);
        Task DeleteProductAsync(int id);
    }
}
