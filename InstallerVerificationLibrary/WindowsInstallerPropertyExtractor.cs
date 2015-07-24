namespace InstallerVerificationLibrary
{
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.IO;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Text;

    using NativeWrappers;

    public static class WindowsInstallerPropertyExtractor
    {
        private static readonly Collection<DataProperty> PropertiesCache = new Collection<DataProperty>();

        public static string GetValueForProperty(DataProperty property)
        {
            var propertylist = new Collection<DataProperty> { property };
            CacheProperties(propertylist);
            var dataProperty = PropertiesCache.FirstOrDefault(x => x.Id == property.Id);
            if (dataProperty != null)
            {
                return dataProperty.Value;
            }

            throw new InstallerVerificationLibraryException("Could not find property : " + property.Id);
        }

        public static void CacheProperties(Collection<DataProperty> properties)
        {
            if (properties == null)
            {
                throw new ArgumentNullException("list");
            }

            var handle = IntPtr.Zero;

            foreach (var item in properties.Where(item => !PropertiesCache.Contains(item)))
            {
                if (handle == IntPtr.Zero)
                {
                    handle = OpenMsipackage();
                }

                var stringSize = 0;
                if (MsiMethods.MsiGetProperty(handle, item.Id, new StringBuilder(0), ref stringSize)
                    != (int)MsiMethods.ErrorCodes.MoreData)
                {
                    continue;
                }

                stringSize += 1;
                var sb = new StringBuilder(stringSize);
                if (MsiMethods.MsiGetProperty(handle, item.Id, sb, ref stringSize)
                    == (int)MsiMethods.ErrorCodes.Success)
                {
                    item.Value = sb.ToString();
                    PropertiesCache.Add(item);
                }
            }

            if (handle != IntPtr.Zero)
            {
                MsiMethods.MsiCloseHandle(handle);
            }
        }

        private static IntPtr OpenMsipackage()
        {
            var filename = GetTestMsiFileName();

            var handle = IntPtr.Zero;
            if (MsiMethods.MsiOpenPackage(filename, out handle) == (int)MsiMethods.ErrorCodes.Success)
            {
                return handle;
            }

            var errorCode = Marshal.GetLastWin32Error();
            throw new InstallerVerificationLibraryException("Could not open msi = " + filename, new Win32Exception(errorCode));
        }

        private static string GetTestMsiFileName()
        {
            var filename = Path.Combine(Path.GetTempPath(), "test.msi");
            
            if (File.Exists(filename))
            {
                File.Delete(filename);
            }

            var db = MsiDatabase.CreateNew(
                filename, 
                "{00000000-0000-0000-0000-000000000000}", 
                "{00000000-0000-0000-0000-000000000000}");
            db.Close();

            return filename;
        }
    }
}