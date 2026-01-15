using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using RoyalFlora.AuthDTO;
using Microsoft.AspNetCore.Authorization;
using System.Security.Cryptography.X509Certificates;

namespace RoyalFlora.Controllers
{
    // Controller voor authenticatie en gebruikersbeheer:
    // - Registratie en login (JWT-token generatie)
    // - Gebruikersprofiel en bedrijf-informatie ophalen/bijwerken
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly MyDbContext _context;
        private readonly IConfiguration _configuration;

        // Dependency injection: DbContext voor database toegang en IConfiguration voor app-instellingen (bv. JWT-sleutel)
        public AuthController(MyDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }


        // POST api/auth/register
        // Registreert een nieuwe gebruiker. Valideert invoer, maakt optioneel een nieuw bedrijf aan
        // en retourneert een JWT-token en gebruikersinfo bij succesvolle registratie.
        [HttpPost("register")]
        public async Task<ActionResult<RegisterResponse>> Register([FromBody] RegisterRequest request)
        {
            // Basisvalidatie: controleer of verplichte velden zijn ingevuld
            if (string.IsNullOrWhiteSpace(request.VoorNaam) ||
                string.IsNullOrWhiteSpace(request.AchterNaam) ||
                string.IsNullOrWhiteSpace(request.E_mail) ||
                string.IsNullOrWhiteSpace(request.Wachtwoord))
            {
                return BadRequest(new RegisterResponse
                {
                    Success = false,
                    Message = "Alle velden zijn verplicht"
                });
            }

            // Controleer of het e-mailadres al in gebruik is (case-insensitief)
            if (await _context.Gebruikers.AnyAsync(u => u.Email.ToLower() == request.E_mail.ToLower()))
            {
                return BadRequest(new RegisterResponse
                {
                    Success = false,
                    Message = "Email adres is al in gebruik"
                });
            }

            
            // Optioneel KvK-nummer (alleen wanneer gebruiker een bedrijf opgeeft)
            int? kvkNummer = null;
            
            // Bepaal de rol id op basis van het geselecteerde accounttype
            int rolId;
            if (request.AccountType == "Inkoper")
            {
                rolId = 1;
            }
            else if (request.AccountType == "Aanvoerder")
            {
                rolId = 2;
            }
            else if (request.AccountType == "Veilingmeester")
            {
                rolId = 3;
            }
            else
            {
                return BadRequest(new RegisterResponse
                {
                    Success = false,
                    Message = "Ongeldig account type"
                });
            }
            // Hash het wachtwoord met BCrypt voordat het in de database wordt opgeslagen
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Wachtwoord);

            // Als de gebruiker een bedrijfsnaam opgeeft, valideer en verwerk KvK-nummer
            if (!string.IsNullOrWhiteSpace(request.BedrijfNaam))
            {
                // Valideer KvK-nummer: moet bestaan uit 8 cijfers
                if (string.IsNullOrWhiteSpace(request.KvkNummer) || request.KvkNummer.Length != 8 || !int.TryParse(request.KvkNummer, out int parsedKvk))
                {
                    return BadRequest(new RegisterResponse
                    {
                        Success = false,
                        Message = "KvK-nummer moet 8 cijfers bevatten"
                    });
                }

                kvkNummer = parsedKvk;

                // Controleer of het bedrijf al bestaat, maak anders een nieuwe aan
                var existing = await _context.Bedrijven.FindAsync(kvkNummer.Value);
                if (existing == null)
                {
                    var bedrijf = new Bedrijf
                    {
                        KVK = kvkNummer.Value,
                        BedrijfNaam = request.BedrijfNaam,
                        Adress = request.BedrijfAdres,
                        Postcode = request.BedrijfPostcode,
                        Oprichter = null
                    };
                    _context.Bedrijven.Add(bedrijf);
                    await _context.SaveChangesAsync();
                }
            }

            // Als gebruiker enkel KvK invult zonder bedrijfsnaam, koppel aan bestaand bedrijf indien gevonden
            if (string.IsNullOrWhiteSpace(request.BedrijfNaam) && !string.IsNullOrWhiteSpace(request.KvkNummer))
            {
                if (request.KvkNummer.Length == 8 && int.TryParse(request.KvkNummer, out int parsedKvkExisting))
                {
                    var existingCompany = await _context.Bedrijven.FindAsync(parsedKvkExisting);
                    if (existingCompany != null)
                    {
                        kvkNummer = parsedKvkExisting;
                    }
                }
            }

