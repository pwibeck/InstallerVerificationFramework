namespace InstallerVerificationLibrary.Check
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using InstallerVerificationLibrary.Attribute;
    using InstallerVerificationLibrary.Data;
    using InstallerVerificationLibrary.Tools;

    using NativeWrappers;

    [TestCheck]
    [DataType("RegistryValue")]
    public sealed class RegistryValueCheck : BaseCheck
    {
        public override CheckResult DoCheck(BaseTestData data, ICollection<FileChange> fileChanges, ICollection<Registryhange> registryChanges)
        {
            if (!VerifyIfCorrectTestData(data))
            {
                return CheckResult.NotCheckDone();
            }

            var regData = data as RegistryValueData;
            UpdateRegDataIfValueIsShortPath(regData);

            var registryChange =
                registryChanges.FirstOrDefault(x => x.Key == regData.Key && 
                                                   x.ValueName == regData.Name &&
                                                   (x.Type == RegistryChangeType.SetValue || x.Type == RegistryChangeType.DeleteValue));
            if (registryChange == null)
            {
                // In repair scenario MSI will not do any registry change if the correct value is allready there
                // ValueName = (Default) is the root key and doesn't have any name
                var value = RegistryTool.RegistryValue(regData.Key, regData.Name == "(Default)" ? "" : regData.Name, null);
                if (regData.Exist && (value == null || value.ToString() != regData.Data))
                {
                    return CheckResult.Failure("Registry key:'" + regData.Key + "' valueName: " + regData.Name + " is supose to exist");
                }

                if (!regData.Exist && value != null)
                {
                    return CheckResult.Failure("Registry key:'" + regData.Key + "' valueName: " + regData.Name + " is not supose to exist");
                }

                return CheckResult.Succeeded(data);
            }

            if (registryChange.Type == RegistryChangeType.DeleteValue && regData.Exist)
            {
                registryChanges.Remove(registryChange);
                return CheckResult.Failure("Registry key:'" + regData.Key + "' valueName: " + regData.Name + " is supose to exist");
            }

            if (registryChange.Type == RegistryChangeType.SetValue && !regData.Exist)
            {
                registryChanges.Remove(registryChange);
                return CheckResult.Failure("Registry key:'" + regData.Key + "' valueName: " + regData.Name + "' is NOT supose to exist");
            }

            if (registryChange.Type == RegistryChangeType.SetValue)
            {
                switch (regData.DataCmpMethod)
                {
                    case RegistryValueData.DataComparisonMethod.Equal:
                        if (string.Compare(registryChange.Value, regData.Data, StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            registryChanges.Remove(registryChange);
                            return CheckResult.Succeeded(data);
                        }
                        break;
                    case RegistryValueData.DataComparisonMethod.Contains:
                        if (registryChange.Value.ToLowerInvariant().Contains(regData.Data.ToLowerInvariant()))
                        {
                            registryChanges.Remove(registryChange);
                            return CheckResult.Succeeded(data);
                        }
                        break;
                }
            }
            else
            {
                registryChanges.Remove(registryChange);
                return CheckResult.Succeeded(data);
            }
            
            registryChanges.Remove(registryChange);
            return CheckResult.Failure("Registry Key:'" + regData.Key + "' with valueName='" + regData.Name + "' should be '" + regData.Data + "' but it is '" + registryChange.Value + "'");
        }

        private static void UpdateRegDataIfValueIsShortPath(RegistryValueData regData)
        {
            // If the value is short file path
            if (regData.Data.StartsWith("[!]", StringComparison.OrdinalIgnoreCase))
            {
                regData.Data = regData.Data.Replace("[!]", string.Empty);
                var shortPath = new StringBuilder(255);
                var replaceValue = regData.Data.Split(',');
                if (Kernel32Methods.GetShortPathName(replaceValue[0], shortPath, shortPath.Capacity) != 0)
                {
                    throw new InstallerVerificationLibraryException("Could not get short name for " + regData.Data);
                }

                regData.Data = regData.Data.Replace(replaceValue[0], shortPath.ToString());
            }
        }
    }
}