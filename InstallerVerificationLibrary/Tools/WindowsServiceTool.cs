namespace InstallerVerificationLibrary.Tools
{
    using System;
    using System.Collections.ObjectModel;
    using System.Configuration.Install;
    using System.Linq;
    using System.ServiceProcess;
    using System.Threading;
    using InstallerVerificationLibrary.Logging;
    using TimeoutException = System.TimeoutException;

    public static class WindowsServiceTool
    {
        private static readonly TimeSpan ServiceTimeOut = new TimeSpan(0, 5, 0);
        private static TimeSpan afterServiceRemoveWaitTime = new TimeSpan(0, 0, 30);

        public static void RemoveService(string name)
        {
            foreach (var controller in ServiceController.GetServices()
                .Where( controller => string.Compare(controller.ServiceName, name, StringComparison.OrdinalIgnoreCase) == 0))
            {
                Log.WriteInfo("Removing service '" + name + "'", "RemoveService");
                if (controller.Status != ServiceControllerStatus.Stopped)
                {
                    controller.Stop();
                    controller.WaitForStatus(ServiceControllerStatus.Stopped, ServiceTimeOut);
                    if (controller.Status != ServiceControllerStatus.Stopped)
                    {
                        throw new TimeoutException("Timeout waiting on service '" + controller.ServiceName +
                                                   "' to stop. Service status = " + controller.Status);
                    }
                }

                using (var serviceInstaller = new ServiceInstaller())
                {
                    serviceInstaller.Context = new InstallContext();
                    serviceInstaller.ServiceName = controller.ServiceName;
                    controller.Dispose();
                    serviceInstaller.Uninstall(null);
                }

                break;
            }

            foreach (var controller in ServiceController.GetServices().Where(controller => controller.ServiceName.Equals(name)))
            {
                Log.WriteError("Could not remove service '" + name + "'", "RemoveService");
            }
        }

        public static void RestartService(string name)
        {
            foreach (var controller in ServiceController.GetServices()
                .Where(controller => string.Compare(controller.ServiceName, name, StringComparison.OrdinalIgnoreCase) == 0))
            {
                Log.WriteInfo("Restarting service '" + name + "'", "RestartService");
                if (controller.Status != ServiceControllerStatus.Stopped)
                {
                    controller.Stop();
                    controller.WaitForStatus(ServiceControllerStatus.Stopped, ServiceTimeOut);
                    if (controller.Status != ServiceControllerStatus.Stopped)
                    {
                        throw new TimeoutException("Timeout waiting on service '" + controller.ServiceName +
                                                   "' to stop. Service status = " + controller.Status);
                    }
                }

                controller.Refresh();

                controller.Start();
                controller.WaitForStatus(ServiceControllerStatus.Running, ServiceTimeOut);
                if (controller.Status != ServiceControllerStatus.Running)
                {
                    throw new TimeoutException("Timeout waiting on service '" + controller.ServiceName +
                                               "' to start. Service status = " + controller.Status);
                }

                break;
            }
        }

        public static void RemoveServices(string nameSearchPattern)
        {
            if (string.IsNullOrEmpty(nameSearchPattern))
            {
                throw new ArgumentNullException("nameSearchPattern");
            }

            var servicesNames = new Collection<string>();

            var controllers = ServiceController.GetServices();
            foreach (var controller in controllers)
            {
                var removeService = GetServiceDisplayName(controller.ServiceName)
                    .ToUpperInvariant()
                    .Contains(nameSearchPattern.ToUpperInvariant()) ||
                                    controller.ServiceName.Contains(nameSearchPattern);

                if (removeService)
                {
                    servicesNames.Add(controller.ServiceName);
                }
            }

            RemoveServices(servicesNames);
        }

        public static void RemoveServices(Collection<string> serviceNames)
        {
            if (serviceNames == null)
            {
                throw new ArgumentNullException("serviceNames");
            }

            foreach (var name in serviceNames)
            {
                RemoveService(name);
            }

            // To give windows pause to ensure that you can install services again
            if (serviceNames.Count > 0)
            {
                Thread.Sleep((int) afterServiceRemoveWaitTime.TotalMilliseconds);
            }
        }

        private static string GetServiceDisplayName(string serviceName)
        {
            var registryPath = @"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\" + serviceName;
            return (string) RegistryTool.RegistryValue(registryPath, "DisplayName", null) ?? string.Empty;
        }
    }
}