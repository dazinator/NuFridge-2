using System.Linq;
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

        public User Find(string username)
        {
            using (var connection = GetConnection())
            {
                return connection.Query<User>($"SELECT TOP(1) * FROM [NuFridge].[{TableName}] WHERE Username = @username", new {  username }).FirstOrDefault();
            }
        }

        public void Update(User user)
        {
            using (var connection = GetConnection())
            {
                connection.Update(user);
            }
        }
    }

    public interface IUserRepository
    {
        int GetCount();
        void Insert(User user);
        User Find(string username);
        User Find(int userId);
        void Update(User user);
    }
}