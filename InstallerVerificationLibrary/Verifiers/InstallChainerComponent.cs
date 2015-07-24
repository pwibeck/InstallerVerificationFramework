namespace InstallerVerificationLibrary
{
    using System.Collections.ObjectModel;
    using InstallerVerificationLibrary.Data;

    public class InstallChainerComponent
    {
        public string Id { get; set; }

        public string InstallationDir { get; set; }
        
        public Collection<BaseTestData> TestData { get; set; }

        public override string ToString()
        {
            return "ID:" + Id + ", Installation directory:" + InstallationDir;
        }
    }
}