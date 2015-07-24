namespace InstallerVerificationLibrary.Check
{
    using System.Collections.Generic;
    using System.Linq;
    using InstallerVerificationLibrary.Attribute;
    using InstallerVerificationLibrary.Data;
    using InstallerVerificationLibrary.Tools;

    [TestCheck]
    [DataType("RegistryKey")]
    public sealed class RegistryKeyCheck : BaseCheck
    {
        public override CheckResult DoCheck(BaseTestData data, ICollection<FileChange> fileChanges, ICollection<Registryhange> registryChanges)
        {
            if (!VerifyIfCorrectTestData(data))
            {
                return CheckResult.NotCheckDone();
            }

            var regData = data as RegistryKeyData;
            var registryChange =
                registryChanges.FirstOrDefault(x => x.Key == regData.Key &&
                                                   (x.Type == RegistryChangeType.DeleteKey ||
                                                    x.Type == RegistryChangeType.CreateKey));
            if (registryChange == null)
            {
                if (regData.Exist && !RegistryTool.RegistryKeyExist(regData.Key))
                {
                    return CheckResult.Failure("Registry key:'" + regData.Key + "' is supose to exist");
                }

                if (!regData.Exist && RegistryTool.RegistryKeyExist(regData.Key))
                {
                    return CheckResult.Failure("Registry key:'" + regData.Key + "' is not supose to exist");
                }

                return CheckResult.Succeeded(data);
            }

            if (registryChange.Type == RegistryChangeType.DeleteKey && regData.Exist)
            {
                registryChanges.Remove(registryChange);
                return CheckResult.Failure("Registry key:'" + regData.Key + "' is supose to exist");
            }

            if (registryChange.Type == RegistryChangeType.CreateKey && !regData.Exist)
            {
                registryChanges.Remove(registryChange);
                return CheckResult.Failure("Registry key:'" + regData.Key + "' is NOT supose to exist");
            }

            registryChanges.Remove(registryChange);
            return CheckResult.Succeeded(data);
        }
    }
}