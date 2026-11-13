using System.Security.Cryptography;

namespace GameDataCollection.Extension
{
    public static class PasswordGenerator
    {
        // 10 characters with at least one uppercase, lowercase, digit and symbol (Identity's default rules).
        // Look-alike characters such as 0/O and 1/l/I are left out so the password is easy to type.
        public static string Generate()
        {
            const string upper = "ABCDEFGHJKLMNPQRSTUVWXYZ";
            const string lower = "abcdefghijkmnpqrstuvwxyz";
            const string digits = "23456789";
            const string symbols = "@#$%*!";

            var chars = RandomNumberGenerator.GetItems<char>(upper + lower + digits + symbols, 6)
                .Concat(RandomNumberGenerator.GetItems<char>(upper, 1))
                .Concat(RandomNumberGenerator.GetItems<char>(lower, 1))
                .Concat(RandomNumberGenerator.GetItems<char>(digits, 1))
                .Concat(RandomNumberGenerator.GetItems<char>(symbols, 1))
                .ToArray();
            RandomNumberGenerator.Shuffle<char>(chars);
            return new string(chars);
        }
    }
}
