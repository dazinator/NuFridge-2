using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuFridge.Shared.Server.Web
{
    public class SignInRequest
    {
        public string UserName { get; set; }
        public string Password { get; set; }
    }
}
