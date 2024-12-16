using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using MetaQuestTrayManager.Utils;

namespace MetaQuestTrayManager.Utils
{
    public static class FileExplorerUtilities
    {
        /// <summary>
        /// Opens the file explorer and selects the specified file.
        /// </summary>
        public static void ShowFileInDirectory(string fullPath)
        {
            try
            {
                if (File.Exists(fullPath))
                {
                    Process.Start("explorer.exe", $@"/select,""{fullPath}""");
                }
                else
                {
                    ErrorLogger.LogError(new FileNotFoundException($"File not found: {fullPath}"));
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, "Failed to show file in directory.");
            }
        }

        /// <summary>
        /// Opens a dialog to select a single file.
        /// </summary>
        public static string OpenSingle(string defaultDirectory = "", string defaultExtension = "", string fileExtensionFilters = "*.*;", bool mustExist = true)
        {
            try
            {
                var files = DoFileBrowser(defaultDirectory, defaultExtension, fileExtensionFilters, false, mustExist);
                return files.Count == 1 ? files[0] : string.Empty;
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, "Failed to open file dialog for a single file.");
                return string.Empty;
            }
        }

        /// <summary>
        /// Opens a dialog to select multiple files.
        /// </summary>
        public static List<string> OpenMultiple(string defaultDirectory = "", string defaultExtension = "", string fileExtensionFilters = "*.*;", bool mustExist = true)
        {
            try
            {
                return DoFileBrowser(defaultDirectory, defaultExtension, fileExtensionFilters, true, mustExist);
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, "Failed to open file dialog for multiple files.");
                return new List<string>();
            }
        }

        /// <summary>
        /// Performs the core file browsing logic.
        /// </summary>
        private static List<string> DoFileBrowser(string defaultDirectory, string defaultExtension, string fileExtensionFilters, bool multipleFiles, bool mustExist)
        {
            var files = new List<string>();

            try
            {
                if (string.IsNullOrEmpty(defaultDirectory))
                {
                    defaultDirectory = GetCurrentExecutableDirectory();
                }

                var fileTypes = ParseFileExtensions(fileExtensionFilters);

                var dlg = new OpenFileDialog
                {
                    DefaultExt = defaultExtension,
                    Filter = BuildFilterString(fileTypes),
                    InitialDirectory = defaultDirectory,
                    AddExtension = !string.IsNullOrEmpty(defaultExtension),
                    CheckFileExists = mustExist,
                    CheckPathExists = true,
                    ValidateNames = true,
                    Multiselect = multipleFiles
                };

                if (dlg.ShowDialog() == true)
                {
                    files.AddRange(dlg.FileNames);
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, "Error occurred in file browsing dialog.");
            }

            return files;
        }

        /// <summary>
        /// Parses file extensions into a dictionary for the file dialog filter.
        /// </summary>
        private static Dictionary<string, string> ParseFileExtensions(string fileExtensionFilters)
        {
            var fileTypes = new Dictionary<string, string>();

            if (!fileExtensionFilters.Contains("*.*"))
            {
                var splitFilters = fileExtensionFilters.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var ext in splitFilters)
                {
                    if (!fileTypes.ContainsKey(ext))
                    {
                        var name = GetDescription(ext.Replace("*", ""));
                        name = string.IsNullOrEmpty(name) ? ext.Replace("*.", "").ToUpper() + " File" : name;
                        fileTypes.Add(ext, name);
                    }
                }
            }
            else
            {
                fileTypes.Add("*.*", "All Files");
            }

            return fileTypes;
        }

        /// <summary>
        /// Builds the filter string for the OpenFileDialog.
        /// </summary>
        private static string BuildFilterString(Dictionary<string, string> fileTypes)
        {
            var filterString = "Filtered Files|";
            foreach (var filter in fileTypes)
            {
                filterString += $"{filter.Key};";
            }

            filterString += "|" + string.Join("|", fileTypes.Select(ft => $"{ft.Value}|{ft.Key}"));
            return filterString.TrimEnd('|');
        }

        /// <summary>
        /// Reads the default value of a registry key.
        /// </summary>
        private static string? ReadDefaultValue(string regKey)
        {
            try
            {
                using var key = Registry.ClassesRoot.OpenSubKey(regKey, false);
                return key?.GetValue("") as string;
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, $"Failed to read default value for registry key: {regKey}");
                return null;
            }
        }

        /// <summary>
        /// Gets the description of a file extension.
        /// </summary>
        private static string GetDescription(string ext)
        {
            if (ext.StartsWith(".") && ext.Length > 1) ext = ext.Substring(1);

            var description = ReadDefaultValue(ext + "file");
            if (!string.IsNullOrEmpty(description)) return description;

            using var key = Registry.ClassesRoot.OpenSubKey("." + ext, false);
            using var subKey = key?.OpenSubKey("OpenWithProgids");

            return subKey?.GetValueNames()
                .Select(ReadDefaultValue)
                .FirstOrDefault(name => !string.IsNullOrEmpty(name)) ?? string.Empty;
        }

        /// <summary>
        /// Gets the directory of the currently running executable.
        /// </summary>
        private static string GetCurrentExecutableDirectory()
        {
            try
            {
                return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty;
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, "Failed to get current executable directory.");
                return string.Empty;
            }
        }
    }
}
