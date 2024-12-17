using MetaQuestTrayManager.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

#nullable disable

namespace MetaQuestTrayManager.Managers.Oculus
{
    public static class OculusSoftwareFunctions
    {
        /// <summary>
        /// Checks if Oculus software is installed.
        /// </summary>
        /// <returns>True if Oculus is installed; otherwise, false.</returns>
        public static bool IsOculusInstalled()
        {
            try
            {
                // Replace with a valid implementation from your OculusAppChecker class
                return OculusAppChecker.IsOculusInstalled();
            }
            catch (Exception ex)
            {
                // Log the error using a centralized logger
                ErrorLogger.LogError(ex, "An error occurred while checking if Oculus is installed.");
                return false;
            }
        }

        /// <summary>
        /// Retrieves a list of all installed Oculus applications.
        /// </summary>
        /// <param name="oculusPaths">A list of Oculus paths to search.</param>
        /// <returns>A list of installed Oculus app names.</returns>
        public static List<string> GetInstalledApps(List<string> oculusPaths)
        {
            var installedApps = new List<string>();

            try
            {
                foreach (string oculusPath in oculusPaths)
                {
                    // Ensure the path exists
                    if (Directory.Exists(oculusPath))
                    {
                        var appDirectories = Directory.GetDirectories(oculusPath)
                            .Select(Path.GetFileName)
                            .Where(name => !string.IsNullOrEmpty(name)) // Ensure valid names
                            .Where(name => !Regex.IsMatch(name, @"^[A-Z]_:")) // Exclude directories starting with drive letters
                            .Select(name => name.Replace("-", " ")) // Replace "-" with spaces
                            .ToList();

                        installedApps.AddRange(appDirectories);
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the error and continue
                ErrorLogger.LogError(ex, "An error occurred while retrieving installed Oculus apps.");
            }

            return installedApps;
        }
    }
}
