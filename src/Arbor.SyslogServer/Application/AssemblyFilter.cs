using System;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;

namespace Arbor.SyslogServer.Application
{
    public static class AssemblyFilter
    {
        private static readonly string[] _BlackListed =
        {
        };

        public static bool FilterAssemblies([NotNull] Assembly assembly)
        {
            if (assembly == null)
            {
                throw new ArgumentNullException(nameof(assembly));
            }

            string assemblyName = assembly.GetName().Name;

            bool isIncluded = assemblyName.StartsWith("Arbor", StringComparison.Ordinal);

            if (_BlackListed.Any(blacklisted => assemblyName.StartsWith(blacklisted, StringComparison.OrdinalIgnoreCase)))
            {
                return false;
            }

            return isIncluded;
        }

    }
}
