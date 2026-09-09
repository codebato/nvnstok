namespace NvnStok.Domain.Entities;


public class StockMovement
{
    public Guid Id { get; set; }

    public Guid ProductId { get; set; }
    public Product? Product { get; set; }


    public int Quantity { get; set; }

    public StockMovementType Type { get; set; }
    public string Source { get; set; } = string.Empty;

    public string? Note { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}