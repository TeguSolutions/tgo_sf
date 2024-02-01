using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace APU.DataV2.Migrations
{
    /// <inheritdoc />
    public partial class ContractorCountyrelationadded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CountyId",
                table: "Contractors",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Contractors_CountyId",
                table: "Contractors",
                column: "CountyId");

            migrationBuilder.AddForeignKey(
                name: "FK_Contractors_Counties_CountyId",
                table: "Contractors",
                column: "CountyId",
                principalTable: "Counties",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Contractors_Counties_CountyId",
                table: "Contractors");

            migrationBuilder.DropIndex(
                name: "IX_Contractors_CountyId",
                table: "Contractors");

            migrationBuilder.DropColumn(
                name: "CountyId",
                table: "Contractors");
        }
    }
}
