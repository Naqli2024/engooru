namespace Engooru.DTOs;

public class VerifyResetMobileOtpDto
{
    public required string Mobile { get; set; }
    public required string Otp { get; set; }
}