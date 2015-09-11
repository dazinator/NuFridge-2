using Nancy;

namespace NuFridge.Shared.Web.Nancy
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