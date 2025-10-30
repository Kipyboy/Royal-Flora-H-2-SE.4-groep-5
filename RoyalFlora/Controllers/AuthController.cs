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

    public class UserInfo
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }
}