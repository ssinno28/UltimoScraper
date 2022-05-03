using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Versioning;

namespace UltimoScraper.Tests.Helpers
{
    public static class AssemblyHelper
    {
        /// <summary>
        /// Gets the directory of the referenced assembly
        /// </summary>
        /// <param name="assembly"></param>
        /// <returns></returns>
        public static string GetAssemblyDirectory(this Assembly assembly)
        {
            string codeBase = assembly.CodeBase;
            UriBuilder uri = new UriBuilder(codeBase);
            string path = Uri.UnescapeDataString(uri.Path);
            return Path.GetDirectoryName(path);
        }

        public static Type[] GetTypesInNamespace(this Assembly assembly, string nameSpace)
        {
            return assembly.GetTypes().Where(t => String.Equals(t.Namespace, nameSpace, StringComparison.Ordinal)).ToArray();
        }

        public static IEnumerable<Type> GetTypesThatImplInterface(this Assembly assembly, Type @interface)
        {
            return assembly.GetExportedTypes().Where(x => x.GetInterfaces().Contains(@interface));
        }

        public static IList<string> GetAllDirectoriesInPath(this Assembly assembly)
        {
            return assembly.Location
                .Split(Path.DirectorySeparatorChar)
                .ToList();
        }

        public static string GetFrameworkVersion(this Assembly assembly)
        {
            return assembly.GetCustomAttribute<TargetFrameworkAttribute>()?.FrameworkName;
        }
    }
}
