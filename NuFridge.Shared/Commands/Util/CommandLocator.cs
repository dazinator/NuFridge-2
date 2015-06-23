using System;
using System.Collections.Generic;
using System.Linq;
using NuFridge.Shared.Commands.Interfaces;

namespace NuFridge.Shared.Commands.Util
{
  public class CommandLocator : ICommandLocator
  {
    private readonly IEnumerable<Lazy<ICommand, CommandMetadata>> _commands;

    public CommandLocator(IEnumerable<Lazy<ICommand, CommandMetadata>> commands)
    {
      _commands = commands;
    }

    public CommandMetadata[] List()
    {
      return _commands.Select(x => x.Metadata).ToArray();
    }

    public Lazy<ICommand, CommandMetadata> Find(string name)
    {
        return _commands.Select(command => new
            {
                command,
                commandName = command.Metadata.Name
            }).Select(selector => new
            {
                aliases = selector.command.Metadata.Aliases,
                TransparentIdentifier = selector
            }).Where(selector =>
            {
                if (selector.TransparentIdentifier.commandName != name)
                {
                    return selector.TransparentIdentifier.command.Metadata.Aliases.Any(a => a == name);
                }
                return true;
            }).Select(selector => selector.TransparentIdentifier.command).FirstOrDefault();
    }
  }
}
