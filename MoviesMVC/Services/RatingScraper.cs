using MoviesMVC.Models;
using System.Text.Json;

namespace MoviesMVC.Services;

public class RatingScraper
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly string _apiKey;
    private readonly JsonSerializerOptions _options;
    public RatingScraper(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri("https://www.omdbapi.com/");
        _configuration = configuration;
        _apiKey = GetApiKey();
        _options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }
    public string GetApiKey()
    {
        return _configuration["API_KEY"]!;
    }

    public async Task<OmdbMovie?> GetMovieByTitleAsync(string title)
    {
        var url = $"http://www.omdbapi.com/?t={Uri.EscapeDataString(title)}&apikey={_apiKey}";
        var response = await _httpClient.GetStringAsync(url);

        var movie = JsonSerializer.Deserialize<OmdbMovie>(response, _options);

        return movie;
    }
}