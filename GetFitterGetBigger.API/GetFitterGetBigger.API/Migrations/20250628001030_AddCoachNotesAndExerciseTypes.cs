using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace GetFitterGetBigger.API.Migrations
{
    /// <inheritdoc />
    public partial class AddCoachNotesAndExerciseTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Instructions",
                table: "Exercises");

            migrationBuilder.CreateTable(
                name: "CoachNotes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ExerciseId = table.Column<Guid>(type: "uuid", nullable: false),
                    Text = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Order = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CoachNotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CoachNotes_Exercises_ExerciseId",
                        column: x => x.ExerciseId,
                        principalTable: "Exercises",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExerciseTypes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExerciseTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ExerciseExerciseTypes",
                columns: table => new
                {
                    ExerciseId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExerciseTypeId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExerciseExerciseTypes", x => new { x.ExerciseId, x.ExerciseTypeId });
                    table.ForeignKey(
                        name: "FK_ExerciseExerciseTypes_ExerciseTypes_ExerciseTypeId",
                        column: x => x.ExerciseTypeId,
                        principalTable: "ExerciseTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExerciseExerciseTypes_Exercises_ExerciseId",
                        column: x => x.ExerciseId,
                        principalTable: "Exercises",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "ExerciseTypes",
                columns: new[] { "Id", "Description", "DisplayOrder", "IsActive", "Value" },
                values: new object[,]
                {
                    { new Guid("11223344-5566-7788-99aa-bbccddeeff00"), "Exercises performed to prepare the body for more intense activity", 1, true, "Warmup" },
                    { new Guid("22334455-6677-8899-aabb-ccddeeff0011"), "Main exercises that form the core of the training session", 2, true, "Workout" },
                    { new Guid("33445566-7788-99aa-bbcc-ddeeff001122"), "Exercises performed to help the body recover after intense activity", 3, true, "Cooldown" },
                    { new Guid("44556677-8899-aabb-ccdd-eeff00112233"), "Periods of rest between exercises or sets - cannot be combined with other exercise types", 4, true, "Rest" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_CoachNote_ExerciseId_Order",
                table: "CoachNotes",
                columns: new[] { "ExerciseId", "Order" });

            migrationBuilder.CreateIndex(
                name: "IX_ExerciseExerciseTypes_ExerciseTypeId",
                table: "ExerciseExerciseTypes",
                column: "ExerciseTypeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CoachNotes");

            migrationBuilder.DropTable(
                name: "ExerciseExerciseTypes");

            migrationBuilder.DropTable(
                name: "ExerciseTypes");

            migrationBuilder.AddColumn<string>(
                name: "Instructions",
                table: "Exercises",
                type: "character varying(5000)",
                maxLength: 5000,
                nullable: false,
                defaultValue: "");
        }
    }
}
