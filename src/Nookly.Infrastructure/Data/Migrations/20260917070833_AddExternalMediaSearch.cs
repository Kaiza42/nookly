using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nookly.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddExternalMediaSearch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_media_items_ExternalSource_ExternalId",
                table: "media_items");

            migrationBuilder.CreateIndex(
                name: "IX_media_items_ExternalSource_ExternalId",
                table: "media_items",
                columns: new[] { "ExternalSource", "ExternalId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_media_items_ExternalSource_ExternalId",
                table: "media_items");

            migrationBuilder.CreateIndex(
                name: "IX_media_items_ExternalSource_ExternalId",
                table: "media_items",
                columns: new[] { "ExternalSource", "ExternalId" });
        }
    }
}
