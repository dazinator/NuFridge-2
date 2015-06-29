using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NuFridge.Shared.Server.Storage;

namespace NuFridge.Shared.Model
{
    public class DatabaseContext : DbContext
    {
        public DatabaseContext(IStore store) : base(store.ConnectionString)
        {
            
        }

        public DbSet<InternalPackage> Packages { get; set; }
    }
}
