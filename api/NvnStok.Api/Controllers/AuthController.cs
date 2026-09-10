using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NvnStok.Domain.Entities;
using NvnStok.Infrastructure.Auth;
using NvnStok.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;

namespace NvnStok.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly JwtTokenGenerator _tokenGenerator;
    private readonly NvnStokDbContext _dbContext;  

    public AuthController(
        UserManager<ApplicationUser> userManager,
        JwtTokenGenerator tokenGenerator,
        NvnStokDbContext dbContext)
    {
        _userManager = userManager;
        _tokenGenerator = tokenGenerator;
        _dbContext = dbContext;
    }


    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser is not null)
            return BadRequest("Bu email zaten kayıtlı.");

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email
        };


        var result = await _userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
        {

            var errors = result.Errors.Select(e => e.Description);
            return BadRequest(new { errors });
        }

        var token = _tokenGenerator.GenerateToken(user);
        return Ok(new { token, email = user.Email });
    }


    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null)
            return Unauthorized("Email veya şifre hatalı.");


        var isPasswordValid = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!isPasswordValid)
            return Unauthorized("Email veya şifre hatalı.");

        var token = _tokenGenerator.GenerateToken(user);
        return Ok(new { token, email = user.Email });
    }


[HttpPost("api-keys")]
[Authorize]
public async Task<IActionResult> CreateApiKey([FromBody] CreateApiKeyRequest request)
{
    var userId = User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value;
    if (userId is null) return Unauthorized();


    var apiKey = new ApiKey
    {
        Id = Guid.NewGuid(),
        UserId = userId,
        Key = Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N"),
        Name = request.Name
    };

    _dbContext.ApiKeys.Add(apiKey);
    await _dbContext.SaveChangesAsync();

    return Ok(new { apiKey.Id, apiKey.Key, apiKey.Name });
}

public record CreateApiKeyRequest(string Name);
}

public record RegisterRequest(string Email, string Password);
public record LoginRequest(string Email, string Password);