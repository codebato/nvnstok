using Microsoft.AspNetCore.Mvc;
using NvnStok.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace NvnStok.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] 
public class AssistantController : ControllerBase
{
    private readonly IAiAssistantService _aiAssistantService;

    public AssistantController(IAiAssistantService aiAssistantService)
    {
        _aiAssistantService = aiAssistantService;
    }

    [HttpPost("ask")]
    public async Task<IActionResult> Ask([FromBody] AskRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Question))
            return BadRequest("Soru boş olamaz.");

        var answer = await _aiAssistantService.AskAsync(request.Question);
        return Ok(new { question = request.Question, answer });
    }
}

public record AskRequest(string Question);