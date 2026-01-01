using Microsoft.AspNetCore.Authorization;

namespace AuthenticationDemo.Models.Authentication;

public class SpecialPremiumContentRequirement
           : IAuthorizationRequirement
{
    public string Country { get; }

    public SpecialPremiumContentRequirement(string country)
    {
        Country = country;
    }
}
