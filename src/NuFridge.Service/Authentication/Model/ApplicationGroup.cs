using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NuFridge.Service.Authentication.Model
{
    public class ApplicationGroup
    {
        public ApplicationGroup() { }


        public ApplicationGroup(string name)
            : this()
        {
            this.Roles = new List<ApplicationRoleGroup>();
            this.Name = name;
        }


        [Key]
        [Required]
        public virtual int Id { get; set; }

        public virtual string Name { get; set; }
        public virtual ICollection<ApplicationRoleGroup> Roles { get; set; }
    }
}
