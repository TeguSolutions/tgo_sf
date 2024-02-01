using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace APU.DataV2.Migrations
{
    /// <inheritdoc />
    public partial class ContractorTraderelationadded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "TradeId",
                table: "Contractors",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Contractors_TradeId",
                table: "Contractors",
                column: "TradeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Contractors_Trades_TradeId",
                table: "Contractors",
                column: "TradeId",
                principalTable: "Trades",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Contractors_Trades_TradeId",
                table: "Contractors");

            migrationBuilder.DropIndex(
                name: "IX_Contractors_TradeId",
                table: "Contractors");

            migrationBuilder.DropColumn(
                name: "TradeId",
                table: "Contractors");
        }
    }
}
