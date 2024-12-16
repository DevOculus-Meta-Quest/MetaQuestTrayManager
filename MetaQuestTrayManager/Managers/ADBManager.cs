using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using MetaQuestTrayManager.Utils;

namespace MetaQuestTrayManager.Managers
{
    public class ADBManager
    {
        private readonly string adbPath;

        public ADBManager()
        {
            try
            {
                adbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "Binaries", "adb.exe");

                // Verify that adb.exe exists at the specified location
                if (!File.Exists(adbPath))
                {
                    throw new FileNotFoundException("ADB executable not found.", adbPath);
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, "Failed to initialize ADBManager. Ensure adb.exe is in the correct path.");
                throw; // Rethrow to alert the calling code
            }
        }

        /// <summary>
        /// Runs an ADB command and returns the output.
        /// </summary>
        public string RunCommand(string arguments)
        {
            try
            {
                using (var process = new Process())
                {
                    process.StartInfo = new ProcessStartInfo
                    {
                        FileName = adbPath,
                        Arguments = arguments,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };

                    process.Start();

                    // Read the standard output and error streams
                    string output = process.StandardOutput.ReadToEnd();
                    string errorOutput = process.StandardError.ReadToEnd();

                    process.WaitForExit();

                    if (process.ExitCode != 0)
                    {
                        throw new InvalidOperationException($"ADB command failed with error: {errorOutput}");
                    }

                    return output;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, $"Failed to run ADB command: '{arguments}'");
                return string.Empty; // Return an empty string to signify failure
            }
        }

        /// <summary>
        /// Retrieves a list of connected ADB devices.
        /// </summary>
        public List<string> GetConnectedDevices()
        {
            try
            {
                string output = RunCommand("devices");
                var devices = new List<string>();

                foreach (var line in output.Split('\n'))
                {
                    if (line.Contains("\tdevice"))
                    {
                        devices.Add(line.Split('\t')[0]);
                    }
                }

                return devices;
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, "Failed to retrieve connected ADB devices.");
                return new List<string>(); // Return an empty list on failure
            }
        }
    }
}
