using FluentAssertions;
using Moq;
using NvnStok.Application.Interfaces;
using NvnStok.Application.Services;
using NvnStok.Domain.Entities;
using Xunit;

namespace NvnStok.Tests;

public class StockCalculationServiceTests
{
    // Moq ile SAHTE bir IStockRepository üretiyoruz — gerçek veritabanına hiç dokunmuyoruz.
    // Bu, testi hem çok hızlı yapıyor hem de sadece StockCalculationService'in
    // KENDİ mantığını (toplama işlemini) izole şekilde test etmemizi sağlıyor.
    [Fact]
    public async Task GetCurrentStockAsync_HareketleriDoğruToplar()
    {
        // ARRANGE
        var productId = Guid.NewGuid();
        var mockRepo = new Mock<IStockRepository>();

        // "Bu productId için sorgu gelirse, şu sahte hareketleri döndür" diyoruz
        mockRepo.Setup(r => r.GetMovementsByProductIdAsync(productId))
            .ReturnsAsync(new List<StockMovement>
            {
                new StockMovement { Quantity = 10, Type = StockMovementType.InitialStock },
                new StockMovement { Quantity = -3, Type = StockMovementType.Sale },
                new StockMovement { Quantity = 2, Type = StockMovementType.Return }
            });

        var service = new StockCalculationService(mockRepo.Object);

        // ACT
        var result = await service.GetCurrentStockAsync(productId);

        // ASSERT — 10 - 3 + 2 = 9 olmalı
        result.Should().Be(9);
    }

    [Fact]
    public async Task IsLowStockAsync_StokEşiğinAltındaysaTrueDöner()
    {
        // ARRANGE
        var productId = Guid.NewGuid();
        var mockRepo = new Mock<IStockRepository>();

        mockRepo.Setup(r => r.GetProductByIdAsync(productId))
            .ReturnsAsync(new Product { Id = productId, LowStockThreshold = 5 });

        mockRepo.Setup(r => r.GetMovementsByProductIdAsync(productId))
            .ReturnsAsync(new List<StockMovement>
            {
                new StockMovement { Quantity = 3, Type = StockMovementType.InitialStock }
            });

        var service = new StockCalculationService(mockRepo.Object);

        // ACT
        var result = await service.IsLowStockAsync(productId);

        // ASSERT — stok 3, eşik 5, yani kritik seviyede (true dönmeli)
        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsLowStockAsync_StokEşiğinÜstündeyseFalseDöner()
    {
        var productId = Guid.NewGuid();
        var mockRepo = new Mock<IStockRepository>();

        mockRepo.Setup(r => r.GetProductByIdAsync(productId))
            .ReturnsAsync(new Product { Id = productId, LowStockThreshold = 5 });

        mockRepo.Setup(r => r.GetMovementsByProductIdAsync(productId))
            .ReturnsAsync(new List<StockMovement>
            {
                new StockMovement { Quantity = 20, Type = StockMovementType.InitialStock }
            });

        var service = new StockCalculationService(mockRepo.Object);

        var result = await service.IsLowStockAsync(productId);

        result.Should().BeFalse();
    }
}