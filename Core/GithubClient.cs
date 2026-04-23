using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Orchestrate.Core
{
    public class GithubClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public GithubClient(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _apiKey = config["Keys:Github"] ?? "";
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("MyApp");
        }

        public async Task<List<GithubRepo>> GetUserReposAsync(int page = 1, int perPage = 100)
        {
            var url = $"user/repos?per_page={perPage}&page={page}";

            var response = await _httpClient.GetAsync(url);
            var responseString = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"GitHub API Error: {response.StatusCode}\n{responseString}");
            }

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            return JsonSerializer.Deserialize<List<GithubRepo>>(responseString, options) ?? new List<GithubRepo>();
        }
    }

    public class GithubRepo
    {
        [JsonPropertyName("name")] public string? Name { get; set; }
        [JsonPropertyName("full_name")] public string? FullName { get; set; }
        [JsonPropertyName("private")] public bool IsPrivate { get; set; }
        [JsonPropertyName("html_url")] public string? URL { get; set; }
    }
}