using System;

namespace NuFridge.Shared.Database.Model
{
    public class ApiKey
    {
        public int Id { get; protected set; }

        public string Purpose { get; set; }

        public int UserId { get; set; }

        public string ApiKeyHashed { get; set; }

        public DateTimeOffset Created { get; set; }
    }
}