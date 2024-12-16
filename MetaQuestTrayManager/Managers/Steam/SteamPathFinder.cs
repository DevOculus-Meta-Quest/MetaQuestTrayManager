using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using MetaQuestTrayManager.Utils;

namespace MetaQuestTrayManager.Managers.Steam
{
    /// <summary>
    /// Finds Steam installation paths and game library paths.
    /// </summary>
    public static class SteamPathFinder
    {
        /// <summary>
        /// Finds the main Steam installation path.
        /// </summary>
        /// <returns>The path to the Steam installation directory, or null if not found.</returns>
        public static string FindSteamInstallPath()
        {
            try
            {
                const string registryKey = @"HKEY_CURRENT_USER\Software\Valve\Steam";
                string steamPath = (string)Registry.GetValue(registryKey, "SteamPath", null);

                if (!string.IsNullOrEmpty(steamPath))
                {
                    Debug.WriteLine($"Steam is installed at: {steamPath}");
                    return steamPath;
                }

                // Fallback to checking registry for 64-bit or 32-bit systems
                return GetSteamPathFromRegistry();
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, "Error in FindSteamInstallPath");
                return null;
            }
        }

        /// <summary>
        /// Finds all Steam game library paths, including the main Steam install path.
        /// </summary>
        /// <returns>A list of game library paths.</returns>
        public static List<string> FindAllGamePaths()
        {
            var allPaths = new List<string>();

            try
            {
                string mainSteamPath = FindSteamInstallPath();

                if (!string.IsNullOrEmpty(mainSteamPath))
                {
                    allPaths.Add(mainSteamPath);
                    string libraryFoldersPath = Path.Combine(mainSteamPath, "steamapps", "libraryfolders.vdf");

                    if (File.Exists(libraryFoldersPath))
                    {
                        var libraryPaths = ParseLibraryFoldersVdf(libraryFoldersPath);
                        allPaths.AddRange(libraryPaths);
                    }
                }
                else
                {
                    Debug.WriteLine("No valid Steam installation path found.");
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, "Error in FindAllGamePaths");
            }

            return allPaths;
        }

        /// <summary>
        /// Retrieves the Steam path from Windows registry, checking both 32-bit and 64-bit paths.
        /// </summary>
        private static string GetSteamPathFromRegistry()
        {
            const string registryValueName = "InstallPath";

            string[] registryPaths =
            {
                @"SOFTWARE\WOW6432Node\Valve\Steam",  // 64-bit registry path
                @"SOFTWARE\Valve\Steam"              // 32-bit registry path
            };

            foreach (var registryPath in registryPaths)
            {
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(registryPath))
                {
                    if (key?.GetValue(registryValueName) is string value)
                    {
                        Debug.WriteLine($"Found Steam path in registry: {value}");
                        return value;
                    }
                }
            }

            Debug.WriteLine("Steam path not found in registry.");
            return null;
        }

        /// <summary>
        /// Parses the "libraryfolders.vdf" file to retrieve additional Steam library paths.
        /// </summary>
        /// <param name="filePath">Path to the "libraryfolders.vdf" file.</param>
        /// <returns>A list of Steam library paths.</returns>
        private static List<string> ParseLibraryFoldersVdf(string filePath)
        {
            var paths = new List<string>();

            try
            {
                string fileContent = File.ReadAllText(filePath);
                var matches = Regex.Matches(fileContent, "\"path\"\\s*\"(.+?)\"");

                foreach (Match match in matches)
                {
                    if (match.Groups.Count > 1)
                    {
                        string path = match.Groups[1].Value.Replace("\\\\", "\\"); // Normalize slashes
                        if (Directory.Exists(path))
                        {
                            Debug.WriteLine($"Found library path: {path}");
                            paths.Add(path);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, "Error in ParseLibraryFoldersVdf");
            }

            return paths;
        }
    }
}
