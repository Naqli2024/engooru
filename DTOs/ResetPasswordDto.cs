namespace Engooru.DTOs;

public class ResetPasswordDto
{
    public string? Email { get; set; }
    public string? Mobile { get; set; }
    public required string NewPassword { get; set; }
    public required string ConfirmPassword { get; set; }
}