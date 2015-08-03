using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NuFridge.Shared.Model.Interfaces;
using NuFridge.Shared.Model.Mappings;
using NuFridge.Shared.Server.Storage;
using TrackerEnabledDbContext;

namespace NuFridge.Shared.Model
{
    public class WritableDatabaseContext : TrackerContext
    {
        public WritableDatabaseContext(IStore store) : base(store.ConnectionString)
        {
            Configuration.AutoDetectChangesEnabled = false;
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<InternalPackage>().Ignore(u => u.IsAbsoluteLatestVersion);
            modelBuilder.Entity<InternalPackage>().Ignore(u => u.IsLatestVersion);
        }

        public DbSet<InternalPackage> Packages { get; set; }
    }
}