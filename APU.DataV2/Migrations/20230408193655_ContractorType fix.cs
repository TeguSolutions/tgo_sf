using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace APU.DataV2.Migrations
{
    /// <inheritdoc />
    public partial class ContractorTypefix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "ContractorType",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.UpdateData(
                table: "ContractorType",
                keyColumn: "Id",
                keyValue: 1,
                column: "Name",
                value: "SUPLIER");

            migrationBuilder.UpdateData(
                table: "ContractorType",
                keyColumn: "Id",
                keyValue: 2,
                column: "Name",
                value: "SUBCONT");

            migrationBuilder.UpdateData(
                table: "ContractorType",
                keyColumn: "Id",
                keyValue: 3,
                column: "Name",
                value: "MANUFACTURER");

            migrationBuilder.UpdateData(
                table: "ContractorType",
                keyColumn: "Id",
                keyValue: 4,
                column: "Name",
                value: "LABOR");

            migrationBuilder.UpdateData(
                table: "ContractorType",
                keyColumn: "Id",
                keyValue: 5,
                column: "Name",
                value: "WHOLESALE");

            migrationBuilder.UpdateData(
                table: "ContractorType",
                keyColumn: "Id",
                keyValue: 6,
                column: "Name",
                value: "SBE SUBCONT");

            migrationBuilder.UpdateData(
                table: "ContractorType",
                keyColumn: "Id",
                keyValue: 7,
                column: "Name",
                value: "PROF SERVICES");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "ContractorType",
                keyColumn: "Id",
                keyValue: 1,
                column: "Name",
                value: "TYPE");

            migrationBuilder.UpdateData(
                table: "ContractorType",
                keyColumn: "Id",
                keyValue: 2,
                column: "Name",
                value: "SUPLIER");

            migrationBuilder.UpdateData(
                table: "ContractorType",
                keyColumn: "Id",
                keyValue: 3,
                column: "Name",
                value: "SUBCONT");

            migrationBuilder.UpdateData(
                table: "ContractorType",
                keyColumn: "Id",
                keyValue: 4,
                column: "Name",
                value: "MANUFACTURER");

            migrationBuilder.UpdateData(
                table: "ContractorType",
                keyColumn: "Id",
                keyValue: 5,
                column: "Name",
                value: "LABOR");

            migrationBuilder.UpdateData(
                table: "ContractorType",
                keyColumn: "Id",
                keyValue: 6,
                column: "Name",
                value: "WHOLESALE");

            migrationBuilder.UpdateData(
                table: "ContractorType",
                keyColumn: "Id",
                keyValue: 7,
                column: "Name",
                value: "SBE SUBCONT");

            migrationBuilder.InsertData(
                table: "ContractorType",
                columns: new[] { "Id", "Name" },
                values: new object[] { 8, "PROF SERVICES" });
        }
    }
}
