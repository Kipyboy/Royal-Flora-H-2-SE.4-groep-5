using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;

namespace RoyalFlora.Controllers
{
    [ApiController]
    [Route("api/[controller]")]

    public class AuthController : ControllerBase
    {
        private readonly MyDbContext _context;

        public AuthController(MyDbContext context)
        {
            _context = context;
        }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
    {
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

            // Sla user info op in session
            HttpContext.Session.SetInt32("UserId", user.Id);
            HttpContext.Session.SetString("Username", user.Username);
            HttpContext.Session.SetString("Email", user.Email);
            HttpContext.Session.SetString("Role", user.Role);

            return Ok(new LoginResponse
            {
                Success = true,
                Message = "Login succesvol",
                User = user
            });
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

            // Sla user info op in session (automatisch inloggen na registratie)
            HttpContext.Session.SetInt32("UserId", user.Id);
            HttpContext.Session.SetString("Username", user.Username);
            HttpContext.Session.SetString("Email", user.Email);
            HttpContext.Session.SetString("Role", user.Role);

            return Ok(new RegisterResponse
            {
                Success = true,
                Message = "Registratie succesvol",
                User = user
            });
        }

        [HttpPost("logout")]
        public ActionResult Logout()
        {
            HttpContext.Session.Clear();
            return Ok(new { message = "Succesvol uitgelogd" });
        }

        [HttpGet("session")]
        public ActionResult<UserInfo> GetSession()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            
            if (userId == null)
            {
                return Unauthorized(new { message = "Niet ingelogd" });
            }

            return Ok(new UserInfo
            {
                Id = userId.Value,
                Username = HttpContext.Session.GetString("Username") ?? "",
                Email = HttpContext.Session.GetString("Email") ?? "",
                Role = HttpContext.Session.GetString("Role") ?? ""
            });
        }
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
    }

    public class UserInfo
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }
}