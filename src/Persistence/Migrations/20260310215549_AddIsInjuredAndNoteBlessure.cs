using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddIsInjuredAndNoteBlessure : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsInjured",
                table: "Athletes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "NotesBlessure",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AthleteId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Contenu = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotesBlessure", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NotesBlessure_Athletes_AthleteId",
                        column: x => x.AthleteId,
                        principalTable: "Athletes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_NotesBlessure_AthleteId",
                table: "NotesBlessure",
                column: "AthleteId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NotesBlessure");

            migrationBuilder.DropColumn(
                name: "IsInjured",
                table: "Athletes");
        }
    }
}
