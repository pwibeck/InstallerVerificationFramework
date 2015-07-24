namespace InstallerVerificationLibrary
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Linq;
    using System.Xml.Linq;

    public class ProcmonDataExtractor
    {
        public IEnumerable<FileChange> FileChanges { get; private set; }
        public IEnumerable<Registryhange> registryChanges { get; private set; }

        public enum TypeOfInstaller
        {
            Msi,
            Exe
        }

        public void ExtractData(ProcmonController controller, TypeOfInstaller typeOfInstaller, string exeName = null)
        {
            var eventlist = GetEventlist(controller);
            ExtractData(typeOfInstaller, exeName, eventlist);
        }

        public void ExtractData(TypeOfInstaller typeOfInstaller, string exeName, List<Event> eventlist)
        {
            var allInterestingProcessIds = FindAllInterestingProcess(typeOfInstaller, exeName, eventlist);
            var allEventsPidFiltered =
                eventlist.Where(c => allInterestingProcessIds.Contains(c.ProcessId)).OrderBy(o => o.Time).ToList();

            FileChanges = ExtractFileChangeData(allEventsPidFiltered, typeOfInstaller);
            registryChanges = ExtractRegistryChangeData(allEventsPidFiltered, typeOfInstaller);
        }

        private static List<Event> GetEventlist(ProcmonController controller)
        {
            var xmlLogFile = controller.GetLogFile();
            return GetEventlist(xmlLogFile);
        }

        public static List<Event> GetEventlist(string xmlLogFile)
        {
            var rootElement = XElement.Load(xmlLogFile);
            var eventlist =
                rootElement.Descendants()
                    .First(c => c.Name == "eventlist")
                    .Descendants()
                    .Where(x => x.Name == "event")
                    .Select(s =>
                    {
                        var timeOfDay = GetSubeElementValue(s, "Time_of_Day");
                        var res = new Event
                        {
                            Detail = GetSubeElementValue(s, "Detail"),
                            Operation = GetSubeElementValue(s, "Operation"),
                            Path = GetSubeElementValue(s, "Path"),
                            ProcessName = GetSubeElementValue(s, "Process_Name"),
                            Time = CreateDateTimeFromEventTime(timeOfDay)
                        };

                        int processId;
                        if (int.TryParse(GetSubeElementValue(s, "PID"), out processId))
                        {
                            res.ProcessId = processId;
                        }

                        return res;
                    }).ToList();
            return eventlist;
        }

        private static DateTime CreateDateTimeFromEventTime(string timeOfDay)
        {
            if (string.IsNullOrWhiteSpace(timeOfDay))
            {
                throw new ArgumentNullException("timeOfDay");
            }

            var hour = int.Parse(timeOfDay.Substring(0, timeOfDay.IndexOf(":", StringComparison.Ordinal)));
            timeOfDay = timeOfDay.Substring(timeOfDay.IndexOf(":", StringComparison.Ordinal) + 1);
            var minute = int.Parse(timeOfDay.Substring(0, timeOfDay.IndexOf(":", StringComparison.Ordinal)));
            timeOfDay = timeOfDay.Substring(timeOfDay.IndexOf(":", StringComparison.Ordinal) + 1);
            var seconds = int.Parse(timeOfDay.Substring(0, timeOfDay.IndexOf(",", StringComparison.Ordinal)));
            var ticks = int.Parse(timeOfDay.Substring(timeOfDay.IndexOf(",", StringComparison.Ordinal) + 1));
            var time = new DateTime(2015, 4, 1, hour, minute, seconds).AddTicks(ticks);
            return time;
        }

        private static Collection<int> FindAllInterestingProcess(TypeOfInstaller typeOfInstaller, string exeName,
            List<Event> eventlist)
        {
            var allInterestingProcessIds = new Collection<int>();
            if (typeOfInstaller == TypeOfInstaller.Msi)
            {
                var firstMsiexecProcessEvent =
                    eventlist.First(c => c.Operation == "Process Start" && c.ProcessName == "msiexec.exe");
                allInterestingProcessIds.Add(firstMsiexecProcessEvent.ProcessId);
                FindAllChildProcesses(eventlist, firstMsiexecProcessEvent, allInterestingProcessIds);

                // Find all msiexce pids and ensure they are on ower watch list
                // this is due to some work could be doen by an already running msiexe.exe process
                foreach (var pid in eventlist
                    .Where(c => c.ProcessName == "msiexec.exe" && !allInterestingProcessIds.Contains(c.ProcessId))
                    .Select(c => c.ProcessId))
                {
                    allInterestingProcessIds.Add(pid);
                }
            }
            else if (typeOfInstaller == TypeOfInstaller.Exe && !string.IsNullOrWhiteSpace(exeName))
            {
                var firstProcessEvent = eventlist.First(c => c.Operation == "Process Start" && c.ProcessName == exeName);
                allInterestingProcessIds.Add(firstProcessEvent.ProcessId);
                FindAllChildProcesses(eventlist, firstProcessEvent, allInterestingProcessIds);
            }
            else
            {
                throw new Exception("Could not find procees to analyze");
            }
            return allInterestingProcessIds;
        }

        private static void FindAllChildProcesses(List<Event> eventlist, Event firstMsiexecProcessEvent,
            ICollection<int> allInterestingProcessIds)
        {
            var processToCheck = eventlist.Where(c =>
                c.Operation == "Process Start" &&
                c.Detail.Contains(string.Format("Parent PID: {0},", firstMsiexecProcessEvent.ProcessId))).ToList();

            if (!processToCheck.Any())
            {
                return;
            }

            foreach (var procees in processToCheck.Where(procees => !allInterestingProcessIds.Contains(procees.ProcessId)))
            {
                allInterestingProcessIds.Add(procees.ProcessId);
                FindAllChildProcesses(eventlist, procees, allInterestingProcessIds);
            }
        }

        private static IEnumerable<Registryhange> ExtractRegistryChangeData(List<Event> allEventsPidFiltered,
            TypeOfInstaller installerType)
        {
            var registryResult = new Collection<Registryhange>();
            foreach (var e in allEventsPidFiltered)
            {
                if (e.Operation == "RegCreateKey" && e.Detail.Contains("REG_CREATED_NEW_KEY"))
                {
                    // If registry have ben deleted before created then remove the delete event
                    if (registryResult.Any(c => c.Key == e.Path))
                    {
                        var res = registryResult.First(c => c.Key == e.Path);
                        if (res.Type == RegistryChangeType.DeleteKey)
                        {
                            registryResult.Remove(res);
                        }
                    }

                    registryResult.Add(new Registryhange
                    {
                        Key = e.Path,
                        Type = RegistryChangeType.CreateKey
                    });
                }

                if (e.Operation == "RegDeleteKey")
                {
                    // If registry have ben created before created then remove the create event
                    if (registryResult.Any(c => c.Key == e.Path))
                    {
                        var res = registryResult.First(c => c.Key == e.Path);
                        if (res.Type == RegistryChangeType.CreateKey)
                        {
                            registryResult.Remove(res);
                        }
                    }

                    registryResult.Add(new Registryhange
                    {
                        Key = e.Path,
                        Type = RegistryChangeType.DeleteKey
                    });
                }

                if (e.Operation == "RegSetValue" || e.Operation == "RegDeleteValue")
                {
                    var key = e.Path.Substring(0, e.Path.LastIndexOf("\\", StringComparison.Ordinal));
                    var valueName = e.Path.Substring(e.Path.LastIndexOf("\\", StringComparison.Ordinal) + 1);

                    if (e.Operation == "RegSetValue")
                    {
                        // If registry value have ben deleted before created then remove the delete event
                        if (registryResult.Any(c => c.Key == key && c.ValueName == valueName))
                        {
                            var res = registryResult.First(c => c.Key == key && c.ValueName == valueName);
                            if (res.Type == RegistryChangeType.DeleteValue)
                            {
                                registryResult.Remove(res);
                            }
                        }

                        registryResult.Add(new Registryhange
                        {
                            Key = key,
                            ValueName = valueName,
                            Type = RegistryChangeType.SetValue,
                            Value = e.Detail.Substring(e.Detail.IndexOf("Data:", StringComparison.Ordinal) + 6)
                        });
                    }

                    if (e.Operation == "RegDeleteValue")
                    {
                        // If registry value have ben created before created then remove the created event
                        if (registryResult.Any(c => c.Key == key && c.ValueName == valueName))
                        {
                            var res = registryResult.First(c => c.Key == key && c.ValueName == valueName);
                            if (res.Type == RegistryChangeType.CreateKey)
                            {
                                registryResult.Remove(res);
                            }
                        }

                        registryResult.Add(new Registryhange
                        {
                            Key = key,
                            ValueName = valueName,
                            Type = RegistryChangeType.DeleteValue
                        });
                    }
                }
            }

            var regKeysUsedByMsiexec = new Collection<string>
            {
                @"HKLM\Software\Microsoft\Windows\CurrentVersion\Installer\",
                @"HKLM\Software\Wow6432Node\Microsoft\Windows\CurrentVersion\Installer\",
                @"HKLM\Software\Microsoft\Windows\CurrentVersion\Uninstall\",
                @"HKLM\Software\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\",
                @"HKLM\Software\Classes\Installer\",
                @"HKCU\Software\Microsoft\RestartManager\",
                @"HKCU\Software\Microsoft\Installer\",
                @"HKCR\Installer\"
            }.Select(c => c.ToLowerInvariant());

            var regKeysUsedByMui = new Collection<string>
            {
                @"HKLM\System\CurrentControlSet\Control\MUI\",
                @"HKU\.DEFAULT\Software\Classes\Local Settings\MuiCache\",
                @"HKCU\Software\Classes\Local Settings\MuiCache\"
            }.Select(c => c.ToLowerInvariant());

            if (installerType == TypeOfInstaller.Msi)
            {
                return registryResult.Where(e =>
                {
                    var path = e.Key.ToLowerInvariant();
                    if (regKeysUsedByMsiexec.Any(item => path.Contains(item)))
                    {
                        return false;
                    }

                    if (regKeysUsedByMui.Any(item => path.Contains(item)))
                    {
                        return false;
                    }

                    return true;
                });
            }

            return registryResult;
        }

        private static IEnumerable<FileChange> ExtractFileChangeData(List<Event> allEventsPidFiltered, TypeOfInstaller installerType)
        {
            var fileResult = new Collection<FileChange>();
            foreach (var e in allEventsPidFiltered)
            {
                if (e.Operation == "CreateFile" && e.Detail.Contains("OpenResult: Created"))
                {
                    // If file have deleted before created then remove the delete event
                    if (fileResult.Any(c => c.Path == e.Path))
                    {
                        var res = fileResult.First(c => c.Path == e.Path);
                        if (res.Type == FileChangeType.Delete)
                        {
                            fileResult.Remove(res);
                        }
                    }

                    var options = GetOptionsPartFromDetails(e);
                    fileResult.Add(new FileChange
                    {
                        Path = e.Path,
                        Type = FileChangeType.Create,
                        IsDirectory = !options.Contains("Non-Directory File")
                    });
                }

                if (e.Operation == "SetRenameInformationFile")
                {
                    if (fileResult.Any(c => c.Path == e.Path))
                    {
                        var res = fileResult.First(c => c.Path == e.Path);
                        fileResult.Remove(res);
                        res.OldPath = e.Path;
                        res.Path = e.Detail.Substring(e.Detail.IndexOf("FileName: ", StringComparison.Ordinal) + 10);
                        fileResult.Add(res);
                    }
                    else
                    {
                        fileResult.Add(new FileChange
                        {
                            OldPath = e.Path,
                            Path = e.Detail.Substring(e.Detail.IndexOf("FileName: ", StringComparison.Ordinal) + 10),
                            Type = FileChangeType.Rename
                        });
                    }
                }

                if (e.Operation == "SetDispositionInformationFile" && e.Detail.Contains("Delete: True"))
                {
                    if (fileResult.Any(c => c.Path == e.Path))
                    {
                        var res = fileResult.First(c => c.Path == e.Path);
                        if (res.Type == FileChangeType.Create)
                        {
                            fileResult.Remove(res);
                        }
                        /*** MSI behavoiur
                         * When uninstalling:
                         * 1. Rename file to temporary name
                         * 2. Delete the temporary file
                         * 
                         * When repairing
                         * 1. Rename to temporary name
                         * 2. Create a new file
                         * 3. Delete the temporary file
                         ***/
                        else if (res.Type == FileChangeType.Rename)
                        {
                            // Check if it's a repair scenario
                            var createFileChange = fileResult.FirstOrDefault(x => x.Path == res.OldPath && x.Type == FileChangeType.Create);
                            if (createFileChange != null)
                            {
                                fileResult.Remove(res);
                            }
                            else
                            {
                                var options = GetOptionsPartFromDeailsForDeleteEvent(allEventsPidFiltered, e, res.Path);
                                fileResult.Remove(res);
                                res.Path = res.OldPath;
                                res.Type = FileChangeType.Delete;
                                res.IsDirectory = !options.Contains("Non-Directory File");
                                fileResult.Add(res);
                            }
                        }
                    }
                    else
                    {
                        var options = GetOptionsPartFromDeailsForDeleteEvent(allEventsPidFiltered, e, e.Path);
                        fileResult.Add(new FileChange { Path = e.Path, Type = FileChangeType.Delete, IsDirectory = !options.Contains("Non-Directory File") });
                    }
                }
            }

            if (installerType == TypeOfInstaller.Msi)
            {
                return fileResult.Where(e => !e.Path.StartsWith(@"C:\Windows\Temp\", StringComparison.OrdinalIgnoreCase) &&
                                             !e.Path.StartsWith(Path.GetTempPath(), StringComparison.OrdinalIgnoreCase) &&
                                             !e.Path.StartsWith(@"c:\Config.Msi", StringComparison.OrdinalIgnoreCase) &&
                                             !e.Path.StartsWith(@"C:\Windows\Installer\", StringComparison.OrdinalIgnoreCase));
            }

            return fileResult;
        }

        private static string GetOptionsPartFromDeailsForDeleteEvent(List<Event> allEventsPidFiltered, Event e, string filePath)
        {
            var eventTime = e.Time;
            var createEvent = allEventsPidFiltered.Where(x =>
            {
                if (x.Time > eventTime)
                {
                    return false;
                }
                if (x.Operation != "CreateFile")
                {
                    return false;
                }
                if (x.Path != filePath)
                {
                    return false;
                }

                var desiredAccess = x.Detail.Substring(0,
                    x.Detail.IndexOf("Disposition:", StringComparison.Ordinal));
                if (!desiredAccess.Contains("Delete"))
                {
                    return false;
                }

                return true;
            }).Last();

            var options = GetOptionsPartFromDetails(createEvent);
            return options;
        }

        private static string GetOptionsPartFromDetails(Event e)
        {
            var optionsIndex = e.Detail.IndexOf("Options:", StringComparison.Ordinal);
            var options = e.Detail.Substring(optionsIndex + 9);
            options = options.Substring(0, options.IndexOf(":", StringComparison.Ordinal));
            return options;
        }

        //public static Dictionary<string, string> GetDictionaryOfDetails(string details)
        //{
        //    var result = new Dictionary<string, string>();

        //    var index = details.IndexOf(":");
        //    var operation = details.Substring(0, index);
        //    var tmp = details.Substring(index + 1);
        //    var tmp2 = tmp.Substring(0, tmp.IndexOf(":"));
        //    var index2 = tmp2.LastIndexOf(",");
        //    var value = tmp2.Substring(0, index2);

        //    result.Add(operation, value);

        //    return result;
        //}

        private static string GetSubeElementValue(XElement c, string elementName)
        {
            var pid = c.Elements().FirstOrDefault(n => n.Name == elementName);
            return pid != null ? pid.Value : string.Empty;
        }
    }
}
