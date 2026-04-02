namespace Engooru.Models;

public class VerificationCode
{
    public Guid Id { get; set; }

    public required string Email { get; set; }

    public required string Mobile { get; set; }

    public required string EmailCode { get; set; }

    public required string MobileCode { get; set; }

    public bool EmailVerified { get; set; } = false;

    public bool MobileVerified { get; set; } = false;

    public string? ResetEmailOtp { get; set; }
    public string? ResetMobileOtp { get; set; }

    public bool IsResetEmailVerified { get; set; } = false;
    public bool IsResetMobileVerified { get; set; } = false;

    public DateTime? ResetOtpExpiry { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}