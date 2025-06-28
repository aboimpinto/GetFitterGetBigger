using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace GetFitterGetBigger.API.Migrations
{
    /// <inheritdoc />
    public partial class ReplaceEnumsWithReferenceData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "LastLoginAt",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "PasswordHash",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "BodyPart",
                table: "MuscleGroups");

            migrationBuilder.DropColumn(
                name: "Role",
                table: "ExerciseTargetedMuscles");

            migrationBuilder.DropColumn(
                name: "Difficulty",
                table: "Exercises");

            migrationBuilder.DropColumn(
                name: "KineticChain",
                table: "Exercises");

            migrationBuilder.AddColumn<Guid>(
                name: "BodyPartId",
                table: "MuscleGroups",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "MovementPatterns",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<Guid>(
                name: "MuscleRoleId",
                table: "ExerciseTargetedMuscles",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "DifficultyLevelId",
                table: "Exercises",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "KineticChainTypeId",
                table: "Exercises",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "BodyParts",
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
                    table.PrimaryKey("PK_BodyParts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DifficultyLevels",
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
                    table.PrimaryKey("PK_DifficultyLevels", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KineticChainTypes",
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
                    table.PrimaryKey("PK_KineticChainTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MuscleRoles",
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
                    table.PrimaryKey("PK_MuscleRoles", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "BodyParts",
                columns: new[] { "Id", "Description", "DisplayOrder", "IsActive", "Value" },
                values: new object[,]
                {
                    { new Guid("3e9f8a7d-6c5b-4a3e-8d2f-1b7c9a6d5e4c"), null, 6, true, "Core" },
                    { new Guid("4a6f1b42-5c9b-4c4e-878a-b3d9f2c1f1f5"), null, 3, true, "Legs" },
                    { new Guid("7c5a2d6e-e87e-4c8a-9f1d-9eb734f3df3c"), null, 1, true, "Chest" },
                    { new Guid("9c5f1b4e-2b8a-4c9d-8e7f-c5a9e2d7b8c1"), null, 5, true, "Arms" },
                    { new Guid("b2d89d5c-cb8a-4f5d-8a9e-2c3b76612c5a"), null, 2, true, "Back" },
                    { new Guid("d7e0e24c-f8d4-4b8a-b1e0-cf9c2e6b5d0a"), null, 4, true, "Shoulders" }
                });

            migrationBuilder.InsertData(
                table: "DifficultyLevels",
                columns: new[] { "Id", "Description", "DisplayOrder", "IsActive", "Value" },
                values: new object[,]
                {
                    { new Guid("3e27f9a7-d5a5-4f8e-8a76-6de2d23c9a3c"), "Suitable for those with significant fitness experience", 3, true, "Advanced" },
                    { new Guid("8a8adb1d-24d2-4979-a5a6-0d760e6da24b"), "Suitable for those new to fitness", 1, true, "Beginner" },
                    { new Guid("9c7b59a4-bcd8-48a6-971a-cd67b0a7ab5a"), "Suitable for those with some fitness experience", 2, true, "Intermediate" }
                });

            migrationBuilder.InsertData(
                table: "KineticChainTypes",
                columns: new[] { "Id", "Description", "DisplayOrder", "IsActive", "Value" },
                values: new object[,]
                {
                    { new Guid("2b3e7cb2-9a3e-4c9a-88d8-b7c019c90d1b"), "Exercises that work a single muscle group", 2, true, "Isolation" },
                    { new Guid("f5d5a2de-9c4e-4b87-b8c3-5d1e17d0b1f4"), "Exercises that work multiple muscle groups", 1, true, "Compound" }
                });

            migrationBuilder.InsertData(
                table: "MuscleRoles",
                columns: new[] { "Id", "Description", "DisplayOrder", "IsActive", "Value" },
                values: new object[,]
                {
                    { new Guid("1a2b3c4d-5e6f-7a8b-9c0d-1e2f3a4b5c6d"), "A muscle that helps stabilize the body during the exercise", 3, true, "Stabilizer" },
                    { new Guid("5d8e9a7b-3c2d-4f6a-9b8c-1e5d7f3a2c9b"), "The main muscle targeted by the exercise", 1, true, "Primary" },
                    { new Guid("8c7d6b5a-4e3f-2a1b-9c8d-7e6f5d4c3b2a"), "A muscle that assists in the exercise", 2, true, "Secondary" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_MuscleGroups_BodyPartId",
                table: "MuscleGroups",
                column: "BodyPartId");

            migrationBuilder.CreateIndex(
                name: "IX_ExerciseTargetedMuscles_MuscleRoleId",
                table: "ExerciseTargetedMuscles",
                column: "MuscleRoleId");

            migrationBuilder.CreateIndex(
                name: "IX_Exercises_DifficultyLevelId",
                table: "Exercises",
                column: "DifficultyLevelId");

            migrationBuilder.CreateIndex(
                name: "IX_Exercises_KineticChainTypeId",
                table: "Exercises",
                column: "KineticChainTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Exercises_DifficultyLevels_DifficultyLevelId",
                table: "Exercises",
                column: "DifficultyLevelId",
                principalTable: "DifficultyLevels",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Exercises_KineticChainTypes_KineticChainTypeId",
                table: "Exercises",
                column: "KineticChainTypeId",
                principalTable: "KineticChainTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ExerciseTargetedMuscles_MuscleRoles_MuscleRoleId",
                table: "ExerciseTargetedMuscles",
                column: "MuscleRoleId",
                principalTable: "MuscleRoles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MuscleGroups_BodyParts_BodyPartId",
                table: "MuscleGroups",
                column: "BodyPartId",
                principalTable: "BodyParts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Exercises_DifficultyLevels_DifficultyLevelId",
                table: "Exercises");

            migrationBuilder.DropForeignKey(
                name: "FK_Exercises_KineticChainTypes_KineticChainTypeId",
                table: "Exercises");

            migrationBuilder.DropForeignKey(
                name: "FK_ExerciseTargetedMuscles_MuscleRoles_MuscleRoleId",
                table: "ExerciseTargetedMuscles");

            migrationBuilder.DropForeignKey(
                name: "FK_MuscleGroups_BodyParts_BodyPartId",
                table: "MuscleGroups");

            migrationBuilder.DropTable(
                name: "BodyParts");

            migrationBuilder.DropTable(
                name: "DifficultyLevels");

            migrationBuilder.DropTable(
                name: "KineticChainTypes");

            migrationBuilder.DropTable(
                name: "MuscleRoles");

            migrationBuilder.DropIndex(
                name: "IX_MuscleGroups_BodyPartId",
                table: "MuscleGroups");

            migrationBuilder.DropIndex(
                name: "IX_ExerciseTargetedMuscles_MuscleRoleId",
                table: "ExerciseTargetedMuscles");

            migrationBuilder.DropIndex(
                name: "IX_Exercises_DifficultyLevelId",
                table: "Exercises");

            migrationBuilder.DropIndex(
                name: "IX_Exercises_KineticChainTypeId",
                table: "Exercises");

            migrationBuilder.DropColumn(
                name: "BodyPartId",
                table: "MuscleGroups");

            migrationBuilder.DropColumn(
                name: "MuscleRoleId",
                table: "ExerciseTargetedMuscles");

            migrationBuilder.DropColumn(
                name: "DifficultyLevelId",
                table: "Exercises");

            migrationBuilder.DropColumn(
                name: "KineticChainTypeId",
                table: "Exercises");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Users",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "LastLoginAt",
                table: "Users",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PasswordHash",
                table: "Users",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "BodyPart",
                table: "MuscleGroups",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "MovementPatterns",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Role",
                table: "ExerciseTargetedMuscles",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Difficulty",
                table: "Exercises",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "KineticChain",
                table: "Exercises",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
