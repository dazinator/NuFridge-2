using System;
using System.IO;
using System.Linq;
using System.Reflection;
using NuFridge.Shared.Commands.Interfaces;
using NuFridge.Shared.Commands.Options;
using NuFridge.Shared.Commands.Util;
using NuFridge.Shared.Extensions;

namespace NuFridge.Shared.Commands
{
    public class HelpCommand : ICommand
    {
        private readonly ICommandLocator _commands;

        public HelpCommand(ICommandLocator commands)
        {
            _commands = commands;
        }

        public void WriteHelp(TextWriter writer)
        {
        }

        public void Start(string[] commandLineArguments, ICommandRuntime commandRuntime, OptionSet commonOptions)
        {
            string withoutExtension = Path.GetFileNameWithoutExtension(Assembly.GetEntryAssembly().FullLocalPath());
            string name = commandLineArguments.Length > 0 ? commandLineArguments[0] : null;
            if (string.IsNullOrEmpty(name))
            {
                PrintGeneralHelp(withoutExtension);
            }
            else
            {
                Lazy<ICommand, CommandMetadata> lazy = _commands.Find(name);
                if (lazy == null)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Command '{0}' is not supported", name);
                    Console.ResetColor();
                    PrintGeneralHelp(withoutExtension);
                }
                else
                    PrintCommandHelp(withoutExtension, lazy.Value, lazy.Metadata, commonOptions);
            }
        }

        public void Stop()
        {
        }

        private void PrintCommandHelp(string executable, ICommand command, CommandMetadata metadata, OptionSet commonOptions)
        {
            Console.ResetColor();
            Console.Write("Usage: ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(executable + " " + metadata.Name + " [<options>]");
            Console.ResetColor();
            Console.WriteLine();
            Console.WriteLine("Where [<options>] is any of: ");
            Console.WriteLine();
            command.WriteHelp(Console.Out);
            if (!commonOptions.Any())
                return;
            Console.WriteLine();
            Console.WriteLine("Or one of the common options: ");
            Console.WriteLine();
            commonOptions.WriteOptionDescriptions(Console.Out);
        }

        private void PrintGeneralHelp(string executable)
        {
            Console.ResetColor();
            Console.Write("Usage: ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(executable + " <command> [<options>]");
            Console.ResetColor();
            Console.WriteLine();
            Console.WriteLine("Where <command> is one of: ");
            Console.WriteLine();
            foreach (CommandMetadata commandMetadata in _commands.List().OrderBy(x => x.Name))
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("  " + commandMetadata.Name.PadRight(15, ' '));
                Console.ResetColor();
                Console.WriteLine("   " + commandMetadata.Description);
            }
            Console.WriteLine();
            Console.Write("Or use ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("help <command>");
            Console.ResetColor();
            Console.WriteLine(" for more details.");
        }
    }
}
