using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace APU.DataV2.Migrations
{
    /// <inheritdoc />
    public partial class ProjectScheduleDependencyrenamedtoPredecessor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Dependency",
                table: "ProjectSchedules",
                newName: "Predecessor");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Predecessor",
                table: "ProjectSchedules",
                newName: "Dependency");
        }
    }
}
