using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace APU.DataV2.Migrations
{
    /// <inheritdoc />
    public partial class ContractorTypesdbsetadded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Contractors_ContractorType_TypeId",
                table: "Contractors");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ContractorType",
                table: "ContractorType");

            migrationBuilder.RenameTable(
                name: "ContractorType",
                newName: "ContractorTypes");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ContractorTypes",
                table: "ContractorTypes",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Contractors_ContractorTypes_TypeId",
                table: "Contractors",
                column: "TypeId",
                principalTable: "ContractorTypes",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Contractors_ContractorTypes_TypeId",
                table: "Contractors");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ContractorTypes",
                table: "ContractorTypes");

            migrationBuilder.RenameTable(
                name: "ContractorTypes",
                newName: "ContractorType");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ContractorType",
                table: "ContractorType",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Contractors_ContractorType_TypeId",
                table: "Contractors",
                column: "TypeId",
                principalTable: "ContractorType",
                principalColumn: "Id");
        }
    }
}
