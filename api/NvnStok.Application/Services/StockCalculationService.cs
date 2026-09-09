using NvnStok.Application.Interfaces;

namespace NvnStok.Application.Services;

public class StockCalculationService
{
    private readonly IStockRepository _stockRepository;

    public StockCalculationService(IStockRepository stockRepository)
    {
        _stockRepository = stockRepository;
    }

    public async Task<int> GetCurrentStockAsync(Guid productId)
    {
        var movements = await _stockRepository.GetMovementsByProductIdAsync(productId);
        return movements.Sum(m => m.Quantity);
    }

    public async Task<bool> IsLowStockAsync(Guid productId)
    {
        var product = await _stockRepository.GetProductByIdAsync(productId);
        if (product is null) return false;

        var currentStock = await GetCurrentStockAsync(productId);
        return currentStock <= product.LowStockThreshold;
    }
}