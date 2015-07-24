namespace Tests
{
    using InstallerVerificationLibrary;
    using InstallerVerificationLibrary.Logging;
    using InstallerVerificationLibrary.Tools;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class Installer1Tests
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
        [DeploymentItem(@"C:\tmp\Workspace\Thesis\Installers\Installer1\bin\Debug\Installer1.msi", "Installers")]
        [DeploymentItem(@"TestData\", "TestData")]
        [DeploymentItem(@"Procmon.exe")]
        public void Installer1_Install_DefaultFeature()
        {
            var config = new SetupConfigInstaller1
            {
                TypeOfInstallation = TypeOfInstallation.Install,
                FeatureOne = true,
                FeatureTwo = false
            };

            var testBed = new MsiTestBed(config);
            Assert.IsTrue(testBed.Execute());
        }

        [TestMethod]
        [DeploymentItem(@"C:\Users\Peter\Source\Workspaces\Hobby\Thesis\Installers\Installer1\bin\Debug\Installer1.msi", "Installers")]
        [DeploymentItem(@"TestData\", "TestData")]
        [DeploymentItem(@"Procmon.exe")]
        public void Installer1_Install_ClientFeature()
        {
            var config = new SetupConfigInstaller1
            {
                TypeOfInstallation = TypeOfInstallation.Install,
                FeatureOne = false,
                FeatureTwo = true
            };

            var testBed = new MsiTestBed(config);
            Assert.IsTrue(testBed.Execute());
        }

        [TestMethod]
        [DeploymentItem(@"C:\Users\Peter\Source\Workspaces\Hobby\Thesis\Installers\Installer1\bin\Debug\Installer1.msi", "Installers")]
        [DeploymentItem(@"TestData\", "TestData")]
        [DeploymentItem(@"Procmon.exe")]
        public void Installer1_Install_ALLFeatures()
        {
            var config = new SetupConfigInstaller1
            {
                TypeOfInstallation = TypeOfInstallation.Install,
                FeatureOne = true,
                FeatureTwo = true
            };

            var testBed = new MsiTestBed(config);
            Assert.IsTrue(testBed.Execute());
        }

        [TestMethod]
        [DeploymentItem(@"C:\Users\Peter\Source\Workspaces\Hobby\Thesis\Installers\Installer1\bin\Debug\Installer1.msi", "Installers")]
        [DeploymentItem(@"TestData\", "TestData")]
        [DeploymentItem(@"Procmon.exe")]
        public void Installer1_UnInstall()
        {
            var config = new SetupConfigInstaller1
            {
                TypeOfInstallation = TypeOfInstallation.UnInstall
            };

            MsiInstaller.InstallMSI(config, config.FilePathToMsiFile, true, true);

            var testBed = new MsiTestBed(config);
            Assert.IsTrue(testBed.Execute());
        }

        [TestMethod]
        [DeploymentItem(@"C:\Users\Peter\Source\Workspaces\Hobby\Thesis\Installers\Installer1\bin\Debug\Installer1.msi", "Installers")]
        [DeploymentItem(@"TestData\", "TestData")]
        [DeploymentItem(@"Procmon.exe")]
        public void Installer1_RepairNone()
        {
            var config = new SetupConfigInstaller1
            {
                TypeOfInstallation = TypeOfInstallation.Repair
            };

            var testBed = new MsiTestBed(config);
            MsiInstaller.InstallMSI(testBed.Verifier.SetupConfiguration, ((SetupConfigBaseMsi)testBed.Verifier.SetupConfiguration).FilePathToMsiFile, true, true);
            Assert.IsTrue(testBed.Execute());
        }

        [TestMethod]
        [DeploymentItem(@"C:\Users\Peter\Source\Workspaces\Hobby\Thesis\Installers\Installer1\bin\Debug\Installer1.msi", "Installers")]
        [DeploymentItem(@"TestData\", "TestData")]
        [DeploymentItem(@"Procmon.exe")]
        public void Installer1_RepairFile()
        {
            var config = new SetupConfigInstaller1
            {
                TypeOfInstallation = TypeOfInstallation.Repair
            };

            var testBed = new MsiTestBed(config);
            MsiInstaller.InstallMSI(testBed.Verifier.SetupConfiguration, ((SetupConfigBaseMsi)testBed.Verifier.SetupConfiguration).FilePathToMsiFile, true, true);
            FileSystemTool.RemoveFile(config.InstallFolderParameter + @"\Payload1.txt");
            Assert.IsTrue(testBed.Execute());
        }
    }

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

            FilePathToMsiFile = @"Installers\Installer1.msi";
            FilePathToTestData = @"TestData\Installer1.xml";

            Msiid = "Installer1";
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
