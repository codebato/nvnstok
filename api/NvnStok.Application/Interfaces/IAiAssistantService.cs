namespace NvnStok.Application.Interfaces;

public interface IAiAssistantService
{
    Task<string>AskAsync(string question);

}
