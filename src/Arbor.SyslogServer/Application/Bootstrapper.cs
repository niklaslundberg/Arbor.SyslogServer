using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using Arbor.SyslogServer.Extensions;
using Autofac;
using Autofac.Core;
using Serilog;

namespace Arbor.SyslogServer.Application
{
    public static class Bootstrapper
    {
        public static IContainer Start(
            string basePathFromArg,
            IReadOnlyList<IModule> readOnlyList,
            ILogger logger,
            ImmutableArray<Assembly> scanAssemblies)
        {
            var builder = new ContainerBuilder();

            foreach (IModule module in readOnlyList)
            {
                builder.RegisterModule(module);
            }

            logger.Information("Done running configuration modules");

            var existingTypes = readOnlyList.Select(item => item.GetType()).ToArray();

            Type[] modules = scanAssemblies
                .Select(assembly =>
                    assembly.GetLoadableTypes()
                        .Where(type => type.IsClass && !type.IsAbstract && type.IsAssignableTo<IModule>()))
                .SelectMany(types => types)
                .Except(existingTypes)
                .ToArray();

            foreach (Type moduleType in modules.Where(type => type.IsPublicClassWithDefaultConstructor()))
            {
                if (Activator.CreateInstance(moduleType) is IModule module)
                {
                    builder.RegisterModule(module);
                }
            }

            IContainer container = builder.Build();

            return container;
        }
    }
}
