using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NuFridge.Shared.Model.Interfaces;
using NuFridge.Shared.Model.Mappings;
using NuFridge.Shared.Server.Storage;

namespace NuFridge.Shared.Model
{
    public class DatabaseContext : DbContext
    {
        private readonly int _feedId;

        public DatabaseContext(int feedId, IStore store) : base(store.ConnectionString)
        {
            _feedId = feedId;
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<InternalPackage>().ToTable(InternalPackageMap.GetPackageTable(_feedId), "NuFridge");
        }

        public DbSet<InternalPackage> Packages { get; set; }
    }
}