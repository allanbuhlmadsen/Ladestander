using System.Security.Cryptography;
using System.Text;

Console.Write("Enter admin password: ");
string? password = Console.ReadLine();

if (string.IsNullOrWhiteSpace(password))
{
    Console.WriteLine("Password cannot be empty.");
    return;
}

using var sha256 = SHA256.Create();
byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
string hash = Convert.ToBase64String(hashBytes);

Console.WriteLine();
Console.WriteLine("Generated password hash:");
Console.WriteLine(hash);
Console.WriteLine();
Console.WriteLine("Copy this hash into the SQL INSERT statement.");