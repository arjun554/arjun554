using Grpc.Core;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AutoMapper;
using FoodDash.Database;
using FoodDash.Database.Entities;
using FoodDash.Shared.Protos;
using FoodDash.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace FoodDash.AuthService.Services;

public class AuthGrpcService : AuthService.AuthServiceBase
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly IConfiguration _configuration;
    private readonly FoodDashDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<AuthGrpcService> _logger;

    public AuthGrpcService(
        UserManager<User> userManager,
        SignInManager<User> signInManager,
        IConfiguration configuration,
        FoodDashDbContext context,
        IMapper mapper,
        ILogger<AuthGrpcService> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _configuration = configuration;
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public override async Task<LoginResponse> Login(LoginRequest request, ServerCallContext context)
    {
        try
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                return new LoginResponse
                {
                    Success = false,
                    Message = "Invalid email or password"
                };
            }

            if (!user.IsActive)
            {
                return new LoginResponse
                {
                    Success = false,
                    Message = "Account is inactive"
                };
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: false);
            if (!result.Succeeded)
            {
                return new LoginResponse
                {
                    Success = false,
                    Message = "Invalid email or password"
                };
            }

            var token = await GenerateJwtTokenAsync(user.Id);
            var refreshToken = GenerateRefreshToken();

            // Store refresh token
            user.SecurityStamp = refreshToken;
            await _userManager.UpdateAsync(user);

            return new LoginResponse
            {
                Success = true,
                Message = "Login successful",
                Token = token,
                RefreshToken = refreshToken,
                ExpiresAt = DateTimeOffset.UtcNow.AddHours(24).ToUnixTimeSeconds(),
                User = MapToUserProfile(user)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for email: {Email}", request.Email);
            return new LoginResponse
            {
                Success = false,
                Message = "An error occurred during login"
            };
        }
    }

    public override async Task<LoginResponse> Register(RegisterRequest request, ServerCallContext context)
    {
        try
        {
            // Check if user already exists
            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser != null)
            {
                return new LoginResponse
                {
                    Success = false,
                    Message = "Email is already registered"
                };
            }

            // Validate password confirmation
            if (request.Password != request.ConfirmPassword)
            {
                return new LoginResponse
                {
                    Success = false,
                    Message = "Password and confirm password do not match"
                };
            }

            // Create new user
            var user = new User
            {
                UserName = request.Email,
                Email = request.Email,
                FullName = request.FullName,
                PhoneNumber = request.PhoneNumber,
                UserType = (UserType)request.UserType,
                Address = request.Address,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return new LoginResponse
                {
                    Success = false,
                    Message = $"Registration failed: {errors}"
                };
            }

            // Create role-specific records
            await CreateRoleSpecificRecords(user);

            // Generate token for immediate login
            var token = await GenerateJwtTokenAsync(user.Id);
            var refreshToken = GenerateRefreshToken();

            user.SecurityStamp = refreshToken;
            await _userManager.UpdateAsync(user);

            return new LoginResponse
            {
                Success = true,
                Message = "Registration successful",
                Token = token,
                RefreshToken = refreshToken,
                ExpiresAt = DateTimeOffset.UtcNow.AddHours(24).ToUnixTimeSeconds(),
                User = MapToUserProfile(user)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registration for email: {Email}", request.Email);
            return new LoginResponse
            {
                Success = false,
                Message = "An error occurred during registration"
            };
        }
    }

    public override async Task<ChangePasswordResponse> ChangePassword(ChangePasswordRequest request, ServerCallContext context)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(request.UserId);
            if (user == null)
            {
                return new ChangePasswordResponse
                {
                    Success = false,
                    Message = "User not found"
                };
            }

            var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return new ChangePasswordResponse
                {
                    Success = false,
                    Message = $"Password change failed: {errors}"
                };
            }

            return new ChangePasswordResponse
            {
                Success = true,
                Message = "Password changed successfully"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error changing password for user: {UserId}", request.UserId);
            return new ChangePasswordResponse
            {
                Success = false,
                Message = "An error occurred while changing password"
            };
        }
    }

    public override async Task<ForgotPasswordResponse> ForgotPassword(ForgotPasswordRequest request, ServerCallContext context)
    {
        try
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                // For security, always return success even if user doesn't exist
                return new ForgotPasswordResponse
                {
                    Success = true,
                    Message = "If the email exists in our system, a reset link has been sent"
                };
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            
            // TODO: Send email with reset token
            // For now, just log it (in production, send via email service)
            _logger.LogInformation("Password reset token for {Email}: {Token}", request.Email, token);

            return new ForgotPasswordResponse
            {
                Success = true,
                Message = "If the email exists in our system, a reset link has been sent"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in forgot password for email: {Email}", request.Email);
            return new ForgotPasswordResponse
            {
                Success = false,
                Message = "An error occurred while processing your request"
            };
        }
    }

    public override async Task<ResetPasswordResponse> ResetPassword(ResetPasswordRequest request, ServerCallContext context)
    {
        try
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                return new ResetPasswordResponse
                {
                    Success = false,
                    Message = "Invalid reset request"
                };
            }

            var result = await _userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return new ResetPasswordResponse
                {
                    Success = false,
                    Message = $"Password reset failed: {errors}"
                };
            }

            return new ResetPasswordResponse
            {
                Success = true,
                Message = "Password reset successfully"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting password for email: {Email}", request.Email);
            return new ResetPasswordResponse
            {
                Success = false,
                Message = "An error occurred while resetting password"
            };
        }
    }

    public override async Task<LoginResponse> RefreshToken(RefreshTokenRequest request, ServerCallContext context)
    {
        try
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.SecurityStamp == request.RefreshToken);
            if (user == null || !user.IsActive)
            {
                return new LoginResponse
                {
                    Success = false,
                    Message = "Invalid refresh token"
                };
            }

            var token = await GenerateJwtTokenAsync(user.Id);
            var newRefreshToken = GenerateRefreshToken();

            user.SecurityStamp = newRefreshToken;
            await _context.SaveChangesAsync();

            return new LoginResponse
            {
                Success = true,
                Message = "Token refreshed successfully",
                Token = token,
                RefreshToken = newRefreshToken,
                ExpiresAt = DateTimeOffset.UtcNow.AddHours(24).ToUnixTimeSeconds(),
                User = MapToUserProfile(user)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing token");
            return new LoginResponse
            {
                Success = false,
                Message = "An error occurred while refreshing token"
            };
        }
    }

    public override async Task<GetProfileResponse> GetProfile(GetProfileRequest request, ServerCallContext context)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(request.UserId);
            if (user == null)
            {
                return new GetProfileResponse
                {
                    Success = false,
                    Message = "User not found"
                };
            }

            return new GetProfileResponse
            {
                Success = true,
                Message = "Profile retrieved successfully",
                User = MapToUserProfile(user)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting profile for user: {UserId}", request.UserId);
            return new GetProfileResponse
            {
                Success = false,
                Message = "An error occurred while retrieving profile"
            };
        }
    }

    public override async Task<UpdateProfileResponse> UpdateProfile(UpdateProfileRequest request, ServerCallContext context)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(request.UserId);
            if (user == null)
            {
                return new UpdateProfileResponse
                {
                    Success = false,
                    Message = "User not found"
                };
            }

            user.FullName = request.FullName;
            user.PhoneNumber = request.PhoneNumber;
            user.Address = request.Address;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return new UpdateProfileResponse
                {
                    Success = false,
                    Message = $"Profile update failed: {errors}"
                };
            }

            return new UpdateProfileResponse
            {
                Success = true,
                Message = "Profile updated successfully",
                User = MapToUserProfile(user)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating profile for user: {UserId}", request.UserId);
            return new UpdateProfileResponse
            {
                Success = false,
                Message = "An error occurred while updating profile"
            };
        }
    }

    private async Task<string> GenerateJwtTokenAsync(int userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null) return string.Empty;

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_configuration["Jwt:SecretKey"] ?? "your-secret-key-here");
        
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email ?? ""),
            new(ClaimTypes.Name, user.FullName ?? ""),
            new("UserType", user.UserType.ToString())
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddHours(24),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
            Issuer = _configuration["Jwt:Issuer"],
            Audience = _configuration["Jwt:Audience"]
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    private static string GenerateRefreshToken()
    {
        return Guid.NewGuid().ToString();
    }

    private async Task CreateRoleSpecificRecords(User user)
    {
        switch (user.UserType)
        {
            case UserType.Customer:
                var customer = new Customer
                {
                    UserId = user.Id,
                    LoyaltyPoints = 0,
                    PreferredLanguage = "en",
                    CreatedAt = DateTime.UtcNow
                };
                _context.Customers.Add(customer);
                break;

            case UserType.DeliveryPartner:
                var deliveryPartner = new DeliveryPartner
                {
                    UserId = user.Id,
                    VehicleType = "Motorcycle",
                    LicenseNumber = "",
                    IsAvailable = true,
                    CurrentLatitude = 0,
                    CurrentLongitude = 0,
                    CreatedAt = DateTime.UtcNow
                };
                _context.DeliveryPartners.Add(deliveryPartner);
                break;
        }

        await _context.SaveChangesAsync();
    }

    private static UserProfile MapToUserProfile(User user)
    {
        return new UserProfile
        {
            Id = user.Id.ToString(),
            Email = user.Email ?? "",
            FullName = user.FullName ?? "",
            PhoneNumber = user.PhoneNumber ?? "",
            UserType = (int)user.UserType,
            Address = user.Address ?? "",
            IsEmailVerified = user.EmailConfirmed,
            CreatedAt = ((DateTimeOffset)user.CreatedAt).ToUnixTimeSeconds()
        };
    }
}