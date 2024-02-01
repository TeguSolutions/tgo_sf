using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace APU.DataV2.Migrations
{
    public partial class ProjectCountyremoved : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Projects_Counties_CountyId",
                table: "Projects");

            migrationBuilder.DropIndex(
                name: "IX_Projects_CountyId",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "CountyId",
                table: "Projects");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CountyId",
                table: "Projects",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Projects_CountyId",
                table: "Projects",
                column: "CountyId");

            migrationBuilder.AddForeignKey(
                name: "FK_Projects_Counties_CountyId",
                table: "Projects",
                column: "CountyId",
                principalTable: "Counties",
                principalColumn: "Id");
        }
    }
}
