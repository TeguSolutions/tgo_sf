using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace APU.DataV2.Migrations
{
    /// <inheritdoc />
    public partial class ContractorTyperelationreadded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "TypeId",
                table: "Contractors",
                type: "uniqueidentifier",
                nullable: true);

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Contractors_ContractorTypes_TypeId",
                table: "Contractors");

            migrationBuilder.DropIndex(
                name: "IX_Contractors_TypeId",
                table: "Contractors");

            migrationBuilder.DropColumn(
                name: "TypeId",
                table: "Contractors");
        }
    }
}
