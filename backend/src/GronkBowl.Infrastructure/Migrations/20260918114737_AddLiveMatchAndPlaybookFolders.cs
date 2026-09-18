using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GronkBowl.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddLiveMatchAndPlaybookFolders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DistanceToGo",
                table: "Matches",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Down",
                table: "Matches",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "FieldPosition",
                table: "Matches",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "PendingDefensePlayId",
                table: "Matches",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PendingOffensePlayId",
                table: "Matches",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PlaysRemainingInQuarter",
                table: "Matches",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "PossessionTeamId",
                table: "Matches",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<int>(
                name: "Quarter",
                table: "Matches",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "PlaybookFolders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TeamId = table.Column<Guid>(type: "uuid", nullable: false),
                    Category = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    PlayIds = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlaybookFolders", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PlaybookFolders_TeamId",
                table: "PlaybookFolders",
                column: "TeamId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlaybookFolders");

            migrationBuilder.DropColumn(
                name: "DistanceToGo",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "Down",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "FieldPosition",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "PendingDefensePlayId",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "PendingOffensePlayId",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "PlaysRemainingInQuarter",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "PossessionTeamId",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "Quarter",
                table: "Matches");
        }
    }
}
