using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nookly.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMemberPublicId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PublicId",
                table: "members",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE members
                SET "PublicId" = upper(substr(md5("Id"::text), 1, 10))
                WHERE "PublicId" IS NULL;
                """);

            migrationBuilder.AlterColumn<string>(
                name: "PublicId",
                table: "members",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(10)",
                oldMaxLength: 10,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_members_PublicId",
                table: "members",
                column: "PublicId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_members_PublicId",
                table: "members");

            migrationBuilder.DropColumn(
                name: "PublicId",
                table: "members");
        }
    }
}
