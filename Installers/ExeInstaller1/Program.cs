using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.Win32;

namespace ExeInstaller1
{
    internal class Program
    {
        private static string installDir;
        private static string filePath1;
        private static string filePath2;
        private static string regKeyPath;
        private static void Main(string[] args)
        {
            installDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "ExeInstaller1");
            filePath1 = Path.Combine(installDir, "File1.txt");
            filePath2 = Path.Combine(installDir, "File2.txt");
            regKeyPath = @"Software\ExecInstaller1";

            if (args.Count() != 1)
            {
                throw new Exception("No valid argument");
            }

            if (args[0] == "Install")
            {
                Install();
            }
            else if(args[0] == "UnInstall")
            {
                UnInstall();
            }
            else if (args[0] == "Repair")
            {
                UnInstall();
                Install();
            }
            else
            {
                throw new Exception("No valid argument");
            }
        }

        private static void UnInstall()
        {
            File.Delete(filePath1);

            Registry.CurrentUser.DeleteSubKeyTree(regKeyPath);

            var externalProcess = new Process();
            externalProcess.StartInfo = new ProcessStartInfo
            {
                WindowStyle = ProcessWindowStyle.Hidden,
                FileName = "cmd.exe",
                Arguments = "/c del /f /q \"" + filePath2 + "\""
            };
            externalProcess.Start();
            externalProcess.WaitForExit();

            Directory.Delete(installDir);
        }

        private static void Install()
        {
            Directory.CreateDirectory(installDir);

            File.Create(filePath1).Close();

            var key = Registry.CurrentUser.CreateSubKey(regKeyPath);
            key.SetValue("Name", "Isabella");
            key.Close();

            var externalProcess = new Process();
            externalProcess.StartInfo = new ProcessStartInfo
            {
                WindowStyle = ProcessWindowStyle.Hidden,
                FileName = "cmd.exe",
                Arguments = "/c echo \"hi there\" > \"" + filePath2 + "\""
            };
            externalProcess.Start();
            externalProcess.WaitForExit();
        }
    }
}