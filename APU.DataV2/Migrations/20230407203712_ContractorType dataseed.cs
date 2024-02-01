using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace APU.DataV2.Migrations
{
    /// <inheritdoc />
    public partial class ContractorTypedataseed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "ContractorType",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "TYPE" },
                    { 2, "SUPLIER" },
                    { 3, "SUBCONT" },
                    { 4, "MANUFACTURER" },
                    { 5, "LABOR" },
                    { 6, "WHOLESALE" },
                    { 7, "SBE SUBCONT" },
                    { 8, "PROF SERVICES" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "ContractorType",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "ContractorType",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "ContractorType",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "ContractorType",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "ContractorType",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "ContractorType",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "ContractorType",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "ContractorType",
                keyColumn: "Id",
                keyValue: 8);
        }
    }
}
