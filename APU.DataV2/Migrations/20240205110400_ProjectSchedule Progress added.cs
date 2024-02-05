﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace APU.DataV2.Migrations
{
    /// <inheritdoc />
    public partial class ProjectScheduleProgressadded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Progress",
                table: "ProjectSchedules",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Progress",
                table: "ProjectSchedules");
        }
    }
}
