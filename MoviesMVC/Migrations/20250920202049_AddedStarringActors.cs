using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MoviesMVC.Migrations
{
    /// <inheritdoc />
    public partial class AddedStarringActors : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Actors",
                table: "Movie",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Actors",
                table: "Movie");
        }
    }
}
