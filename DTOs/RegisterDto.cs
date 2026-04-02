namespace Engooru.DTOs;

public class RegisterRequestDto
{
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Email { get; set; }
    public required string Password { get; set; }
    public required string Mobile { get; set; }
    public required string Role { get; set; }
    public string? Profile { get; set; }
}

public class RegisterResponseDto
{
    public Guid Id { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Email { get; set; }
    public required string Mobile { get; set; }
    public required string Role { get; set; }
    public string? Profile { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsVerified { get; set; } = false;
}