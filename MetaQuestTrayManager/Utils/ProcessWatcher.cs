using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Management;
using MetaQuestTrayManager.Utils;

namespace MetaQuestTrayManager.Utils
{
    public static class ProcessWatcher
    {
        public delegate void ProcessEventHandler(string processName, int processId);

        public static event ProcessEventHandler? ProcessStarted;
        public static event ProcessEventHandler? ProcessExited;

        private static readonly ManagementEventWatcher? ProcessStartEventWatcher;
        private static readonly ManagementEventWatcher? ProcessStopEventWatcher;

        private static readonly HashSet<string> IgnoredExeNames = new();

        static ProcessWatcher()
        {
            try
            {
                // Initialize the ManagementEventWatchers with WQL queries
                ProcessStartEventWatcher = new ManagementEventWatcher("SELECT * FROM __InstanceCreationEvent WITHIN 1 WHERE TargetInstance ISA 'Win32_Process'");
                ProcessStopEventWatcher = new ManagementEventWatcher("SELECT * FROM __InstanceDeletionEvent WITHIN 1 WHERE TargetInstance ISA 'Win32_Process'");

                ProcessStartEventWatcher.EventArrived += OnProcessStarted;
                ProcessStopEventWatcher.EventArrived += OnProcessExited;
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, "Failed to initialize process watchers.");
            }
        }

        /// <summary>
        /// Add an executable name to the ignore list.
        /// </summary>
        public static void IgnoreExeName(string exeName)
        {
            if (!string.IsNullOrEmpty(exeName))
            {
                IgnoredExeNames.Add(exeName);
            }
        }

        /// <summary>
        /// Remove an executable name from the ignore list.
        /// </summary>
        public static void RemoveIgnoreExeName(string exeName)
        {
            if (!string.IsNullOrEmpty(exeName))
            {
                IgnoredExeNames.Remove(exeName);
            }
        }

        /// <summary>
        /// Start watching for process creation and termination events.
        /// </summary>
        public static bool Start()
        {
            try
            {
                ProcessStartEventWatcher?.Start();
                ProcessStopEventWatcher?.Start();
                return true;
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, "Failed to start process watchers.");
                return false;
            }
        }

        /// <summary>
        /// Stop watching for process events.
        /// </summary>
        public static bool Stop()
        {
            try
            {
                ProcessStartEventWatcher?.Stop();
                ProcessStopEventWatcher?.Stop();
                return true;
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, "Failed to stop process watchers.");
                return false;
            }
        }

        /// <summary>
        /// Dispose the process watchers.
        /// </summary>
        public static void Dispose()
        {
            Stop();
            IgnoredExeNames.Clear();

            ProcessStartEventWatcher?.Dispose();
            ProcessStopEventWatcher?.Dispose();
        }

        /// <summary>
        /// Handles process start events.
        /// </summary>
        private static void OnProcessStarted(object sender, EventArrivedEventArgs e)
        {
            HandleEvent(e, ProcessStarted);
        }

        /// <summary>
        /// Handles process exit events.
        /// </summary>
        private static void OnProcessExited(object sender, EventArrivedEventArgs e)
        {
            HandleEvent(e, ProcessExited);
        }

        /// <summary>
        /// Common handler for process start/exit events.
        /// </summary>
        private static void HandleEvent(EventArrivedEventArgs e, ProcessEventHandler? handler)
        {
            if (handler == null) return;

            try
            {
                var targetInstance = (ManagementBaseObject)e.NewEvent["TargetInstance"];
                var name = targetInstance["Name"]?.ToString();
                var id = Convert.ToInt32(targetInstance["Handle"]?.ToString());

                if (!string.IsNullOrEmpty(name) && !IgnoredExeNames.Contains(name))
                {
                    handler.Invoke(name, id);
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, "Failed to handle process event.");
            }
        }
    }
}
