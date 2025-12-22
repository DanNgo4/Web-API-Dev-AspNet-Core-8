using AuthenticationDemo.Models.User;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

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

        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, user.UserName)
        };
        claims.AddRange(userRoles.Select(x => new Claim(ClaimTypes.Role, x)));

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
