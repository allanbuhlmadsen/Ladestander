using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Ladestander.Api.Entities;
using Ladestander.Api.Security.Interfaces;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;

namespace Ladestander.Api.Security;

public class JwtService : IJwtService
{
    private readonly IConfiguration _configuration;

    public JwtService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateToken(AdminUser adminUser)
    {
        var issuer = _configuration["JwtSettings:Issuer"]
     ?? throw new InvalidOperationException("JwtSettings:Issuer is missing.");

        var audience = _configuration["JwtSettings:Audience"]
            ?? throw new InvalidOperationException("JwtSettings:Audience is missing.");

        var secretKey = _configuration["JwtSettings:SecretKey"]
            ?? throw new InvalidOperationException("JwtSettings:SecretKey is missing.");

        var expirationMinutesValue = _configuration["JwtSettings:ExpirationMinutes"]
            ?? throw new InvalidOperationException("JwtSettings:ExpirationMinutes is missing.");

        var expirationMinutes = int.Parse(expirationMinutesValue);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, adminUser.AdminUserId.ToString()),
            new Claim(ClaimTypes.Name, adminUser.Username),
            new Claim(ClaimTypes.Role, adminUser.Role)
        };

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(secretKey!));

        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}