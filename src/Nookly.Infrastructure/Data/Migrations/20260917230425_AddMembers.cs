using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nookly.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMembers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_media_items_ExternalSource_ExternalId",
                table: "media_items");

            migrationBuilder.DropIndex(
                name: "IX_discovery_preferences_ExternalSource_ExternalId",
                table: "discovery_preferences");

            migrationBuilder.AddColumn<Guid>(
                name: "MemberId",
                table: "media_items",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "MemberId",
                table: "discovery_preferences",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "members",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PasswordHash = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_members", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_media_items_MemberId_ExternalSource_ExternalId",
                table: "media_items",
                columns: new[] { "MemberId", "ExternalSource", "ExternalId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_discovery_preferences_MemberId_ExternalSource_ExternalId",
                table: "discovery_preferences",
                columns: new[] { "MemberId", "ExternalSource", "ExternalId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_members_Email",
                table: "members",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "members");

            migrationBuilder.DropIndex(
                name: "IX_media_items_MemberId_ExternalSource_ExternalId",
                table: "media_items");

            migrationBuilder.DropIndex(
                name: "IX_discovery_preferences_MemberId_ExternalSource_ExternalId",
                table: "discovery_preferences");

            migrationBuilder.DropColumn(
                name: "MemberId",
                table: "media_items");

            migrationBuilder.DropColumn(
                name: "MemberId",
                table: "discovery_preferences");

            migrationBuilder.CreateIndex(
                name: "IX_media_items_ExternalSource_ExternalId",
                table: "media_items",
                columns: new[] { "ExternalSource", "ExternalId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_discovery_preferences_ExternalSource_ExternalId",
                table: "discovery_preferences",
                columns: new[] { "ExternalSource", "ExternalId" },
                unique: true);
        }
    }
}
