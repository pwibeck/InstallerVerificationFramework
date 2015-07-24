namespace DemoConsoleApp
{
    using InstallerVerificationLibrary;

    public class SetupConfigInstaller1 : SetupConfigBaseMsi
    {
        private const string FeatureOneId = "Feature1";
        private const string FeatureTwoId = "Feature2";
        private const string InstallFolderParameterId = "INSTALLFOLDER";

        public SetupConfigInstaller1()
        {
            ParameterList.Add(new SetupConfigParameterData { Id = InstallFolderParameterId, Value = @"[ProgramFilesFolder]InstallationDirectory1" });
            ComponentList.Add(new SetupConfigComponentData { Id = FeatureOneId, Installed = true });
            ComponentList.Add(new SetupConfigComponentData { Id = FeatureTwoId, Installed = true });

            Msiid = "Installer1";

            FilePathToMsiFile = @"Installers\Installer1.msi";
            FilePathToTestData = @"TestData\Installer1.xml";
        }

        #region Features
        public bool FeatureOne
        {
            get { return GetComponent(FeatureOneId).Installed; }
            set { GetComponent(FeatureOneId).Installed = value; }
        }
        public bool FeatureTwo
        {
            get { return GetComponent(FeatureTwoId).Installed; }
            set { GetComponent(FeatureTwoId).Installed = value; }
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
