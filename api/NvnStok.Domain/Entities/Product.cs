namespace NvnStok.Domain.Entities;


public class Product
{
    public Guid Id { get; set; }

    
    public string Sku { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;


    public string? TrendyolProductCode { get; set; }
    public string? HepsiburadaListingId { get; set; }


    public int LowStockThreshold { get; set; } = 5;

    public string UserId { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<StockMovement> Movements { get; set; } = new List<StockMovement>();
}

