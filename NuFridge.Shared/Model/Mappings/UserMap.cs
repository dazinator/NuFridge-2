using NuFridge.Shared.Server.Storage;

namespace NuFridge.Shared.Model.Mappings
{
    public class UserMap : EntityMapping<User>
    {
        public UserMap()
        {
            Column(m => m.Username);
            Column(m => m.IsActive);
            Column(m => m.IsService);
            Column(m => m.IdentificationToken);
            Unique("UserUsernameUnique", "Username", "A user with this username already exists. Please provide a different username.");
        }
    }
}