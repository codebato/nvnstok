using Microsoft.AspNetCore.Mvc;
using NvnStok.Application.Interfaces;
using NvnStok.Application.Services;
using NvnStok.Domain.Entities;
using Microsoft.AspNetCore.Authorization;

namespace NvnStok.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProductsController : ControllerBase
{
    private readonly IStockRepository _stockRepository;
    private readonly StockCalculationService _stockCalculationService;
    private readonly ITrendyolSyncService _trendyolSyncService;
    private readonly IHepsiburadaSyncService _hepsiburadaSyncService;


    private string GetCurrentUserId()
    {
        return User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value
            ?? throw new UnauthorizedAccessException("Kullanıcı kimliği bulunamadı.");
    }


    public ProductsController(
        IStockRepository stockRepository,
        StockCalculationService stockCalculationService,
        ITrendyolSyncService trendyolSyncService,
        IHepsiburadaSyncService hepsiburadaSyncService)


    {
        _stockRepository = stockRepository;
        _stockCalculationService = stockCalculationService;
        _trendyolSyncService = trendyolSyncService;
        _hepsiburadaSyncService = hepsiburadaSyncService;
    }


    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var userId = GetCurrentUserId();
        var allProducts = await _stockRepository.GetAllProductsAsync();
        var products = allProducts.Where(p => p.UserId == userId).ToList();

        var result = new List<object>();
        foreach (var product in products)
        {
            var currentStock = await _stockCalculationService.GetCurrentStockAsync(product.Id);
            result.Add(new
            {
                product.Id,
                product.Sku,
                product.Name,
                CurrentStock = currentStock,
                product.LowStockThreshold
            });
        }

