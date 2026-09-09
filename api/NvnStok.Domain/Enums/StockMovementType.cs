namespace NvnStok.Domain.Entities;

public enum StockMovementType
{
    Sale,
    Return,
    ManualAdjustment,
    MarketplaceSync,
    InitialStock
}