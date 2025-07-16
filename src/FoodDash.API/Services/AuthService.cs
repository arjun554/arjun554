using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AutoMapper;
using FoodDash.Database;
using FoodDash.Database.Entities;
using FoodDash.Shared.DTOs.Auth;
using FoodDash.Shared.DTOs.Common;
using FoodDash.Shared.Enums;

namespace FoodDash.API.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly IConfiguration _configuration;
    private readonly FoodDashDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        UserManager<User> userManager,
        SignInManager<User> signInManager,
        IConfiguration configuration,
        FoodDashDbContext context,
        IMapper mapper,
        ILogger<AuthService> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _configuration = configuration;
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<ApiResponse<LoginResponseDto>> LoginAsync(LoginRequestDto request)
    {
        try
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                return ApiResponse<LoginResponseDto>.ErrorResponse("Invalid email or password");
            }

            if (!user.IsActive)
            {
                return ApiResponse<LoginResponseDto>.ErrorResponse("Account is inactive");
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: false);
            if (!result.Succeeded)
            {
                return ApiResponse<LoginResponseDto>.ErrorResponse("Invalid email or password");
            }

            var token = await GenerateJwtTokenAsync(user.Id);
            var refreshToken = GenerateRefreshToken();

            // Store refresh token (in a real app, store this securely)
            user.SecurityStamp = refreshToken;
            await _userManager.UpdateAsync(user);

            var response = new LoginResponseDto
            {
                Token = token,
                RefreshToken = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddHours(24),
                User = _mapper.Map<UserDto>(user)
            };

            return ApiResponse<LoginResponseDto>.SuccessResponse(response, "Login successful");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for email: {Email}", request.Email);
            return ApiResponse<LoginResponseDto>.ErrorResponse("Login failed");
        }
    }

    public async Task<ApiResponse<LoginResponseDto>> RegisterAsync(RegisterRequestDto request)
    {
        try
        {
            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser != null)
            {
                return ApiResponse<LoginResponseDto>.ErrorResponse("Email already exists");
            }

            var user = new User
            {
                UserName = request.Email,
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                PhoneNumber = request.PhoneNumber,
                Role = request.Role,
                Address = request.Address,
                City = request.City,
                State = request.State,
                ZipCode = request.ZipCode,
                IsActive = true
            };

            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description).ToList();
                return ApiResponse<LoginResponseDto>.ErrorResponse("Registration failed", errors);
            }

            // Add user to role
            await _userManager.AddToRoleAsync(user, request.Role.ToString());

            // Create role-specific records
            await CreateRoleSpecificRecords(user);

            var token = await GenerateJwtTokenAsync(user.Id);
            var refreshToken = GenerateRefreshToken();

            user.SecurityStamp = refreshToken;
            await _userManager.UpdateAsync(user);

            var response = new LoginResponseDto
            {
                Token = token,
                RefreshToken = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddHours(24),
                User = _mapper.Map<UserDto>(user)
            };

            return ApiResponse<LoginResponseDto>.SuccessResponse(response, "Registration successful");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registration for email: {Email}", request.Email);
            return ApiResponse<LoginResponseDto>.ErrorResponse("Registration failed");
        }
    }

    public async Task<ApiResponse> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                return ApiResponse.ErrorResponse("User not found");
            }

            var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description).ToList();
                return ApiResponse.ErrorResponse("Password change failed", errors);
            }

            return ApiResponse.SuccessResponse("Password changed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error changing password for user: {UserId}", userId);
            return ApiResponse.ErrorResponse("Password change failed");
        }
    }

    public async Task<ApiResponse> ForgotPasswordAsync(string email)
    {
        try
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                // Don't reveal that the user doesn't exist
                return ApiResponse.SuccessResponse("If the email exists, a password reset link has been sent");
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            
            // TODO: Send email with reset token
            // For now, just log it (in production, send via email service)
            _logger.LogInformation("Password reset token for {Email}: {Token}", email, token);

            return ApiResponse.SuccessResponse("If the email exists, a password reset link has been sent");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in forgot password for email: {Email}", email);
            return ApiResponse.ErrorResponse("Password reset failed");
        }
    }

    public async Task<ApiResponse> ResetPasswordAsync(string email, string token, string newPassword)
    {
        try
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return ApiResponse.ErrorResponse("Invalid reset token");
            }

            var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description).ToList();
                return ApiResponse.ErrorResponse("Password reset failed", errors);
            }

            return ApiResponse.SuccessResponse("Password reset successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting password for email: {Email}", email);
            return ApiResponse.ErrorResponse("Password reset failed");
        }
    }

    public async Task<ApiResponse<LoginResponseDto>> RefreshTokenAsync(string refreshToken)
    {
        try
        {
            var user = _context.Users.FirstOrDefault(u => u.SecurityStamp == refreshToken);
            if (user == null)
            {
                return ApiResponse<LoginResponseDto>.ErrorResponse("Invalid refresh token");
            }

            var newToken = await GenerateJwtTokenAsync(user.Id);
            var newRefreshToken = GenerateRefreshToken();

            user.SecurityStamp = newRefreshToken;
            await _userManager.UpdateAsync(user);

            var response = new LoginResponseDto
            {
                Token = newToken,
                RefreshToken = newRefreshToken,
                ExpiresAt = DateTime.UtcNow.AddHours(24),
                User = _mapper.Map<UserDto>(user)
            };

            return ApiResponse<LoginResponseDto>.SuccessResponse(response, "Token refreshed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing token");
            return ApiResponse<LoginResponseDto>.ErrorResponse("Token refresh failed");
        }
    }

    public async Task<ApiResponse> LogoutAsync(int userId)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user != null)
            {
                user.SecurityStamp = Guid.NewGuid().ToString();
                await _userManager.UpdateAsync(user);
            }

            return ApiResponse.SuccessResponse("Logged out successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout for user: {UserId}", userId);
            return ApiResponse.ErrorResponse("Logout failed");
        }
    }

    public async Task<string> GenerateJwtTokenAsync(int userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null) throw new ArgumentException("User not found");

        var roles = await _userManager.GetRolesAsync(user);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.UserName ?? ""),
            new(ClaimTypes.Email, user.Email ?? ""),
            new("FirstName", user.FirstName),
            new("LastName", user.LastName),
            new("Role", user.Role.ToString())
        };

        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Key"] ?? "YourSuperSecretKeyThatIsAtLeast32CharactersLong123456789"));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration["JWT:Issuer"] ?? "FoodDashAPI",
            audience: _configuration["JWT:Audience"] ?? "FoodDashUsers",
            claims: claims,
            expires: DateTime.UtcNow.AddHours(24),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static string GenerateRefreshToken()
    {
        return Guid.NewGuid().ToString();
    }

    private async Task CreateRoleSpecificRecords(User user)
    {
        switch (user.Role)
        {
            case UserRole.Customer:
                var customer = new Customer
                {
                    UserId = user.Id,
                    LoyaltyPoints = 0,
                    TotalOrders = 0,
                    TotalSpent = 0
                };
                _context.Customers.Add(customer);
                break;

            case UserRole.DeliveryPartner:
                var deliveryPartner = new DeliveryPartner
                {
                    UserId = user.Id,
                    Status = DeliveryStatus.Available,
                    Rating = 0,
                    ReviewCount = 0,
                    TotalDeliveries = 0,
                    TotalEarnings = 0,
                    IsAvailable = true,
                    IsVerified = false
                };
                _context.DeliveryPartners.Add(deliveryPartner);
                break;
        }

        await _context.SaveChangesAsync();
    }
}