using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using RoyalFlora.AuthDTO;

namespace RoyalFlora.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly MyDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(MyDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("register")]
        public async Task<ActionResult<RegisterResponse>> Register([FromBody] RegisterRequest request)
        {
            // Basic validation
            if (string.IsNullOrWhiteSpace(request.VoorNaam) || string.IsNullOrWhiteSpace(request.AchterNaam) ||
                string.IsNullOrWhiteSpace(request.E_mail) || string.IsNullOrWhiteSpace(request.Wachtwoord))
            {
                return BadRequest(new RegisterResponse
                {
                    Success = false,
                    Message = "Alle velden zijn verplicht"
                });
            }

            if (!int.TryParse(request.KvkNummer, out int kvkNummer) || request.KvkNummer.Length != 8)
            {
                return BadRequest(new RegisterResponse
                {
                    Success = false,
                    Message = "KvK-nummer moet 8 cijfers bevatten"
                });
            }

            // Check if email already exists
            if (await _context.Gebruikers.AnyAsync(u => u.Email.ToLower() == request.E_mail.ToLower()))
            {
                return BadRequest(new RegisterResponse
                {
                    Success = false,
                    Message = "Email adres is al in gebruik"
                });
            }

            // Role & hash password
            int rolId = request.AccountType == "bedrijf" ? 1 : 2;
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Wachtwoord);

            // Create company if needed
            var bedrijf = await _context.Bedrijven.FirstOrDefaultAsync(b => b.KVK == kvkNummer);
            if (request.AccountType == "bedrijf" && bedrijf != null)
            {
                return BadRequest(new RegisterResponse
                {
                    Success = false,
                    Message = "KvK-nummer is al in gebruik"
                });
            }

            if (bedrijf == null)
            {
                bedrijf = new Bedrijf
                {
                    KVK = kvkNummer,
                    BedrijfNaam = $"Bedrijf {request.VoorNaam} {request.AchterNaam}",
                    Oprichter = null
                };
                _context.Bedrijven.Add(bedrijf);
                await _context.SaveChangesAsync();
            }

            // Create user
            var newGebruiker = new Gebruiker
            {
                VoorNaam = request.VoorNaam,
                AchterNaam = request.AchterNaam,
                Email = request.E_mail,
                Wachtwoord = hashedPassword,
                Telefoonnummer = request.Telefoonnummer,
                Rol = rolId,
                KVK = kvkNummer
            };

            _context.Gebruikers.Add(newGebruiker);
            await _context.SaveChangesAsync();

            // Update company founder if needed
            if (request.AccountType == "bedrijf")
            {
                bedrijf.Oprichter = newGebruiker.IdGebruiker;
                await _context.SaveChangesAsync();
            }

            // Generate JWT
            var userInfo = new UserInfo
            {
                Id = newGebruiker.IdGebruiker,
                Username = $"{newGebruiker.VoorNaam} {newGebruiker.AchterNaam}",
                Email = newGebruiker.Email,
                Role = request.AccountType == "bedrijf" ? "Aanvoerder" : "Inkooper"
            };

            var token = GenerateJwtToken(userInfo);

            return Ok(new RegisterResponse
            {
                Success = true,
                Message = "Registratie succesvol",
                Token = token,
                User = userInfo
            });
        }

        [HttpPost("login")]
        public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new LoginResponse
                {
                    Success = false,
                    Message = "Email en wachtwoord zijn verplicht"
                });
            }

            var gebruiker = await _context.Gebruikers
                .Include(g => g.RolNavigation)
                .FirstOrDefaultAsync(g => g.Email.ToLower() == request.Email.ToLower());

            if (gebruiker == null)
            {
                return Unauthorized(new LoginResponse
                {
                    Success = false,
                    Message = "Ongeldige inloggegevens"
                });
            }

            // Verify password
            if (!BCrypt.Net.BCrypt.Verify(request.Password, gebruiker.Wachtwoord))
            {
                return Unauthorized(new LoginResponse
                {
                    Success = false,
                    Message = "Ongeldige inloggegevens"
                });
            }

            var userInfo = new UserInfo
            {
                Id = gebruiker.IdGebruiker,
                Username = $"{gebruiker.VoorNaam} {gebruiker.AchterNaam}",
                Email = gebruiker.Email,
                Role = gebruiker.RolNavigation?.RolNaam ?? "User"
            };

            var token = GenerateJwtToken(userInfo);

            return Ok(new LoginResponse
            {
                Success = true,
                Message = "Login succesvol",
                Token = token,
                User = userInfo
            });
        }

        private string GenerateJwtToken(UserInfo user)
        {
            var jwtSettings = _configuration.GetSection("Jwt");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(Convert.ToDouble(jwtSettings["ExpirationInMinutes"])),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
