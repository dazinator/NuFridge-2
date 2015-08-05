using System;
using System.Collections.Generic;
using System.Linq;
using Nancy.Security;

namespace NuFridge.Shared.Server.Web
{
    //TODO
    public class UserDatabase
    {
        private static readonly List<Tuple<string, string>> Users = new List<Tuple<string, string>>();

        static UserDatabase()
        {
            Users.Add(new Tuple<string, string>("administrator", "password"));
        }

        public static IUserIdentity ValidateUser(SignInRequest signInRequest)
        {
            //try to get a user from the "database" that matches the given username and password
            var userRecord = Users.FirstOrDefault(u => u.Item1 == signInRequest.UserName && u.Item2 == signInRequest.Password);

            if (userRecord == null)
            {
                return null;
            }

            return new TemporaryAdminUserIdentity { UserName = userRecord.Item1, Claims = new List<string>()};
        }
    }
}