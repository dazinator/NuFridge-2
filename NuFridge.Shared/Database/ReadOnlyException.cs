using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuFridge.Shared.Database
{
    public class ReadOnlyException : Exception
    {
        public ReadOnlyException() :base("The website is in read-only mode.")
        {
            
        }
    }
}