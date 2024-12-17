using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using MetaQuestTrayManager.Utils; // For ErrorLogger

#nullable disable

namespace MetaQuestTrayManager.Managers.Steam
{
    /// <summary>
    /// Manages installed Steam games by retrieving their details.
    /// </summary>
    internal class SteamGameManager
    {
        /// <summary>
        /// Represents the details of a Steam game.
        /// </summary>
        public class SteamGameDetails
        {
            public string Name { get; set; }
            public string ID { get; set; }
            public string Path { get; set; }
            public string ImagePath { get; set; } // Property for the image path
        }

        /// <summary>
        /// Retrieves a list of installed Steam games, including paths and image locations.
        /// </summary>
        public List<SteamGameDetails> GetInstalledGames()
        {
            var gamesList = new List<SteamGameDetails>();
            var steamMainFolder = @"C:\Program Files (x86)\Steam"; // Default path, adjust if necessary
            var libraryFoldersFile = Path.Combine(steamMainFolder, @"steamapps\libraryfolders.vdf");
            var libraryFolders = new List<string> { steamMainFolder };

            try
            {
                // Read the library folders file to get all Steam library paths
                if (File.Exists(libraryFoldersFile))
                {
                    var libraryFoldersContent = File.ReadAllText(libraryFoldersFile);
                    var matches = Regex.Matches(libraryFoldersContent, "\"path\"\\s*\"([^\"]+)\"");

                    foreach (Match match in matches)
                    {
                        var folderPath = match.Groups[1].Value.Replace(@"\\", @"\");
                        if (Directory.Exists(folderPath))
                        {
                            libraryFolders.Add(folderPath);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, "Error reading Steam library folders file.");
            }

            // Search each library folder for installed games
            foreach (var libraryFolder in libraryFolders)
            {
                var steamAppsFolder = Path.Combine(libraryFolder, "steamapps");

                if (Directory.Exists(steamAppsFolder))
                {
                    try
                    {
                        foreach (var filePath in Directory.GetFiles(steamAppsFolder, "appmanifest_*.acf"))
                        {
                            var gameDetails = ParseGameManifest(filePath);
                            if (gameDetails != null)
                            {
                                gamesList.Add(gameDetails);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        ErrorLogger.LogError(ex, $"Error processing library folder: {steamAppsFolder}");
                    }
                }
            }

            // Assign the image path for each game
            foreach (var game in gamesList)
            {
                game.ImagePath = GetImagePathForGame(game.ID);
            }

            return gamesList;
        }

        /// <summary>
        /// Parses a Steam appmanifest file to extract game details.
        /// </summary>
        private SteamGameDetails ParseGameManifest(string manifestPath)
        {
            try
            {
                var fileContent = File.ReadAllText(manifestPath);

                var idMatch = Regex.Match(fileContent, "\"appid\"\\s*\"(\\d+)\"");
                var nameMatch = Regex.Match(fileContent, "\"name\"\\s*\"([^\"]+)\"");
                var pathMatch = Regex.Match(fileContent, "\"installdir\"\\s*\"([^\"]+)\"");

                if (idMatch.Success && nameMatch.Success && pathMatch.Success)
                {
                    var gameDetails = new SteamGameDetails
                    {
                        ID = idMatch.Groups[1].Value,
                        Name = nameMatch.Groups[1].Value,
                        Path = Path.Combine(Path.GetDirectoryName(manifestPath), "common", pathMatch.Groups[1].Value)
                    };

                    return gameDetails;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, $"Error parsing manifest file: {manifestPath}");
            }

            return null;
        }

        /// <summary>
        /// Finds the image path for a game based on its ID.
        /// </summary>
        private string GetImagePathForGame(string gameId)
        {
            try
            {
                var imageCacheDirectory = @"C:\Program Files (x86)\Steam\appcache\librarycache"; // Set to your image cache directory
                var searchPattern = $"{gameId}_library_600x900.*";

                if (Directory.Exists(imageCacheDirectory))
                {
                    var imageFiles = Directory.GetFiles(imageCacheDirectory, searchPattern);

                    if (imageFiles.Length > 0)
                    {
                        return imageFiles[0];
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, $"Error finding image for game ID: {gameId}");
            }

            return null; // Return null if image is not found
        }
    }
}
