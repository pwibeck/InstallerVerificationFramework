namespace InstallerVerificationLibrary.Check
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using InstallerVerificationLibrary.Attribute;
    using InstallerVerificationLibrary.Data;

    [TestCheck]
    [DataType("File")]
    public sealed class FileCheck : BaseCheck
    {
        public override CheckResult DoCheck(BaseTestData data, ICollection<FileChange> fileChanges, ICollection<Registryhange> registryChanges)
        {
            if (!VerifyIfCorrectTestData(data))
            {
                return CheckResult.NotCheckDone();
            }

            var fileData = data as FileData;
            var fileChange = fileChanges.FirstOrDefault(x => x.Path == fileData.Path && x.IsDirectory == false);

            if (fileChange == null)
            {
                if (fileData.Exist)
                {
                    return CheckResult.Failure("File=" + fileData.Path + " is supose to exist");
                }

                if (File.Exists(fileData.Path))
                {
                    return CheckResult.Failure("File=" + fileData.Path + " is not supose to exist");
                }

                return CheckResult.Succeeded(data);
            }

            if (fileChange.Type == FileChangeType.Delete && fileData.Exist)
            {
                fileChanges.Remove(fileChange);
                return CheckResult.Failure("File=" + fileData.Path + " is supose to exist");
            }

            if (fileChange.Type != FileChangeType.Delete && !fileData.Exist)
            {
                fileChanges.Remove(fileChange);
                return CheckResult.Failure("File=" + fileData.Path + " is not supose to exist");
            }

            // Check file version
            var info = new FileInfo(fileData.Path);
            if (!string.IsNullOrEmpty(fileData.FileVersion) && info.Exists)
            {
                var fileVersion = FileVersionInfo.GetVersionInfo(info.FullName);
                if (string.Compare(fileData.FileVersion, fileVersion.FileVersion, StringComparison.OrdinalIgnoreCase) != 0)
                {
                    fileChanges.Remove(fileChange);
                    return CheckResult.Failure("File=" + fileData.Path + " does have wrong file version. Found version:'" +
                                    fileVersion.FileVersion + "' expected version:'" + fileData.FileVersion + "'");
                }
            }

            fileChanges.Remove(fileChange);
            return CheckResult.Succeeded(data);
        }
    }
}