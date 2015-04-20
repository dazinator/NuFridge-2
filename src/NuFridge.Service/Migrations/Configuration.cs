using System.Data.Entity.Migrations;
using NuFridge.Service.Repositories;

namespace NuFridge.Service.Migrations
{
    internal sealed class Configuration : DbMigrationsConfiguration<NuFridgeContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }
    }
}