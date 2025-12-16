namespace RoyalFlora.AuthDTO
{
    public class GetBedrijfInfoResponse
    {
        public string BedrijfNaam {get; set;} = string.Empty;
        public string Postcode {get;set;} = string.Empty;
        public string Adres {get;set;} = string.Empty;
        public string Oprichter {get;set;} = string.Empty;
        public bool IsOprichter{get;set;}
    }
    public class UpdateBedrijfInfoRequest
    {
        public string Field {get;set;} = string.Empty;
        public string NewValue {get;set;} = string.Empty;
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
        public string? Postcode { get; set; }
        public string? Adres { get; set; }
        public string? BedrijfNaam { get; set; }
        public string? BedrijfPostcode {get; set; }
        public string? BedrijfAdres { get; set; }
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
        public string? KVK { get; set; } = string.Empty;
    }
}