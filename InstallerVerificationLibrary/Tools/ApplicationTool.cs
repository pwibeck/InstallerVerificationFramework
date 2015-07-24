namespace InstallerVerificationLibrary.Tools
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IO;
    using InstallerVerificationLibrary.Logging;

    public static class ApplicationTool
    {
        public static void InstallMSI(string filePath, TimeSpan timeout, string parameters = null, string logFilePath = null)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                throw new ArgumentNullException("filePath");
            }

            if (!File.Exists(filePath))
            {
                throw new InstallerVerificationLibraryException("Could not find file '" + filePath + "'");
            }

            var fileInfo = new FileInfo(filePath);

            var argument = "/qn /i \"" + filePath + "\"";
            if (!string.IsNullOrEmpty(logFilePath))
            {
                argument += " /l*v \"" + logFilePath + "\"";
            }

            if (!string.IsNullOrEmpty(parameters))
            {
                argument += " " + parameters;
            }

            using (var setup = new Process())
            {
                setup.StartInfo.FileName = "Msiexec.exe";
                setup.StartInfo.Arguments = argument;
                RunProcessAndWaitForExit(setup, fileInfo.Name + " time out", timeout);
                Log.WriteInfo(fileInfo.Name + " installed", "Installer");
            }
        }

        public static void RepairMSI(string filePath, TimeSpan timeout, string logFilePath = null)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                throw new ArgumentNullException("filePath");
            }

            if (!File.Exists(filePath))
            {
                throw new InstallerVerificationLibraryException("Could not find file '" + filePath + "'");
            }

            var fileInfo = new FileInfo(filePath);

            var argument = "/qn /f \"" + filePath + "\"";
            if (!string.IsNullOrEmpty(logFilePath))
            {
                argument += " /l*v \"" + logFilePath + "\"";
            }

            using (var setup = new Process())
            {
                setup.StartInfo.FileName = "Msiexec.exe";
                setup.StartInfo.Arguments = argument;
                RunProcessAndWaitForExit(setup, fileInfo.Name + " time out", timeout);
                Log.WriteInfo(fileInfo.Name + " repaired", "RepairMSI");
            }
        }

        public static void RepairMSIByProductCode(string productCode, TimeSpan timeout)
        {
            if (string.IsNullOrEmpty(productCode))
            {
                throw new ArgumentNullException("productCode");
            }

            using (var setup = new Process())
            {
                setup.StartInfo.FileName = "Msiexec.exe";
                setup.StartInfo.Arguments = "/qn /f {" + productCode + "}";
                RunProcessAndWaitForExit(setup, productCode + " time out", timeout);
                Log.WriteInfo(productCode + " repaired", "RepairMSI");
            }
        }


        public static void UninstallMSI(string filePath, TimeSpan timeout, string logFilePath = null)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                throw new ArgumentNullException("filePath");
            }

            if (!File.Exists(filePath))
            {
                throw new InstallerVerificationLibraryException("Could not find file '" + filePath + "'");
            }

            var fileInfo = new FileInfo(filePath);

            var argument = "/qn /x \"" + filePath + "\"";
            if (!string.IsNullOrEmpty(logFilePath))
            {
                argument += " /l*v \"" + logFilePath + "\"";
            }

            using (var setup = new Process())
            {
                setup.StartInfo.FileName = "Msiexec.exe";
                setup.StartInfo.Arguments = argument;
                RunProcessAndWaitForExit(setup, fileInfo.Name + " time out", timeout);
                Log.WriteInfo(fileInfo.Name + " UnInstalled", "UnInstallMSI");
            }
        }

        public static void UninstallMSIByProductCode(string productCode, TimeSpan timeout)
        {
            if (string.IsNullOrEmpty(productCode))
            {
                throw new ArgumentNullException("productCode");
            }

            using (var setup = new Process())
            {
                setup.StartInfo.FileName = "Msiexec.exe";
                setup.StartInfo.Arguments = "/qn /x {" + productCode + "}";
                RunProcessAndWaitForExit(setup, productCode + " time out", timeout);
                Log.WriteInfo(productCode + " UnInstalled", "UnInstallMSI");
            }
        }

        public static void RunExe(string filePath, string argument, TimeSpan timeout)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                throw new ArgumentNullException("filePath");
            }

            if (argument == null)
            {
                throw new ArgumentNullException("argument");
            }

            if (!File.Exists(filePath))
            {
                throw new InstallerVerificationLibraryException("Could not find file '" + filePath + "'");
            }

            var fileInfo = new FileInfo(filePath);

            using (var setup = new Process())
            {
                setup.StartInfo.FileName = filePath;
                setup.StartInfo.Arguments = argument;
                RunProcessAndWaitForExit(setup, fileInfo.Name + " time out", timeout);
                setup.Refresh();
                Log.WriteInfo(fileInfo.Name + " executed with exit code:" + setup.ExitCode, "Installer");
            }
        }

        private static void RunProcessAndWaitForExit(Process proc, string errorMessageIfFail, TimeSpan timeout)
        {
            try
            {
                proc.Start();
                proc.WaitForExit((int) timeout.TotalMilliseconds);
                if (!proc.HasExited)
                {
                    proc.Kill();
                    throw new TimeoutException(errorMessageIfFail);
                }
            }
            catch (Win32Exception e)
            {
                if (e.Message.Contains("The system cannot find the file specified"))
                {
                    Log.WriteError("Could not find file " + proc.StartInfo.FileName, "ApplicationTool-> RunProcess");
                }
            }
        }
    }
}