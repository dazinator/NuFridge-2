using NuFridge.Shared.Server.Application;

namespace NuFridge.Shared.Commands
{
  public abstract class AbstractStandardCommand : AbstractCommand
  {
    private readonly IApplicationInstanceSelector _instanceSelector;

    protected AbstractStandardCommand(IApplicationInstanceSelector instanceSelector)
    {
      _instanceSelector = instanceSelector;
    }

    protected override void Start()
    {
        _instanceSelector.LoadInstance();
    }
  }
}
