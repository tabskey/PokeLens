using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace PokeBattle.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Favorites : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SavedPrediction",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    Target = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Area = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Method = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Version = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    Milestone = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Objective = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    Scenario = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    Candidates = table.Column<List<string>>(type: "text[]", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SavedPrediction", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SavedPrediction_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SavedPrediction_UserId_CreatedAt",
                table: "SavedPrediction",
                columns: new[] { "UserId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_SavedPrediction_UserId_Target_Area_Method_Version_Milestone~",
                table: "SavedPrediction",
                columns: new[] { "UserId", "Target", "Area", "Method", "Version", "Milestone", "Objective", "Scenario" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SavedPrediction");
        }
    }
}
