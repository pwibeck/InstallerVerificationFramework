namespace InstallerVerificationLibrary.Tools
{
    using System;
    using System.Linq;
    using Logging;
    using Microsoft.Win32;

    public static class RegistryTool
    {
        private const string LocalMachineRegKeyName = @"HKEY_LOCAL_MACHINE\";
        private const string LocalMachineRegKeyNameShort = @"HKLM\";
        private const string ClassesRootRegKeyName = @"HKEY_CLASSES_ROOT\";
        private const string ClassesRootRegKeyNameShort = @"HKCR\";
        private const string CurrentConfigRegKeyName = @"HKEY_CURRENT_CONFIG\";
        private const string CurrentConfigRegKeyNameShort = @"HKCC\";
        private const string CurrentUserRegKeyName = @"HKEY_CURRENT_USER\";
        private const string CurrentUserRegKeyNameShort = @"HKCU\";
        private const string UserRegKeyName = @"HKEY_USERS\";
        private const string UserRegKeyNameShort = @"HKU\";

        public static void RemoveRegistryKey(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException(nameof(key));
            }

            var subkey = GetSubKeyPath(key);
            var hive = GetRootRegistryKey(key);
            hive.DeleteSubKeyTree(subkey, false);

            Log.WriteInfo("Deleted registry key:" + key, "RegistryTool");
        }

        private static RegistryKey GetRootRegistryKey(string key)
        {
            RegistryKey hive;
            if (key.StartsWith(LocalMachineRegKeyName) || key.StartsWith(LocalMachineRegKeyNameShort))
            {
                hive = Registry.LocalMachine;
            }
            else if (key.StartsWith(ClassesRootRegKeyName) || key.StartsWith(ClassesRootRegKeyNameShort))
            {
                hive = Registry.ClassesRoot;
            }
            else if (key.StartsWith(CurrentConfigRegKeyName) || key.StartsWith(CurrentConfigRegKeyNameShort))
            {
                hive = Registry.CurrentConfig;
            }
            else if (key.StartsWith(CurrentUserRegKeyName) || key.StartsWith(CurrentUserRegKeyNameShort))
            {
                hive = Registry.CurrentUser;
            }
            else if (key.StartsWith(UserRegKeyName) || key.StartsWith(UserRegKeyNameShort))
            {
                hive = Registry.Users;
            }
            else
            {
                throw new InstallerVerificationLibraryException("Could not find hive for key:" + key);
            }
            return hive;
        }

        public static bool RegistryKeyExist(string key)
        {
            var subkey = GetSubKeyPath(key);
            var hive = GetRootRegistryKey(key);

            var sub = hive.OpenSubKey(subkey);

            return sub != null;
        }

        public static object RegistryValue(string key, string valueName, object defaultValue = null)
        {
            if (!RegistryKeyExist(key))
            {
                return defaultValue;
            }

            var subkey = GetSubKeyPath(key);
            var hive = GetRootRegistryKey(key);

            var value = hive.OpenSubKey(subkey).GetValue(valueName);
            return value ?? defaultValue;
        }
        
        private static string GetSubKeyPath(string key)
        {
            var subKeyPath = string.Empty;

            foreach (var name in new[]
            {
                LocalMachineRegKeyName,
                LocalMachineRegKeyNameShort,
                ClassesRootRegKeyName,
                ClassesRootRegKeyNameShort,
                CurrentConfigRegKeyName,
                CurrentConfigRegKeyNameShort,
                CurrentUserRegKeyName,
                CurrentUserRegKeyNameShort,
                UserRegKeyName,
                UserRegKeyNameShort
            }.Where(name => key.StartsWith(name, StringComparison.OrdinalIgnoreCase)))
            {
                subKeyPath = key.Replace(name, string.Empty);
                break;
            }

            return subKeyPath;
        }
    }
}