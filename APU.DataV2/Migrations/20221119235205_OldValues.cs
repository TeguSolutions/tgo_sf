using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace APU.DataV2.Migrations
{
    /// <inheritdoc />
    public partial class OldValues : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OldId",
                table: "Projects",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OldId",
                table: "Counties",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OldId",
                table: "Cities",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OldId",
                table: "BasePerformances",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OldId",
                table: "BaseMaterials",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OldId",
                table: "BaseLabors",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OldId",
                table: "BaseEquipments",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OldId",
                table: "BaseContracts",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OldId",
                table: "Apus",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OldId",
                table: "ApuPerformances",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OldId",
                table: "ApuMaterials",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OldId",
                table: "ApuLabors",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OldId",
                table: "ApuEquipments",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OldId",
                table: "ApuContracts",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OldId",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "OldId",
                table: "Counties");

            migrationBuilder.DropColumn(
                name: "OldId",
                table: "Cities");

            migrationBuilder.DropColumn(
                name: "OldId",
                table: "BasePerformances");

            migrationBuilder.DropColumn(
                name: "OldId",
                table: "BaseMaterials");

            migrationBuilder.DropColumn(
                name: "OldId",
                table: "BaseLabors");

            migrationBuilder.DropColumn(
                name: "OldId",
                table: "BaseEquipments");

            migrationBuilder.DropColumn(
                name: "OldId",
                table: "BaseContracts");

            migrationBuilder.DropColumn(
                name: "OldId",
                table: "Apus");

            migrationBuilder.DropColumn(
                name: "OldId",
                table: "ApuPerformances");

            migrationBuilder.DropColumn(
                name: "OldId",
                table: "ApuMaterials");

            migrationBuilder.DropColumn(
                name: "OldId",
                table: "ApuLabors");

            migrationBuilder.DropColumn(
                name: "OldId",
                table: "ApuEquipments");

            migrationBuilder.DropColumn(
                name: "OldId",
                table: "ApuContracts");
        }
    }
}
