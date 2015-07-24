namespace InstallerVerificationLibrary.Check
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using InstallerVerificationLibrary.Data;
    using InstallerVerificationLibrary.Logging;

    public abstract class BaseCheck : ICheck
    {
        private string checkName = string.Empty;

        public string Name 
        {
            get
            {
                if (string.IsNullOrEmpty(checkName))
                {
                    checkName = CheckerDataTypeLoader.FindDataType(GetType());
                }

                return checkName;
            }
        }

        public CheckResult Check(BaseTestData data, ICollection<FileChange> fileChanges, ICollection<Registryhange> registryChanges)
        {
            if (!VerifyIfCorrectTestData(data))
            {
                return CheckResult.NotCheckDone();
            }
            
            var result = DoCheck(data, fileChanges, registryChanges);

            if (!result.Success)
            {
                Log.WriteCheckError(result.ErrorMessage, Name, data.ComponentID);
            }
            else
            {
                Log.WriteCheckInfo(result.InfomrationMessage, Name, data.ComponentID);
            }

            return result;
        }

        public abstract CheckResult DoCheck(BaseTestData data, ICollection<FileChange> fileChanges, ICollection<Registryhange> registryChanges);


        public CheckResult Check(Collection<BaseTestData> dataCollection, ICollection<FileChange> fileChanges, ICollection<Registryhange> registryChanges)
        {
            var ok = true;
            foreach (var data in dataCollection)
            {
                if(!Check(data, fileChanges, registryChanges).Success)
                {
                    ok = false;
                }
            }

            return new CheckResult { Success = ok };
        }

        protected bool VerifyIfCorrectTestData(BaseTestData data)
        {
            return string.Compare(CheckerDataTypeLoader.FindDataType(data.GetType()), Name, StringComparison.OrdinalIgnoreCase) == 0;
        }
    }
}