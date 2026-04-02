namespace Engooru.DTOs;

public class VerifyEmailOtpDto
{
    public required string Email { get; set; }
    public required string EmailCode { get; set; }
}

public class VerifyMobileOtpDto
{
    public required string Mobile { get; set; }
    public required string MobileCode { get; set; }
}