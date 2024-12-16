using System;

namespace MetaQuestTrayManager.Utils
{
    public static class StringManipulationUtilities
    {
        /// <summary>
        /// Removes a specified substring from the end of a given string.
        /// </summary>
        public static string RemoveStringFromEnd(string text, string remove)
        {
            if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(remove))
                return text;

            return text.EndsWith(remove, StringComparison.OrdinalIgnoreCase)
                ? text.Substring(0, text.Length - remove.Length)
                : text;
        }

        /// <summary>
        /// Removes a specified substring from the start of a given string.
        /// </summary>
        public static string RemoveStringFromStart(string text, string remove)
        {
            if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(remove))
                return text;

            return text.StartsWith(remove, StringComparison.OrdinalIgnoreCase)
                ? text.Substring(remove.Length)
                : text;
        }

        /// <summary>
        /// Validates whether a string is a well-formed URL.
        /// </summary>
        public static bool IsValidUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return false;

            return Uri.TryCreate(url, UriKind.Absolute, out Uri? uriResult)
                && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
        }

        /// <summary>
        /// Returns a full URL with "http://" prefix if missing.
        /// </summary>
        public static string GetFullUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return string.Empty;

            if (IsValidUrl(url))
            {
                return url.StartsWith("http", StringComparison.OrdinalIgnoreCase)
                    ? url
                    : $"http://{url}";
            }

            return url;
        }
    }
}
