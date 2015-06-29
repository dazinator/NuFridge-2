using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Threading.Tasks;
using Autofac;
using FluentScheduler;
using NLog;
using NLog.Config;
using NLog.Targets;
using NuFridge.Shared.Commands.Interfaces;
using NuFridge.Shared.Commands.Modules;
using NuFridge.Shared.Commands.Options;
using NuFridge.Shared.Commands.Util;
using NuFridge.Shared.Extensions;
using NuFridge.Shared.Logging;
using NuFridge.Shared.Server.Hosts;
using NuFridge.Shared.Server.Modules;
using NuFridge.Shared.Server.Scheduler;
using NuFridge.Shared.Server.Scheduler.Jobs;
using LogLevel = NuFridge.Shared.Logging.LogLevel;

namespace NuFridge.Service
{
    public abstract class StartupBase
    {
         private readonly ILog _log = LogProvider.For<StartupBase>();
        private readonly string _displayName;
        private readonly OptionSet _commonOptions;
        private IContainer _container;
        private ICommand _commandInstance;
        private string[] _commandLineArguments;
        private bool _forceConsole;

        protected OptionSet CommonOptions
        {
            get
            {
                return _commonOptions;
            }
        }

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
            _commonOptions = new OptionSet();
            _commonOptions.Add("console", "Don't run as a service.", v => _forceConsole = true);
        }

        public int Run()
        {
            var config = new LoggingConfiguration();

            var fileTarget = new FileTarget();
            fileTarget.KeepFileOpen = true;
            fileTarget.EnableArchiveFileCompression = true;
            fileTarget.ConcurrentWrites = false;
            fileTarget.ArchiveAboveSize = 10000000; //10MB
            fileTarget.ArchiveEvery = FileArchivePeriod.Hour;
            fileTarget.ArchiveOldFileOnStartup = true;
            fileTarget.MaxArchiveFiles = 8;
            fileTarget.ArchiveFileName = "Logs/server-{#}-logs.zip";
            config.AddTarget("file", fileTarget);

            fileTarget.FileName = "${basedir}/Logs/server.log";
            fileTarget.Layout = "${longdate} ${level} ${logger}: ${message}";

            var consoleTarget = new ColoredConsoleTarget();
            consoleTarget.Layout = "${time}: ${message}";
            config.AddTarget("console", consoleTarget);

            var rule1 = new LoggingRule("*", NLog.LogLevel.Trace, consoleTarget);
           
            config.LoggingRules.Add(rule1);

            var rule2 = new LoggingRule("*", NLog.LogLevel.Info, fileTarget);
            config.LoggingRules.Add(rule2);

            LogManager.Configuration = config;

            TaskScheduler.UnobservedTaskException += (EventHandler<UnobservedTaskExceptionEventArgs>)((sender, args) =>
            {
                _log.ErrorException("Unhandled task exception occurred: {0}", args.Exception.UnpackFromContainers(), args.Exception.GetErrorSummary());
                args.SetObserved();
            });
            AppDomain.CurrentDomain.UnhandledException += (UnhandledExceptionEventHandler)((sender, args) =>
            {
                Exception error = args.ExceptionObject as Exception;
                _log.FatalException("Unhandled AppDomain exception occurred: {0}", error, error == null ? args.ExceptionObject : error.Message);
            });
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
                    if (error is FileNotFoundException)
                    {
                        FileNotFoundException notFoundException = error as FileNotFoundException;
                        if (!string.IsNullOrEmpty(notFoundException.FusionLog))
                            _log.ErrorFormat("Fusion log: {0}", notFoundException.FusionLog);
                    }
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
            if (_forceConsole)
            {
                _log.Trace("The --console switch was passed");
                return new ConsoleHost(_displayName);
            }
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
                if (System.Diagnostics.Debugger.IsAttached)
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
            containerBuilder.Register((c => _container)).As<IContainer>();
            containerBuilder.Update(_container);
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
