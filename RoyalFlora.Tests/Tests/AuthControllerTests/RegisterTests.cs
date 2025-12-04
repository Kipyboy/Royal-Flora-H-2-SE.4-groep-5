using Microsoft.AspNetCore.Identity.Data;

namespace RoyalFlora.Tests.Tests.AuthControllerTests;

public class RegisterTests
{
    [Fact]
    public void Register_returnsBadRequest_WhenMissingReqField ()
    {
        var request = new AuthDTO.RegisterRequest
        {
            VoorNaam = "Test",
            E_mail = "test@gmail.nl",
            Wachtwoord = "test123",
        };
    }
    [Fact]
    public void Register_ReturnsBadRequest_WhenDuplicateEmail ()
    {
        
    }
    
}