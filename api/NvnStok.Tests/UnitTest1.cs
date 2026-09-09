using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace NvnStok.Tests;

// WebApplicationFactory, API'mizi test için bellekte ayağa kaldırıyor —
// gerçek dotnet run'a, Scalar'a gerek yok, testler saniyeler içinde çalışıyor.
public class AuthorizationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public AuthorizationTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    // Yardımcı metod: verilen email/şifre ile kayıt olup token döner.
    // Her testte email'i benzersiz yapıyoruz (Guid ekleyerek) — testler tekrar tekrar
    // çalıştığında "email zaten kayıtlı" hatası almamak için.
    private async Task<string> RegisterAndGetTokenAsync(string emailPrefix)
    {
        var email = $"{emailPrefix}-{Guid.NewGuid()}@test.com";
        var response = await _client.PostAsJsonAsync("/api/auth/register", new
        {
            email,
            password = "Test123!"
        });

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<RegisterResponse>();
        return result!.Token;
    }

    [Fact]
    public async Task KullanıcıB_KullanıcıA_ninÜrününeErişemez()
    {
        // ARRANGE (hazırlık): iki farklı kullanıcı oluşturuyoruz
        var tokenA = await RegisterAndGetTokenAsync("userA");
        var tokenB = await RegisterAndGetTokenAsync("userB");

        // Kullanıcı A, bir ürün oluşturuyor
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokenA);

        var createResponse = await _client.PostAsJsonAsync("/api/products", new
        {
            sku = $"TEST-{Guid.NewGuid()}",
            name = "Test Ürünü",
            initialStock = 10,
            lowStockThreshold = 5
        });
        createResponse.EnsureSuccessStatusCode();
        var product = await createResponse.Content.ReadFromJsonAsync<ProductResponse>();

        // ACT (eylem): Kullanıcı B, Kullanıcı A'nın ürününe erişmeye çalışıyor
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokenB);

        var getResponse = await _client.GetAsync($"/api/products/{product!.Id}");

        // ASSERT (doğrulama): 404 dönmeli — Kullanıcı B bu ürünü GÖREMEMELİ
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task KullanıcıA_KendiÜrününeErişebilir()
    {
        var tokenA = await RegisterAndGetTokenAsync("userA");

        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokenA);

        var createResponse = await _client.PostAsJsonAsync("/api/products", new
        {
            sku = $"TEST-{Guid.NewGuid()}",
            name = "Test Ürünü",
            initialStock = 10,
            lowStockThreshold = 5
        });
        createResponse.EnsureSuccessStatusCode();
        var product = await createResponse.Content.ReadFromJsonAsync<ProductResponse>();

        var getResponse = await _client.GetAsync($"/api/products/{product!.Id}");

        // Kendi ürünü — 200 OK dönmeli
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task TokenOlmadanİstekAtmak_401Döner()
    {
        // Authorization header'ı YOK — hiç giriş yapmamış gibi
        var response = await _client.GetAsync("/api/products");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}

// Backend'in döndüğü cevapların şeklini burada tanımlıyoruz (test projesi kendi DTO'larını kullanır)
public record RegisterResponse(string Token, string Email);
public record ProductResponse(Guid Id, string Sku, string Name, int CurrentStock, int LowStockThreshold);