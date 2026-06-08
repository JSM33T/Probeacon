using System.Security.Cryptography;
using System.Text;

namespace ProBeacon.Application.Common.Services;

public static class SecureToken
{
    /// <summary>
    /// Generates a random URL-safe token. Returns the raw token (sent to the user) and its
    /// SHA-256 hex hash (stored in the database for lookup).
    /// </summary>
    public static (string Raw, string Hash) Generate()
    {
        var raw = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
        return (raw, Hash(raw));
    }

    public static string Hash(string raw)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(raw))).ToLowerInvariant();
}
