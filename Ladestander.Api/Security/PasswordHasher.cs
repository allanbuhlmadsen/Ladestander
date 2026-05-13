using Ladestander.Api.Entities;
using Ladestander.Api.Security.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace Ladestander.Api.Security;

public class PasswordHasher : IPasswordHasher
{
    private readonly PasswordHasher<AdminUser> _passwordHasher = new();

    public string Hash(string password)
    {
        return _passwordHasher.HashPassword(new AdminUser(), password);
    }

    public bool Verify(string password, string passwordHash)
    {
        var result = _passwordHasher.VerifyHashedPassword(
            new AdminUser(),
            passwordHash,
            password);

        return result == PasswordVerificationResult.Success ||
               result == PasswordVerificationResult.SuccessRehashNeeded;
    }
}