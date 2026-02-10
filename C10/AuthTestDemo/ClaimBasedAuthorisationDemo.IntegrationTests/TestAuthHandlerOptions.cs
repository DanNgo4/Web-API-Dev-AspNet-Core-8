using Microsoft.AspNetCore.Authentication;

namespace ClaimBasedAuthorisationDemo.IntegrationTests;
// You can customize the options if you want
public class TestAuthHandlerOptions : AuthenticationSchemeOptions
{
    public string UserName { get; set; } = string.Empty;
}
