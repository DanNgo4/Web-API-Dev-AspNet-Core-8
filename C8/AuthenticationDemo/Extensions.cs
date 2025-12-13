using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AuthenticationDemo;

internal static class Extensions
{
    public static string GenerateToken(this IConfiguration configuration, string userName)
    {
        var secret   = configuration["JwtConfig:Secret"];
        var issuer   = configuration["JwtConfig:ValidIssuer"];
        var audience = configuration["JwtConfig:ValidAudiences"];

        if (secret is null || issuer is null || audience is null)
        {
            throw new ApplicationException("JWT is not set in the configuraiton");
        }

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject            = new ClaimsIdentity([new Claim(ClaimTypes.Name, userName)]),
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
