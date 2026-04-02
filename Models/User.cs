namespace Engooru.Models;

public class User
{
    public Guid Id { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Email { get; set; }
    public required string PasswordHash { get; set; }
    public required string Mobile { get; set; }
    public required string Role { get; set; }
    public string? Profile { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsVerified { get; set; } = false;
}