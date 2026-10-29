using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PokeBattle.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AdminRole : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[] { "6f0b1a2e-3c4d-4e5f-8a9b-0c1d2e3f4a5b", "a1b2c3d4-e5f6-4a7b-8c9d-0e1f2a3b4c5d", "Admin", "ADMIN" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "6f0b1a2e-3c4d-4e5f-8a9b-0c1d2e3f4a5b");
        }
    }
}