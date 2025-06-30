using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GetFitterGetBigger.API.Migrations
{
    /// <inheritdoc />
    public partial class AddEquipmentAuditFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Equipment",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Equipment",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: DateTime.UtcNow);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Equipment",
                type: "timestamp with time zone",
                nullable: true);
                
            // Update existing records to have IsActive = true and CreatedAt = current time
            migrationBuilder.Sql("UPDATE \"Equipment\" SET \"IsActive\" = true, \"CreatedAt\" = NOW() WHERE \"IsActive\" IS NULL OR \"CreatedAt\" IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Equipment");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Equipment");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Equipment");
        }
    }
}
