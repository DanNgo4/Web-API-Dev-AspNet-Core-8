using AuthenticationDemo.Models.Authentication;
using AuthenticationDemo.Models.User;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AuthenticationDemo;

internal static class Extensions
{
    public static async Task<string> GenerateToken(this IConfiguration configuration, 
                                            UserManager<AppUser> userManager, 
                                            AppUser user)
    {
        if (user.UserName is null)
        {
            throw new ArgumentNullException(nameof(user.UserName));
        }

        var secret   = configuration["JwtConfig:Secret"];
        var issuer   = configuration["JwtConfig:ValidIssuer"];
        var audience = configuration["JwtConfig:ValidAudiences"];

        if (secret is null || issuer is null || audience is null)
        {
            throw new ApplicationException("JWT is not set in the configuraiton");
        }

        var userRoles = await userManager.GetRolesAsync(user);

        // Role-based authorisation
        /*var claims = new List<Claim>
        {
            new(ClaimTypes.Name, user.UserName)
        };
        claims.AddRange(userRoles.Select(x => new Claim(ClaimTypes.Role, x)));*/

        // Claim-based authorisation
        var claims = new[]
        {
            new Claim(ClaimTypes.Name, user.UserName),

            // suppose the user's info is stored in the DB so that we can retrieve it from the DB
            new Claim(ClaimTypes.Country, "New Zealand"),   

            // custom claims
            new Claim(AppClaimTypes.AccessNumber, "12345678"),
            new Claim(AppClaimTypes.DrivingLicenseNumber, "123456789")
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject            = new ClaimsIdentity(claims),
            Expires            = DateTime.UtcNow.AddDays(1),
            Issuer             = issuer,
            Audience           = audience,
            SigningCredentials = new SigningCredentials
            (
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)), 
                SecurityAlgorithms.HmacSha256Signature
            )
        };

        var tokenHandler = new JwtSecurityTokenHandler();

        var securityToken = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(securityToken);
    }
}
