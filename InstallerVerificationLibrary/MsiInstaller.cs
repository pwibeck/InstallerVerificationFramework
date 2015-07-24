namespace InstallerVerificationLibrary
{
    using System;
    using System.IO;
    using System.Linq;
    using InstallerVerificationLibrary.Tools;

    public static class MsiInstaller
    {
        private const int timeoutMinutes = 10;

        public static string InstallMSI(SetupConfig setupConfig, string msiFilePath, bool useAddLocal, bool useParameters)            
        {
            if (setupConfig == null)
            {
                throw new ArgumentNullException("setupConfig");
            }

            var parameter = string.Empty;
            if (useAddLocal)
            {
                parameter += "ADDLOCAL=";
                parameter = setupConfig.ComponentList.Where(feature => feature.Installed).Aggregate(parameter, (current, feature) => current + (feature.Id + ","));
                parameter = parameter.TrimEnd(',');
            }

            if (useParameters)
            {
                parameter = setupConfig.ParameterList.Aggregate(parameter, (current, parm) => current + (" " + parm.Id + "=\"" + parm.Value + "\""));
            }

            parameter = parameter.TrimStart(' ');

            var logFile = Path.GetTempFileName();
            ApplicationTool.InstallMSI(msiFilePath, new TimeSpan(0, timeoutMinutes, 0), parameter, logFile);
            return logFile;
        }
        
        public static string UnInstallerMSI(string msiFilePath)
        {
            var logFile = Path.GetTempFileName();
            ApplicationTool.UninstallMSI(msiFilePath, new TimeSpan(0, timeoutMinutes, 0), logFile);
            return logFile;
        }

        public static string RepairMSI(string msiFilePath)
        {
            var logFile = Path.GetTempFileName();
            ApplicationTool.RepairMSI(msiFilePath, new TimeSpan(0, timeoutMinutes, 0), logFile);
            return logFile;
        }
    }
}
