using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Shared;
using Autoszerelo.Data; // Ezt írd át a te DbContext-ed mappájára, ha máshol van!

namespace Autoszerelo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;

        // Injektáljuk az adatbázist és a konfigurációt (hogy elérjük a titkos kulcsot)
        public AuthController(AppDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        [HttpPost("login")]
        public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
        {
            if (request.Email == "admin@admin.hu" && request.Password == "admin")
            {
                var adminTokenHandler = new JwtSecurityTokenHandler();
                var adminKey = Encoding.ASCII.GetBytes(_config["JwtSettings:SecretKey"]!);
                var adminDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new[]
                    {
                new Claim(ClaimTypes.NameIdentifier, "999"),
                new Claim(ClaimTypes.Email, "admin@admin.hu"),
                new Claim(ClaimTypes.Role, "Admin"), // Ő admin lesz!
                new Claim(ClaimTypes.Name, "Főnök Úr")
            }),
                    Expires = DateTime.UtcNow.AddDays(1),
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(adminKey), SecurityAlgorithms.HmacSha256Signature)
                };
                var aToken = adminTokenHandler.CreateToken(adminDescriptor);

                return Ok(new LoginResponse { Token = adminTokenHandler.WriteToken(aToken), Role = "Admin", Name = "Főnök Úr" });
            }
            // 1. Megkeressük az usert email ÉS jelszó alapján
            //var user = await _context.Customers
            //.FirstOrDefaultAsync(c => c.Email == request.Email && c.Password == request.Password);

            var user = await _context.Customers.FirstOrDefaultAsync(c => c.Email == request.Email);

            if (user == null || user.Password != request.Password)
            {
                return Unauthorized("Nincs ilyen e-mail!");
            }

            // Ha nincs ilyen, vagy rossz a jelszó
            if (user == null)
            {
                return Unauthorized("Hibás email vagy jelszó!");
            }

            // 2. JWT Token generálása (A "belépőkártya" elkészítése)
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_config["JwtSettings:SecretKey"]!);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, user.Role), // ITT MONDJUK MEG A BLAZORNAK, HOGY ADMIN VAGY USER!
                    new Claim(ClaimTypes.Name, user.Name)
                }),
                Expires = DateTime.UtcNow.AddDays(1), // 1 napig érvényes
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);

            // 3. Válasz visszaküldése a Frontendnek
            return Ok(new LoginResponse
            {
                Token = tokenHandler.WriteToken(token),
                Role = user.Role,
                Name = user.Name
            });
        }
    }
}