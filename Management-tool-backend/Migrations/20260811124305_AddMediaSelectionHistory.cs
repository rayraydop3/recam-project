using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RecamNewBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddMediaSelectionHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MediaSelectionHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IsSelected = table.Column<bool>(type: "bit", nullable: false),
                    ChangedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ListingCaseId = table.Column<int>(type: "int", nullable: false),
                    MediaAssetId = table.Column<int>(type: "int", nullable: false),
                    AgentId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MediaSelectionHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MediaSelectionHistories_AspNetUsers_AgentId",
                        column: x => x.AgentId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MediaSelectionHistories_ListingCases_ListingCaseId",
                        column: x => x.ListingCaseId,
                        principalTable: "ListingCases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MediaSelectionHistories_MediaAssets_MediaAssetId",
                        column: x => x.MediaAssetId,
                        principalTable: "MediaAssets",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_MediaSelectionHistories_AgentId",
                table: "MediaSelectionHistories",
                column: "AgentId");

            migrationBuilder.CreateIndex(
                name: "IX_MediaSelectionHistories_ListingCaseId",
                table: "MediaSelectionHistories",
                column: "ListingCaseId");

            migrationBuilder.CreateIndex(
                name: "IX_MediaSelectionHistories_MediaAssetId",
                table: "MediaSelectionHistories",
                column: "MediaAssetId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MediaSelectionHistories");
        }
    }
}
