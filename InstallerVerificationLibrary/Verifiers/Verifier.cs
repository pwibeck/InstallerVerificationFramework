namespace InstallerVerificationLibrary.Verifiers
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using InstallerTestingToolset.Verifiers;
    using InstallerVerificationLibrary.Data;
    using InstallerVerificationLibrary.Logging;

    public class Verifier
    {
        public SetupConfig SetupConfiguration { get; set; }
        public Collection<BaseTestData> TestData { get; set; }
        public Collection<PluginData> Plugins { get; set; }

        public Verifier(string filePathToConfigurationData, SetupConfig conf)
        {
            SetupConfiguration = conf;
            TestData = TestDataLoader.LoadTestDataFromFile(filePathToConfigurationData, SetupConfiguration);
            Plugins = TestDataLoader.LoadPluginsDataFromFile(filePathToConfigurationData);
            TestDataLoader.ResolveSetupConfigParameters(filePathToConfigurationData, conf);
        }

        public bool VerifyInstallation(ICollection<FileChange> fileChanges, ICollection<Registryhange> registryChanges)
        {
            var ok = true;
            var asm = new CheckerDataTypeLoader(Plugins);

            Log.WriteInfo("Checking " + TestData.Count + " number of items", "VerifyInstallation");
            Log.Indent();
            foreach (var check in asm.GetAllChecks())
            {
                try
                {
                    if (!check.Check(TestData, fileChanges, registryChanges).Success)
                    {
                        ok = false;
                    }
                }
                catch (Exception e)
                {
                    Log.WriteError(e, "An exception was thrown when executing check:'" + check.Name + "'");
                    ok = false;
                }                
            }

            Log.Unindent();

            if (fileChanges.Any() || registryChanges.Any())
            {
                Log.WriteError("Unexpected changes detected", "VerifyInstallation");
                ok = false;

            }

            Log.Indent();

            // Log all changes that have not been handled
            foreach (var fileChange in fileChanges)
            {
                Log.WriteError("Unexpected file change => " + fileChange, "VerifyInstallation");
            }

            foreach (var registryhange in registryChanges)
            {
                Log.WriteError("Unexpected registry change => " + registryhange, "VerifyInstallation");
            }

            Log.Unindent();

            return ok;
        }
    }
}