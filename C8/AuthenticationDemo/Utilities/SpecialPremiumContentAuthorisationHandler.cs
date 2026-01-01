using AuthenticationDemo.Models.Authentication;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace AuthenticationDemo.Utilities;

public class SpecialPremiumContentAuthorisationHandler
           : AuthorizationHandler<SpecialPremiumContentRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, SpecialPremiumContentRequirement requirement)
    {
        var hasPremiumSubscriptionClaim = context.User
                                                 .HasClaim(x => x.Type == AppClaimTypes.Subscription
                                                             && x.Value == "Premium");
        if (!hasPremiumSubscriptionClaim)
        {
            return Task.CompletedTask;
        }

        var countryClaim = context.User.FindFirst(x => x.Type == ClaimTypes.Country);
        if (countryClaim is null || string.IsNullOrWhiteSpace(countryClaim.ToString()))
        {
            return Task.CompletedTask;
        }

        if (countryClaim.Value == requirement.Country)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
