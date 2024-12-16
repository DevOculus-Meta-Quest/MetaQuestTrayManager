using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using MetaQuestTrayManager.Utils; // Ensure ErrorLogger is available

namespace MetaQuestTrayManager.Managers.Steam
{
    /// <summary>
    /// Handles communication with the SteamGridDB API to fetch game assets like covers, logos, and icons.
    /// </summary>
    public class SteamApiManager
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        /// <summary>
        /// Initializes the SteamApiManager with an HTTP client and retrieves the API key from environment variables.
        /// </summary>
        public SteamApiManager()
        {
            _httpClient = new HttpClient();

            // Retrieve the SteamGridDB API key from environment variables
            _apiKey = Environment.GetEnvironmentVariable("STEAMGRIDDB_API_KEY");

            if (string.IsNullOrEmpty(_apiKey))
            {
                throw new InvalidOperationException("SteamGridDB API key not found. Set the 'STEAMGRIDDB_API_KEY' environment variable.");
            }

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
        }

        /// <summary>
        /// Fetches the game cover image by SteamGridDB Game ID.
        /// </summary>
        public async Task<string> GetGameCoverByGridDbIdAsync(string gameId)
        {
            return await FetchImageUrlAsync($"https://www.steamgriddb.com/api/v2/grids/game/{gameId}");
        }

        /// <summary>
        /// Fetches the game cover image by Steam App ID.
        /// </summary>
        public async Task<string> GetGameCoverBySteamIdAsync(string steamAppId)
        {
            return await FetchImageUrlAsync($"https://www.steamgriddb.com/api/v2/grids/steam/{steamAppId}");
        }

        /// <summary>
        /// Fetches the game logo by SteamGridDB Game ID.
        /// </summary>
        public async Task<string> GetGameLogoByGridDbIdAsync(string gameId)
        {
            return await FetchImageUrlAsync($"https://www.steamgriddb.com/api/v2/logos/game/{gameId}");
        }

        /// <summary>
        /// Fetches the game icon by SteamGridDB Game ID.
        /// </summary>
        public async Task<string> GetGameIconByGridDbIdAsync(string gameId)
        {
            return await FetchImageUrlAsync($"https://www.steamgriddb.com/api/v2/icons/game/{gameId}");
        }

        /// <summary>
        /// Searches for games using a search term.
        /// </summary>
        public async Task<string> SearchGamesAsync(string searchTerm)
        {
            try
            {
                var response = await _httpClient.GetAsync($"https://www.steamgriddb.com/api/v2/search/autocomplete/{searchTerm}");
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                return content;
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, $"Error performing search for term: {searchTerm}");
                return null;
            }
        }

        /// <summary>
        /// Generic method to fetch an image URL from the provided API endpoint.
        /// </summary>
        private async Task<string> FetchImageUrlAsync(string requestUri)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{requestUri}?key={_apiKey}");
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var json = JObject.Parse(content);

                // Check if the response contains image data
                if (json["data"] != null && json["data"].Any())
                {
                    var imageUrl = json["data"][0]["url"]?.ToString();
                    return imageUrl;
                }

                Debug.WriteLine($"No image data found for request: {requestUri}");
                return null;
            }
            catch (HttpRequestException httpEx)
            {
                ErrorLogger.LogError(httpEx, "HTTP error occurred while fetching image URL.");
                return null;
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, $"Error fetching image URL for request: {requestUri}");
                return null;
            }
        }
    }
}
