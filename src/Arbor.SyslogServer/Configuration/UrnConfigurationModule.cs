using System;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using Arbor.KVConfiguration.Core;
using Arbor.KVConfiguration.Urns;
using Arbor.SyslogServer.Validation;
using Autofac;
using JetBrains.Annotations;
using Serilog;
using Module = Autofac.Module;

namespace Arbor.SyslogServer.Configuration
{
    [UsedImplicitly]
    public class UrnConfigurationModule : Module
    {
        private readonly IKeyValueConfiguration _keyValueConfiguration;
        private readonly ILogger _logger;
        private readonly ImmutableArray<Assembly> _assemblies;

        public UrnConfigurationModule([NotNull] IKeyValueConfiguration keyValueConfiguration, ILogger logger, ImmutableArray<Assembly> assemblies)
        {
            _keyValueConfiguration = keyValueConfiguration ?? throw new ArgumentNullException(nameof(keyValueConfiguration));
            _logger = logger;
            _assemblies = assemblies;
        }

        protected override void Load(ContainerBuilder builder)
        {
            _logger.Debug("Running Urn module");

            ImmutableArray<Type> urnMappedTypes = UrnTypes.GetUrnTypesInAssemblies(_assemblies);

            if (!urnMappedTypes.Any())
            {
                _logger.Debug("Found no URN-bound types");
                return;
            }

            _logger.Debug("Found URN-bound types {Types}", urnMappedTypes.Select(t => t.Name).ToArray());

            if (!bool.TryParse(_keyValueConfiguration[UrnConfigurationConstants.TreatWarningsAsErrors], out bool treatWarningsAsErrors))
            {
                treatWarningsAsErrors = false;
            }

            foreach (Type urnMappedType in urnMappedTypes)
            {
                try
                {
                    Register(builder, urnMappedType, treatWarningsAsErrors);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Could not register URN-mapped type {Type}", urnMappedType);
                }
            }
        }

        private void Register(ContainerBuilder builder, Type type, bool treatWarningsAsErrors)
        {
            ImmutableArray<object> instances = _keyValueConfiguration.GetInstances(type);

            IValidationObject[] validationObjects = instances.Select(instance =>
                instance as IValidationObject).Where(item => item != null).ToArray();

            if (validationObjects.Length > 0 && !validationObjects.Any(validatedObject => validatedObject.IsValid))
            {
                _logger.Warning("There are [{ValidationObjectCount}] but no valid instance of type {Type}", validationObjects.Length, type.FullName);

                if (treatWarningsAsErrors)
                {
                    throw new InvalidOperationException($"Could not create instance of type {type.FullName}, the instance is invalid, using configuration chain {(_keyValueConfiguration as MultiSourceKeyValueConfiguration)?.SourceChain}");
                }
            }

            object validInstance = validationObjects.FirstOrDefault(validationObject => validationObject.IsValid);

            object usedInstance = validInstance ?? instances.FirstOrDefault();

            if (usedInstance is null)
            {
                _logger.Error("Could not register URN-mapped type {Type}, instance is null", type);

                return;
            }

            _logger.Debug("Registering URN-bound instance {Instance}, {Type}", usedInstance, usedInstance.GetType().FullName);

            builder
                .RegisterInstance(usedInstance)
                .AsSelf()
                .SingleInstance();
        }
    }
}
