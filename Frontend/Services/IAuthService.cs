using Shared; // Feltételezve, hogy a LoginRequest a Shared projektben van

namespace Frontend.Services
{
    public interface IAuthService
    {
        Task<LoginResponse?> LoginAsync(LoginRequest request);
        Task LogoutAsync();
    }
}