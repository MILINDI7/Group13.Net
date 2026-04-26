using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ThirteenthAvenue.Data.Migrations
{
    /// <inheritdoc />
    public partial class FixPendingChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "AdminSharePercentage",
                table: "OrganizerProfiles",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "OrganizerSharePercentage",
                table: "OrganizerProfiles",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AdminSharePercentage",
                table: "OrganizerProfiles");

            migrationBuilder.DropColumn(
                name: "OrganizerSharePercentage",
                table: "OrganizerProfiles");
        }
    }
}
