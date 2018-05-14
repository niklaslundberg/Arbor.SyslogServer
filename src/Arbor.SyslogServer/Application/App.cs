using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Arbor.KVConfiguration.Core;
using Arbor.SyslogServer.AspNetCore;
using Arbor.SyslogServer.Configuration;
using Arbor.SyslogServer.Logging;
using Arbor.SyslogServer.Messaging;
using Autofac;
using Autofac.Core;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Hosting;
using ILogger = Serilog.ILogger;

namespace Arbor.SyslogServer.Application
{
    [UsedImplicitly]
    public sealed class App : IDisposable
    {
        private readonly CancellationTokenSource _cancellationTokenSource;

        private readonly ILogger _logger;

        private bool _disposed;
        private bool _disposing;
        public const string Name = "Arbor.SyslogServer";

        public App(
            [NotNull] IWebHostBuilder webHost,
            [NotNull] CancellationTokenSource cancellationTokenSource,
            [NotNull] ILogger appLogger)
        {
            _cancellationTokenSource = cancellationTokenSource ??
                                       throw new ArgumentNullException(nameof(cancellationTokenSource));
            _logger = appLogger ?? throw new ArgumentNullException(nameof(appLogger));
            HostBuilder = webHost ?? throw new ArgumentNullException(nameof(webHost));
        }

        public ILifetimeScope WebHostScope { get; private set; }

        public IWebHostBuilder HostBuilder { get; private set; }

        public IWebHost WebHost { get; private set; }

        public IContainer Container { get; private set; }

        public static App Create(
            CancellationTokenSource cancellationTokenSource,
            params string[] args)
        {
            if (args == null)
            {
                throw new ArgumentNullException(nameof(args));
            }

            try
            {
                App app = BuildApp(cancellationTokenSource, args);

                return app;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Error in startup, " + ex);
                throw;
            }
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            if (!_disposing)
            {
                Stop();
                _disposing = true;
            }

            WebHost?.Dispose();
            WebHostScope?.Dispose();
            Container?.Dispose();

            if (_logger is IDisposable disposable)
            {
                disposable.Dispose();
            }

            WebHost = null;
            HostBuilder = null;
            Container = null;
            WebHostScope = null;
            _disposed = true;
            _disposing = false;
        }

        public void Stop()
        {
            _cancellationTokenSource.Cancel();
        }

        public async Task<int> RunAsync([NotNull] params string[] args)
        {
            if (args == null)
            {
                throw new ArgumentNullException(nameof(args));
            }

            _logger.Information("Starting Arbor.SyslogServer");

            WebHost = HostBuilder.Build();

            await WebHost.StartAsync(_cancellationTokenSource.Token);

            _logger.Debug("Started webhost");

            return 0;
        }

        private static App BuildApp(
            CancellationTokenSource cancellationTokenSource,
            string[] args)
        {
            ImmutableArray<Assembly> scanAssemblies = AppDomain.CurrentDomain.FilteredAssemblies();

            string basePathFromArg = args.SingleOrDefault(arg =>
                arg.StartsWith("urn:arbor:syslogserver:base-path", StringComparison.OrdinalIgnoreCase));

            string basePath = basePathFromArg?.Split('=').LastOrDefault() ?? AppDomain.CurrentDomain.BaseDirectory;

            ILogger startupLogger =
                SerilogApiInitialization.InitializeStartupLogging(file => GetBaseDirectoryFile(basePath, file));

            MultiSourceKeyValueConfiguration configuration =
                ConfigurationInitialization.InitializeConfiguration(file => GetBaseDirectoryFile(basePath, file),
                    startupLogger);

            ILogger appLogger = SerilogApiInitialization.InitializeAppLogging(configuration, startupLogger);

            appLogger.Information("Started with command line args, {Args}", args);

            IReadOnlyList<IModule> modules =
                GetConfigurationModules(configuration, cancellationTokenSource, appLogger, scanAssemblies);

            IContainer container = Bootstrapper.Start(basePathFromArg, modules, appLogger, scanAssemblies);

            ILifetimeScope webHostScope =
                container.BeginLifetimeScope(builder =>
                {
                    builder.RegisterType<Startup>().AsSelf();
                });

            IWebHostBuilder webHostBuilder =
                CustomWebHostBuilder.GetWebHostBuilder(container, webHostScope, appLogger);

            var buildApp = new App(webHostBuilder, cancellationTokenSource, appLogger)
            {
                Container = container,
                WebHostScope = webHostScope
            };

            return buildApp;
        }

        private static IReadOnlyList<IModule> GetConfigurationModules(
            MultiSourceKeyValueConfiguration configuration,
            [NotNull] CancellationTokenSource cancellationTokenSource,
            ILogger logger,
            ImmutableArray<Assembly> scanAssemblies)
        {
            var modules = new List<IModule>();

            if (cancellationTokenSource == null)
            {
                throw new ArgumentNullException(nameof(cancellationTokenSource));
            }

            var loggingModule = new LoggingModule(logger);

            var module = new KeyValueConfigurationModule(configuration, logger);

            var urnModule = new UrnConfigurationModule(configuration, logger, scanAssemblies);

            modules.Add(loggingModule);
            modules.Add(module);
            modules.Add(urnModule);
            modules.Add(new MediatRModule(scanAssemblies));

            return modules;
        }

        private static string GetBaseDirectoryFile(string basePath, string fileName)
        {
            return Path.Combine(basePath, fileName);
        }
    }
}
