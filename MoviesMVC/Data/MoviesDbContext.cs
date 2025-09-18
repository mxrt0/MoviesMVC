using Microsoft.EntityFrameworkCore;
using MoviesMVC.Models;

namespace MoviesMVC.Data
{
    public class MoviesDbContext : DbContext
    {
        public MoviesDbContext(DbContextOptions<MoviesDbContext> options)
            : base(options)
        {
        }

        public DbSet<Movie> Movie { get; set; } = default!;
    }
}
