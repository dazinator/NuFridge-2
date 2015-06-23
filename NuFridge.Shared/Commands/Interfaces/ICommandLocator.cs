using System;
using NuFridge.Shared.Commands.Util;

namespace NuFridge.Shared.Commands.Interfaces
{
    public interface ICommandLocator
    {
        CommandMetadata[] List();

        Lazy<ICommand, CommandMetadata> Find(string name);
    }
}
