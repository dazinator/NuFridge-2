using System;
using NuFridge.Shared.Commands.Interfaces;
using NuFridge.Shared.Exceptions.Kb;
using NuFridge.Shared.Extensions;
using NuFridge.Shared.Logging;

namespace NuFridge.Shared.Server.Hosts
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
                Console.WriteLine("Base Error: " + ex.GetBaseException().GetErrorSummary());
                Console.WriteLine(new string('-', 79));
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.Write("At: ");
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.ResetColor();
                ExceptionKnowledgeBaseEntry entry;
                if (ExceptionKnowledgeBase.TryInterpret(ex, out entry))
                {
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine(new string('=', 79));
                    Console.WriteLine(entry.Summary);
                    if (entry.HelpText != null)
                    {
                        Console.WriteLine(new string('-', 79));
                        if (entry.HelpText != null)
                            Console.WriteLine(entry.HelpText);
                    }
                    Console.WriteLine(new string('=', 79));
                    Console.ResetColor();
                }
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
