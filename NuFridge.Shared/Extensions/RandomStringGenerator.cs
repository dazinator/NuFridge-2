using System;
using System.Security.Cryptography;
using System.Text;

namespace NuFridge.Shared.Extensions
{
    public class RandomStringGenerator
    {
        public static string Generate(int length)
        {
            StringBuilder stringBuilder = new StringBuilder(length);
            using (RNGCryptoServiceProvider cryptoServiceProvider = new RNGCryptoServiceProvider())
            {
                for (int index = 0; index < length; ++index)
                    stringBuilder.Append("ABCDEFGHJKLMNPQRSTUVWXYZ23456789"[Next(cryptoServiceProvider, "ABCDEFGHJKLMNPQRSTUVWXYZ23456789".Length - 1)]);
                return stringBuilder.ToString();
            }
        }

        private static byte Next(RandomNumberGenerator rngCsp, int numberSides)
        {
            if (numberSides <= 0)
                throw new ArgumentOutOfRangeException("numberSides");
            byte[] data = new byte[1];
            do
            {
                rngCsp.GetBytes(data);
            }
            while (!IsFairRoll(data[0], numberSides));
            return (byte)(data[0] % numberSides + 1);
        }

        private static bool IsFairRoll(byte roll, int numSides)
        {
            int num = byte.MaxValue / numSides;
            return roll < numSides * num;
        }
    }
}
