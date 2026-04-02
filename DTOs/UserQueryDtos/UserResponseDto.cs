namespace Engooru.DTOs.UserQueryDtos;

public class UserResponseDto
{
    public Guid Id { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Email { get; set; }
    public required string Mobile { get; set; }
    public required string Role { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsVerified { get; set; }
}
