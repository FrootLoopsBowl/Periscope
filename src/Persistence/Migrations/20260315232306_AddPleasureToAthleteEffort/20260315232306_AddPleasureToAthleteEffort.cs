using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPleasureToAthleteEffort : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF NOT EXISTS (
                    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
                    WHERE TABLE_NAME = 'AthleteEfforts' AND COLUMN_NAME = 'Pleasure'
                )
                BEGIN
                    ALTER TABLE [AthleteEfforts] ADD [Pleasure] int NULL;
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Pleasure",
                table: "AthleteEfforts");
        }
    }
}
