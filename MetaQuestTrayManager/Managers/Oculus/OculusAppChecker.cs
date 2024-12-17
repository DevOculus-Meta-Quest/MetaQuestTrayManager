using MetaQuestTrayManager.Utils;
using Microsoft.Win32;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace MetaQuestTrayManager.Managers.Oculus
{
    /// <summary>
    /// Represents details about an Oculus application.
    /// </summary>
    public class OculusAppDetails
    {
        public string Name { get; set; }  // Application name
        public string ID { get; set; }    // Application ID
        public string InstallPath { get; set; }  // Path where the app is installed
        public string ImagePath { get; set; }    // Path to the app's cover image
    }

    /// <summary>
    /// Utility class to check Oculus apps and their installation details.
    /// </summary>
    public static class OculusAppChecker
    {
        private static List<string> _installedAppsCache;  // Cached list of installed apps

        /// <summary>
        /// Checks if a specific Oculus app is installed.
        /// </summary>
        /// <param name="appName">The name of the app to check.</param>
        /// <returns>True if the app is installed; otherwise, false.</returns>
        public static bool IsOculusAppInstalled(string appName)
        {
            try
            {
                // Populate cache if null
                _installedAppsCache ??= GetInstalledApps(GetOculusLibraryPaths());

                // Case-insensitive search for the app
                return _installedAppsCache.Contains(appName, StringComparer.OrdinalIgnoreCase);
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, "Error checking Oculus app installation.");
                return false;
            }
        }

        /// <summary>
        /// Retrieves all installed Oculus apps.
        /// </summary>
        /// <returns>List of installed Oculus app names.</returns>
        public static List<string> GetInstalledApps()
        {
            // Force cache update
            IsOculusAppInstalled("CacheCheck");
            return _installedAppsCache;
        }

        /// <summary>
        /// Checks if Oculus software is installed on the system.
        /// </summary>
        /// <returns>True if Oculus is installed; otherwise, false.</returns>
        public static bool IsOculusInstalled()
        {
            try
            {
                using var key = Registry.LocalMachine.OpenSubKey(@"Software\Oculus VR, LLC\Oculus");
                var installDir = key?.GetValue("InstallDir") as string;

                return !string.IsNullOrEmpty(installDir) && Directory.Exists(installDir);
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, "Error checking Oculus installation status.");
                return false;
            }
        }

        /// <summary>
        /// Retrieves the list of Oculus library paths from the registry.
        /// </summary>
        /// <returns>List of Oculus library paths.</returns>
        private static List<string> GetOculusLibraryPaths()
        {
            var oculusPaths = new List<string>();

            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(@"Software\Oculus VR, LLC\Oculus\Libraries");
                if (key != null)
                {
                    foreach (string subKeyName in key.GetSubKeyNames())
                    {
                        using var subKey = key.OpenSubKey(subKeyName);
                        var path = subKey?.GetValue("OriginalPath") as string;

                        if (!string.IsNullOrEmpty(path))
                        {
                            var adjustedPath = Path.Combine(path, "Software");
                            if (Directory.Exists(adjustedPath))
                            {
                                oculusPaths.Add(adjustedPath);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, "Error retrieving Oculus library paths.");
            }

            return oculusPaths;
        }

        /// <summary>
        /// Retrieves installed Oculus apps based on library paths.
        /// </summary>
        /// <param name="oculusPaths">Paths to Oculus libraries.</param>
        /// <returns>List of installed app names.</returns>
        private static List<string> GetInstalledApps(List<string> oculusPaths)
        {
            var installedApps = new List<string>();

            foreach (var oculusPath in oculusPaths)
            {
                if (Directory.Exists(oculusPath))
                {
                    var appDirectories = Directory.GetDirectories(oculusPath)
                        .Select(Path.GetFileName)
                        .Where(name => !Regex.IsMatch(name, @"^[A-Z]_"))
                        .ToList();

                    installedApps.AddRange(appDirectories);
                }
            }

            return installedApps;
        }

        /// <summary>
        /// Retrieves detailed information for all installed Oculus apps.
        /// </summary>
        /// <returns>List of OculusAppDetails.</returns>
        public static List<OculusAppDetails> GetOculusAppDetails()
        {
            const string manifestsPath = @"C:\\Program Files\\Oculus\\CoreData\\Manifests";
            const string storeAssetsPath = @"C:\\Program Files\\Oculus\\CoreData\\Software\\StoreAssets";

            var appDetailsList = new List<OculusAppDetails>();

            if (Directory.Exists(manifestsPath))
            {
                foreach (var manifestFile in Directory.GetFiles(manifestsPath, "*.json"))
                {
                    try
                    {
                        var jsonData = File.ReadAllText(manifestFile);
                        var jsonObject = JObject.Parse(jsonData);

                        var appName = jsonObject["canonicalName"]?.ToString().Replace("_assets", "").Replace("-", " ");
                        var appID = jsonObject["appId"]?.ToString();
                        var installPath = jsonObject["install_path"]?.ToString();

                        var assetFolderName = ConvertAppNameToAssetFolderName(appName.Replace(" ", "-"));
                        var imagePath = Path.Combine(storeAssetsPath, assetFolderName, "cover_square_image.jpg");

                        appDetailsList.Add(new OculusAppDetails
                        {
                            Name = appName,
                            ID = appID,
                            InstallPath = installPath,
                            ImagePath = File.Exists(imagePath) ? imagePath : null
                        });
                    }
                    catch (Exception ex)
                    {
                        ErrorLogger.LogError(ex, $"Error parsing manifest file: {manifestFile}");
                    }
                }
            }

            return appDetailsList;
        }

        /// <summary>
        /// Converts an app name to its asset folder equivalent.
        /// </summary>
        /// <param name="appName">The app name to convert.</param>
        /// <returns>Formatted asset folder name.</returns>
        private static string ConvertAppNameToAssetFolderName(string appName)
            => appName.Replace(" ", "-").ToLower() + "_assets";
    }
}
