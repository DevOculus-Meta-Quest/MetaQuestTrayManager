using MetaQuestTrayManager.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace MetaQuestTrayManager.Managers.Steam
{
    public static class SteamSoftwareFunctions
    {
        private static readonly string SteamUserDataPath = @"C:\Program Files (x86)\Steam\userdata";
        private static readonly string OculusStoreAssetsPath = @"C:\Program Files\Oculus\CoreData\Software\StoreAssets";

        /// <summary>
        /// Retrieves details of non-Steam applications from VDF files.
        /// </summary>
        public static List<NonSteamAppDetails> GetNonSteamAppDetails()
        {
            var nonSteamApps = new List<NonSteamAppDetails>();

            if (!Directory.Exists(SteamUserDataPath))
            {
                ErrorLogger.LogError(new DirectoryNotFoundException(), $"Steam userdata directory not found at {SteamUserDataPath}");
                return nonSteamApps;
            }

            Debug.WriteLine($"Searching for VDF files in {SteamUserDataPath}");

            foreach (var userDirectory in Directory.GetDirectories(SteamUserDataPath))
            {
                var vdfFilePath = Path.Combine(userDirectory, @"config\shortcuts.vdf");

                if (!File.Exists(vdfFilePath))
                {
                    Debug.WriteLine($"VDF file not found at {vdfFilePath}");
                    continue;
                }

                try
                {
                    var tempFilePath = Path.GetTempFileName();
                    WriteParsedDataToTempFile(vdfFilePath, tempFilePath);
                    var apps = ReadDataFromTempFile(tempFilePath);

                    foreach (var app in apps)
                    {
                        app.ImagePath = FindImagePath(OculusStoreAssetsPath, app.Name);
                        Debug.WriteLine($"Image path for {app.Name}: {app.ImagePath}");
                    }

                    nonSteamApps.AddRange(apps);
                    File.Delete(tempFilePath); // Clean up temp file
                }
                catch (Exception ex)
                {
                    ErrorLogger.LogError(ex, $"Error processing VDF file at {vdfFilePath}");
                }
            }

            return nonSteamApps;
        }

        /// <summary>
        /// Parses the VDF file and writes JSON data to a temporary file.
        /// </summary>
        private static void WriteParsedDataToTempFile(string vdfFilePath, string tempFilePath)
        {
            try
            {
                var parser = new VdfParser();
                var parsedData = parser.ParseVdf(vdfFilePath);

                Debug.WriteLine("Parsed VDF Data: " + JsonConvert.SerializeObject(parsedData));

                var jsonData = JsonConvert.SerializeObject(parsedData, Formatting.Indented);
                File.WriteAllText(tempFilePath, jsonData);
                Debug.WriteLine($"Written JSON data to temp file at {tempFilePath}");
            }
            catch (Exception ex)
            {
                throw new Exception($"Error parsing VDF file: {vdfFilePath}", ex);
            }
        }

        /// <summary>
        /// Reads non-Steam app details from a temporary JSON file.
        /// </summary>
        private static List<NonSteamAppDetails> ReadDataFromTempFile(string tempFilePath)
        {
            var nonSteamApps = new List<NonSteamAppDetails>();

            try
            {
                var jsonData = File.ReadAllText(tempFilePath);
                Debug.WriteLine("Read JSON data from temp file: " + jsonData);

                var parsedData = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonData);

                if (parsedData?.ContainsKey("shortcuts") == true)
                {
                    var shortcuts = parsedData["shortcuts"] as JObject;

                    foreach (var shortcutEntry in shortcuts)
                    {
                        var shortcutDetails = shortcutEntry.Value?.ToObject<Dictionary<string, object>>();
                        if (shortcutDetails == null) continue;

                        var details = new NonSteamAppDetails
                        {
                            Name = shortcutDetails.ContainsKey("AppName") ? shortcutDetails["AppName"]?.ToString() : "Unknown App",
                            ExePath = shortcutDetails.ContainsKey("Exe") ? shortcutDetails["Exe"]?.ToString() : "Unknown Path"
                        };

                        nonSteamApps.Add(details);
                        Debug.WriteLine($"Added NonSteamApp: {details.Name}, Path: {details.ExePath}");
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error reading data from temp file: {tempFilePath}", ex);
            }

            return nonSteamApps;
        }

        /// <summary>
        /// Attempts to locate the image path for a given app name.
        /// </summary>
        private static string FindImagePath(string basePath, string appName)
        {
            if (!Directory.Exists(basePath))
            {
                Debug.WriteLine($"Base path for images does not exist: {basePath}");
                return "Image Not Found";
            }

            var searchName = appName.Replace(" ", "");
            var directories = Directory.GetDirectories(basePath, $"*{searchName}*", SearchOption.AllDirectories);

            foreach (var dir in directories)
            {
                var imagePath = Path.Combine(dir, "cover_square_image.jpg");
                if (File.Exists(imagePath))
                {
                    Debug.WriteLine($"Found image for {appName} at {imagePath}");
                    return imagePath;
                }
            }

            Debug.WriteLine($"Image not found for {appName}");
            return "Image Not Found";
        }

        /// <summary>
        /// Represents details of a non-Steam application.
        /// </summary>
        public class NonSteamAppDetails
        {
            public string Name { get; set; }
            public string ExePath { get; set; }
            public string ImagePath { get; set; }
        }
    }
}
