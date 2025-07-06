using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MiniApi.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class changeproductentities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OwnerId",
                table: "Products",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "Products");
        }
    }
}
