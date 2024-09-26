using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DynamicObjectAPI.Data.Migrations
{
    public partial class mig_2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CustomerId",
                table: "DynamicObjects",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_DynamicObjects_CustomerId",
                table: "DynamicObjects",
                column: "CustomerId",
                filter: "\"CustomerId\" IS NOT NULL");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DynamicObjects_CustomerId",
                table: "DynamicObjects");

            migrationBuilder.DropColumn(
                name: "CustomerId",
                table: "DynamicObjects");
        }
    }
}
