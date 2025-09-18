using Microsoft.EntityFrameworkCore;
using MoviesMVC.Data;
using MoviesMVC.Models;
using OfficeOpenXml;
namespace MoviesMVC
{
    public class Program
    {
        public static void Main(string[] args)
        {
            EPPlusLicense license = new EPPlusLicense();
            license.SetNonCommercialPersonal("mxrt0");

            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddDbContext<MoviesDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("MoviesDbContext") ?? throw new InvalidOperationException("Connection string 'MoviesDbContext' not found.")));

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            var app = builder.Build();

            // Configure a start-up scope for seeding data 
            using var scope = app.Services.CreateScope();
            var services = scope.ServiceProvider;
            SeedData.Initialize(services);

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseRouting();

            app.UseAuthorization();

            app.MapStaticAssets();
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Movies}/{action=Index}/{id?}")
                .WithStaticAssets();

            app.Run();
        }
    }
}
