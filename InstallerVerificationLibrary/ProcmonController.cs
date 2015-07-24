namespace InstallerVerificationLibrary
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Threading;

    public class ProcmonController
    {
        private string backingfile = Path.Combine(Environment.CurrentDirectory, "trace.pml");
        private string xmlLogFile = Path.Combine(Environment.CurrentDirectory, "trace.xml");

        public void Start()
        {
            if (File.Exists(backingfile))
            {
                File.Delete(backingfile);
            }

            var processStart = new Process();
            processStart.StartInfo = new ProcessStartInfo
            {
                WindowStyle = ProcessWindowStyle.Hidden,
                FileName = "cmd.exe",
                Arguments = "/c procmon.exe /quiet /minimized /AcceptEula /backingfile \"" + backingfile + "\""
            };

            processStart.Start();
            // TODO find a better way to determince procmon is runing ok
            Thread.Sleep(1000);
        }

        public void Stop()
        {
            var processStop = new Process();
            processStop.StartInfo = new ProcessStartInfo
            {
                WindowStyle = ProcessWindowStyle.Hidden,
                FileName = "cmd.exe",
                Arguments = "/c procmon.exe /terminate"
            };

            processStop.Start();
            processStop.WaitForExit();
        }

        public string GetLogFile()
        {
            if (File.Exists(xmlLogFile))
            {
                File.Delete(xmlLogFile);
            }

            var processSaveAs = new Process();
            processSaveAs.StartInfo = new ProcessStartInfo
            {
                WindowStyle = ProcessWindowStyle.Hidden,
                FileName = "cmd.exe",
                Arguments = string.Format("/c procmon.exe /openLog \"{0}\" /SaveApplyFilter /SaveAs \"{1}\"", backingfile, xmlLogFile)
            };

            processSaveAs.Start();
            processSaveAs.WaitForExit();
            return xmlLogFile;
        }
    }
}
