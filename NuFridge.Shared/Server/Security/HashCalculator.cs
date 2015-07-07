using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace NuFridge.Shared.Server.Security
{
    public static class HashCalculator
    {
        public static string Hash(Stream stream)
        {
            return BitConverter.ToString(GetAlgorithm().ComputeHash(stream)).Replace("-", "").ToLowerInvariant();
        }

        public static string Hash(string input)
        {
            return BitConverter.ToString(GetAlgorithm().ComputeHash(Encoding.UTF8.GetBytes(input))).Replace("-", "").ToLowerInvariant();
        }

        private static HashAlgorithm GetAlgorithm()
        {
            return new SHA1CryptoServiceProvider();
        }
    }
}
