using System.Security.Cryptography;
using System.Text;

namespace WilliamAPI.Helpers
{
    public static class PasswordHelper
    {
        // Hash simple con SHA256 (para demo). En producción usá PBKDF2/BCrypt/Argon2.
        public static string Hash(string plain)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(plain));
            return BitConverter.ToString(bytes).Replace("-", "").ToLowerInvariant();
        }
    }
}
