namespace DemoConsoleApp
{
    using System;
    using InstallerVerificationLibrary;
    using InstallerVerificationLibrary.Logging;
    using InstallerVerificationLibrary.Tools;

    class Program
    {
        static void Main()
        {
            var logListener = new ConsoleLogListener(LogErrorType.Information, false);
            
            new TestCleaner().CleanMachine();
            logListener.StartLogging();

            InstallAll();
            Console.WriteLine();
            UninstallAll();
            Console.WriteLine();
            RepairFile();
            Console.WriteLine();
            Installer2_InstallAll_Failure();
            Console.WriteLine();
            Installer2_UnInstallAll_Failure();
        }

        private static void InstallAll()
        {
            WriteToConsoleWithColor(ConsoleColor.Yellow, "TEST: Install ALL");
            WriteToConsoleWithColor(ConsoleColor.Yellow, "This test verifies that all features have been insalled as expected");
            Console.WriteLine("Press enter to start test");
            Console.ReadLine();

            var config = new SetupConfigInstaller1
            {
                TypeOfInstallation = TypeOfInstallation.Install
            };

            RunTest(config);
        }

        private static void UninstallAll()
        {
            WriteToConsoleWithColor(ConsoleColor.Yellow, "TEST: UnInstall ALL");
            WriteToConsoleWithColor(ConsoleColor.Yellow, "This test verifies that all feature is removed from the machine as expected");
            Console.WriteLine("Press enter to start test");
            Console.ReadLine();

            var config = new SetupConfigInstaller1
            {
                TypeOfInstallation = TypeOfInstallation.UnInstall,
                FeatureOne = true,
                FeatureTwo = true
            };

            var testBed = new MsiTestBed(config);
            MsiInstaller.InstallMSI(testBed.Verifier.SetupConfiguration,
                ((SetupConfigBaseMsi) testBed.Verifier.SetupConfiguration).FilePathToMsiFile, true, true);

            var result = testBed.Execute();
            WriteResult(result);

            new TestCleaner().CleanMachine();
        }

        private static void RepairFile()
        {
            WriteToConsoleWithColor(ConsoleColor.Yellow, "TEST: Repair File");
            WriteToConsoleWithColor(ConsoleColor.Yellow, "This test will delete an installed file and verify that the installer does repair the file");
            Console.WriteLine("Press enter to start test");
            Console.ReadLine();

            var config = new SetupConfigInstaller1
            {
                TypeOfInstallation = TypeOfInstallation.Repair,
                FeatureOne = true,
                FeatureTwo = true
            };

            var testBed = new MsiTestBed(config);
            MsiInstaller.InstallMSI(testBed.Verifier.SetupConfiguration,
                ((SetupConfigBaseMsi)testBed.Verifier.SetupConfiguration).FilePathToMsiFile, true, true);
            
            WriteToConsoleWithColor(ConsoleColor.Cyan, "Deleting file");
            FileSystemTool.RemoveFile(config.InstallFolderParameter + @"\Payload1.txt");

            var result = testBed.Execute();
            WriteResult(result);

            new TestCleaner().CleanMachine();
        }

        private static void Installer2_InstallAll_Failure()
        {
            WriteToConsoleWithColor(ConsoleColor.Yellow, "TEST: Installer2 install ALL");
            WriteToConsoleWithColor(ConsoleColor.Yellow, "This test will fail since not all files and registry keys are installed as expected");
            Console.WriteLine("Press enter to start test");
            Console.ReadLine();

            var config = new SetupConfigInstaller2
            {
                TypeOfInstallation = TypeOfInstallation.Install
            };

            RunTest(config);
        }

        private static void Installer2_UnInstallAll_Failure()
        {
            WriteToConsoleWithColor(ConsoleColor.Yellow, "TEST: Installer2 UnInstall ALL");
            WriteToConsoleWithColor(ConsoleColor.Yellow, "This test will fail since not all files and registry keys are UnInstalled as expected");
            Console.WriteLine("Press enter to start test");
            Console.ReadLine();

            var config = new SetupConfigInstaller2
            {
                TypeOfInstallation = TypeOfInstallation.UnInstall,
                FilePathToTestData = @"TestData\Installer2_UnInstall.xml"
            };

            var testBed = new MsiTestBed(config);
            MsiInstaller.InstallMSI(testBed.Verifier.SetupConfiguration,
                ((SetupConfigBaseMsi)testBed.Verifier.SetupConfiguration).FilePathToMsiFile, true, true);

            var result = testBed.Execute();
            WriteResult(result);

            new TestCleaner().CleanMachine();
        }

        private static void RunTest(SetupConfigBaseMsi config)
        {
            var testBed = new MsiTestBed(config);
            var result = testBed.Execute();
            WriteResult(result);

            new TestCleaner().CleanMachine();
        }

        private static void WriteResult(bool result)
        {
            if (result)
            {
                WriteToConsoleWithColor(ConsoleColor.Green, "TEST passed");
            }
            else
            {
                WriteToConsoleWithColor(ConsoleColor.Red, "TEST failed");
            }
        }

        private static void WriteToConsoleWithColor(ConsoleColor color, string txt)
        {
            var colorTmp = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(txt);
            Console.ForegroundColor = colorTmp;
        }
    }
}
