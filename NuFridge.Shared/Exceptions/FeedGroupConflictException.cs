using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuFridge.Shared.Exceptions
{
    public class FeedGroupConflictException : Exception
    {
        public FeedGroupConflictException(string message) : this(message, null)
        {

        }

        public FeedGroupConflictException(string message, Exception ex) : base(message, ex)
        {

        }
    }
}
