using System;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace NuFridge.Shared.Server.Security
{
    public class PasswordHasher
    {
        private static readonly RandomNumberGenerator RandomSource = RandomNumberGenerator.Create();
        private const int SaltSize = 16;
        private const int HashIterations = 1000;

        public static string HashPassword(string plainTextPassword)
        {
            byte[] salt = GenerateSalt();
            byte[] hashedPassword = Pbkdf(plainTextPassword, HashIterations, salt);
            return EncodeHashString(HashIterations, salt, hashedPassword);
        }

        public static bool VerifyPassword(string candidatePlainTextPassword, string knownHash)
        {
            int iterations;
            byte[] salt;
            byte[] hashedPassword;
            if (DecodeHashString(knownHash, out iterations, out salt, out hashedPassword))
                return Pbkdf(candidatePlainTextPassword, iterations, salt).SequenceEqual(hashedPassword);
            return false;
        }

        private static byte[] GenerateSalt()
        {
            byte[] data = new byte[SaltSize];
            lock (RandomSource)
                RandomSource.GetBytes(data);
            return data;
        }

        private static string EncodeHashString(int iterations, byte[] salt, byte[] hashedPassword)
        {
            return iterations.ToString("X") + "$" + Convert.ToBase64String(salt) + "$" + Convert.ToBase64String(hashedPassword);
        }

        private static bool DecodeHashString(string hash, out int iterations, out byte[] salt, out byte[] hashedPassword)
        {
            string[] strArray = hash.Split('$');
            if (strArray.Length == 3)
            {
                iterations = int.Parse(strArray[0], NumberStyles.HexNumber);
                salt = Convert.FromBase64String(strArray[1]);
                hashedPassword = Convert.FromBase64String(strArray[2]);
                return true;
            }
            iterations = 0;
            salt = null;
            hashedPassword = null;
            return false;
        }

        private static byte[] Pbkdf(string plainTextPassword, int iterations, byte[] salt)
        {
            using (Rfc2898DeriveBytes rfc2898DeriveBytes = new Rfc2898DeriveBytes(Encoding.UTF8.GetBytes(plainTextPassword), salt, iterations))
                return rfc2898DeriveBytes.GetBytes(24);
        }
    }
}
