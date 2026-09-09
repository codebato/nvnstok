

namespace NvnStok.Application.Interfaces;

public interface ITrendyolSyncService
{
    Task<List<TrendyolProductInfo>> GetProductStockInfoAsync();

}

public class TrendyolProductInfo
{
    public string Barcode {get; set;} = string.Empty;
    public int Quantity {get; set;}
    public decimal SalePrice {get; set;}

}