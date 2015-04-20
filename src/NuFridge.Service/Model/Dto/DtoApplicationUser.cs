using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuFridge.Service.Model.Dto
{
    public class DtoApplicationUser
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Email { get; set; }

        public bool EmailConfirmed { get; set; }

        public string Id { get; set; }

        public string UserName { get; set; }
    }
}