namespace NvnStok.Domain.Entities;

public class ApiKey
{
    public Guid Id { get; set; }

    public string UserId { get; set; } = string.Empty;

    public string Key { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public bool IsActive { get; set; } = true;
}