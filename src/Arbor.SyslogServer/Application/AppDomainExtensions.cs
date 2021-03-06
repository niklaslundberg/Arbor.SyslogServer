using System;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyModel;

namespace Arbor.SyslogServer.Application
{
    public static class AppDomainExtensions
    {
        private static ImmutableArray<Assembly> _cache;

        public static ImmutableArray<Assembly> FilteredAssemblies([NotNull] this AppDomain appDomain)
        {
            if (appDomain == null)
            {
                throw new ArgumentNullException(nameof(appDomain));
            }

            if (!_cache.IsDefaultOrEmpty)
            {
                return _cache;
            }

            string[] whiteListed = { "arbor" };

            ImmutableArray<RuntimeLibrary> defaultRuntimeLibraries = DependencyContext.Default?.RuntimeLibraries?.ToImmutableArray() ?? ImmutableArray<RuntimeLibrary>.Empty;

            RuntimeLibrary[] whiteListedLibraries = defaultRuntimeLibraries
                .Where(item =>
                    whiteListed.Any(listed => item.Name.StartsWith(listed, StringComparison.OrdinalIgnoreCase)))
                .ToArray();

            Assembly[] loadedAssemblies = AppDomain.CurrentDomain
                .GetAssemblies()
                .Where(assembly => !assembly.IsDynamic)
                .Where(AssemblyFilter.FilterAssemblies)
                .ToArray();

            AssemblyName[] loadedAssemblyNames = loadedAssemblies.Select(assembly => assembly.GetName()).ToArray();
            RuntimeLibrary[] toLoad = whiteListedLibraries
                .Where(lib =>
                    !loadedAssemblyNames.Any(loaded => loaded.Name.Equals(lib.Name, StringComparison.Ordinal)))
                .ToArray();

            AssemblyName[] assemblyNames = toLoad
                .SelectMany(lib => lib.GetDefaultAssemblyNames(DependencyContext.Default))
                .ToArray();

            foreach (AssemblyName assemblyName in assemblyNames)
            {
                if (!AppDomain.CurrentDomain.GetAssemblies()
                    .Any(assembly => !assembly.IsDynamic && assembly.GetName().FullName == assemblyName.FullName))
                {
                    AppDomain.CurrentDomain.Load(assemblyName);
                }
            }

            ImmutableArray<Assembly> filteredAssemblies = AppDomain.CurrentDomain.GetAssemblies().Where(assembly =>
                !assembly.IsDynamic && whiteListed.Any(listed =>
                    assembly.FullName.StartsWith(listed, StringComparison.OrdinalIgnoreCase))).ToImmutableArray();

            _cache = filteredAssemblies;

            return filteredAssemblies;
        }
    }
}
