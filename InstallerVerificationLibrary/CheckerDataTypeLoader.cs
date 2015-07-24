namespace InstallerVerificationLibrary
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using InstallerVerificationLibrary.Attribute;
    using InstallerVerificationLibrary.Check;
    using InstallerVerificationLibrary.Data;
    using InstallerVerificationLibrary.Loader;

    internal class CheckerDataTypeLoader
    {
        private static bool reflectionOnlyAssemblyResolveAdded;
        private readonly ICollection<PluginData> plugins;
        private ICollection<ICheck> checks;
        private ICollection<ITestDataLoader> loaders;

        public CheckerDataTypeLoader(ICollection<PluginData> plugins)
        {
            this.plugins = plugins;
        }

        internal ICollection<ICheck> GetAllChecks()
        {
            if (checks != null)
            {
                return CollectionTools.CopyCollection(checks);
            }

            checks = this.GetDataType<ICheck>();
            return CollectionTools.CopyCollection(checks);
        }

        internal ICollection<ITestDataLoader> GetAllLoaders()
        {
            if (loaders != null)
            {
                return CollectionTools.CopyCollection(loaders);
            }

            loaders = this.GetDataType<ITestDataLoader>();
            return CollectionTools.CopyCollection(loaders);
        }

        private ICollection<T> GetDataType<T>()
        {
            Type dataType;
            if (typeof(T) == typeof(ITestDataLoader))
            {
                dataType = typeof(TestDataLoaderAttribute);
            }
            else if (typeof(T) == typeof(ICheck))
            {
                dataType = typeof(TestCheckAttribute);
            }
            else
            {
                throw new InstallerVerificationLibraryException("Can't load data type of type:" + typeof(T));
            }

            var result = new Collection<T>();

            foreach (var pluginData in this.plugins)
            {
                var typesInAssembly = GetTypesFromAssembly(pluginData.PluginDllPath);
                if (typesInAssembly == null)
                {
                    throw new InstallerVerificationLibraryException(
                        "'" + pluginData.PluginDllPath + "' doesn't contain any types");
                }

                var item = typesInAssembly.Where(x => x.GetCustomAttributes(dataType, false).Length > 0)
                        .Select(x => (T)x.GetConstructor(Type.EmptyTypes).Invoke(null))
                        .FirstOrDefault(x => FindDataType(x.GetType()) == pluginData.DataType);
                if (item != null)
                {
                    result.Add(item);
                }
                else
                {
                    throw new InstallerVerificationLibraryException(
                        "Didn't find data type '" + pluginData.DataType + "' in '" + pluginData.PluginDllPath + "'");
                }
            }

            //// Get build in data types
            var typesInExecutingAssembly = GetTypesFromExecutingAssembly();
            if (typesInExecutingAssembly == null)
            {
                throw new InstallerVerificationLibraryException("Built in assembly doesn't contain any types");
            }

            foreach (var item in typesInExecutingAssembly
                        .Where(x => x.GetCustomAttributes(dataType, false).Length > 0)
                        .Select(x => (T)x.GetConstructor(Type.EmptyTypes).Invoke(null)))
            {
                result.Add(item);
            }

            return result;
        }

        private static Type[] GetTypesFromAssembly(string filePath)
        {
            var file = new FileInfo(filePath);
            if (!file.Exists)
            {
                throw new ArgumentException("File '" + file.FullName + "' does need to exist");
            }

            if (!reflectionOnlyAssemblyResolveAdded)
            {
                AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve += CurrentDomain_ReflectionOnlyAssemblyResolve;
                reflectionOnlyAssemblyResolveAdded = true;
            }

            var assembly = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(
                    x =>
                    x.ManifestModule.Name != "<In Memory Module>" && !string.IsNullOrEmpty(x.Location)
                    && string.Compare(new FileInfo(x.Location).Name, file.Name, StringComparison.OrdinalIgnoreCase)
                    == 0);

            if (assembly == null)
            {
                try
                {
                    assembly = Assembly.ReflectionOnlyLoadFrom(file.FullName);
                }
                catch (BadImageFormatException)
                {
                    return null;
                }
                catch (FileLoadException)
                {
                    // Eat the exception for the moment to we find a better way to handle it.
                    return null;
                }
                catch (AccessViolationException)
                {
                    // Eat the exception for the moment to we find a better way to handle it.
                    return null;
                }
            }

            if (assembly == null || assembly.ReflectionOnly)
            {
                assembly = Assembly.LoadFrom(file.FullName);
            }

            Type[] types;
            try
            {
                types = assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException)
            {
                return null;
            }

            if (types == null)
            {
                throw new InstallerVerificationLibraryException("'" + file.FullName + "' doesn't contain any types");
            }

            return types;
        }

        private static Assembly CurrentDomain_ReflectionOnlyAssemblyResolve(object sender, ResolveEventArgs args)
        {
            return Assembly.ReflectionOnlyLoad(args.Name);
        }

        private static IEnumerable<Type> GetTypesFromExecutingAssembly()
        {
            Assembly assembly;
            try
            {
                assembly = Assembly.GetExecutingAssembly();
            }
            catch (BadImageFormatException)
            {
                return null;
            }

            Type[] types;
            try
            {
                types = assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException)
            {
                return null;
            }

            return types;
        }

        internal static string FindDataType(Type obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException("obj");
            }

            var attribute = obj.GetCustomAttributes(typeof(DataTypeAttribute), false);
            if (attribute.Length <= 0)
            {
                return null;
            }

            var attrib = (DataTypeAttribute)attribute[0];
            return attrib.CheckType;
        }
    }
}