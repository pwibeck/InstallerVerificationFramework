namespace DemoConsoleApp
{
    using InstallerVerificationLibrary;

    public class SetupConfigInstaller2 : SetupConfigBaseMsi
    {
        private const string FeatureOneId = "Feature1";
        private const string InstallFolderParameterId = "INSTALLFOLDER";

        public SetupConfigInstaller2()
        {
            ParameterList.Add(new SetupConfigParameterData { Id = InstallFolderParameterId, Value = @"[ProgramFilesFolder]InstallationDirectory2" });
            ComponentList.Add(new SetupConfigComponentData { Id = FeatureOneId, Installed = true });

            Msiid = "Installer2";
            FilePathToMsiFile = @"Installers\Installer2.msi";
            FilePathToTestData = @"TestData\Installer2.xml";
        }

        #region Features
        public bool FeatureOne
        {
            get { return GetComponent(FeatureOneId).Installed; }
            set { GetComponent(FeatureOneId).Installed = value; }
        }
        #endregion

        #region Parameters
        public string InstallFolderParameter
        {
            get { return GetParameter(InstallFolderParameterId).Value; }
            set { GetParameter(InstallFolderParameterId).Value = value; }
        }
        #endregion
    }
}
