namespace Engooru.DTOs;

public class VerifyResetEmailOtpDto
{
    public required string Email { get; set; }
    public required string Otp { get; set; }
}