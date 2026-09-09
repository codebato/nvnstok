using NvnStok.Application.Interfaces;
namespace NvnStok.Infrastructure.ExternalServices;

public class MockHepsiburadaSyncService : IHepsiburadaSyncService
{
    private readonly Random _random = new();
    public Task<List<HepsiburadaProductInfo>> GetProductStockInfoAsync()
    {
        var mockData = new List<HepsiburadaProductInfo>
        {
            new HepsiburadaProductInfo
            {
                MerchantSku = "TEST-001",
                AvailableStock = _random.Next(0,15)
            }
        };
        return Task.FromResult(mockData);

    }
}
