using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Timers;
using Microsoft.Win32;
using MetaQuestTrayManager.Managers;      // Service_Manager, TimerManager
using MetaQuestTrayManager.Managers.Oculus; // Oculus_Link
using MetaQuestTrayManager.Utils;         // WindowUtilities and ErrorLogger

#nullable disable

namespace MetaQuestTrayManager.Managers.Steam
{
    public static class SteamRunning
    {
        private static bool _isSetup;

        public delegate void SteamVRRunningStateChanged();
        public static event SteamVRRunningStateChanged SteamVRRunningStateChangedEvent;

        public static bool ManagerCalledExit { get; set; }
        public static string SteamDirectory { get; private set; }
        public static bool SteamInstalled { get; private set; }
        public static bool SteamRunningState { get; private set; }
        public static string SteamVRDirectory { get; private set; }
        public static bool SteamVRInstalled { get; private set; }
        public static bool SteamVRMonitorRunning { get; private set; }
        public static bool SteamVRServerRunning { get; private set; }

        public static void CheckInstalled()
        {
            var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var openVRPath = Path.Combine(localAppData, "openvr\\openvrpaths.vrpath");

            if (File.Exists(openVRPath))
            {
                try
                {
                    var json = File.ReadAllText(openVRPath);
                    var config = JsonConvert.DeserializeObject<OpenVRStripped>(json);

                    SteamDirectory = config?.config?.FirstOrDefault()?.Replace("\\config", "");
                    SteamVRDirectory = config?.runtime?.FirstOrDefault();

                    SteamInstalled = !string.IsNullOrEmpty(SteamDirectory) && Directory.Exists(SteamDirectory);
                    SteamVRInstalled = !string.IsNullOrEmpty(SteamVRDirectory) && Directory.Exists(SteamVRDirectory);
                }
                catch (Exception ex)
                {
                    ErrorLogger.LogError(ex, "Failed to parse OpenVR configuration.");
                }
            }
        }

        public static void Setup()
        {
            if (_isSetup) return;

            _isSetup = true;
            CheckInstalled();

            SteamVRRunningStateChangedEvent += HandleSteamVRStateChange;
            TimerManager.CreateTimer("SteamVR Focus Fix", TimeSpan.FromSeconds(1), CheckSteamVRFocusProblem);

            foreach (var processName in new[] { "steam", "vrserver", "vrmonitor" })
            {
                if (Process.GetProcessesByName(processName).Any())
                    SetRunningState($"{processName}.exe", true);
            }
        }

        public static void CloseSteamVRAndResetLink()
        {
            CloseSteamVRServer();
            Oculus_Link.StopLink();
            CloseSteamVRServer();

            var resetThread = new Thread(StartLinkInAMoment);
            resetThread.Start();
        }

        public static void CloseSteamVRServer()
        {
            KillProcess("vrserver");
            KillProcess("vrmonitor");
        }

        private static void StartLinkInAMoment()
        {
            Thread.Sleep(2000);
            ManagerCalledExit = true;
            Oculus_Link.StartLink();
        }

        private static void KillProcess(string processName)
        {
            var processes = Process.GetProcessesByName(processName);
            foreach (var process in processes)
            {
                try
                {
                    process.CloseMainWindow();
                    process.Kill();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Failed to kill process {processName}: {ex.Message}");
                }
            }
        }

        private static void SetRunningState(string processName, bool state)
        {
            switch (processName.ToLower())
            {
                case "steam.exe":
                    SteamRunningState = state;
                    break;
                case "vrserver.exe":
                    SteamVRServerRunning = state;
                    SteamVRRunningStateChangedEvent?.Invoke();
                    break;
                case "vrmonitor.exe":
                    SteamVRMonitorRunning = state;
                    break;
            }
        }

        private static void HandleSteamVRStateChange()
        {
            if (!SteamVRServerRunning && !ManagerCalledExit &&
                Properties.Settings.Default.ExitLinkOn_UserExit_SteamVR)
            {
                CloseSteamVRAndResetLink();
            }

            ManagerCalledExit = false;
        }

        private static void CheckSteamVRFocusProblem(object sender, ElapsedEventArgs args)
        {
            if (SteamVRServerRunning && Properties.Settings.Default.SteamVRFocusFix)
            {
                if (WindowUtilities.GetActiveWindowTitle() == "Task View")
                {
                    FocusSteamVRMonitor();
                }
            }
        }

        public static void FocusSteamVRMonitor()
        {
            var vrMonitor = Process.GetProcessesByName("vrmonitor").FirstOrDefault();

            if (vrMonitor?.MainWindowHandle != IntPtr.Zero)
            {
                WindowUtilities.BringWindowToTopAndFocus(vrMonitor.MainWindowHandle);
            }
        }

        private class OpenVRStripped
        {
            public List<string> config { get; set; }
            public List<string> runtime { get; set; }
        }
    }
}
