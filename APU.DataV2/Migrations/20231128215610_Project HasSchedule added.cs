using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace APU.DataV2.Migrations
{
    /// <inheritdoc />
    public partial class ProjectHasScheduleadded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "HasSchedule",
                table: "Projects",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HasSchedule",
                table: "Projects");
        }
    }
}
