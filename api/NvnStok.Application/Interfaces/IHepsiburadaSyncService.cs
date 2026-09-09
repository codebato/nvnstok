namespace NvnStok.Application.Interfaces;

public interface IHepsiburadaSyncService
{
    Task<List<HepsiburadaProductInfo>> GetProductStockInfoAsync();

}

public class HepsiburadaProductInfo
{
    public string MerchantSku {get; set;} = string.Empty;
    public int AvailableStock {get; set;}

}
