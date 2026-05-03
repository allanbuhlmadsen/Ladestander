namespace Ladestander.Api.Entities;

public class AdminUser
{
    public int AdminUserId { get; set; }

    public string Username { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;

    public string Role { get; set; } = "Admin";

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}