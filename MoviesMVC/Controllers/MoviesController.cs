using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MoviesMVC.Data;
using MoviesMVC.Models;
using MoviesMVC.Services;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;

namespace MoviesMVC.Controllers
{
    public class MoviesController : Controller
    {
        private readonly MoviesDbContext _context;
        private readonly RatingScraper _scraper;

        public MoviesController(MoviesDbContext context, RatingScraper scraper)
        {
            _context = context;
            _scraper = scraper;
        }

        public async Task<IActionResult> Index(string movieGenre, string searchString, bool favoritesOnly)
        {
            try
            {
                if (_context.Movie == null)
                {
                    return Problem("Entity set 'MoviesDbContext.Movie'  is null.");
                }

                IQueryable<string?> genreQuery = _context.Movie.OrderBy(m => m.Genre).Select(m => m.Genre);

                IQueryable<Movie> movies = _context.Movie;

                ViewData["HasMovies"] = movies.Any();

                if (!string.IsNullOrEmpty(movieGenre))
                {
                    movies = movies.Where(x => x.Genre == movieGenre);
                }

                if (!string.IsNullOrEmpty(searchString))
                {
                    movies = movies.Where(movie => EF.Functions.Like(movie.Title!, $"%{searchString}%"));
                }

                var movieGenreVM = new MovieGenreViewModel
                {
                    Genres = new SelectList(await genreQuery.Distinct().ToListAsync()),
                    Movies = favoritesOnly ? await movies.Where(m => m.Favorited).ToListAsync() : await movies.ToListAsync(),
                    SearchString = searchString,
                    FavoritesOnly = favoritesOnly,
                    MovieGenre = movieGenre
                };

                ViewData["TotalPrice"] = movieGenreVM.Movies.Sum(m => m.Price);
                ViewData["HasMatchingMovies"] = movieGenreVM.Movies.Any();
                return View(movieGenreVM);
            }
            catch (Exception)
            {
                return Problem("An unexpected error occurred.");
            }

        }

