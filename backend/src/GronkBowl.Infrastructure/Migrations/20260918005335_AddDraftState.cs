using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GronkBowl.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDraftState : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DraftStates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SeasonId = table.Column<Guid>(type: "uuid", nullable: false),
                    PickOrder = table.Column<string>(type: "jsonb", nullable: false),
                    CurrentPickIndex = table.Column<int>(type: "integer", nullable: false),
                    ProspectPlayerIds = table.Column<string>(type: "jsonb", nullable: false),
                    DraftedProspectPlayerIds = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DraftStates", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DraftStates");
        }
    }
}
