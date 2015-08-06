using Dapper;
using NuFridge.Shared.Database.Model;

namespace NuFridge.Shared.Database.Repository
{
    public class UserRepository : BaseRepository<User>, IUserRepository
    {
        private const string TableName = "User";

        public UserRepository() : base(TableName)
        {

        }

        public void Insert(User user)
        {
            using (var connection = GetConnection())
            {
                connection.Insert<int>(user);
            }
        }
    }

    public interface IUserRepository
    {
        int GetCount();
        void Insert(User user);
    }
}