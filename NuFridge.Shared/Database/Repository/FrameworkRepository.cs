using System.Collections.Generic;
using Dapper;
using NuFridge.Shared.Database.Model;

namespace NuFridge.Shared.Database.Repository
{
    public class FrameworkRepository : BaseRepository<Framework>, IFrameworkRepository
    {
        private const string TableName = "Framework";

        public FrameworkRepository() : base(TableName)
        {
            
        }

        public void Insert(Framework framework)
        {
            using (var connection = GetConnection())
            {
                framework.Id = connection.Insert<int>(framework);
            }
        }
    }

    public interface IFrameworkRepository
    {
        IEnumerable<Framework> GetAll();
        void Insert(Framework framework);
    }
}