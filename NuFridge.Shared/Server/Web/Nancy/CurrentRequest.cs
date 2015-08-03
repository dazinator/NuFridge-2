using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nancy;

namespace NuFridge.Shared.Server.Web.Nancy
{
    public class CurrentRequest : ICurrentRequest
    {
        public CurrentRequest(NancyContext context)
        {
            Context = context;
        }

        public NancyContext Context { get; }
    }

    public interface ICurrentRequest
    {
        NancyContext Context { get;  }
    }
}