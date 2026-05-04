using Shared; 
namespace Frontend.Services
{
    public interface IAuthService
    {
        Task<LoginResponse?> LoginAsync(LoginRequest request);
        Task LogoutAsync();
    }
}