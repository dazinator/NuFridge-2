using Autofac;
using Nancy;
using NuFridge.Shared.Web.Actions.SymbolsApi;

namespace NuFridge.Shared.Web.Modules
{
    public class SymbolsModule: NancyModule
    {
        public SymbolsModule(IContainer container)
        {
            //Upload symbol package
            Put["feeds/{feed}/api/symbols"] = p => container.Resolve<UploadSymbolPackageAction>().Execute(p, this);

            //Get symbol file
            Get["feeds/{feed}/api/symbols/{path}"] = p => container.Resolve<GetSymbolFileAction>().Execute(p, this);

            //Get source files
            Get["feeds/{feed}/api/symbols/{id}/{version}/{path}"] = p => container.Resolve<GetSourceFilesAction>().Execute(p, this);
        }
    }
}