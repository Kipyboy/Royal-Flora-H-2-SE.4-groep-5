namespace RoyalFlora.Model
{
    // Login DTOs
    public class LoginRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class LoginResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? Token { get; set; }
        public UserInfo? User { get; set; }
    }

    // Register DTOs
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
        public string? Token { get; set; }
        public UserInfo? User { get; set; }
    }

    // User Info DTO
    public class UserInfo
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }
}
