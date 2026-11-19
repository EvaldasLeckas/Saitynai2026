using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using YamlDotNet.Core.Tokens;
namespace Saitynai.Auth;


public class JwtTokenService
{
    private string? _audience;
    private string? _issuer;
    private SymmetricSecurityKey _authSigningKey;

    public JwtTokenService(IConfiguration configuration)
    {        
        _issuer = configuration["Jwt:ValidIssuer"];
        _audience = configuration["Jwt:ValidAudience"];
        _authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Secret"]));
    }

    public string CreateAccessToken(string email, string userId, IEnumerable<string> roles)
    {
        var authClaims = new List<Claim>
        {
            new(ClaimTypes.Name, email),
            new(ClaimTypes.Email, email),
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Sub, userId)

        };

        authClaims.AddRange(roles.Select(o => new Claim(ClaimTypes.Role, o )));
        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            expires: DateTime.UtcNow.AddMinutes(5),
            claims: authClaims,
            signingCredentials: new SigningCredentials(_authSigningKey, SecurityAlgorithms.HmacSha256)
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
    public string CreateRefreshToken(Guid sessionId, string userId, DateTime expires)
    {
        var authClaims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Sub, userId),
            new("SessionId", sessionId.ToString())

        };
        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            expires: expires,
            claims: authClaims,
            signingCredentials: new SigningCredentials(_authSigningKey, SecurityAlgorithms.HmacSha256)
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    // JWT tokeno validacija
    public bool TryParseRefreshToken(string refreshToken, out ClaimsPrincipal? claims)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler()
            {
                MapInboundClaims = false 
            };
            var validationParameters = new TokenValidationParameters()
            {
                ValidIssuer = _issuer,
                ValidAudience = _audience,
                IssuerSigningKey = _authSigningKey,
                ValidateLifetime = true
            };

            claims = tokenHandler.ValidateToken(refreshToken, validationParameters, out _);
            return true;
        }
        catch
        {
            claims = null;
            return false;
        }
    }
}