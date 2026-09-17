using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nookly.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMemberForeignKeys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddForeignKey(
                name: "FK_discovery_preferences_members_MemberId",
                table: "discovery_preferences",
                column: "MemberId",
                principalTable: "members",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_media_items_members_MemberId",
                table: "media_items",
                column: "MemberId",
                principalTable: "members",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_discovery_preferences_members_MemberId",
                table: "discovery_preferences");

            migrationBuilder.DropForeignKey(
                name: "FK_media_items_members_MemberId",
                table: "media_items");
        }
    }
}
