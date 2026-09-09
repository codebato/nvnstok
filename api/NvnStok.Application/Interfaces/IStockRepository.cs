using NvnStok.Domain.Entities;

namespace NvnStok.Application.Interfaces;

public interface IStockRepository
{
    Task<List<StockMovement>> GetMovementsByProductIdAsync(Guid productId);
    Task AddMovementAsync(StockMovement movement);
    Task<Product?> GetProductByIdAsync(Guid productId);
    Task AddProductAsync(Product product);
    Task<List<Product>> GetAllProductsAsync();
}