using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RecamNewBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddShareTokenToListingCase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ShareToken",
                table: "ListingCases",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ShareToken",
                table: "ListingCases");
        }
    }
}
