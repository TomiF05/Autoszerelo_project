using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using Shared;
using System.Net.Http.Json;

namespace Frontend.Services
{
    public class AuthService : IAuthService
    {
        private readonly HttpClient _httpClient;
        private readonly IJSRuntime _js;
        private readonly AuthenticationStateProvider _authStateProvider;

        public AuthService(HttpClient httpClient, IJSRuntime js, AuthenticationStateProvider authStateProvider)
        {
            _httpClient = httpClient;
            _js = js;
            _authStateProvider = authStateProvider;
        }

        public async Task<LoginResponse?> LoginAsync(LoginRequest request)
        {
            var response = await _httpClient.PostAsJsonAsync("api/auth/login", request);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
                if (result != null)
                {
                    await _js.InvokeVoidAsync("localStorage.setItem", "authToken", result.Token);
                    await _js.InvokeVoidAsync("localStorage.setItem", "userRole", result.Role);

                    ((CustomAuthStateProvider)_authStateProvider).NotifyAuthState();

                    return result;
                }
            }
            return null;
        }

        public async Task LogoutAsync()
        {
            await _js.InvokeVoidAsync("localStorage.removeItem", "authToken");
            await _js.InvokeVoidAsync("localStorage.removeItem", "userRole");

            ((CustomAuthStateProvider)_authStateProvider).NotifyAuthState();
        }
    }
}