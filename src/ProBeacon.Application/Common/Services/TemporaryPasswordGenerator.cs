using System.Security.Cryptography;

namespace ProBeacon.Application.Common.Services;

public static class TemporaryPasswordGenerator
{
    public static string Generate()
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz23456789!@#$%";
        Span<char> password = stackalloc char[16];
        for (var i = 0; i < password.Length; i++)
            password[i] = chars[RandomNumberGenerator.GetInt32(chars.Length)];

        return new string(password);
    }
}
