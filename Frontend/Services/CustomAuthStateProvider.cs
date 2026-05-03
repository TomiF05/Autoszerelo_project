using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using System.Security.Claims;

namespace Frontend.Services
{
    public class CustomAuthStateProvider : AuthenticationStateProvider
    {
        private readonly IJSRuntime _js;

        public CustomAuthStateProvider(IJSRuntime js)
        {
            _js = js;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            try
            {
                // Kiolvassuk a szerepkört a böngészőből
                var role = await _js.InvokeAsync<string>("localStorage.getItem", "userRole");

                if (string.IsNullOrEmpty(role))
                {
                    // Ha nincs, akkor ismeretlen "vendég"
                    return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
                }

                // Ha van, megadjuk neki a jogosultságokat
                var claims = new List<Claim> { new Claim(ClaimTypes.Role, role) };
                var identity = new ClaimsIdentity(claims, "jwtAuth");
                var user = new ClaimsPrincipal(identity);

                return new AuthenticationState(user);
            }
            catch
            {
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }
        }

        public void NotifyAuthState()
        {
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }
    }
}