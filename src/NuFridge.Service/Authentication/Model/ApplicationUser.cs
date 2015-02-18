using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNet.Identity.EntityFramework;
using NuFridge.Service.Data.Model;

namespace NuFridge.Service.Authentication.Model
{
    public class ApplicationUser : IdentityUser, IEntityBase
    {
        public ApplicationUser()
        {
        }

        [Required]
        public string FirstName { get; set; }
  
        [Required]
        public string LastName { get; set; }
  
        public virtual ICollection<ApplicationUserGroup> Groups { get; set; }
    }
}