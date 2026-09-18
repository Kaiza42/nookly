using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nookly.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMonthlyBanking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_bank_accounts_MemberId",
                table: "bank_accounts");

            migrationBuilder.AddColumn<int>(
                name: "Month",
                table: "bank_accounts",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Year",
                table: "bank_accounts",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql("""
                UPDATE bank_accounts
                SET "Year" = EXTRACT(YEAR FROM "UpdatedAtUtc")::integer,
                    "Month" = EXTRACT(MONTH FROM "UpdatedAtUtc")::integer;
                """);

            migrationBuilder.CreateIndex(
                name: "IX_bank_accounts_MemberId_Year_Month",
                table: "bank_accounts",
                columns: new[] { "MemberId", "Year", "Month" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_bank_accounts_MemberId_Year_Month",
                table: "bank_accounts");

            migrationBuilder.DropColumn(
                name: "Month",
                table: "bank_accounts");

            migrationBuilder.DropColumn(
                name: "Year",
                table: "bank_accounts");

            migrationBuilder.CreateIndex(
                name: "IX_bank_accounts_MemberId",
                table: "bank_accounts",
                column: "MemberId",
                unique: true);
        }
    }
}
