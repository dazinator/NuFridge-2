using NuFridge.Shared.Server;

namespace NuFridge.Service
{
    class Program
    {
        static int Main(string[] args)
        {
            args = new[] {"-configure"};
            var code = Startup.Run(args);
            return code;
        }
    }
}