        return Ok(result);
    }
    [HttpGet("low-stock")]
    public async Task<IActionResult> GetLowStock()
    {
        var userId = GetCurrentUserId();
        var allProducts = await _stockRepository.GetAllProductsAsync();
        var products = allProducts.Where(p => p.UserId == userId).ToList();

        var result = new List<object>();
        foreach (var product in products)
        {
            var isLow = await _stockCalculationService.IsLowStockAsync(product.Id);
            if (!isLow) continue;
           

            var currentStock = await _stockCalculationService.GetCurrentStockAsync(product.Id);
            result.Add(new
            {
                product.Id,
                product.Sku,
                product.Name,
                CurrentStock = currentStock,
                product.LowStockThreshold
            });
        }
        return Ok(result);
    }


    [HttpPost("sync-trendyol")]
    public async Task<IActionResult> SyncTrendyol()
    {
        var trendyolProducts = await _trendyolSyncService.GetProductStockInfoAsync();
        var syncResults = new List<object>();

        foreach (var trendyolProduct in trendyolProducts)
        {

            var allProducts = await _stockRepository.GetAllProductsAsync();
            var matchedProduct = allProducts.FirstOrDefault(p => p.Sku == trendyolProduct.Barcode);

            if (matchedProduct is null)
            {
                syncResults.Add(new { trendyolProduct.Barcode, Status = "Eşleşen ürün bulunamadı" });
                continue;
            }


            var ourCurrentStock = await _stockCalculationService.GetCurrentStockAsync(matchedProduct.Id);
            var difference = trendyolProduct.Quantity - ourCurrentStock;

            if (difference != 0)
            {

                await _stockRepository.AddMovementAsync(new StockMovement
                {
                    Id = Guid.NewGuid(),
                    ProductId = matchedProduct.Id,
                    Quantity = difference,
                    Type = StockMovementType.MarketplaceSync,
                    Source = "trendyol",
                    Note = $"Trendyol senkronizasyonu: {ourCurrentStock} -> {trendyolProduct.Quantity}"
                });
            }

            syncResults.Add(new
            {
                trendyolProduct.Barcode,
                PreviousStock = ourCurrentStock,
                TrendyolStock = trendyolProduct.Quantity,
                Adjustment = difference,
                Status = difference == 0 ? "Zaten güncel" : "Güncellendi"
            });
        }

        return Ok(syncResults);
    }


    [HttpPost("sync-hepsiburada")]
    public async Task<IActionResult> SyncHepsiburada()
    {
        var hbProducts = await _hepsiburadaSyncService.GetProductStockInfoAsync();
        var syncResults = new List<object>();

        foreach (var hbProduct in hbProducts)
        {
            var allProducts = await _stockRepository.GetAllProductsAsync();
            var matchedProduct = allProducts.FirstOrDefault(p => p.Sku == hbProduct.MerchantSku);

            if (matchedProduct is null)
            {
                syncResults.Add(new { hbProduct.MerchantSku, Status = "Eşleşen ürün bulunamadı" });
                continue;
            }

            var ourCurrentStock = await _stockCalculationService.GetCurrentStockAsync(matchedProduct.Id);
            var difference = hbProduct.AvailableStock - ourCurrentStock;

            if (difference != 0)
            {
                await _stockRepository.AddMovementAsync(new StockMovement
                {
                    Id = Guid.NewGuid(),
                    ProductId = matchedProduct.Id,
                    Quantity = difference,
                    Type = StockMovementType.MarketplaceSync,
                    Source = "hepsiburada",
                    Note = $"Hepsiburada senkronizasyonu: {ourCurrentStock} -> {hbProduct.AvailableStock}"
                });
            }

            syncResults.Add(new
            {
                hbProduct.MerchantSku,
                PreviousStock = ourCurrentStock,
                HepsiburadaStock = hbProduct.AvailableStock,
                Adjustment = difference,
                Status = difference == 0 ? "Zaten güncel" : "Güncellendi"
            });
        }

        return Ok(syncResults);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProductRequest request)
    {
        var userId = GetCurrentUserId();
        var product = new Product
        {
            Id = Guid.NewGuid(),
            Sku = request.Sku,
            Name = request.Name,
            LowStockThreshold = request.LowStockThreshold,
            UserId = userId
        };

        await _stockRepository.AddProductAsync(product);


        if (request.InitialStock > 0)
        {
            await _stockRepository.AddMovementAsync(new StockMovement
            {
                Id = Guid.NewGuid(),
                ProductId = product.Id,
                Quantity = request.InitialStock,
                Type = StockMovementType.InitialStock,
                Source = "manual"
            });
        }

        return CreatedAtAction(nameof(GetById), new { id = product.Id }, new
        {
            product.Id,
            product.Sku,
            product.Name,
            CurrentStock = request.InitialStock,
            product.LowStockThreshold
        });
    }


    [HttpPost("{id}/movements")]
    public async Task<IActionResult> AddMovement(Guid id, [FromBody] AddMovementRequest request)
    {
        var userId = GetCurrentUserId();
        var product = await _stockRepository.GetProductByIdAsync(id);
        if (product is null) return NotFound($"Ürün bulunamadı: {id}");
        if (product.UserId != userId) return NotFound($"Ürün bulunamadı: {id}");
        var movement = new StockMovement
        {
            Id = Guid.NewGuid(),
            ProductId = id,
            Quantity = request.Quantity,
            Type = request.Type,
            Source = request.Source,
            Note = request.Note
        };

        await _stockRepository.AddMovementAsync(movement);


        var currentStock = await _stockCalculationService.GetCurrentStockAsync(id);
        var isLowStock = await _stockCalculationService.IsLowStockAsync(id);

        return Ok(new
        {
            movement.Id,
            movement.Quantity,
            movement.Type,
            movement.CreatedAt,
            CurrentStock = currentStock,
            IsLowStock = isLowStock
        });
    }


    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var userId = GetCurrentUserId();
        var product = await _stockRepository.GetProductByIdAsync(id);
        if (product is null) return NotFound();
        if (product.UserId != userId) return NotFound();

        var currentStock = await _stockCalculationService.GetCurrentStockAsync(id);

        return Ok(new
        {
            product.Id,
            product.Sku,
            product.Name,
            CurrentStock = currentStock,
            product.LowStockThreshold
        });
    }
}

public record CreateProductRequest(string Sku, string Name, int InitialStock, int LowStockThreshold = 5);
public record AddMovementRequest(int Quantity, StockMovementType Type, string Source, string? Note = null);