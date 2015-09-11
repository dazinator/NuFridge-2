using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Threading.Tasks;
using Autofac;
using Autofac.Integration.SignalR;
using Microsoft.AspNet.SignalR;
using NLog;
using NLog.Config;
using NLog.Targets;
using NuFridge.Shared.Autofac;
using NuFridge.Shared.Commands.Interfaces;
using NuFridge.Shared.Commands.Modules;
using NuFridge.Shared.Commands.Options;
using NuFridge.Shared.Commands.Util;
using NuFridge.Shared.Extensions;
using NuFridge.Shared.Hosts;
using NuFridge.Shared.Logging;
using LogLevel = NLog.LogLevel;

namespace NuFridge.Service
{
    public abstract class StartupBase
    {
         private readonly ILog _log = LogProvider.For<StartupBase>();
        private readonly string _displayName;
        private IContainer _container;
        private ICommand _commandInstance;
        private string[] _commandLineArguments;

        protected OptionSet CommonOptions { get; }

        public IContainer Container
        {
            get
            {
                if (_container == null)
                    throw new ApplicationException("The container has not yet been initialized.");
                return _container;
            }
        }

        protected StartupBase(string displayName, string[] commandLineArguments)
        {
            _commandLineArguments = commandLineArguments;
            _displayName = displayName;
            CommonOptions = new OptionSet();
        }

        public int Run()
        {
            var config = new LoggingConfiguration();

            var fileTarget = new FileTarget();
            fileTarget.KeepFileOpen = true;
            fileTarget.EnableArchiveFileCompression = true;
            fileTarget.ConcurrentWrites = false;
            fileTarget.ArchiveAboveSize = 10000000; //10MB
            fileTarget.ArchiveEvery = FileArchivePeriod.Day;
            fileTarget.ArchiveOldFileOnStartup = false;
            fileTarget.MaxArchiveFiles = 10;
            fileTarget.ArchiveFileName = "${basedir}\\Logs\\archive-{#}-log.zip";
            fileTarget.FileName = "${basedir}\\Logs\\log.txt";
            fileTarget.Layout = "${longdate} ${level} ${logger}: ${message} ${exception:format=ToString}";

            config.AddTarget("file", fileTarget);

            var consoleTarget = new ColoredConsoleTarget();
            consoleTarget.Layout = "${time}: ${message} ${exception:format=ToString}";
            config.AddTarget("console", consoleTarget);

            var rule1 = new LoggingRule("*", LogLevel.Trace, consoleTarget);
           
            config.LoggingRules.Add(rule1);

            var rule2 = new LoggingRule("*", LogLevel.Debug, fileTarget);
            config.LoggingRules.Add(rule2);

            LogManager.Configuration = config;

            try
            {
                _commandLineArguments = ProcessCommonOptions();
                SelectMostAppropriateHost().Run(Start, Stop);
                return Environment.ExitCode;
            }
            catch (ArgumentException ex)
            {
                _log.Fatal(ex.Message);
                return 1;
            }
            catch (SecurityException ex)
            {
                _log.FatalException("A security exception was encountered. Please try re-running the command as an Administrator.", ex);
                return 42;
            }
            catch (ReflectionTypeLoadException ex)
            {
                _log.FatalException(ex.Message, ex);
                foreach (Exception error in ex.LoaderExceptions)
                {
                    _log.ErrorException(error.Message, error);
                    FileNotFoundException notFoundException = error as FileNotFoundException;
                    if (!string.IsNullOrEmpty(notFoundException?.FusionLog))
                        _log.ErrorFormat("Fusion log: {0}", notFoundException.FusionLog);
                }
                return 43;
            }
            catch (Exception ex)
            {
                _log.FatalException(ex.Message, ex);
                return 100;
            }
        }

        private ICommandHost SelectMostAppropriateHost()
        {
            _log.Trace("Selecting the host");

            if (Environment.UserInteractive)
            {
                _log.Trace("The program is running using a console host");
                return new ConsoleHost(_displayName);
            }


            _log.Trace("The program is using a Windows Service host");
            return new WindowsServiceHost();
        }

        private string[] ProcessCommonOptions()
        {
            _log.Trace("Processing command line options");
            return CommonOptions.Parse(_commandLineArguments).ToArray();
        }

        private void Start(ICommandRuntime commandRuntime)
        {
            _log.Trace("Creating the Autofac container");
            _container = BuildContainer();
            RegisterAdditionalModules();
            ICommandLocator commandLocator = _container.Resolve<ICommandLocator>();
            string commandName = ExtractCommandName(ref _commandLineArguments);
            _log.TraceFormat("Finding the command: {0}", commandName);
            Lazy<ICommand, CommandMetadata> lazy = commandLocator.Find(commandName);
            if (lazy == null)
            {
                if (Debugger.IsAttached)
                {
                    lazy = commandLocator.Find("run");
                }
                else
                {
                    lazy = commandLocator.Find("help");
                    Environment.ExitCode = -1;
                }
            }
            _commandInstance = lazy.Value;
            _log.TraceFormat("Executing command: {0}", _commandInstance.GetType().Name);
            _commandInstance.Start(_commandLineArguments, commandRuntime, CommonOptions);
        }

        protected abstract IContainer BuildContainer();



        private void RegisterAdditionalModules()
        {
            ContainerBuilder containerBuilder = new ContainerBuilder();
            containerBuilder.RegisterModule(new CommandModule());
            containerBuilder.RegisterModule(new SchedulerModule());
            containerBuilder.Register((c => _container)).As<IContainer>().SingleInstance();
            containerBuilder.Update(_container);
            GlobalHost.DependencyResolver = new AutofacDependencyResolver(_container);
        }



        private static string ExtractCommandName(ref string[] args)
        {
            string str = (args.FirstOrDefault() ?? string.Empty).ToLowerInvariant().TrimStart('-', '/');
            args = args.Skip(1).ToArray();
            return str;
        }

        private void Stop()
        {
            if (_commandInstance != null)
            {
                _log.TraceFormat("Stopping the current command");
                _commandInstance.Stop();
            }
            _log.TraceFormat("Disposing the Autofac container");
            _container.Dispose();
        }
    }
}
