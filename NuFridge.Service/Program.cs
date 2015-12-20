using System;

namespace NuFridge.Service
{
    class Program
    {
        static int Main(string[] args)
        {
            var code = Startup.Run(args);
            if (Environment.UserInteractive)
            {
                if (code > 0)
                {

                    Console.WriteLine();
                    Console.WriteLine("NuFridge has encountered an error. Press any key to exit.");
                    Console.Read();
                }
            }
            return code;
        }
    }
}
