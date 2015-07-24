namespace Tests
{
    using System;
    using InstallerVerificationLibrary;
    using InstallerVerificationLibrary.Tools;

    public class TestCleaner
    {
        private readonly ManualUnInstallerTool cleaner = new ManualUnInstallerTool();
        public TestCleaner()
        {
            cleaner.ProductCodes.Add("62AA8B8D-CDC3-41EE-8C80-CB832AF5F0B0"); // Installer1
            cleaner.ProductCodes.Add("4AF91E3F-4696-4BC2-A60B-43D0CED88E0B"); // Installer2

            cleaner.Directories.Add(@"C:\Program Files (x86)\InstallationDirectory1");
            cleaner.Directories.Add(@"C:\Program Files (x86)\InstallationDirectory2");

            cleaner.RegistryKeys.Add(@"HKCU\Software\Installer2");
        }

        public void CleanMachine()
        {
            cleaner.Execute();
        }
    }
}
