using System.Net.Http.Headers;
using System.Text.Json;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;

namespace FoodDash.WebApp.Services;

public class AuthService : IAuthService
{
    private readonly HttpClient _httpClient;
    private readonly ILocalStorageService _localStorage;
    private readonly AuthenticationStateProvider _authStateProvider;

    public AuthService(
        HttpClient httpClient,
        ILocalStorageService localStorage,
        AuthenticationStateProvider authStateProvider)
    {
        _httpClient = httpClient;
        _localStorage = localStorage;
        _authStateProvider = authStateProvider;
    }

    public async Task<bool> LoginAsync(LoginRequestDto loginRequest)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/auth/login", loginRequest);
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonSerializer.Deserialize<ApiResponse<LoginResponseDto>>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (apiResponse?.Success == true && apiResponse.Data != null)
                {
                    await _localStorage.SetItemAsync("authToken", apiResponse.Data.Token);
                    await _localStorage.SetItemAsync("refreshToken", apiResponse.Data.RefreshToken);
                    await _localStorage.SetItemAsync("user", apiResponse.Data.User);

                    _httpClient.DefaultRequestHeaders.Authorization = 
                        new AuthenticationHeaderValue("Bearer", apiResponse.Data.Token);

                    await _authStateProvider.GetAuthenticationStateAsync();
                    return true;
                }
            }

            return false;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> RegisterAsync(RegisterRequestDto registerRequest)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/auth/register", registerRequest);
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonSerializer.Deserialize<ApiResponse<LoginResponseDto>>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (apiResponse?.Success == true && apiResponse.Data != null)
                {
                    await _localStorage.SetItemAsync("authToken", apiResponse.Data.Token);
                    await _localStorage.SetItemAsync("refreshToken", apiResponse.Data.RefreshToken);
                    await _localStorage.SetItemAsync("user", apiResponse.Data.User);

                    _httpClient.DefaultRequestHeaders.Authorization = 
                        new AuthenticationHeaderValue("Bearer", apiResponse.Data.Token);

                    await _authStateProvider.GetAuthenticationStateAsync();
                    return true;
                }
            }

            return false;
        }
        catch
        {
            return false;
        }
    }

    public async Task LogoutAsync()
    {
        try
        {
            await _httpClient.PostAsync("api/auth/logout", null);
        }
        catch
        {
            // Continue with logout even if API call fails
        }

        await _localStorage.RemoveItemAsync("authToken");
        await _localStorage.RemoveItemAsync("refreshToken");
        await _localStorage.RemoveItemAsync("user");

        _httpClient.DefaultRequestHeaders.Authorization = null;
        await _authStateProvider.GetAuthenticationStateAsync();
    }

    public async Task<UserDto?> GetCurrentUserAsync()
    {
        try
        {
            return await _localStorage.GetItemAsync<UserDto>("user");
        }
        catch
        {
            return null;
        }
    }

    public async Task<bool> IsAuthenticatedAsync()
    {
        try
        {
            var token = await _localStorage.GetItemAsync<string>("authToken");
            return !string.IsNullOrEmpty(token);
        }
        catch
        {
            return false;
        }
    }
}