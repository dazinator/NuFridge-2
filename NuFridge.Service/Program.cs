using NuFridge.Shared.Server;

namespace NuFridge.Service
{
    class Program
    {
        static void Main(string[] args)
        {
          var code = Startup.Run(args);
          
        }
    }
}
