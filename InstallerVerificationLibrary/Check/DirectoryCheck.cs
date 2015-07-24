namespace InstallerVerificationLibrary.Check
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using InstallerVerificationLibrary.Attribute;
    using InstallerVerificationLibrary.Data;

    [TestCheck]
    [DataType("Directory")]
    public sealed class DirectoryCheck : BaseCheck
    {
        public override CheckResult DoCheck(BaseTestData data, ICollection<FileChange> fileChanges, ICollection<Registryhange> registryChanges)
        {
            if (!VerifyIfCorrectTestData(data))
            {
                return CheckResult.NotCheckDone();
            }

            var dirData = data as DirectoryData;
            var fileChange = fileChanges.FirstOrDefault(x => x.Path == dirData.Path && x.IsDirectory);

            if (fileChange == null)
            {
                //If directory allready exist it may not be created 
                if (dirData.Exist && !Directory.Exists(dirData.Path))
                {
                    return CheckResult.Failure("Directory=" + dirData.Path + " is supose to exist");
                }

                if (!dirData.Exist && Directory.Exists(dirData.Path))
                {
                    return CheckResult.Failure("Directory=" + dirData.Path + " is not supose to exist");
                }

                return CheckResult.Succeeded(data);
            }

            if (fileChange.Type == FileChangeType.Delete && dirData.Exist)
            {
                fileChanges.Remove(fileChange);
                return CheckResult.Failure("Directory=" + dirData.Path + " is supose to exist");
            }

            if (fileChange.Type != FileChangeType.Delete && !dirData.Exist)
            {
                fileChanges.Remove(fileChange);
                return CheckResult.Failure("Directory=" + dirData.Path + " is not supose to exist");
            }

            fileChanges.Remove(fileChange);
            return CheckResult.Succeeded(data);
        }
    }
}