namespace InstallerVerificationLibrary.Check
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    public interface ICheck
    {
        CheckResult Check(Collection<Data.BaseTestData> data, ICollection<FileChange> fileChanges, ICollection<Registryhange> registryChanges);

        CheckResult Check(Data.BaseTestData data, ICollection<FileChange> fileChanges, ICollection<Registryhange> registryChanges);
        
        string Name { get; }
    }
}
