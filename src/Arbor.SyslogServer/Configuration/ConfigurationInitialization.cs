using System;
using System.IO;
using Arbor.KVConfiguration.Core;
using Arbor.KVConfiguration.Core.Decorators;
using Arbor.KVConfiguration.JsonConfiguration;
using Arbor.KVConfiguration.UserConfiguration;
using Arbor.SyslogServer.Extensions;
using JetBrains.Annotations;
using Serilog;

namespace Arbor.SyslogServer.Configuration
{
    public static class ConfigurationInitialization
    {
        public static MultiSourceKeyValueConfiguration InitializeConfiguration(
            [NotNull] Func<string, string> basePath,
            ILogger logger)
        {
            if (basePath == null)
            {
                throw new ArgumentNullException(nameof(basePath));
            }

            string environmentBasedSettingsPath =
                Environment.GetEnvironmentVariable(Configuration.ConfigurationConstants.JsonSettingsFile);

            string environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";

            AppSettingsBuilder appSettingsBuilder = KeyValueConfigurationManager
                .Add(new ReflectionKeyValueConfiguration(typeof(ConfigurationInitialization).Assembly))
                .Add(new JsonKeyValueConfiguration(basePath("settings.json"), false))
                .Add(new JsonKeyValueConfiguration(basePath($"settings.{environmentName}.json"), false))
                .Add(new JsonKeyValueConfiguration(basePath($"settings.{Environment.MachineName}.json"), false));

            if (environmentBasedSettingsPath.HasValue() && File.Exists(environmentBasedSettingsPath))
            {
                appSettingsBuilder =
                    appSettingsBuilder.Add(new JsonKeyValueConfiguration(environmentBasedSettingsPath,
                        true));

                logger.Information("Added environment based configuration from key '{Key}', file '{File}'", Configuration.ConfigurationConstants.JsonSettingsFile, environmentBasedSettingsPath);
            }

            MultiSourceKeyValueConfiguration multiSourceKeyValueConfiguration = appSettingsBuilder
                .Add(new EnvironmentVariableKeyValueConfigurationSource())
                .Add(new UserConfiguration())
                .DecorateWith(new ExpandKeyValueConfigurationDecorator())
                .Build();

            logger.Information("Configuration done using chain {Chain}",
                multiSourceKeyValueConfiguration.SourceChain);

            return multiSourceKeyValueConfiguration;
        }
    }
}
