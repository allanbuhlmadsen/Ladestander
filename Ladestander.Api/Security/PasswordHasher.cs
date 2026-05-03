using System.Security.Cryptography;
using System.Text;
using Ladestander.Api.Security.Interfaces;

namespace Ladestander.Api.Security;

public class PasswordHasher : IPasswordHasher
{
    public string Hash(string password)
    {
        using var sha256 = SHA256.Create();
        byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(hashBytes);
    }

    public bool Verify(string password, string passwordHash)
    {
        string hash = Hash(password);
        return hash == passwordHash;
    }
}