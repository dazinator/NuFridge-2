using System;
using System.Linq;
using System.Security.Cryptography;
using NuFridge.Shared.Model.Interfaces;
using NuFridge.Shared.Server.Security;

namespace NuFridge.Shared.Model
{
    public class ApiKey : IEntity
    {
        private static readonly RandomNumberGenerator RandomSource = RandomNumberGenerator.Create();

        public int Id { get; protected set; }

        public string Purpose { get; set; }

        public int UserId { get; set; }

        public string ApiKeyHashed { get; set; }

        public DateTimeOffset Created { get; set; }

        public static ApiKey GenerateFor(int userId, string purpose, out string apiKey)
        {
            apiKey = MakeOpaqueKey();
            return Import(userId, purpose, apiKey);
        }

        public static ApiKey Import(int userId, string purpose, string apiKey)
        {
            string str = PasswordHasher.HashPassword(apiKey);
            return new ApiKey
            {
                UserId = userId,
                ApiKeyHashed = str,
                Created = DateTimeOffset.UtcNow,
                Purpose = purpose
            };
        }



        public bool Verify(string apiKey)
        {
            return PasswordHasher.VerifyPassword(apiKey, ApiKeyHashed);
        }

        private static string MakeOpaqueKey()
        {
            byte[] numArray = new byte[20];
            lock (RandomSource)
                RandomSource.GetBytes(numArray);
            return "API-" + new string(Convert.ToBase64String(numArray).Where(char.IsLetterOrDigit).ToArray()).ToUpperInvariant();
        }


        public string Name
        {
            get { return Id.ToString(); }
        }
    }
}
