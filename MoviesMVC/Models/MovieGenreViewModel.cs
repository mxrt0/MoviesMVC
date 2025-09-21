using Microsoft.AspNetCore.Mvc.Rendering;

namespace MoviesMVC.Models;

public class MovieGenreViewModel
{
    public List<Movie>? Movies { get; set; }
    public SelectList? Genres { get; set; }
    public string? MovieGenre { get; set; }
    public string? SearchString { get; set; }
    public bool FavoritesOnly { get; set; } = false;
    public string? CurrentSort { get; set; }
    public bool SortDescending { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int PageCount { get; set; }
}
