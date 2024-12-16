using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using MetaQuestTrayManager.Utils;

namespace MetaQuestTrayManager.Managers
{
    public static class Service_Manager
    {
        private static readonly Dictionary<string, ServiceController> Services = new();

        /// <summary>
        /// Registers a service for management.
        /// </summary>
        public static void RegisterService(string serviceName)
        {
            if (!Services.ContainsKey(serviceName))
            {
                try
                {
                    var service = new ServiceController(serviceName);
                    Services.Add(serviceName, service);
                }
                catch (Exception ex)
                {
                    ErrorLogger.LogError(ex, $"Unable to load or find service: {serviceName}");
                }
            }
        }

        /// <summary>
        /// Stops a registered service.
        /// </summary>
        public static void StopService(string serviceName)
        {
            if (Services.TryGetValue(serviceName, out var service))
            {
                service.Refresh();

                if (IsRunning(service.Status))
                {
                    try
                    {
                        service.Stop();
                        service.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(30));
                    }
                    catch (Exception ex)
                    {
                        ErrorLogger.LogError(ex, $"Unable to stop service: {serviceName}");
                    }
                }
            }
        }

        /// <summary>
        /// Starts a registered service.
        /// </summary>
        public static void StartService(string serviceName)
        {
            if (Services.TryGetValue(serviceName, out var service))
            {
                service.Refresh();

                if (!IsRunning(service.Status))
                {
                    try
                    {
                        service.Start();
                        service.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(30));
                    }
                    catch (Exception ex)
                    {
                        ErrorLogger.LogError(ex, $"Unable to start service: {serviceName}");
                    }
                }
            }
        }

        /// <summary>
        /// Sets a service to automatic startup.
        /// </summary>
        public static void SetAutomaticStartup(string serviceName)
        {
            ChangeStartupMode(serviceName, ServiceStartMode.Automatic);
        }

        /// <summary>
        /// Sets a service to manual startup.
        /// </summary>
        public static void SetManualStartup(string serviceName)
        {
            ChangeStartupMode(serviceName, ServiceStartMode.Manual);
        }

        /// <summary>
        /// Retrieves the current state of a service.
        /// </summary>
        public static string GetState(string serviceName)
        {
            if (Services.TryGetValue(serviceName, out var service))
            {
                service.Refresh();
                return service.Status.ToString();
            }

            return "Not Found";
        }

        /// <summary>
        /// Retrieves the startup type of a service.
        /// </summary>
        public static string GetStartup(string serviceName)
        {
            if (Services.TryGetValue(serviceName, out var service))
            {
                service.Refresh();
                return service.StartType.ToString();
            }

            return "Not Found";
        }

        /// <summary>
        /// Manages the state of a service (start/stop).
        /// </summary>
        public static bool ManageService(string serviceName, bool startService)
        {
            try
            {
                if (startService)
                {
                    StartService(serviceName);
                }
                else
                {
                    StopService(serviceName);
                }

                return true;
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, $"Error managing the service: {serviceName}");
                return false;
            }
        }

        /// <summary>
        /// Changes the startup mode of a service.
        /// </summary>
        private static void ChangeStartupMode(string serviceName, ServiceStartMode mode)
        {
            if (Services.TryGetValue(serviceName, out var service))
            {
                try
                {
                    ServiceHelper.ChangeStartMode(service, mode);
                }
                catch (Exception ex)
                {
                    ErrorLogger.LogError(ex, $"Unable to change startup mode for service: {serviceName}");
                }
            }
        }

        /// <summary>
        /// Checks if a service is in a running state.
        /// </summary>
        private static bool IsRunning(ServiceControllerStatus status)
        {
            return status is ServiceControllerStatus.Running or
                   ServiceControllerStatus.Paused or
                   ServiceControllerStatus.StartPending or
                   ServiceControllerStatus.StopPending;
        }
    }

    public static class ServiceHelper
    {
        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern bool ChangeServiceConfig(
            IntPtr hService, uint nServiceType, uint nStartType, uint nErrorControl,
            string? lpBinaryPathName, string? lpLoadOrderGroup, IntPtr lpdwTagId,
            [In] char[]? lpDependencies, string? lpServiceStartName, string? lpPassword,
            string? lpDisplayName);

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern IntPtr OpenService(
            IntPtr hSCManager, string lpServiceName, uint dwDesiredAccess);

        [DllImport("advapi32.dll", EntryPoint = "OpenSCManagerW", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern IntPtr OpenSCManager(string? machineName, string? databaseName, uint dwAccess);

        [DllImport("advapi32.dll", EntryPoint = "CloseServiceHandle")]
        public static extern int CloseServiceHandle(IntPtr hSCObject);

        private const uint SERVICE_NO_CHANGE = 0xFFFFFFFF;
        private const uint SERVICE_QUERY_CONFIG = 0x00000001;
        private const uint SERVICE_CHANGE_CONFIG = 0x00000002;
        private const uint SC_MANAGER_ALL_ACCESS = 0x000F003F;

        /// <summary>
        /// Changes the startup mode of a service.
        /// </summary>
        public static void ChangeStartMode(ServiceController svc, ServiceStartMode mode)
        {
            var scManagerHandle = OpenSCManager(null, null, SC_MANAGER_ALL_ACCESS);
            if (scManagerHandle == IntPtr.Zero)
            {
                throw new ExternalException("Open Service Manager Error");
            }

            var serviceHandle = OpenService(scManagerHandle, svc.ServiceName, SERVICE_QUERY_CONFIG | SERVICE_CHANGE_CONFIG);
            if (serviceHandle == IntPtr.Zero)
            {
                throw new ExternalException("Open Service Error");
            }

            var result = ChangeServiceConfig(
                serviceHandle, SERVICE_NO_CHANGE, (uint)mode, SERVICE_NO_CHANGE,
                null, null, IntPtr.Zero, null, null, null, null);

            if (!result)
            {
                throw new ExternalException($"Could not change service start type. Error: {Marshal.GetLastWin32Error()}");
            }

            CloseServiceHandle(serviceHandle);
            CloseServiceHandle(scManagerHandle);
        }
    }
}
