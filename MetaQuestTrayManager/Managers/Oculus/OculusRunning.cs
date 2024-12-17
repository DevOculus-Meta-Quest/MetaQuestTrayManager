using MetaQuestTrayManager.Utils;
using System;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Threading;

namespace MetaQuestTrayManager.Managers.Oculus
{
    public static class OculusRunning
    {
        // Properties
        public static string Oculus_Main_Directory { get; private set; }
        public static string Oculus_Dash_Directory { get; private set; }
        public static string Oculus_Dash_File { get; private set; }
        public static string Oculus_Client_EXE { get; private set; }
        public static string Oculus_DebugTool_EXE { get; private set; }
        public static bool Oculus_Is_Installed { get; private set; }

        private static bool _ClientJustExited;
        private static bool _Report_ClientJustExited;
        private static bool _IsSetup;

        /// <summary>
        /// Setup event handlers for process monitoring.
        /// </summary>
        public static void Setup()
        {
            if (!_IsSetup)
            {
                _IsSetup = true;
                ProcessWatcher.ProcessStarted += ProcessWatcher_ProcessStarted;
                ProcessWatcher.ProcessExited += ProcessWatcher_ProcessExited;
            }
        }

        private static void ProcessWatcher_ProcessStarted(string processName, int processId)
        {
            Debug.WriteLine($"Started: {processName} - {DateTime.Now}");
        }

        private static void ProcessWatcher_ProcessExited(string processName, int processId)
        {
            Debug.WriteLine($"Stopped: {processName} - {DateTime.Now}");

            if (processName == "OculusClient.exe" && _Report_ClientJustExited)
            {
                Debug.WriteLine("Set Client Minimize Exit Trigger");
                _ClientJustExited = true;
                _Report_ClientJustExited = false;
            }
        }

        /// <summary>
        /// Checks if Oculus software is installed and sets relevant paths.
        /// </summary>
        public static void Check_Oculus_Is_Installed()
        {
            var oculusPath = Environment.GetEnvironmentVariable("OculusBase");

            if (!string.IsNullOrEmpty(oculusPath) && Directory.Exists(oculusPath))
            {
                Oculus_Main_Directory = oculusPath;
                Oculus_Dash_Directory = Path.Combine(oculusPath, @"Support\oculus-dash\dash\bin");
                Oculus_Client_EXE = Path.Combine(oculusPath, @"Support\oculus-client\OculusClient.exe");
                Oculus_DebugTool_EXE = Path.Combine(oculusPath, @"Support\oculus-diagnostics\OculusDebugTool.exe");
                Oculus_Dash_File = Path.Combine(Oculus_Dash_Directory, @"OculusDash.exe");

                Oculus_Is_Installed = File.Exists(Oculus_Client_EXE);
            }
        }

        /// <summary>
        /// Starts the Oculus client and handles minimizing if required.
        /// </summary>
        public static void StartOculusClient()
        {
            if (File.Exists(Oculus_Client_EXE) && Process.GetProcessesByName("OculusClient").Length == 0)
            {
                // Start the Oculus runtime service if not already running
                if (Service_Manager.GetState("OVRService") != "Running")
                {
                    var serviceLauncherPath = Path.Combine(Oculus_Main_Directory, "Support\\oculus-runtime\\OVRServiceLauncher.exe");

                    if (File.Exists(serviceLauncherPath))
                    {
                        var serviceLauncher = Process.Start(serviceLauncherPath, "-start");
                        serviceLauncher.WaitForExit();

                        for (int i = 0; i < 100; i++)
                        {
                            Thread.Sleep(1000);

                            if (Process.GetProcessesByName("OVRRedir").Length > 0)
                            {
                                Debug.WriteLine("OVRRedir Started");
                                Thread.Sleep(2000);
                                break;
                            }
                        }
                    }
                }

                // Start Oculus Client
                var clientInfo = new ProcessStartInfo
                {
                    WorkingDirectory = Path.GetDirectoryName(Oculus_Client_EXE),
                    FileName = Oculus_Client_EXE
                };

                var client = Process.Start(clientInfo);

                // Optionally minimize Oculus Client
                if (MetaQuestTrayManager.Properties.Settings.Default.Minimize_Oculus_Client_OnClientStart)
                {
                    _Report_ClientJustExited = true;

                    for (int i = 0; i < 50; i++)
                    {
                        Thread.Sleep(250);
                        if (_ClientJustExited) break;
                    }

                    _Report_ClientJustExited = false;

                    for (int i = 0; i < 20; i++)
                    {
                        WindowUtilities.MinimizeExternalWindow(client.MainWindowHandle);
                        Thread.Sleep(250);
                    }

                    Debug.WriteLine("Client Window Minimized");
                }
            }
        }

        /// <summary>
        /// Retrieves the path to the Oculus Debug Tool executable.
        /// </summary>
        public static string GetOculusDebugToolPath()
        {
            return Oculus_DebugTool_EXE;
        }
    }
}
