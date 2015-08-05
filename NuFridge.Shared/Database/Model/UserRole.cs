namespace NuFridge.Shared.Database.Model
{
    public class UserRole
    {
        public int Id { get; protected set; }

        public string Name { get; set; }

        public string Description { get; set; }


        public bool CanBeDeleted { get; set; }

        public UserRole()
        {
            CanBeDeleted = true;
        }

        public UserRole(string name)
        {
            Name = name;
            CanBeDeleted = true;
        }
    }
}
