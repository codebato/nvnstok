using Microsoft.EntityFrameworkCore;
using NvnStok.Application.Interfaces;
using NvnStok.Domain.Entities;
using NvnStok.Infrastructure.Data;

namespace NvnStok.Infrastructure.Repositories;

// IStockRepository'nin EF Core + PostgreSQL kullanan gerçek implementasyonu.
// Application katmanı bu sınıfın varlığından haberdar değil — sadece interface'i biliyor.
public class StockRepository : IStockRepository
{
    private readonly NvnStokDbContext _context;

    public StockRepository(NvnStokDbContext context)
    {
        _context = context;
    }

    public async Task<List<StockMovement>> GetMovementsByProductIdAsync(Guid productId)
    {
        return await _context.StockMovements
            .Where(m => m.ProductId == productId)
            .OrderBy(m => m.CreatedAt)
            .ToListAsync();
    }

    public async Task AddMovementAsync(StockMovement movement)
    {
        _context.StockMovements.Add(movement);
        await _context.SaveChangesAsync();
    }

    public async Task<Product?> GetProductByIdAsync(Guid productId)
    {
        return await _context.Products.FindAsync(productId);
    }

    public async Task AddProductAsync(Product product)
    {
        _context.Products.Add(product);
        await _context.SaveChangesAsync();
    }

    public async Task<List<Product>> GetAllProductsAsync()
    {
        return await _context.Products.ToListAsync();
    }
}