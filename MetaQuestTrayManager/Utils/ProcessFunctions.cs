using System;
using System.Diagnostics;
using System.IO;
using System.Security.Principal;
using MetaQuestTrayManager.Utils;

namespace MetaQuestTrayManager.Utils
{
    public static class ProcessFunctions
    {
        /// <summary>
        /// Checks if the current process is running with elevated privileges (administrator).
        /// </summary>
        public static bool IsCurrentProcessElevated()
        {
            try
            {
                using var identity = WindowsIdentity.GetCurrent();
                var principal = new WindowsPrincipal(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, "Failed to check if the current process is elevated.");
                return false;
            }
        }

        /// <summary>
        /// Gets the directory of the current executable.
        /// </summary>
        public static string GetCurrentExecutableDirectory()
        {
            try
            {
                var currentProcess = Process.GetCurrentProcess();
                return Path.GetDirectoryName(currentProcess.MainModule?.FileName) ?? string.Empty;
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, "Failed to get the current executable directory.");
                return string.Empty;
            }
        }

        /// <summary>
        /// Gets the directory of the current assembly.
        /// </summary>
        public static string GetCurrentAssemblyDirectory()
        {
            try
            {
                var assemblyLocation = System.Reflection.Assembly.GetExecutingAssembly().Location;
                return Path.GetDirectoryName(assemblyLocation) ?? string.Empty;
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, "Failed to get the current assembly directory.");
                return string.Empty;
            }
        }

        /// <summary>
        /// Starts a process with the given path and arguments.
        /// </summary>
        public static Process? StartProcess(string path, string arguments = "")
        {
            try
            {
                if (File.Exists(path))
                {
                    return Process.Start(new ProcessStartInfo
                    {
                        FileName = path,
                        Arguments = arguments,
                        UseShellExecute = true
                    });
                }
                else
                {
                    ErrorLogger.LogError(new FileNotFoundException($"File not found: {path}"), "Failed to start process.");
                    return null;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, $"Failed to start process at path: {path}");
                return null;
            }
        }

        /// <summary>
        /// Starts a URL or application with arguments (useful for fallback cases).
        /// </summary>
        public static Process? StartUrlOrProcess(string pathOrUrl, string arguments = "")
        {
            try
            {
                return Process.Start(new ProcessStartInfo
                {
                    FileName = pathOrUrl,
                    Arguments = arguments,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, $"Failed to start URL or process: {pathOrUrl}");
                return null;
            }
        }
    }
}
