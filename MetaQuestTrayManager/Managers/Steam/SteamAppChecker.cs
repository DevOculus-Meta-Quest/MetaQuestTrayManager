using Microsoft.Win32;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using MetaQuestTrayManager.Utils;

namespace MetaQuestTrayManager.Managers.Steam
{
    public class SteamAppDetails
    {
        public string Name { get; set; }
        public string ID { get; set; }
        public string InstallPath { get; set; }
        public string ImagePath { get; set; }
    }

    public static class SteamAppChecker
    {
        private static List<string> _installedApps;

        /// <summary>
        /// Checks if a specific Steam app is installed by name.
        /// </summary>
        public static bool IsAppInstalled(string appName)
        {
            try
            {
                // Populate cache if it's null
                _installedApps ??= GetInstalledApps(GetLibraryPaths());

                return _installedApps.Contains(appName, StringComparer.OrdinalIgnoreCase);
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, "Error checking if Steam app is installed.");
                return false;
            }
        }

        /// <summary>
        /// Retrieves all installed Steam app names.
        /// </summary>
        public static List<string> GetInstalledApps()
        {
            _installedApps ??= GetInstalledApps(GetLibraryPaths());
            return _installedApps;
        }

        /// <summary>
        /// Retrieves Steam installation path from the registry.
        /// </summary>
        private static string GetSteamPath()
        {
            var steamPath = (string)Registry.GetValue(@"HKEY_CURRENT_USER\Software\Valve\Steam", "SteamPath", null);

            if (string.IsNullOrEmpty(steamPath) || !Directory.Exists(steamPath))
            {
                throw new DirectoryNotFoundException("Steam installation path not found.");
            }

            return steamPath;
        }

        /// <summary>
        /// Retrieves all library paths where Steam games may be installed.
        /// </summary>
        private static List<string> GetLibraryPaths()
        {
            var libraryPaths = new List<string>();
            var steamPath = GetSteamPath();

            if (!string.IsNullOrEmpty(steamPath))
            {
                libraryPaths.Add(steamPath);

                var libraryFoldersPath = Path.Combine(steamPath, @"steamapps\libraryfolders.vdf");
                if (File.Exists(libraryFoldersPath))
                {
                    var content = File.ReadAllLines(libraryFoldersPath);

                    foreach (var line in content)
                    {
                        var match = Regex.Match(line, "\"path\"\\s+\"([^\"]+)\"");
                        if (match.Success)
                        {
                            var path = match.Groups[1].Value.Replace("\\\\", "\\");
                            if (Directory.Exists(path))
                                libraryPaths.Add(path);
                        }
                    }
                }
            }

            return libraryPaths;
        }

        /// <summary>
        /// Retrieves all installed Steam app names from the library paths.
        /// </summary>
        private static List<string> GetInstalledApps(List<string> libraryPaths)
        {
            var installedApps = new List<string>();

            foreach (var path in libraryPaths)
            {
                var commonPath = Path.Combine(path, @"steamapps\common");

                if (Directory.Exists(commonPath))
                {
                    installedApps.AddRange(Directory.GetDirectories(commonPath).Select(Path.GetFileName));
                }
            }

            return installedApps;
        }

        /// <summary>
        /// Checks if SteamVR is running in beta mode.
        /// </summary>
        public static bool IsSteamVRBeta()
        {
            try
            {
                var steamPath = GetSteamPath();
                var manifestPath = Path.Combine(steamPath, @"steamapps\appmanifest_250820.acf");

                if (File.Exists(manifestPath))
                {
                    var content = File.ReadAllText(manifestPath);
                    var match = Regex.Match(content, "\"betakey\"\\s*\"([^\"]+)\"", RegexOptions.IgnoreCase);

                    return match.Success && !string.IsNullOrEmpty(match.Groups[1].Value);
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, "Error checking if SteamVR is beta.");
            }

            return false;
        }

        /// <summary>
        /// Retrieves detailed information about installed Steam apps.
        /// </summary>
        public static List<SteamAppDetails> GetSteamAppDetails()
        {
            var appDetailsList = new List<SteamAppDetails>();
            var manifestsPath = @"C:\Program Files (x86)\Steam\steamapps";
            var imageRootPath = @"C:\Program Files (x86)\Steam\appcache\librarycache";

            try
            {
                if (Directory.Exists(manifestsPath))
                {
                    var manifestFiles = Directory.GetFiles(manifestsPath, "appmanifest_*.acf");

                    foreach (var manifestFile in manifestFiles)
                    {
                        var content = File.ReadAllText(manifestFile);
                        var nameMatch = Regex.Match(content, "\"name\"\\s*\"([^\"]+)\"");
                        var idMatch = Regex.Match(Path.GetFileName(manifestFile), @"appmanifest_(\d+).acf");

                        if (nameMatch.Success && idMatch.Success)
                        {
                            var appName = nameMatch.Groups[1].Value;
                            var appID = idMatch.Groups[1].Value;
                            var installPath = Path.Combine(manifestsPath, "common", appName);
                            var imagePath = Path.Combine(imageRootPath, $"{appID}_library_600x900.jpg");

                            appDetailsList.Add(new SteamAppDetails
                            {
                                Name = appName,
                                ID = appID,
                                InstallPath = Directory.Exists(installPath) ? installPath : "N/A",
                                ImagePath = File.Exists(imagePath) ? imagePath : null
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, "Error retrieving Steam app details.");
            }

            return appDetailsList;
        }
    }
}