        public async Task<IActionResult> Details(int? id)
        {
            try
            {
                if (id == null)
                {
                    return NotFound();
                }

                var movie = await _context.Movie
                    .FirstOrDefaultAsync(m => m.Id == id);
                if (movie == null)
                {
                    return NotFound();
                }

                return View(movie);
            }
            catch (Exception)
            {
                return Problem("An unexpected error occurred while showing details.");
            }
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,ReleaseDate,Genre,Price,Rating")] Movie movie)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    try
                    {
                        var omdbMovie = await _scraper.GetMovieByTitleAsync(movie.Title!);
                        movie.ImdbRating = !string.IsNullOrEmpty(omdbMovie?.ImdbRating) && double.TryParse(omdbMovie.ImdbRating, out _)
                            ? double.Parse(omdbMovie.ImdbRating)
                            : default;
                        movie.Actors = !string.IsNullOrEmpty(omdbMovie?.Actors)
                            ? omdbMovie.Actors.Split(", ", StringSplitOptions.RemoveEmptyEntries).Take(2).ToList()
                            : new();
                    }
                    catch (HttpRequestException)
                    {
                        movie.ImdbRating = default;
                    }
                    _context.Add(movie);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Movie added successfully!";
                    return RedirectToAction(nameof(Index));
                }
                return View(movie);
            }
            catch (Exception ex)
            {
                return Problem($"Error saving movie: {ex.Message}");
            }
        }

        public async Task<IActionResult> Edit(int? id)
        {
            try
            {
                if (id == null)
                {
                    return NotFound();
                }

                var movie = await _context.Movie.FindAsync(id);
                if (movie == null)
                {
                    return NotFound();
                }
                return View(movie);
            }
            catch (Exception)
            {
                return Problem("An unexpected error occurred.");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,ReleaseDate,Genre,Price, Rating")] Movie movie)
        {
            try
            {
                if (id != movie.Id)
                {
                    return NotFound();
                }

                if (ModelState.IsValid)
                {
                    try
                    {
                        _context.Update(movie);
                        await _context.SaveChangesAsync();
                        TempData["SuccessMessage"] = "Movie edited successfully!";
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        if (!MovieExists(movie.Id))
                        {
                            return NotFound();
                        }
                        else
                        {
                            throw;
                        }
                    }
                    return RedirectToAction(nameof(Index));
                }
                return View(movie);
            }
            catch (Exception)
            {
                return Problem("An unexpected error occurred while updating movie.");
            }
        }

        public async Task<IActionResult> Delete(int? id)
        {
            try
            {
                if (id == null)
                {
                    return NotFound();
                }

                var movie = await _context.Movie
                    .FirstOrDefaultAsync(m => m.Id == id);
                if (movie == null)
                {
                    return NotFound();
                }

                return View(movie);
            }
            catch (Exception)
            {
                return Problem("An unexpected error occurred.");
            }
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var movie = await _context.Movie.FindAsync(id);
                if (movie != null)
                {
                    _context.Movie.Remove(movie);
                }

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Movie deleted successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
                return Problem("An unexpected error occurred while deleting record.");
            }
        }

        public IActionResult Export(string movieGenre, string searchString, bool favoritesOnly)
        {
            try
            {
                var package = new ExcelPackage();
                var sheet = package.Workbook.Worksheets.Add("Movies");

                sheet.Cells[1, 1].Value = "Title";
                sheet.Cells[1, 2].Value = "Release Date";
                sheet.Cells[1, 3].Value = "Genre";
                sheet.Cells[1, 4].Value = "Price ($)";
                sheet.Cells[1, 5].Value = "Rating";

                using var range = sheet.Cells[1, 1, 1, 5];
                range.Style.Font.Bold = true;
                range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                range.Style.Fill.BackgroundColor.SetColor(Color.LightGray);
                SetRangeBorderAndAlignmentStyling(range);

                int row = 2;
                IQueryable<Movie> movies = _context.Movie;

                if (!string.IsNullOrEmpty(movieGenre))
                {
                    movies = movies.Where(x => x.Genre == movieGenre);
                }

                if (!string.IsNullOrEmpty(searchString))
                {
                    movies = movies.Where(movie => EF.Functions.Like(movie.Title!, $"%{searchString}%"));
                }

                if (favoritesOnly)
                {
                    movies = movies.Where(m => m.Favorited);
                }

                foreach (var movie in movies)
                {
                    sheet.Cells[row, 1].Value = movie.Title;
                    sheet.Cells[row, 2].Value = movie.ReleaseDate.ToString("dd-MM-yyyy");
                    sheet.Cells[row, 3].Value = movie.Genre;
                    sheet.Cells[row, 4].Value = movie.Price;
                    sheet.Cells[row, 5].Value = movie.Rating;

                    var fillColor = (row % 2 == 0) ? Color.White : Color.LightGray;
                    using var rowRange = sheet.Cells[row, 1, row, 5];
                    rowRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    rowRange.Style.Fill.BackgroundColor.SetColor(fillColor);
                    rowRange.Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.AliceBlue);
                    SetRangeBorderAndAlignmentStyling(rowRange);
                    rowRange.Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.Black);
                    row++;
                }

                using var totalPriceRange = sheet.Cells[row, 3, row, 4];
                sheet.Cells[row, 3].Value = "Total Price:";
                sheet.Cells[row, 4].Value = movies.Sum(m => m.Price);
                totalPriceRange.Style.Font.Bold = true;
                totalPriceRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                totalPriceRange.Style.Fill.BackgroundColor.SetColor(Color.LightCyan);
                SetRangeBorderAndAlignmentStyling(totalPriceRange);

                var tableRange = sheet.Cells[1, 1, row - 1, 5];
                range.Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.Black, true);
                tableRange.Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.Black, true);

                sheet.Cells[sheet.Dimension.Address].AutoFitColumns();

                var stream = new MemoryStream();
                package.SaveAs(stream);
                stream.Position = 0;

                string fileName = "Movies.xlsx";
                string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                TempData["SuccessMessage"] = "Successfully exported to Excel!";
                return File(stream, contentType, fileName);
            }
            catch (Exception ex)
            {
                return Problem("An unexpected error occurred while exporting movies.");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ToggleFavorite([FromBody] FavoriteDto dto)
        {
            try
            {
                var movie = _context.Movie.Find(dto.ItemId);
                if (movie != null)
                {
                    movie.Favorited = !movie.Favorited;
                    _context.SaveChanges();
                    return Json(new { favorited = movie?.Favorited });
                }
                return NotFound();
            }
            catch (Exception ex)
            {
                return Problem("An unexpected error occurred while toggling favourite.");
            }
        }

        private void SetRangeBorderAndAlignmentStyling(ExcelRange range)
        {
            range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            range.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
            range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
            range.Style.Border.Left.Style = ExcelBorderStyle.Thin;
            range.Style.Border.Right.Style = ExcelBorderStyle.Thin;
        }

        private bool MovieExists(int id)
        {
            return _context.Movie.Any(e => e.Id == id);
        }
    }
}
