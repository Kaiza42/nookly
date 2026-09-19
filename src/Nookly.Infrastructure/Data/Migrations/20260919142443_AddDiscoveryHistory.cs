using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nookly.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDiscoveryHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "discovery_history",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MemberId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExternalSource = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ExternalId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    MediaType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Title = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    PosterUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CommunityRating = table.Column<decimal>(type: "numeric", nullable: true),
                    ReleaseDate = table.Column<DateOnly>(type: "date", nullable: true),
                    CastLabel = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ViewedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_discovery_history", x => x.Id);
                    table.ForeignKey(
                        name: "FK_discovery_history_members_MemberId",
                        column: x => x.MemberId,
                        principalTable: "members",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_discovery_history_MemberId_ExternalSource_ExternalId_MediaT~",
                table: "discovery_history",
                columns: new[] { "MemberId", "ExternalSource", "ExternalId", "MediaType" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_discovery_history_MemberId_ViewedAtUtc",
                table: "discovery_history",
                columns: new[] { "MemberId", "ViewedAtUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "discovery_history");
        }
    }
}
