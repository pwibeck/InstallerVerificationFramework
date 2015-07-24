namespace InstallerVerificationLibrary
{
    using System;
    using System.IO;
    using System.Linq;
    using InstallerVerificationLibrary.Logging;
    using InstallerVerificationLibrary.Tools;
    using InstallerVerificationLibrary.Verifiers;

    public class MsiTestBed
    {
        public Verifier Verifier { get; private set; }

        public MsiTestBed(SetupConfigBaseMsi setupConfig)
        {
            if (!File.Exists(setupConfig.FilePathToMsiFile))
            {
                throw new FileNotFoundException(setupConfig.FilePathToMsiFile);
            }

            if (!File.Exists(setupConfig.FilePathToTestData))
            {
                throw new FileNotFoundException(setupConfig.FilePathToTestData);
            }

            Verifier = new Verifier(setupConfig.FilePathToTestData, setupConfig);
        }

        public bool Execute()
        {
            var procmonController = new ProcmonController();
            procmonController.Start();

            switch (((SetupConfigBaseMsi)Verifier.SetupConfiguration).TypeOfInstallation)
            {
                case TypeOfInstallation.Install:
                    MsiInstaller.InstallMSI(Verifier.SetupConfiguration, ((SetupConfigBaseMsi)Verifier.SetupConfiguration).FilePathToMsiFile, true, true);
                    break;
                case TypeOfInstallation.UnInstall:
                    MsiInstaller.UnInstallerMSI(((SetupConfigBaseMsi)Verifier.SetupConfiguration).FilePathToMsiFile);
                    break;
                case TypeOfInstallation.Repair:
                    MsiInstaller.RepairMSI(((SetupConfigBaseMsi)Verifier.SetupConfiguration).FilePathToMsiFile);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            procmonController.Stop();

            var analyzer = new ProcmonDataExtractor();
            analyzer.ExtractData(procmonController, ProcmonDataExtractor.TypeOfInstaller.Msi);

            Log.WriteInfo("FileChanges", "Execute");
            Log.Indent();
            foreach (var fileChange in analyzer.FileChanges)
            {
                Log.WriteInfo(fileChange.ToString(), "Execute");    
            }
            Log.Unindent();
            Log.WriteInfo("RegistryChanges", "Execute");
            Log.Indent();
            foreach (var registryChange in analyzer.registryChanges)
            {
                Log.WriteInfo(registryChange.ToString(), "Execute");
            }
            Log.Unindent();

            return Verifier.VerifyInstallation(analyzer.FileChanges.ToList(), analyzer.registryChanges.ToList());
        }      
    }
}