using System;
using NuFridge.Shared.Commands.Interfaces;
using NuFridge.Shared.Extensions;
using NuFridge.Shared.Logging;

namespace NuFridge.Shared.Hosts
{
    public class ConsoleHost : ICommandHost, ICommandRuntime
    {
        private readonly ILog _log = LogProvider.For<ConsoleHost>();

        private readonly string _displayName;

        public ConsoleHost(string displayName)
        {
            _displayName = displayName;
        }

        public void Run(Action<ICommandRuntime> start, Action shutdown)
        {
            try
            {
                Console.ResetColor();
                Console.Title = _displayName;
                start(this);
                Console.ResetColor();
                shutdown();
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(new string('-', 79));
                Console.WriteLine("Error: " + ex.GetErrorSummary());

                var baseError = ex.GetBaseException();
                if (baseError.Message != ex.Message)
                {
                    Console.WriteLine("Detailed Error: " + baseError.GetErrorSummary());
                }

                Console.WriteLine(new string('-', 79));
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.Write("At: ");
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.ResetColor();
                _log.FatalException(ex.Message, ex);
                throw;
            }
        }

        public void WaitForUserToExit()
        {
            Console.Title = _displayName + " - Running";
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Running. Press <enter> to shut down the service.");
            Console.ResetColor();
            string str;
            do
            {
                str = (Console.ReadLine() ?? string.Empty).ToLowerInvariant();
                if (str == "cls" || str == "clear")
                    Console.Clear();
            }
            while (!string.IsNullOrWhiteSpace(str));
            Console.ResetColor();
            Console.Title = _displayName + " - Stopping.";
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine();
        }
    }
}
