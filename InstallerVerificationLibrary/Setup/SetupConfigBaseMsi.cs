namespace InstallerVerificationLibrary
{
    public class SetupConfigBaseMsi : SetupConfig
    {
        public string Msiid { get; set; }
                
        public string FilePathToMsiFile { get; set; }

        public string FilePathToTestData { get; set; }
    }
}
