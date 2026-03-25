using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AssignAthleteToTeam : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "TeamId",
                table: "Athletes",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Athletes_TeamId",
                table: "Athletes",
                column: "TeamId");

            migrationBuilder.AddForeignKey(
                name: "FK_Athletes_Teams_TeamId",
                table: "Athletes",
                column: "TeamId",
                principalTable: "Teams",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Athletes_Teams_TeamId",
                table: "Athletes");

            migrationBuilder.DropIndex(
                name: "IX_Athletes_TeamId",
                table: "Athletes");

            migrationBuilder.DropColumn(
                name: "TeamId",
                table: "Athletes");
        }
    }
}
