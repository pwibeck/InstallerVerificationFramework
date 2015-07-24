namespace InstallerVerificationLibrary.Check
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using InstallerVerificationLibrary.Attribute;
    using InstallerVerificationLibrary.Data;

    using NativeWrappers;

    [DataType("StartMenu")]
    [TestCheck]
    public sealed class StartMenuCheck : BaseCheck
    {
        public override CheckResult DoCheck(BaseTestData data, ICollection<FileChange> fileChanges, ICollection<Registryhange> registryChanges)
        {
            if (!VerifyIfCorrectTestData(data))
            {
                return CheckResult.NotCheckDone();
            }

            var startMenuData = data as StartMenuData;
            var path = startMenuData.AllUsers ? GetAllUsersMenuFolder() : Environment.GetFolderPath(Environment.SpecialFolder.StartMenu);

            // To get the program folder
            var dirInfo = new DirectoryInfo(path);
            path = Path.Combine(path, dirInfo.GetDirectories()[0].Name);
            path = Path.Combine(path, startMenuData.Name + ".lnk");

            // Find registry GlobalAssocChangedCounter key that is updated when creating a startmenu
            var globalAssocChangedCounterKey = @"HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\explorer";
            if (Environment.Is64BitOperatingSystem)
            {
                globalAssocChangedCounterKey = @"HKLM\SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\explorer";
            }
            var registryChange = registryChanges.FirstOrDefault(x => string.Equals(x.Key, globalAssocChangedCounterKey, StringComparison.InvariantCultureIgnoreCase) && 
                                                                   x.ValueName == "GlobalAssocChangedCounter" &&
                                                                   x.Type == RegistryChangeType.SetValue);

            var fileChange = fileChanges.FirstOrDefault(x => x.Path == path);

            return startMenuData.Exist ? CheckDataIfStartMenuIsExpectedToExist(fileChanges, registryChanges, fileChange, startMenuData, registryChange) : 
                                         CheckDataIfStartMenuIsExpectedToNotExist(fileChanges, registryChanges, fileChange, startMenuData, registryChange);
        }

        private static CheckResult CheckDataIfStartMenuIsExpectedToNotExist(ICollection<FileChange> fileChanges, ICollection<Registryhange> registryChanges,
            FileChange fileChange, StartMenuData startMenuData, Registryhange registryChange)
        {
            if (fileChange == null)
            {
               return CheckResult.Succeeded(startMenuData);
            }

            if (fileChange.Type != FileChangeType.Delete)
            {
                fileChanges.Remove(fileChange);
                if (registryChange != null)
                {
                    registryChanges.Remove(registryChange);
                }
                return CheckResult.Failure("Found menu item:'" + startMenuData.Name + "' when is was not expected");
            }

            // When uninstalling this key get changed
            const string startPageKey = @"HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\StartPage";
            var uninstallStartMenuKey =
                registryChanges.FirstOrDefault(x => string.Equals(x.Key, startPageKey, StringComparison.InvariantCultureIgnoreCase) &&
                                                    x.ValueName == "FavoritesRemovedChanges" &&
                                                    x.Type == RegistryChangeType.SetValue);

            if (uninstallStartMenuKey == null)
            {
                fileChanges.Remove(fileChange);
                return CheckResult.Failure("");
            }

            registryChanges.Remove(uninstallStartMenuKey);
            fileChanges.Remove(fileChange);
            return CheckResult.Succeeded(startMenuData);
        }

        private static CheckResult CheckDataIfStartMenuIsExpectedToExist(ICollection<FileChange> fileChanges, ICollection<Registryhange> registryChanges,
            FileChange fileChange, StartMenuData startMenuData, Registryhange registryChange)
        {
            if (fileChange == null)
            {
                return CheckResult.Failure("Could not find start menu item:'" + startMenuData.Name + "'");
            }

            if (fileChange.Type == FileChangeType.Delete)
            {
                fileChanges.Remove(fileChange);
                return CheckResult.Failure("Could not find start menu item:'" + startMenuData.Name + "'");
            }

            if (registryChange == null)
            {
                fileChanges.Remove(fileChange);
                return CheckResult.Failure("Could not find start menu item:'" + startMenuData.Name + "'");
            }

            fileChanges.Remove(fileChange);
            registryChanges.Remove(registryChange);
            return CheckResult.Succeeded(startMenuData);
        }

        private static string GetAllUsersMenuFolder()
        {
            var path = new StringBuilder(260);
            var retval = ShfolderMethods.SHGetFolderPath(IntPtr.Zero, 0x16, IntPtr.Zero, 0, path);
            return retval == 0 ? path.ToString() : Environment.GetFolderPath(Environment.SpecialFolder.StartMenu);
        }
    }
}