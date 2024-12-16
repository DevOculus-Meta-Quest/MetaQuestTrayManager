using System;
using System.Diagnostics;
using MetaQuestTrayManager.Utils;

namespace MetaQuestTrayManager.Utils
{
    public static class WebUtilities
    {
        /// <summary>
        /// Opens the specified URL in the default web browser.
        /// </summary>
        /// <param name="url">The URL to open.</param>
        public static void OpenURL(string url)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(url))
                {
                    ErrorLogger.LogError(new ArgumentException("URL cannot be null or empty."), "Failed to open URL.");
                    return;
                }

                if (!Uri.IsWellFormedUriString(url, UriKind.Absolute))
                {
                    ErrorLogger.LogError(new ArgumentException("Invalid URL format."), $"Failed to open URL: {url}");
                    return;
                }

                var processStartInfo = new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true,
                    Verb = "open"
                };

                Process.Start(processStartInfo);
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, $"Failed to open URL: {url}");
            }
        }
    }
}
