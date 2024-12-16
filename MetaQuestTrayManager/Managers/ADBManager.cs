using AdvancedSharpAdbClient;
using MetaQuestTrayManager.Utils;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace MetaQuestTrayManager.Managers
{
    public static class ADBManager
    {
        private static Process ADBServer;

        /// <summary>
        /// Starts the ADB server if it's not already running.
        /// </summary>
        public static void StartADB()
        {
            if (!AdbServer.Instance.GetStatus().IsRunning)
            {
                var server = new AdbServer();
                try
                {
                    ProcessWatcher.ProcessStarted += Process_Watcher_ProcessStarted;

                    var result = server.StartServer(@".\Resources\Binaries\adb.exe", false);

                    if (result != null && !result.ToString().Contains("started", StringComparison.OrdinalIgnoreCase))
                    {
                        Debug.WriteLine("Cannot start ADB server.");
                    }

                    var removeWatcherThread = new Thread(RemoveWatcher);
                    removeWatcherThread.Start();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }
            else
            {
                VerifyExistingADBInstances();
            }
        }

        /// <summary>
        /// Stops the ADB server.
        /// </summary>
        public static void StopADB()
        {
            if (ADBServer != null)
            {
                try
                {
                    ADBServer.Kill();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }
        }

        /// <summary>
        /// Installs an APK file on the connected device.
        /// </summary>
        public static void InstallAPK(string apkPath)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = @".\Resources\Binaries\adb.exe",
                Arguments = $"install \"{apkPath}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (var process = new Process { StartInfo = startInfo })
            {
                process.Start();
                var errorOutput = process.StandardError.ReadToEnd();
                process.WaitForExit();

                if (process.ExitCode != 0 || !string.IsNullOrEmpty(errorOutput))
                {
                    var exception = new Exception($"Error installing APK. Exit Code: {process.ExitCode}. Error: {errorOutput}");
                    ErrorLogger.LogError(exception);
                }
            }
        }

        /// <summary>
        /// Executes a custom ADB command.
        /// </summary>
        public static string ExecuteCommand(string command)
        {
            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = @".\Resources\Binaries\adb.exe",
                    Arguments = command,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (var process = Process.Start(startInfo))
                using (var reader = process.StandardOutput)
                {
                    return reader.ReadToEnd();
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex);
                return null;
            }
        }

        #region File Management
        public static string ListFiles(string directory) => ExecuteCommand($"shell ls {directory}");

        public static void DeleteFile(string filePath) => ExecuteCommand($"shell rm {filePath}");

        public static void UploadFile(string localPath, string remotePath) => ExecuteCommand($"push {localPath} {remotePath}");

        public static void DownloadFile(string remotePath, string localPath) => ExecuteCommand($"pull {remotePath} {localPath}");

        public static void CreateDirectory(string directoryPath) => ExecuteCommand($"shell mkdir {directoryPath}");
        #endregion

        #region Private Helpers
        private static void VerifyExistingADBInstances()
        {
            var runningAdbs = Process.GetProcessesByName("adb");
            var myAdbLocation = Path.Combine(Environment.CurrentDirectory, "Resources", "Binaries", "adb.exe");

            foreach (var process in runningAdbs)
            {
                try
                {
                    if (process.MainModule.FileName == myAdbLocation)
                        Process_Watcher_ProcessStarted(process.ProcessName, process.Id);
                }
                catch { /* Ignore processes we cannot access */ }
            }
        }

        private static void Process_Watcher_ProcessStarted(string processName, int processId)
        {
            if (processName == "adb")
                ADBServer = Process.GetProcessById(processId);
        }

        private static void RemoveWatcher()
        {
            Thread.Sleep(1000);
            ProcessWatcher.ProcessStarted -= Process_Watcher_ProcessStarted;
        }
        #endregion
    }
}
