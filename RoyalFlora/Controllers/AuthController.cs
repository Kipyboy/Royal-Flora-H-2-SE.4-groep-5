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

            if (await _context.Gebruikers.AnyAsync(u => u.Email.ToLower() == request.E_mail.ToLower()))
            {
                return BadRequest(new RegisterResponse
                {
                    Success = false,
                    Message = "Email adres is al in gebruik"
                });
            }

            
            int? kvkNummer = null;
            
            
            int rolId = request.AccountType == "Aanvoerder" ? 1 : 2;
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Wachtwoord);

            if (!string.IsNullOrWhiteSpace(request.BedrijfNaam))
            {
                // Validate KVK only when a company is provided
                if (string.IsNullOrWhiteSpace(request.KvkNummer) || request.KvkNummer.Length != 8 || !int.TryParse(request.KvkNummer, out int parsedKvk))
                {
                    return BadRequest(new RegisterResponse
                    {
                        Success = false,
                        Message = "KvK-nummer moet 8 cijfers bevatten"
                    });
                }

                kvkNummer = parsedKvk;

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

            // If no new company name was provided but the user supplied a KVK number,
            // try to resolve it to an existing `Bedrijf` and use that KVK for the gebruiker.
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
                new Claim("KVK", user.KVK ?? ""),
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

        [Authorize]
        [HttpDelete("deleteAccount")]
        public async Task<ActionResult> DeleteAccount()
        {
            // Get user ID from JWT claims
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

        [Authorize]
        [HttpGet("allUserInfo")]
        public async Task<ActionResult<Gebruiker>> GetUserInfo()
        {
            // Get user ID from JWT claims
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

        [HttpPost("updateUserInfo")]
        [Microsoft.AspNetCore.Authorization.Authorize]
        public async Task<ActionResult> UpdateUserInfo([FromBody] UpdateUserInfoRequest request)
        {
            // Get user ID from JWT claims
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

            // Safely get oprichter name (may be null if not set)
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
