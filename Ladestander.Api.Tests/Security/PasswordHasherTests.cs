using Ladestander.Api.Security;

namespace Ladestander.Api.Tests.Security;

public class PasswordHasherTests
{
    private readonly PasswordHasher _passwordHasher = new();

    [Fact]
    public void Verify_ReturnsTrue_WhenPasswordMatchesHash()
    {
        // Arrange
        const string password = "MySecurePassword123";

        var hash = _passwordHasher.Hash(password);

        // Act
        var result = _passwordHasher.Verify(password, hash);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Verify_ReturnsFalse_WhenPasswordDoesNotMatchHash()
    {
        // Arrange
        const string correctPassword = "MySecurePassword123";
        const string wrongPassword = "WrongPassword123";

        var hash = _passwordHasher.Hash(correctPassword);

        // Act
        var result = _passwordHasher.Verify(wrongPassword, hash);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Hash_ReturnsSameHash_WhenPasswordIsSame()
    {
        // Arrange
        const string password = "MySecurePassword123";

        // Act
        var firstHash = _passwordHasher.Hash(password);
        var secondHash = _passwordHasher.Hash(password);

        // Assert
        Assert.Equal(firstHash, secondHash);
    }
}