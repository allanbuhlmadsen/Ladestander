using Ladestander.Api.Security;
using Microsoft.Extensions.Configuration;

namespace Ladestander.Api.Tests.Security;

public class JwtServiceTests
{
    private static JwtService CreateService()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["JwtSettings:Issuer"] = "TestIssuer",
                ["JwtSettings:Audience"] = "TestAudience",
                ["JwtSettings:SecretKey"] = "ThisIsATestSecretKeyThatIsLongEnough123456",
                ["JwtSettings:ExpirationMinutes"] = "60"
            })
            .Build();

        return new JwtService(configuration);
    }

    [Fact]
    public void GenerateToken_ReturnsTokenWithExpectedClaims()
    {
        // Arrange
        var service = CreateService();

        var adminUser = new Ladestander.Api.Entities.AdminUser
        {
            AdminUserId = 1,
            Username = "admin",
            Role = "Admin"
        };

        // Act
        var token = service.GenerateToken(adminUser);

        // Assert
        var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        Assert.Equal("TestIssuer", jwtToken.Issuer);
        Assert.Contains("TestAudience", jwtToken.Audiences);

        Assert.Contains(jwtToken.Claims, c =>
            c.Type == System.Security.Claims.ClaimTypes.NameIdentifier &&
            c.Value == "1");

        Assert.Contains(jwtToken.Claims, c =>
            c.Type == System.Security.Claims.ClaimTypes.Name &&
            c.Value == "admin");

        Assert.Contains(jwtToken.Claims, c =>
            c.Type == System.Security.Claims.ClaimTypes.Role &&
            c.Value == "Admin");
    }

    [Fact]
    public void GenerateToken_ThrowsInvalidOperationException_WhenSecretKeyIsMissing()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["JwtSettings:Issuer"] = "TestIssuer",
                ["JwtSettings:Audience"] = "TestAudience",
                ["JwtSettings:ExpirationMinutes"] = "60"
            })
            .Build();

        var service = new JwtService(configuration);

        var adminUser = new Ladestander.Api.Entities.AdminUser
        {
            AdminUserId = 1,
            Username = "admin",
            Role = "Admin"
        };

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
            service.GenerateToken(adminUser));

        Assert.Equal(
            "JwtSettings:SecretKey is missing.",
            exception.Message);
    }
}