            // Maak en sla de nieuwe gebruiker op in de database
            var newGebruiker = new Gebruiker
            {
                VoorNaam = request.VoorNaam,
                AchterNaam = request.AchterNaam,
                Email = request.E_mail,
                Wachtwoord = hashedPassword,
                Telefoonnummer = request.Telefoonnummer,
                Postcode = request.Postcode,
                Adress = request.Adres,
                Rol = rolId,
                KVK = kvkNummer
            };

            _context.Gebruikers.Add(newGebruiker);
            await _context.SaveChangesAsync();

            
            // Als een nieuw bedrijf is aangemaakt, stel de gebruiker in als oprichter
            if (!string.IsNullOrWhiteSpace(request.BedrijfNaam))
            {
                var bedrijf = await _context.Bedrijven.FindAsync(kvkNummer);
                if (bedrijf != null)
                {
                    bedrijf.Oprichter = newGebruiker.IdGebruiker;
                    _context.Bedrijven.Update(bedrijf);
                    await _context.SaveChangesAsync();
                }
            }

            // Prepareer UserInfo voor token en response
            var userInfo = new UserInfo
            {
                Id = newGebruiker.IdGebruiker,
                Username = $"{newGebruiker.VoorNaam} {newGebruiker.AchterNaam}",
                Email = newGebruiker.Email,
                Role = request.AccountType,
                KVK = newGebruiker.KVK?.ToString() ?? "" // ✅ include KVK
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

        // POST api/auth/login
        // Authenticeert gebruiker op basis van e-mail en wachtwoord en retourneert JWT-token
        [HttpPost("login")]
        public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
        {
            // Basisvalidatie: e-mail en wachtwoord verplicht
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

            // Controleer of gebruiker bestaat en verifieer wachtwoord met BCrypt
            if (gebruiker == null || !BCrypt.Net.BCrypt.Verify(request.Password, gebruiker.Wachtwoord))
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
                Role = gebruiker.RolNavigation?.RolNaam ?? "User",
                KVK = gebruiker.KVK?.ToString() ?? "" // ✅ include KVK
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

        // GET api/auth/kvk-exists/{kvk}
        // Controleer of een KvK-nummer voorkomt in de bedrijven tabel
        [HttpGet("kvk-exists/{kvk}")]
        public async Task<ActionResult<bool>> KvkExists(string kvk)
        {
            if (string.IsNullOrWhiteSpace(kvk) || kvk.Length != 8 || !int.TryParse(kvk, out int kvkNum))
            {
                return BadRequest(false);
            }

            bool existsInBedrijf = await _context.Bedrijven.AnyAsync(b => b.KVK == kvkNum);

            return Ok(existsInBedrijf);
        }

        // Genereer een JWT-token met de belangrijkste gebruikersclaims (id, email, naam, rol, KVK)
        // Token gebruikt instellingen uit appsettings (Key, Issuer, Audience, ExpirationInMinutes)
        private string GenerateJwtToken(UserInfo user)
        {
            var jwtSettings = _configuration.GetSection("Jwt");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Voeg claims toe die in het token aanwezig moeten zijn (gebruikers-id, e-mail, naam, rol, KVK)
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim("KVK", user.KVK ?? ""),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            // Maak en teken JWT met instellingen uit de configuratie
            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(Convert.ToDouble(jwtSettings["ExpirationInMinutes"])),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        [Authorize]
        [HttpDelete("deleteAccount")]
        public async Task<ActionResult> DeleteAccount()
        {
            // Haal gebruiker-id uit JWT-claims en controleer authenticatie
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(new { message = "Ongeldige gebruiker" });
            }

            var gebruiker = await _context.Gebruikers.FindAsync(userId);
            
            if (gebruiker == null)
            {
                return NotFound(new { message = "Gebruiker niet gevonden" });
            }

            // Verwijder gebruiker uit database
            _context.Gebruikers.Remove(gebruiker);

            try
            {
                await _context.SaveChangesAsync();
                return Ok(new { message = "Account succesvol verwijderd" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Fout bij het opslaan", error = ex.Message });
            }
        }

        // GET api/auth/allUserInfo
        // Retourneer gebruikersinformatie voor de momenteel ingelogde gebruiker
        [Authorize]
        [HttpGet("allUserInfo")]
        public async Task<ActionResult<Gebruiker>> GetUserInfo()
        {
            // Haal user-id uit JWT-claims
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new { message = "Niet ingelogd" });
            }

            var gebruiker = await _context.Gebruikers
                .Include(g => g.RolNavigation)
                .FirstOrDefaultAsync(g => g.IdGebruiker == userId);

            if (gebruiker == null)
            {
                return NotFound(new { message = "Gebruiker niet gevonden" });
            }

            // Return alleen relevante velden (niet het wachtwoord)
            return Ok(new Gebruiker
            {
                VoorNaam = gebruiker.VoorNaam,
                AchterNaam = gebruiker.AchterNaam,
                Email = gebruiker.Email,
                Telefoonnummer = gebruiker.Telefoonnummer,
                Adress = gebruiker.Adress,
                Postcode = gebruiker.Postcode
            });
        }

        // POST api/auth/updateUserInfo
        // Update een specifiek veld van de ingelogde gebruiker. Wachtwoord wordt gehasht.
        [HttpPost("updateUserInfo")]
        [Microsoft.AspNetCore.Authorization.Authorize]
        public async Task<ActionResult> UpdateUserInfo([FromBody] UpdateUserInfoRequest request)
        {
            // Haal user-id uit JWT-claims
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))

            {
                return Unauthorized(new { message = "Niet ingelogd" });
            }

            var gebruiker = await _context.Gebruikers.FindAsync(userId);

            if (gebruiker == null)
            {
                return NotFound(new { message = "Gebruiker niet gevonden" });
            }

            // Update alleen het specifieke veld en hash het wachtwoord wanneer nodig
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
                    gebruiker.Wachtwoord = BCrypt.Net.BCrypt.HashPassword(request.NewValue);
                    break;

                default:
                    return BadRequest(new { message = $"Ongeldig veld: {request.Field}" });
            }


