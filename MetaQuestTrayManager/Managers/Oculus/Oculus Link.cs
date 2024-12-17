using AdvancedSharpAdbClient;
using Microsoft.Win32;
using MetaQuestTrayManager.Managers; // Updated namespace for Service_Manager and ADBManager
using MetaQuestTrayManager.Utils;    // ErrorLogger and other utilities
using System.IO;
using System.Linq;
using System.Diagnostics;
using AdvancedSharpAdbClient.DeviceCommands;
using AdvancedSharpAdbClient.Models;
using MetaQuestTrayManager.Managers.Steam;
using MetaQuestTrayManager.Properties;

namespace MetaQuestTrayManager.Managers.Oculus
{
    public static class Oculus_Link
    {
        /// <summary>
        /// Starts Oculus Link on connected Quest devices via ADB.
        /// </summary>
        public static void StartLinkOnDevice()
        {
            if (Settings.Default.QuestPolling)
            {
                ADBManager.StartADB(); // Ensure ADB server is running

                // Allow time for Quest to register with the ADB server
                System.Threading.Thread.Sleep(1000);

                var connectedDevices = ADBManager.ExecuteCommand("devices") // Get connected ADB devices
                    .Split('\n')
                    .Where(line => line.Contains("device") && !line.Contains("List"))
                    .Select(line => line.Split('\t')[0])
                    .ToList();

                foreach (var deviceSerial in connectedDevices)
                {
                    var client = new AdbClient();
                    var device = new DeviceData { Serial = deviceSerial };

                    if (client.GetDevices().Any(d => d.Serial == deviceSerial && d.State == DeviceState.Online))
                    {
                        client.StartApp(device, "com.oculus.xrstreamingclient");
                    }
                }
            }
        }

        /// <summary>
        /// Resets the Oculus service to reinitialize the Link.
        /// </summary>
        public static void ResetLink()
        {
            if (Service_Manager.GetState("OVRService") == "Running")
            {
                SteamRunning.ManagerCalledExit = true;

                Service_Manager.StopService("OVRService");
                Service_Manager.StartService("OVRService");

                SteamRunning.ManagerCalledExit = true;
            }
        }

        /// <summary>
        /// Stops Oculus Link by stopping the Oculus service.
        /// </summary>
        public static void StopLink()
        {
            if (Service_Manager.GetState("OVRService") == "Running")
            {
                SteamRunning.ManagerCalledExit = true;
                Service_Manager.StopService("OVRService");
                SteamRunning.ManagerCalledExit = true;
            }
        }

        /// <summary>
        /// Starts the Oculus Link by ensuring the Oculus service is running.
        /// </summary>
        public static void StartLink()
        {
            if (Service_Manager.GetState("OVRService") != "Running")
            {
                Service_Manager.StartService("OVRService");
            }
        }

        /// <summary>
        /// Sets Oculus as the default OpenXR runtime.
        /// </summary>
        public static void SetToOculusRunTime()
        {
            var oculusMainDir = @"C:\Program Files\Oculus"; // Default Oculus install path
            var oculusRuntimePath = Path.Combine(oculusMainDir, "Support\\oculus-runtime\\oculus_openxr_64.json");

            if (File.Exists(oculusRuntimePath))
            {
                var runTimeKey = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Khronos\OpenXR\1", true);

                if (runTimeKey != null)
                {
                    runTimeKey.SetValue("ActiveRuntime", oculusRuntimePath, RegistryValueKind.ExpandString);
                    runTimeKey.Close();

                    Debug.WriteLine("Oculus runtime set as ActiveRuntime for OpenXR.");
                }
            }
            else
            {
                ErrorLogger.LogError(new FileNotFoundException("Oculus OpenXR runtime file not found."));
            }
        }
    }
}
