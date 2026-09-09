using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NvnStok.Domain.Entities;
using NvnStok.Infrastructure.Auth;

namespace NvnStok.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly JwtTokenGenerator _tokenGenerator;


    public AuthController(UserManager<ApplicationUser> userManager, JwtTokenGenerator tokenGenerator)
    {
        _userManager = userManager;
        _tokenGenerator = tokenGenerator;
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
}

public record RegisterRequest(string Email, string Password);
public record LoginRequest(string Email, string Password);