            try
            {
                await _context.SaveChangesAsync();


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
            [Authorize]
            [HttpGet("GetBedrijfInfo")]
            public async Task<ActionResult<GetBedrijfInfoResponse>> GetBedrijfInfo ()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new { message = "Niet ingelogd" });
            }

            var gebruiker = await _context.Gebruikers
                .Include(g => g.BedrijfNavigation)
                    .ThenInclude(b => b.OprichterNavigation)
                .FirstOrDefaultAsync(g => g.IdGebruiker == userId);

            if (gebruiker == null)
            {
                return NotFound(new { message = "Gebruiker niet gevonden" });
            }

            var bedrijf = gebruiker.BedrijfNavigation;

            if (bedrijf == null)
            {
                return NotFound(new { message = "Gebonden bedrijf niet gevonden" });
            }

            // Oprichter kan null zijn; haal veilig de voornaam op
            var oprichterNaam = bedrijf.OprichterNavigation?.VoorNaam ?? string.Empty;

            var response = new GetBedrijfInfoResponse
            {
                BedrijfNaam = bedrijf.BedrijfNaam ?? string.Empty,
                Postcode = bedrijf.Postcode ?? string.Empty,
                Adres = bedrijf.Adress ?? string.Empty,
                Oprichter = oprichterNaam,
                IsOprichter = !string.IsNullOrEmpty(oprichterNaam) && oprichterNaam.Equals(gebruiker.VoorNaam)
            };

            return Ok(response);
        }
        // POST api/auth/UpdateBedrijfInfo
        // Werk een bepaald veld van het gekoppelde bedrijf bij (bedrijfnaam, postcode, adres)
        [HttpPost("UpdateBedrijfInfo")]
        public async Task<ActionResult> UpdateBedrijfInfo ([FromBody] UpdateBedrijfInfoRequest request)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new { message = "Niet ingelogd" });
            }

            var gebruiker = await _context.Gebruikers
                .Include(g => g.BedrijfNavigation)
                .FirstOrDefaultAsync(g => g.IdGebruiker == userId);

            if (gebruiker == null)
            {
                return NotFound(new { message = "Gebruiker niet gevonden" });
            }

            Bedrijf bedrijf = gebruiker.BedrijfNavigation;

            switch(request.Field.ToLower())
            {
                case "bedrijfnaam":
                bedrijf.BedrijfNaam = request.NewValue;
                break;

                case "postcode":
                bedrijf.Postcode = request.NewValue;
                break;

                case "adress":
                bedrijf.Adress = request.NewValue;
                break;

                default:
                    return BadRequest(new { message = $"Ongeldig veld: {request.Field}" });
            }
            try
            {
                await _context.SaveChangesAsync();


                return Ok(new { 
                    message = "Veld succesvol bijgewerkt",
                    field = request.Field,
                    newValue = request.NewValue
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Fout bij het opslaan", error = ex.Message });
            }
        }        
    }
    }
