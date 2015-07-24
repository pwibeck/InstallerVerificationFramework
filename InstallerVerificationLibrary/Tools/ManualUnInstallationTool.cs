namespace InstallerVerificationLibrary
{
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Diagnostics;
    using InstallerVerificationLibrary.Logging;
    using InstallerVerificationLibrary.Tools;

    public class ManualUnInstallerTool
    {
        private readonly TimeSpan processTimeOut = new TimeSpan(0, 30, 0);

        public Collection<string> Databases { get; set; }
        public Collection<string> Directories { get; set; }
        public Collection<string> Processes { get; set; }
        public Collection<string> ProductCodes { get; set; }
        public Collection<string> ServicesNames { get; set; }
        public Collection<string> RegistryKeys { get; set; }

        public ManualUnInstallerTool()
        {
            Databases = new Collection<string>();
            Directories = new Collection<string>();
            Processes = new Collection<string>();
            ProductCodes = new Collection<string>();
            ServicesNames = new Collection<string>();
            RegistryKeys = new Collection<string>();
        }

        public void Execute()
        {
            Log.WriteInfo("Cleaning machine started", "Cleaner");
            KillProcesses();
            UninstallProducts();
            KillProcesses();
            RemoveServices();
            RemoveRegistryKeys();
            RemoveDatabases();
            RemoveDirectories();
            Log.WriteInfo("Cleaning machine done", "Cleaner");
        }

        private void UninstallProducts()
        {
            foreach (var productCode in ProductCodes)
            {
                ApplicationTool.UninstallMSIByProductCode(productCode, processTimeOut);
            }
        }
        
        private void RemoveDatabases()
        {
            foreach (var databaseName in Databases)
            {
                DatabaseTool.DropDatabase(databaseName);
            }
        }
        
        private void RemoveDirectories()
        {
            foreach (var path in Directories)
            {
                FileSystemTool.RemoveDirectory(path);
            }
        }

        private void KillProcesses()
        {
            foreach (var proc in Processes)
            {
                foreach (var proces in Process.GetProcessesByName(proc))
                {
                    Log.WriteInfo("Killing:" + proces.ProcessName, "KillProcesses");
                    try
                    {
                        proces.Kill();
                        proces.Dispose();
                    }
                    catch (Win32Exception e)
                    {
                        Log.WriteError(
                            "Did catch an exception when trying to kill: '" + proc + "' Exception message: " + e.Message,
                            "ManualUnInstaller->KillProcesses");
                    }
                    catch (InvalidOperationException e)
                    {
                        if (!e.Message.Contains(" has exited"))
                        {
                            Log.WriteError(
                                "Did catch an exception when trying to kill: '" + proc + "' Exception message: " + e.Message,
                                "ManualUnInstaller->KillProcesses");
                        }
                    }
                }
            }
        }

        private void RemoveRegistryKeys()
        {
            foreach (var registryKey in RegistryKeys)
            {
                RegistryTool.RemoveRegistryKey(registryKey);
            }
        }

        private void RemoveServices()
        {
            WindowsServiceTool.RemoveServices(ServicesNames);
        }
    }
}