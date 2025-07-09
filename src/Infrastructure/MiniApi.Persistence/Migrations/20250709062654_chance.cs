using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MiniApi.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class chance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Favourites",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Favourites");
        }
    }
}
