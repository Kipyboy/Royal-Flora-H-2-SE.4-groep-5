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

        // ------------------ LOGIN ------------------
        [HttpPost("login")]
        public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
        {
            try
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

                var user = new UserInfo
                {
                    Id = gebruiker.IdGebruiker,
                    Username = $"{gebruiker.VoorNaam} {gebruiker.AchterNaam}",
                    Email = gebruiker.Email,
                    Role = gebruiker.RolNavigation?.RolNaam ?? "User"
                };

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
            catch (Exception ex)
            {
                return StatusCode(500, new LoginResponse
                {
                    Success = false,
                    Message = "Er is een fout opgetreden tijdens login: " + ex.Message
                });
            }
        }

        // ------------------ REGISTER ------------------
        private string HashPassword(string password) => BCrypt.Net.BCrypt.HashPassword(password);

        [HttpPost("register")]
        public async Task<ActionResult<RegisterResponse>> Register([FromBody] RegisterRequest request)
        {
            try
            {
                // Basic validation
                if (string.IsNullOrEmpty(request.VoorNaam) || string.IsNullOrEmpty(request.AchterNaam))
                    return BadRequest(new RegisterResponse { Success = false, Message = "Voornaam en achternaam zijn verplicht" });

                if (string.IsNullOrEmpty(request.E_mail) || string.IsNullOrEmpty(request.Wachtwoord))
                    return BadRequest(new RegisterResponse { Success = false, Message = "Email en wachtwoord zijn verplicht" });

                if (string.IsNullOrEmpty(request.KvkNummer) || request.KvkNummer.Length != 8)
                    return BadRequest(new RegisterResponse { Success = false, Message = "KvK-nummer moet 8 cijfers bevatten" });

                var existingUser = await _context.Gebruikers.FirstOrDefaultAsync(u => u.Email == request.E_mail);
                if (existingUser != null)
                    return BadRequest(new RegisterResponse { Success = false, Message = "Email adres is al in gebruik" });

                int kvkNummer = int.Parse(request.KvkNummer);
                var bedrijf = await _context.Bedrijven.FirstOrDefaultAsync(b => b.KVK == kvkNummer);

                if (request.AccountType == "bedrijf" && bedrijf != null)
                    return BadRequest(new RegisterResponse { Success = false, Message = "KvK-nummer is al in gebruik" });

                int rolId = request.AccountType == "bedrijf" ? 1 : 2;
                request.Wachtwoord = HashPassword(request.Wachtwoord);

                if (bedrijf == null && request.AccountType == "bedrijf")
                {
                    var newBedrijf = new Bedrijf
                    {
                        KVK = kvkNummer,
                        BedrijfNaam = $"Bedrijf {request.VoorNaam} {request.AchterNaam}",
                        Oprichter = null
                    };
                    _context.Bedrijven.Add(newBedrijf);
                    await _context.SaveChangesAsync();
                }

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

                if (request.AccountType == "bedrijf")
                {
                    var bedrijfToUpdate = await _context.Bedrijven.FirstAsync(b => b.KVK == kvkNummer);
                    bedrijfToUpdate.Oprichter = newGebruiker.IdGebruiker;
                    await _context.SaveChangesAsync();
                }

                var user = new UserInfo
                {
                    Id = newGebruiker.IdGebruiker,
                    Username = $"{request.VoorNaam} {request.AchterNaam}",
                    Email = request.E_mail,
                    Role = request.AccountType == "bedrijf" ? "Aanvoerder" : "Inkooper"
                };

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
            catch (Exception ex)
            {
                return StatusCode(500, new RegisterResponse
                {
                    Success = false,
                    Message = "Er is een fout opgetreden tijdens registratie: " + ex.Message
                });
            }
        }

        // ------------------ LOGOUT ------------------
        [HttpPost("logout")]
        public ActionResult Logout()
        {
            HttpContext.Session.Clear();
            return Ok(new { Success = true, Message = "Succesvol uitgelogd" });
        }

        // ------------------ GET SESSION ------------------
        [HttpGet("session")]
        public ActionResult<UserInfo> GetSession()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return Unauthorized(new { Success = false, Message = "Niet ingelogd" });

            return Ok(new UserInfo
            {
                Id = userId.Value,
                Username = HttpContext.Session.GetString("Username") ?? "",
                Email = HttpContext.Session.GetString("Email") ?? "",
                Role = HttpContext.Session.GetString("Role") ?? ""
            });
        }

        // ------------------ GET ACCOUNT ------------------
        [HttpGet("account")]
        public ActionResult<UserInfo> GetAccount()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return Unauthorized(new { Success = false, Message = "Niet ingelogd" });

            return Ok(new UserInfo
            {
                Id = userId.Value,
                Username = HttpContext.Session.GetString("Username") ?? "",
                Email = HttpContext.Session.GetString("Email") ?? "",
                Role = HttpContext.Session.GetString("Role") ?? ""
            });
        }
    }

    // ------------------ MODELS ------------------
    public class LoginRequest { public string Email { get; set; } = ""; public string Password { get; set; } = ""; }
    public class LoginResponse { public bool Success { get; set; } public string Message { get; set; } = ""; public UserInfo? User { get; set; } }
    public class RegisterRequest
    {
        public string VoorNaam { get; set; } = "";
        public string AchterNaam { get; set; } = "";
        public string Telefoonnummer { get; set; } = "";
        public string E_mail { get; set; } = "";
        public string Wachtwoord { get; set; } = "";
        public string KvkNummer { get; set; } = "";
        public string AccountType { get; set; } = "";
    }
    public class RegisterResponse { public bool Success { get; set; } public string Message { get; set; } = ""; public UserInfo? User { get; set; } }
    public class UserInfo { public int Id { get; set; } public string Username { get; set; } = ""; public string Email { get; set; } = ""; public string Role { get; set; } = ""; }
}
