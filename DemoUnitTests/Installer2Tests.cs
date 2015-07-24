namespace Tests
{
    using InstallerVerificationLibrary;
    using InstallerVerificationLibrary.Logging;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class Installer2Tests
    {
        private static DebugLogListener logListener;
        [ClassInitialize]
        public static void MyClassInitialize(TestContext testContext)
        {
            if (logListener == null)
            {
                logListener = new DebugLogListener();
            }
        }

        [TestInitialize()]
        public void MyTestInitialize()
        {
            logListener.StartLogging();
            // Create a cleaner object and ensure that no msi is installed before run starts
            TestCleaner cleaner = new TestCleaner();
            cleaner.CleanMachine();
        }

        [TestCleanup()]
        public void MyTestCleanup()
        {
            // Create a cleaner object and ensure that no msi is installed after run done
            TestCleaner cleaner = new TestCleaner();
            cleaner.CleanMachine();
            logListener.StopLogging();
        }

        [TestMethod]
        [DeploymentItem(@"C:\Users\Peter\Source\Workspaces\Hobby\Thesis\Installers\Installer2\bin\Debug\Installer2.msi","Installers")]
        [DeploymentItem(@"TestData\", "TestData")]
        [DeploymentItem(@"Procmon.exe")]
        public void Installer2_Install()
        {
            var config = new SetupConfigInstaller2
            {
                TypeOfInstallation = TypeOfInstallation.Install
            };

            var testBed = new MsiTestBed(config);
            Assert.IsTrue(testBed.Execute());
        }
    }

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
