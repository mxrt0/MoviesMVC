using Newtonsoft.Json;
namespace MoviesMVC.Models;

public class OmdbMovie
{
    [JsonProperty("imdbRating")]
    public string ImdbRating { get; set; }

    [JsonProperty("Actors")]
    public string Actors { get; set; }
}
