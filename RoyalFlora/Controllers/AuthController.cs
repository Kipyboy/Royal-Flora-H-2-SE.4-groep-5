using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;
using Google.Protobuf.WellKnownTypes;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using RoyalFlora.Model;

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

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
    {
        // Validate input
        if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
        {
            return BadRequest(new LoginResponse
            {
                Success = false,
                Message = "Email en wachtwoord zijn verplicht"
            });
        }

        var gebruiker = await _context.Gebruikers
            .Include(g => g.RolNavigation)
            .FirstOrDefaultAsync(g => g.Email == request.Email);

        if (gebruiker == null || !BCrypt.Net.BCrypt.Verify(request.Password, gebruiker.Wachtwoord))
        {
            return Unauthorized(new LoginResponse
            {
                Success = false,
                Message = "Ongeldige inloggegevens"
            });
        }

            // Login succesvol
            var user = new UserInfo
            {
                Id = gebruiker.IdGebruiker,
                Username = $"{gebruiker.VoorNaam} {gebruiker.AchterNaam}",
                Email = gebruiker.Email,
                Role = gebruiker.RolNavigation?.RolNaam ?? "User"
            };

            // Generate JWT token
            var token = GenerateJwtToken(user);

            return Ok(new LoginResponse
            {
                Success = true,
                Message = "Login succesvol",
                Token = token,
                User = user
            });
        }

        private string GenerateJwtToken(UserInfo user)
        {
            var jwtSettings = _configuration.GetSection("Jwt");
            var jwtKey = jwtSettings["Key"];
            
            if (string.IsNullOrEmpty(jwtKey))
            {
                throw new InvalidOperationException("JWT Key is not configured");
            }
            
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

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
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        
        private string hashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        [HttpPost("register")]
        public async Task<ActionResult<RegisterResponse>> Register([FromBody] RegisterRequest request)
        {
            // Basis validatie
            if (string.IsNullOrEmpty(request.VoorNaam) || string.IsNullOrEmpty(request.AchterNaam))
            {
                return BadRequest(new RegisterResponse
                {
                    Success = false,
                    Message = "Voornaam en achternaam zijn verplicht"
                });
            }

            if (string.IsNullOrEmpty(request.E_mail) || string.IsNullOrEmpty(request.Wachtwoord))
            {
                return BadRequest(new RegisterResponse
                {
                    Success = false,
                    Message = "Email en wachtwoord zijn verplicht"
                });
            }

            // KvK nummer validatie
            if (string.IsNullOrEmpty(request.KvkNummer) || request.KvkNummer.Length != 8)
            {
                return BadRequest(new RegisterResponse
                {
                    Success = false,
                    Message = "KvK-nummer moet 8 cijfers bevatten"
                });
            }

            // Check of email al bestaat
            var existingUser = await _context.Gebruikers.FirstOrDefaultAsync(u => u.Email == request.E_mail);
            if (existingUser != null)
            {
                return BadRequest(new RegisterResponse
                {
                    Success = false,
                    Message = "Email adres is al in gebruik"
                });
            }

            // Check of KVK al bestaat
            int kvkNummer = int.Parse(request.KvkNummer);
            var bedrijf = await _context.Bedrijven.FirstOrDefaultAsync(b => b.KVK == kvkNummer);
            
            // Voor bedrijf accounttype: KVK mag niet bestaan
            // Voor inkooper: KVK moet bestaan (tenzij we altijd een bedrijf aanmaken)
            if (request.AccountType == "bedrijf" && bedrijf != null)
            {
                return BadRequest(new RegisterResponse
                {
                    Success = false,
                    Message = "KvK-nummer is al in gebruik"
                });
            }
            
            // Bepaal rol ID (1 = Aanvoerder, 2 = Inkooper, aanpasbaar)
            int rolId = request.AccountType == "bedrijf" ? 1 : 2;

            // hash wachtwoord
            request.Wachtwoord = hashPassword(request.Wachtwoord);

            // Maak eerst bedrijf aan als het nog niet bestaat
            if (bedrijf == null)
            {
                var newBedrijf = new Bedrijf
                {
                    KVK = kvkNummer,
                    BedrijfNaam = $"Bedrijf {request.VoorNaam} {request.AchterNaam}",
                    Oprichter = null // Wordt later ingevuld
                };
                _context.Bedrijven.Add(newBedrijf);
                await _context.SaveChangesAsync();
            }

            // Maak nieuwe gebruiker aan
            var newGebruiker = new Gebruiker
            {
                VoorNaam = request.VoorNaam,
                AchterNaam = request.AchterNaam,
                Email = request.E_mail,
                Wachtwoord = request.Wachtwoord, 
                Telefoonnummer = request.Telefoonnummer,
                Rol = rolId,
                KVK = kvkNummer
            };

            _context.Gebruikers.Add(newGebruiker);
            await _context.SaveChangesAsync();

            // Update bedrijf oprichter als het een bedrijfsaccount is
            if (request.AccountType == "bedrijf")
            {
                var bedrijfToUpdate = await _context.Bedrijven.FirstAsync(b => b.KVK == kvkNummer);
                bedrijfToUpdate.Oprichter = newGebruiker.IdGebruiker;
                await _context.SaveChangesAsync();
            }

            // Simuleer succesvolle registratie
            var username = $"{request.VoorNaam} {request.AchterNaam}";
            var role = request.AccountType == "bedrijf" ? "Aanvoerder" : "Inkooper";
            
            var user = new UserInfo
            {
                Id = newGebruiker.IdGebruiker,
                Username = username,
                Email = request.E_mail,
                Role = role
            };

            // Generate JWT token (automatisch inloggen na registratie)
            var token = GenerateJwtToken(user);

            return Ok(new RegisterResponse
            {
                Success = true,
                Message = "Registratie succesvol",
                Token = token,
                User = user
            });
        }

        [HttpPost("logout")]
        public ActionResult Logout()
        {
            // With JWT, logout is handled client-side by removing the token
            // No server-side session to clear
            return Ok(new { message = "Succesvol uitgelogd. Verwijder het token client-side." });
        }

        [HttpGet("user")]
        [Microsoft.AspNetCore.Authorization.Authorize]
        public ActionResult<UserInfo> GetCurrentUser()
        {
            // Get user info from JWT claims
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            var username = User.FindFirst(ClaimTypes.Name)?.Value;
            var email = User.FindFirst(ClaimTypes.Email)?.Value ?? User.FindFirst(JwtRegisteredClaimNames.Email)?.Value;
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            
            if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out var id))
            {
                return Unauthorized(new { message = "Niet ingelogd" });
            }

            return Ok(new UserInfo
            {
                Id = id,
                Username = username ?? "",
                Email = email ?? "",
                Role = role ?? ""
            });
        }

        [HttpGet("allUserInfo")]
        public async Task<ActionResult<LoginResponse>> GetUserInfo()
        {

            var gebruiker = await _context.Gebruikers
                .Include(g => g.RolNavigation)
                .FirstOrDefaultAsync(g => g.Email == HttpContext.Session.GetString("Email"));

            var userId = HttpContext.Session.GetInt32("UserId");
            
            if (userId == null || gebruiker == null)
            {
                return Unauthorized(new { message = "Niet ingelogd" });
            }

            return Ok(new Gebruiker
            {
                VoorNaam = gebruiker.VoorNaam,
                AchterNaam = gebruiker.AchterNaam,
                Email = gebruiker.Email,
                Wachtwoord = gebruiker.Wachtwoord, 
                Telefoonnummer = gebruiker.Telefoonnummer,
                Adress = gebruiker.Adress,
                Postcode = gebruiker.Postcode

            });
        }

        [HttpPost("updateUserInfo")]
        public async Task<ActionResult> UpdateUserInfo([FromBody] UpdateUserInfoRequest request)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            
            if (userId == null)
            {
                return Unauthorized(new { message = "Niet ingelogd" });
            }

            var gebruiker = await _context.Gebruikers.FindAsync(userId.Value);
            
            if (gebruiker == null)
            {
                return NotFound(new { message = "Gebruiker niet gevonden" });
            }

            // Update alleen het specifieke veld
            switch (request.Field.ToLower())
            {
                case "voornaam":
                    gebruiker.VoorNaam = request.NewValue;
                    break;
                case "achternaam":
                    gebruiker.AchterNaam = request.NewValue;
                    break;
                case "email":
                    gebruiker.Email = request.NewValue;
                    break;
                case "telefoonnummer":
                    gebruiker.Telefoonnummer = request.NewValue;
                    break;
                case "adress":
                    gebruiker.Adress = request.NewValue;
                    break;
                case "postcode":
                    gebruiker.Postcode = request.NewValue;
                    break;
                case "wachtwoord":
                    gebruiker.Wachtwoord = hashPassword(request.NewValue);
                    break;
                default:
                    return BadRequest(new { message = $"Ongeldig veld: {request.Field}" });
            }

            try
            {
                await _context.SaveChangesAsync();
                
                // Update session
                if (request.Field.ToLower() == "voornaam" || request.Field.ToLower() == "achternaam")
                {
                    HttpContext.Session.SetString("Username", $"{gebruiker.VoorNaam} {gebruiker.AchterNaam}");
                } else if (request.Field.ToLower() == "email")
                {
                    HttpContext.Session.SetString("Email", gebruiker.Email);
                }

                return Ok(new { 
                    message = "Veld succesvol bijgewerkt",
                    field = request.Field,
                    newValue = request.Field.ToLower() == "wachtwoord" ? "***" : request.NewValue
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Fout bij het opslaan", error = ex.Message });
            }
        }
    }

    public class UpdateUserInfoRequest
    {
        public string Field { get; set; } = string.Empty;
        public string NewValue { get; set; } = string.Empty;
    }

    public class LoginRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class LoginResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public UserInfo? User { get; set; }
    }

    public class RegisterRequest
    {
        public string VoorNaam { get; set; } = string.Empty;
        public string AchterNaam { get; set; } = string.Empty;
        public string Telefoonnummer { get; set; } = string.Empty;
        public string E_mail { get; set; } = string.Empty;
        public string Wachtwoord { get; set; } = string.Empty;
        public string KvkNummer { get; set; } = string.Empty;
        public string AccountType { get; set; } = string.Empty;
    }

    public class RegisterResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public UserInfo? User { get; set; }

        public string Token { get; set; } = string.Empty;
    }

    public class UserInfo
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }
}