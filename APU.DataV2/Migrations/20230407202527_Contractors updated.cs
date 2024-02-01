using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace APU.DataV2.Migrations
{
    /// <inheritdoc />
    public partial class Contractorsupdated : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Name",
                table: "Contractors");

            migrationBuilder.RenameColumn(
                name: "Note",
                table: "Contractors",
                newName: "Url");

            migrationBuilder.AddColumn<string>(
                name: "CEL",
                table: "Contractors",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Comments",
                table: "Contractors",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CompanyName",
                table: "Contractors",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContactPerson",
                table: "Contractors",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "County",
                table: "Contractors",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Contractors",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Email2",
                table: "Contractors",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Trade",
                table: "Contractors",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TypeId",
                table: "Contractors",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "ContractorType",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContractorType", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Contractors_TypeId",
                table: "Contractors",
                column: "TypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Contractors_ContractorType_TypeId",
                table: "Contractors",
                column: "TypeId",
                principalTable: "ContractorType",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Contractors_ContractorType_TypeId",
                table: "Contractors");

            migrationBuilder.DropTable(
                name: "ContractorType");

            migrationBuilder.DropIndex(
                name: "IX_Contractors_TypeId",
                table: "Contractors");

            migrationBuilder.DropColumn(
                name: "CEL",
                table: "Contractors");

            migrationBuilder.DropColumn(
                name: "Comments",
                table: "Contractors");

            migrationBuilder.DropColumn(
                name: "CompanyName",
                table: "Contractors");

            migrationBuilder.DropColumn(
                name: "ContactPerson",
                table: "Contractors");

            migrationBuilder.DropColumn(
                name: "County",
                table: "Contractors");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "Contractors");

            migrationBuilder.DropColumn(
                name: "Email2",
                table: "Contractors");

            migrationBuilder.DropColumn(
                name: "Trade",
                table: "Contractors");

            migrationBuilder.DropColumn(
                name: "TypeId",
                table: "Contractors");

            migrationBuilder.RenameColumn(
                name: "Url",
                table: "Contractors",
                newName: "Note");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Contractors",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
