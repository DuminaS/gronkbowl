using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GronkBowl.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPlayerProgression : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DevelopmentPotential",
                table: "Players",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "DevelopmentPotentialRevealed",
                table: "Players",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "GamesPlayed",
                table: "Players",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Level",
                table: "Players",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SkillPoints",
                table: "Players",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Skills",
                table: "Players",
                type: "jsonb",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "UsedCareerReroll",
                table: "Players",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DevelopmentPotential",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "DevelopmentPotentialRevealed",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "GamesPlayed",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "Level",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "SkillPoints",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "Skills",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "UsedCareerReroll",
                table: "Players");
        }
    }
}
