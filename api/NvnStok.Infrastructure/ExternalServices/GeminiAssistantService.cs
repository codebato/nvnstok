using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using NvnStok.Application.Interfaces;
using NvnStok.Application.Services;

namespace NvnStok.Infrastructure.ExternalServices;

public class GeminiAssistantService : IAiAssistantService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly IStockRepository _stockRepository;
    private readonly StockCalculationService _stockCalculationService;

    public GeminiAssistantService(
        HttpClient httpClient,
        IConfiguration configuration,
        IStockRepository stockRepository,
        StockCalculationService stockCalculationService)
    {
        _httpClient = httpClient;
        _apiKey = configuration["Gemini:ApiKey"]
            ?? throw new InvalidOperationException("Gemini API key bulunamadı. User Secrets kontrol et.");
        _stockRepository = stockRepository;
        _stockCalculationService = stockCalculationService;
    }

    public async Task<string> AskAsync(string question)
    {
        var toolDefinitions = new
        {
            function_declarations = new object[]
            {
                new
                {
                    name = "get_low_stock_products",
                    description = "Kritik stok seviyesinin altındaki ürünleri getirir. Kullanıcı 'hangi ürünler azaldı', 'kritik stok' gibi bir şey sorduğunda kullan.",
                    parameters = new { type = "object", properties = new { } }
                },
                new
                {
                    name = "get_all_products",
                    description = "Sistemdeki tüm ürünleri, güncel stoklarıyla birlikte getirir. Kullanıcı genel bir envanter durumu sorduğunda kullan.",
                    parameters = new { type = "object", properties = new { } }
                }
            }
        };

        var requestBody = new
        {
            contents = new object[]
            {
                new { role = "user", parts = new object[] { new { text = question } } }
            },
            tools = new object[] { toolDefinitions }
        };

        var response = await CallGeminiApiAsync(requestBody);

        var functionCall = ExtractFunctionCall(response);

        if (functionCall is null)
        {
            return ExtractTextResponse(response);
        }

        var toolResult = await ExecuteToolAsync(functionCall.Value.Name);


        var modelPart = functionCall.Value.ThoughtSignature is not null
            ? new Dictionary<string, object>
            {
                ["functionCall"] = new { name = functionCall.Value.Name, args = new { } },
                ["thoughtSignature"] = functionCall.Value.ThoughtSignature
            }
            : new Dictionary<string, object>
            {
                ["functionCall"] = new { name = functionCall.Value.Name, args = new { } }
            };

        var followUpBody = new
        {
            contents = new object[]
            {
        new { role = "user", parts = new object[] { new { text = question } } },
        new { role = "model", parts = new object[] { modelPart } },
        new { role = "user", parts = new object[] { new { function_response = new { name = functionCall.Value.Name, response = new { result = toolResult } } } } }
            }
        };

        var finalResponse = await CallGeminiApiAsync(followUpBody);
        return ExtractTextResponse(finalResponse);
    }

    private async Task<JsonDocument> CallGeminiApiAsync(object requestBody)
    {
        var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-flash-lite-latest:generateContent?key={_apiKey}";
        var json = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync(url, content);
        var responseJson = await response.Content.ReadAsStringAsync();


        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"Gemini API hatası ({response.StatusCode}): {responseJson}");
        }

        return JsonDocument.Parse(responseJson);
    }

    private (string Name, string? ThoughtSignature)? ExtractFunctionCall(JsonDocument response)
    {
        var candidate = response.RootElement.GetProperty("candidates")[0];
        var parts = candidate.GetProperty("content").GetProperty("parts");

        foreach (var part in parts.EnumerateArray())
        {
            if (part.TryGetProperty("functionCall", out var fc))
            {
                string? thoughtSignature = part.TryGetProperty("thoughtSignature", out var ts)
                    ? ts.GetString()
                    : null;

                return (fc.GetProperty("name").GetString()!, thoughtSignature);
            }
        }

        return null;
    }

    private string ExtractTextResponse(JsonDocument response)
    {
        var candidate = response.RootElement.GetProperty("candidates")[0];
        var parts = candidate.GetProperty("content").GetProperty("parts");

        foreach (var part in parts.EnumerateArray())
        {
            if (part.TryGetProperty("text", out var text))
            {
                return text.GetString() ?? "";
            }
        }

        return "Cevap üretilemedi.";
    }

    private async Task<object> ExecuteToolAsync(string toolName)
    {
        switch (toolName)
        {
            case "get_low_stock_products":
                var allProducts = await _stockRepository.GetAllProductsAsync();
                var lowStock = new List<object>();
                foreach (var p in allProducts)
                {
                    if (await _stockCalculationService.IsLowStockAsync(p.Id))
                    {
                        var stock = await _stockCalculationService.GetCurrentStockAsync(p.Id);
                        lowStock.Add(new { p.Sku, p.Name, CurrentStock = stock });
                    }
                }
                return lowStock;

            case "get_all_products":
                var products = await _stockRepository.GetAllProductsAsync();
                var result = new List<object>();
                foreach (var p in products)
                {
                    var stock = await _stockCalculationService.GetCurrentStockAsync(p.Id);
                    result.Add(new { p.Sku, p.Name, CurrentStock = stock });
                }
                return result;

            default:
                return new { error = "Bilinmeyen araç" };
        }
    }
}
