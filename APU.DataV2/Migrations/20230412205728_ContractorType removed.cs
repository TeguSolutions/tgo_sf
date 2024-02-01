using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace APU.DataV2.Migrations
{
    /// <inheritdoc />
    public partial class ContractorTyperemoved : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Contractors_ContractorTypes_TypeId",
                table: "Contractors");

            migrationBuilder.DropTable(
                name: "ContractorTypes");

            migrationBuilder.DropIndex(
                name: "IX_Contractors_TypeId",
                table: "Contractors");

            migrationBuilder.DropColumn(
                name: "TypeId",
                table: "Contractors");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TypeId",
                table: "Contractors",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "ContractorTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContractorTypes", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "ContractorTypes",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "SUPLIER" },
                    { 2, "SUBCONT" },
                    { 3, "MANUFACTURER" },
                    { 4, "LABOR" },
                    { 5, "WHOLESALE" },
                    { 6, "SBE SUBCONT" },
                    { 7, "PROF SERVICES" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Contractors_TypeId",
                table: "Contractors",
                column: "TypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Contractors_ContractorTypes_TypeId",
                table: "Contractors",
                column: "TypeId",
                principalTable: "ContractorTypes",
                principalColumn: "Id");
        }
    }
}
