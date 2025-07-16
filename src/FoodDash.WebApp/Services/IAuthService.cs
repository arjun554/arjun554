namespace FoodDash.WebApp.Services;

public interface IAuthService
{
    Task<bool> LoginAsync(LoginRequestDto loginRequest);
    Task<bool> RegisterAsync(RegisterRequestDto registerRequest);
    Task LogoutAsync();
    Task<UserDto?> GetCurrentUserAsync();
    Task<bool> IsAuthenticatedAsync();
}