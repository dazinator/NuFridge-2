using NuFridge.Shared.Server.Storage;

namespace NuFridge.Shared.Model.Mappings
{
    public class UserRoleMap : EntityMapping<UserRole>
    {
        public UserRoleMap()
        {
            Column(m => m.Name);
            Unique("UserRoleNameUnique", "Name", "A user role with this name already exists. Please choose a different name.");
        }
    }
}
