using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Dapper;
using NuFridge.Shared.Database.Model;
using Palmer;

namespace NuFridge.Shared.Database.Repository
{
    public class UserRepository : BaseRepository<User>, IUserRepository
    {
        private const string TableName = "User";

        public UserRepository(DatabaseContext dbContext) : base(dbContext, TableName)
        {

        }

        public void Insert(User user)
        {
            ThrowIfReadOnly();

            user.LastUpdated = DateTime.UtcNow;

            Retry.On<SqlException>(
                handle => (handle.Context.LastException as SqlException).Number == Constants.SqlExceptionDeadLockNumber)
                .For(5)
                .With(context =>
                {
                    using (var connection = GetConnection())
                    {
                        connection.Insert<int>(user);
                    }
                });
        }

        public User Find(string username)
        {
            using (var connection = GetConnection())
            {
                return connection.Query<User>($"SELECT TOP(1) * FROM [NuFridge].[{TableName}] WHERE Username = @username", new { username }).FirstOrDefault();
            }
        }

        public User Find(string username, string passwordHashed)
        {
            using (var connection = GetConnection())
            {
                return connection.Query<User>($"SELECT TOP(1) * FROM [NuFridge].[{TableName}] WHERE Username = @username AND PasswordHashed = @passwordHashed", new { username, passwordHashed }).FirstOrDefault();
            }
        }

        public void Update(User user)
        {
            ThrowIfReadOnly();

            user.LastUpdated = DateTime.UtcNow;

            Retry.On<SqlException>(
                handle => (handle.Context.LastException as SqlException).Number == Constants.SqlExceptionDeadLockNumber)
                .For(5)
                .With(context =>
                {
                    using (var connection = GetConnection())
                    {
                        connection.Update(user);
                    }
                });
        }

        public IEnumerable<User> GetAll(int pageNumber, int rowsPerPage, out int totalResults)
        {
            return GetAllPaged(pageNumber, rowsPerPage, out totalResults, "WHERE [IsService] = 0", null, true);
        }
    }

    public interface IUserRepository
    {
        int GetCount(bool nolock);
        void Insert(User user);
        User Find(string username, string passwordHashed);
        User Find(string username);
        User Find(int userId);
        void Update(User user);
        IEnumerable<User> GetAll(int pageNumber, int rows, out int totalResults);
    }
}