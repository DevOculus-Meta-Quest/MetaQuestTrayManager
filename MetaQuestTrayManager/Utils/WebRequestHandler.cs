using System;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MetaQuestTrayManager.Utils
{
    public class WebRequestHandler
    {
        private static readonly HttpClient HttpClient = new HttpClient();

        /// <summary>
        /// Sends an HTTP request and retrieves the response as a string.
        /// </summary>
        /// <param name="url">The target URL.</param>
        /// <param name="method">The HTTP method (e.g., "GET", "POST").</param>
        /// <param name="formParams">Optional form parameters for POST requests.</param>
        /// <param name="contentType">Optional content type (e.g., "application/json").</param>
        /// <returns>The response as a string.</returns>
        public async Task<string> GetPageHTMLAsync(
            string url,
            string method = "GET",
            string formParams = "",
            string contentType = "application/x-www-form-urlencoded")
        {
            url = ValidateAndFormatUrl(url);

            try
            {
                HttpRequestMessage request = new HttpRequestMessage
                {
                    Method = new HttpMethod(method),
                    RequestUri = new Uri(url)
                };

                if (method.Equals("POST", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(formParams))
                {
                    request.Content = new StringContent(formParams, Encoding.UTF8, contentType);
                }

                HttpResponseMessage response = await HttpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                return HandleWebException(ex);
            }
        }

        /// <summary>
        /// Validates and formats the URL.
        /// </summary>
        private string ValidateAndFormatUrl(string url)
        {
            if (url.Contains("&amp;"))
                url = url.Replace("&amp;", "&");

            if (!Uri.IsWellFormedUriString(url, UriKind.Absolute))
                throw new ArgumentException("Invalid URL format.", nameof(url));

            return url;
        }

        /// <summary>
        /// Handles web request exceptions.
        /// </summary>
        private static string HandleWebException(Exception ex)
        {
            // Log the exception details for debugging.
            ErrorLogger.LogError(ex, "Web request failed");

            // Provide user-friendly error messages.
            if (ex is HttpRequestException httpEx)
            {
                return httpEx.StatusCode.HasValue ? $"HTTP Error: {(int)httpEx.StatusCode}" : "HTTP Request Error";
            }

            if (ex.Message.Contains("Unable to connect to the remote server"))
            {
                return "Offline";
            }

            return "An error occurred while fetching the webpage.";
        }
    }
}
