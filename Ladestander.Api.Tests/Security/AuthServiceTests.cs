using Ladestander.Api.Repositories.Interfaces;
using Ladestander.Api.Security;
using Ladestander.Api.Security.Interfaces;
using Moq;

namespace Ladestander.Api.Tests.Security;

public class AuthServiceTests
{
    private readonly Mock<IAdminUserRepository> _adminUserRepositoryMock = new();
    private readonly Mock<IPasswordHasher> _passwordHasherMock = new();
    private readonly Mock<IJwtService> _jwtServiceMock = new();

    private AuthService CreateService()
    {
        return new AuthService(
            _adminUserRepositoryMock.Object,
            _passwordHasherMock.Object,
            _jwtServiceMock.Object);
    }

    [Fact]
    public async Task LoginAsync_ReturnsNull_WhenAdminUserDoesNotExist()
    {
        // Arrange
        _adminUserRepositoryMock
            .Setup(r => r.GetByUsernameAsync("admin"))
            .ReturnsAsync((Ladestander.Api.Entities.AdminUser?)null);

        var service = CreateService();

        // Act
        var result = await service.LoginAsync("admin", "password");

        // Assert
        Assert.Null(result);

        _passwordHasherMock.Verify(
            h => h.Verify(It.IsAny<string>(), It.IsAny<string>()),
            Times.Never);

        _jwtServiceMock.Verify(
            j => j.GenerateToken(It.IsAny<Ladestander.Api.Entities.AdminUser>()),
            Times.Never);
    }

    [Fact]
    public async Task LoginAsync_ReturnsNull_WhenPasswordIsInvalid()
    {
        // Arrange
        var adminUser = new Ladestander.Api.Entities.AdminUser
        {
            AdminUserId = 1,
            Username = "admin",
            PasswordHash = "hashed-password",
            Role = "Admin",
            IsActive = true
        };

        _adminUserRepositoryMock
            .Setup(r => r.GetByUsernameAsync("admin"))
            .ReturnsAsync(adminUser);

        _passwordHasherMock
            .Setup(h => h.Verify("wrong-password", "hashed-password"))
            .Returns(false);

        var service = CreateService();

        // Act
        var result = await service.LoginAsync("admin", "wrong-password");

        // Assert
        Assert.Null(result);

        _jwtServiceMock.Verify(
            j => j.GenerateToken(It.IsAny<Ladestander.Api.Entities.AdminUser>()),
            Times.Never);
    }

    [Fact]
    public async Task LoginAsync_ReturnsLoginResponse_WhenCredentialsAreValid()
    {
        // Arrange
        var adminUser = new Ladestander.Api.Entities.AdminUser
        {
            AdminUserId = 1,
            Username = "admin",
            PasswordHash = "hashed-password",
            Role = "Admin",
            IsActive = true
        };

        _adminUserRepositoryMock
            .Setup(r => r.GetByUsernameAsync("admin"))
            .ReturnsAsync(adminUser);

        _passwordHasherMock
            .Setup(h => h.Verify("correct-password", "hashed-password"))
            .Returns(true);

        _jwtServiceMock
            .Setup(j => j.GenerateToken(adminUser))
            .Returns("test-token");

        var service = CreateService();

        // Act
        var result = await service.LoginAsync("admin", "correct-password");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("test-token", result.Token);
        Assert.Equal("admin", result.Username);
        Assert.Equal("Admin", result.Role);
    }
}