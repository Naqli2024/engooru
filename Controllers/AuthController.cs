using Engooru.Data;
using Engooru.DTOs;
using Engooru.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace Engooru.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly PasswordHasher<User> _passwordHasher;
        private readonly IConfiguration _config;

        public UserController(AppDbContext context, IConfiguration config)
        {
            _context = context;
            _passwordHasher = new PasswordHasher<User>();
            _config = config;
        }

        // POST api/users/register
        [HttpPost("register")]
        public async Task<IActionResult> RegisterUser(RegisterRequestDto dto)
        {
            // Check existing user
            if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
            {
                return BadRequest("Email already exists");
            }

            // Generate OTP
            var emailCode = new Random().Next(100000, 999999).ToString();
            var mobileCode = new Random().Next(100000, 999999).ToString();

            // Create temp user object (to satisfy required fields)
            var tempUser = new User
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                Mobile = dto.Mobile,
                Role = dto.Role,
                Profile = dto.Profile,
                PasswordHash = "" // temporary
            };

            // Hash password properly
            var hashedPassword = _passwordHasher.HashPassword(tempUser, dto.Password);

            // Save into VerificationCodes (temporary storage)
            var verification = new VerificationCode
            {
                Email = dto.Email,
                Mobile = dto.Mobile,
                EmailCode = emailCode,
                MobileCode = mobileCode,
                EmailVerified = false,
                MobileVerified = false,

                // store user data temporarily
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Role = dto.Role,
                Profile = dto.Profile,
                PasswordHash = hashedPassword
            };

            _context.VerificationCodes.Add(verification);
            await _context.SaveChangesAsync();

            // Response
            return Ok(new
            {
                message = "Verify email and mobile",
                emailCode = emailCode,
                mobileCode = mobileCode
            });
        }

        [HttpPost("verify-email")]
        public async Task<IActionResult> VerifyEmail(VerifyEmailOtpDto dto)
        {
            var record = await _context.VerificationCodes
                .FirstOrDefaultAsync(v => v.Email == dto.Email);

            if (record == null)
                return BadRequest("Record not found");

            if (record.EmailCode != dto.EmailCode)
                return BadRequest("Invalid Email OTP");

            record.EmailVerified = true;

            await _context.SaveChangesAsync();

            await CheckAndCreateUser(record);

            return Ok("Email verified");
        }

        [HttpPost("verify-mobile")]
        public async Task<IActionResult> VerifyMobile(VerifyMobileOtpDto dto)
        {
            var record = await _context.VerificationCodes
                .FirstOrDefaultAsync(v => v.Mobile == dto.Mobile);

            if (record == null)
                return BadRequest("Record not found");

            if (record.MobileCode != dto.MobileCode)
                return BadRequest("Invalid Mobile OTP");

            record.MobileVerified = true;

            await _context.SaveChangesAsync();

            await CheckAndCreateUser(record);

            return Ok("Mobile verified");
        }

        private async Task CheckAndCreateUser(VerificationCode record)
        {
            if (record.EmailVerified && record.MobileVerified)
            {
                var user = new User
                {
                    FirstName = record.FirstName,
                    LastName = record.LastName,
                    Email = record.Email,
                    Mobile = record.Mobile,
                    Role = record.Role,
                    Profile = record.Profile,
                    PasswordHash = record.PasswordHash,
                    IsVerified = true
                };

                _context.Users.Add(user);

                // remove verification after success
                _context.VerificationCodes.Remove(record);

                await _context.SaveChangesAsync();
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(x => x.Email == dto.Email);

            if (user == null)
                return Unauthorized("Invalid email or password");

            var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, dto.Password);

            if (result == PasswordVerificationResult.Failed)
                return Unauthorized("Invalid email or password");

            var key = _config["Jwt:Key"];

            if (string.IsNullOrEmpty(key))
                return StatusCode(500, "JWT Key not configured");

            var token = JwtHelper.GenerateToken(user, key);

            return Ok(new
            {
                token,
                user.Id,
                user.FirstName,
                user.LastName,
                user.Role
            });
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordDto dto)
        {
            if (string.IsNullOrEmpty(dto.Email) && string.IsNullOrEmpty(dto.Mobile))
                return BadRequest("Provide Email or Mobile");

            var user = await _context.Users.FirstOrDefaultAsync(u =>
                (!string.IsNullOrEmpty(dto.Email) && u.Email == dto.Email) ||
                (!string.IsNullOrEmpty(dto.Mobile) && u.Mobile == dto.Mobile));

            if (user == null)
                return BadRequest("User not found");

            var record = await _context.VerificationCodes.FirstOrDefaultAsync(v =>
                v.Email == user.Email && v.Mobile == user.Mobile);

            if (record == null)
            {
                record = new VerificationCode
                {
                    Email = user.Email,
                    Mobile = user.Mobile,

                    // ✅ REQUIRED FIELDS (IMPORTANT FIX)
                    EmailCode = "",
                    MobileCode = "",

                    EmailVerified = true,
                    MobileVerified = true
                };

                _context.VerificationCodes.Add(record);
            }

            var otp = new Random().Next(100000, 999999).ToString();

            record.ResetOtpExpiry = DateTime.UtcNow.AddMinutes(5);

            if (!string.IsNullOrEmpty(dto.Email))
            {
                record.ResetEmailOtp = otp;
                record.IsResetEmailVerified = false;
            }

            if (!string.IsNullOrEmpty(dto.Mobile))
            {
                record.ResetMobileOtp = otp;
                record.IsResetMobileVerified = false;
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "OTP sent",
                otp = otp
            });
        }

        [HttpPost("verify-reset-email")]
        public async Task<IActionResult> VerifyResetEmail(VerifyResetEmailOtpDto dto)
        {
            var record = await _context.VerificationCodes
                .FirstOrDefaultAsync(v => v.Email == dto.Email);

            if (record == null)
                return BadRequest("Record not found");

            if (record.IsResetEmailVerified)
                return Ok("Email already verified");

            if (record.ResetEmailOtp != dto.Otp)
                return BadRequest("Invalid OTP");

            if (record.ResetOtpExpiry == null || record.ResetOtpExpiry < DateTime.UtcNow)
                return BadRequest("OTP expired");

            record.IsResetEmailVerified = true;

            await _context.SaveChangesAsync();

            return Ok("Email OTP verified");
        }

        [HttpPost("verify-reset-mobile")]
        public async Task<IActionResult> VerifyResetMobile(VerifyResetMobileOtpDto dto)
        {
            var record = await _context.VerificationCodes
                .FirstOrDefaultAsync(v => v.Mobile == dto.Mobile);

            if (record == null)
                return BadRequest("Record not found");

            if (record.IsResetMobileVerified)
                return Ok("Mobile already verified");

            if (record.ResetMobileOtp != dto.Otp)
                return BadRequest("Invalid OTP");

            if (record.ResetOtpExpiry == null || record.ResetOtpExpiry < DateTime.UtcNow)
                return BadRequest("OTP expired");

            record.IsResetMobileVerified = true;

            await _context.SaveChangesAsync();

            return Ok("Mobile OTP verified");
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordDto dto)
        {
            if (string.IsNullOrEmpty(dto.Email) && string.IsNullOrEmpty(dto.Mobile))
                return BadRequest("Email or Mobile is required");

            if (dto.NewPassword != dto.ConfirmPassword)
                return BadRequest("Passwords do not match");

            if (dto.NewPassword.Length < 6)
                return BadRequest("Password must be at least 6 characters");

            var user = await _context.Users.FirstOrDefaultAsync(u =>
                (dto.Email != null && u.Email == dto.Email) ||
                (dto.Mobile != null && u.Mobile == dto.Mobile));

            if (user == null)
                return BadRequest("User not found");

            var record = await _context.VerificationCodes.FirstOrDefaultAsync(v =>
                (dto.Email != null && v.Email == dto.Email) ||
                (dto.Mobile != null && v.Mobile == dto.Mobile));

            if (record == null)
                return BadRequest("Verification not found");

            if (record.ResetOtpExpiry == null || record.ResetOtpExpiry < DateTime.UtcNow)
                return BadRequest("OTP expired");

            bool isVerified =
                (!string.IsNullOrEmpty(dto.Email) && record.IsResetEmailVerified) ||
                (!string.IsNullOrEmpty(dto.Mobile) && record.IsResetMobileVerified);

            if (!isVerified)
                return BadRequest("OTP not verified");

            user.PasswordHash = _passwordHasher.HashPassword(user, dto.NewPassword);

            // Clear OTP after success
            record.ResetEmailOtp = null;
            record.ResetMobileOtp = null;
            record.IsResetEmailVerified = false;
            record.IsResetMobileVerified = false;
            record.ResetOtpExpiry = null;

            await _context.SaveChangesAsync();

            return Ok("Password reset successful");
        }

        [Authorize]
        [HttpPut("update")]
        public async Task<IActionResult> UpdateUser(UpdateUserDto dto)
        {
            // 1. Get logged-in user ID from JWT
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
                return Unauthorized("Invalid token");

            var user = await _context.Users.FindAsync(Guid.Parse(userId));

            if (user == null)
                return NotFound("User not found");

            // 2. Update only provided fields
            if (!string.IsNullOrEmpty(dto.FirstName))
                user.FirstName = dto.FirstName;

            if (!string.IsNullOrEmpty(dto.LastName))
                user.LastName = dto.LastName;

            if (!string.IsNullOrEmpty(dto.Mobile))
                user.Mobile = dto.Mobile;

            if (!string.IsNullOrEmpty(dto.Profile))
                user.Profile = dto.Profile;

            // 3. Save changes
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "User updated successfully",
                data = new
                {
                    user.Id,
                    user.FirstName,
                    user.LastName,
                    user.Email,
                    user.Mobile,
                    user.Role,
                    user.Profile
                }
            });
        }
    }
}