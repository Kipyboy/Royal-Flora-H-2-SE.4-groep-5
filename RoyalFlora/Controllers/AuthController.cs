using Microsoft.AspNetCore.Mvc;

namespace RoyalFlora.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        [HttpPost("login")]
        public ActionResult<LoginResponse> Login([FromBody] LoginRequest request)
        {
            // Hardcoded test credentials
            if (request.Email == "test@test.com" && request.Password == "test123")
            {
                return Ok(new LoginResponse
                {
                    Success = true,
                    Message = "Login succesvol",
                    User = new UserInfo
                    {
                        Id = 1,
                        Username = "TestUser",
                        Email = "test@test.com",
                        Role = "User"
                    }
                });
            }

            return Unauthorized(new LoginResponse
            {
                Success = false,
                Message = "Ongeldige inloggegevens"
            });
        }

        [HttpPost("register")]
        public ActionResult<RegisterResponse> Register([FromBody] RegisterRequest request)
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

            // Email format validatie
            var emailRegex = new System.Text.RegularExpressions.Regex(@"^[^\s@]+@[^\s@]+\.[^\s@]+$");
            if (!emailRegex.IsMatch(request.E_mail))
            {
                return BadRequest(new RegisterResponse
                {
                    Success = false,
                    Message = "Ongeldig email adres"
                });
            }

            // Wachtwoord lengte check
            if (request.Wachtwoord.Length < 6)
            {
                return BadRequest(new RegisterResponse
                {
                    Success = false,
                    Message = "Wachtwoord moet minimaal 6 karakters zijn"
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

            // TODO: Database interactie - Check of email al bestaat
            // var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.E_mail);
            // if (existingUser != null)
            // {
            //     return BadRequest(new RegisterResponse
            //     {
            //         Success = false,
            //         Message = "Email adres is al in gebruik"
            //     });
            // }

            // TODO: Database interactie - Hash wachtwoord
            // var hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Wachtwoord);

            // TODO: Database interactie - Maak nieuwe user aan en sla op
            // var newUser = new User
            // {
            //     VoorNaam = request.VoorNaam,
            //     AchterNaam = request.AchterNaam,
            //     Telefoonnummer = request.Telefoonnummer,
            //     Email = request.E_mail,
            //     Password = hashedPassword,
            //     KvkNummer = request.KvkNummer,
            //     Role = request.AccountType == "bedrijf" ? "Aanvoerder" : "Inkooper",
            //     CreatedAt = DateTime.UtcNow
            // };
            // _context.Users.Add(newUser);
            // await _context.SaveChangesAsync();

            // Simuleer succesvolle registratie (tijdelijk hardcoded ID)
            var username = $"{request.VoorNaam} {request.AchterNaam}";
            var role = request.AccountType == "bedrijf" ? "Aanvoerder" : "Inkooper";
            
            var user = new UserInfo
            {
                Id = 2, // TODO: Gebruik newUser.Id uit database
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