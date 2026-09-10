using System.Security.Claims;
using NvnStok.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace NvnStok.Api.Middleware;


public class ApiKeyMiddleware
{
    private readonly RequestDelegate _next;
    private const string ApiKeyHeaderName = "X-Api-Key";

    public ApiKeyMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, NvnStokDbContext dbContext)
    {
        if (context.Request.Headers.TryGetValue(ApiKeyHeaderName, out var extractedApiKey))
        {
            var apiKey = await dbContext.ApiKeys
                .FirstOrDefaultAsync(k => k.Key == extractedApiKey.ToString() && k.IsActive);

            if (apiKey is not null)
            {

                var claims = new List<Claim>
                {
                    new Claim(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub, apiKey.UserId)
                };
                var identity = new ClaimsIdentity(claims, "ApiKey");
                context.User = new ClaimsPrincipal(identity);
            }
        }

        await _next(context);
    }
}