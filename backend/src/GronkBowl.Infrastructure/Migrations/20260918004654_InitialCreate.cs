using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GronkBowl.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Leagues",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    CurrentRulesVersion = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Leagues", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Matches",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SeasonId = table.Column<Guid>(type: "uuid", nullable: false),
                    HomeTeamId = table.Column<Guid>(type: "uuid", nullable: false),
                    AwayTeamId = table.Column<Guid>(type: "uuid", nullable: false),
                    Week = table.Column<int>(type: "integer", nullable: false),
                    Seed = table.Column<long>(type: "bigint", nullable: false),
                    EventLog = table.Column<string>(type: "jsonb", nullable: false),
                    HomeScore = table.Column<int>(type: "integer", nullable: false),
                    AwayScore = table.Column<int>(type: "integer", nullable: false),
                    IsResolved = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Matches", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Players",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Race = table.Column<string>(type: "text", nullable: false),
                    Position = table.Column<string>(type: "text", nullable: false),
                    Attributes = table.Column<string>(type: "jsonb", nullable: false),
                    ArmorValue = table.Column<int>(type: "integer", nullable: false),
                    Grit = table.Column<int>(type: "integer", nullable: false),
                    Traits = table.Column<string>(type: "jsonb", nullable: false),
                    InjuryStatus = table.Column<string>(type: "text", nullable: false),
                    InjuryWeeksRemaining = table.Column<int>(type: "integer", nullable: false),
                    PermanentAttributePenalty = table.Column<int>(type: "integer", nullable: false),
                    Contract = table.Column<string>(type: "jsonb", nullable: true),
                    ChemistryModifier = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Players", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Plays",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Category = table.Column<string>(type: "text", nullable: false),
                    IsDefaultPlay = table.Column<bool>(type: "boolean", nullable: false),
                    Assignments = table.Column<string>(type: "jsonb", nullable: false),
                    Tags = table.Column<string>(type: "jsonb", nullable: false),
                    RiskProfile = table.Column<int>(type: "integer", nullable: false),
                    IsPassPlay = table.Column<bool>(type: "boolean", nullable: false),
                    PrimaryPosition = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Plays", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Seasons",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Year = table.Column<int>(type: "integer", nullable: false),
                    RulesVersion = table.Column<string>(type: "text", nullable: false),
                    Schedule = table.Column<string>(type: "jsonb", nullable: false),
                    Standings = table.Column<string>(type: "jsonb", nullable: false),
                    DraftOrder = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Seasons", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Teams",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Roster = table.Column<string>(type: "jsonb", nullable: false),
                    Gold = table.Column<int>(type: "integer", nullable: false),
                    CapSpace = table.Column<int>(type: "integer", nullable: false),
                    MedicalFacilityLevel = table.Column<int>(type: "integer", nullable: false),
                    ScoutingFacilityLevel = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Teams", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CallSheets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TeamId = table.Column<Guid>(type: "uuid", nullable: false),
                    Week = table.Column<int>(type: "integer", nullable: false),
                    OffensiveSituationalPlays = table.Column<string>(type: "jsonb", nullable: false),
                    DefensiveSituationalPlays = table.Column<string>(type: "jsonb", nullable: false),
                    Submitted = table.Column<bool>(type: "boolean", nullable: false),
                    WasRandomlyGenerated = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CallSheets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CallSheets_Teams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlayTeam",
                columns: table => new
                {
                    PlaybookId = table.Column<Guid>(type: "uuid", nullable: false),
                    TeamId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayTeam", x => new { x.PlaybookId, x.TeamId });
                    table.ForeignKey(
                        name: "FK_PlayTeam_Plays_PlaybookId",
                        column: x => x.PlaybookId,
                        principalTable: "Plays",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlayTeam_Teams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CallSheets_TeamId",
                table: "CallSheets",
                column: "TeamId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Matches_SeasonId_Week",
                table: "Matches",
                columns: new[] { "SeasonId", "Week" });

            migrationBuilder.CreateIndex(
                name: "IX_PlayTeam_TeamId",
                table: "PlayTeam",
                column: "TeamId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CallSheets");

            migrationBuilder.DropTable(
                name: "Leagues");

            migrationBuilder.DropTable(
                name: "Matches");

            migrationBuilder.DropTable(
                name: "Players");

            migrationBuilder.DropTable(
                name: "PlayTeam");

            migrationBuilder.DropTable(
                name: "Seasons");

            migrationBuilder.DropTable(
                name: "Plays");

            migrationBuilder.DropTable(
                name: "Teams");
        }
    }
}
