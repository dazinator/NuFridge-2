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
    public class ReadOnlyDatabaseContext : DbContext
    {
        public ReadOnlyDatabaseContext(IStore store) : base(store.ConnectionString)
        {
            Configuration.AutoDetectChangesEnabled = false;
        }

        public DbSet<InternalPackage> Packages { get; set; }
    }
}