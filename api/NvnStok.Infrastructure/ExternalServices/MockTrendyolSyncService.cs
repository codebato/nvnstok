using NvnStok.Application.Interfaces;

namespace NvnStok.Infrastructure.ExternalServices;

public class MockTrendyolSyncService : ITrendyolSyncService
{
    private readonly Random _random = new();
    public Task<List<TrendyolProductInfo>> GetProductStockInfoAsync()
    {
        var mockData = new List<TrendyolProductInfo>
        {
            new TrendyolProductInfo
            {
                Barcode = "TEST-001",
                Quantity = _random.Next(0,15),
                SalePrice = 149.90m

            }
        };
        return Task.FromResult(mockData);

    }
}   