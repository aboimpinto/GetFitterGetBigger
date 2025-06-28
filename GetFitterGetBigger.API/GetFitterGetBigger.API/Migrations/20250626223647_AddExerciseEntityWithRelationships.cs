using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GetFitterGetBigger.API.Migrations
{
    /// <inheritdoc />
    public partial class AddExerciseEntityWithRelationships : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ExerciseMovementPatterns_MovementPatterns_PatternId",
                table: "ExerciseMovementPatterns");

            migrationBuilder.DropForeignKey(
                name: "FK_Exercises_DifficultyLevels_DifficultyLevelId",
                table: "Exercises");

            migrationBuilder.DropForeignKey(
                name: "FK_Exercises_KineticChainTypes_KineticChainTypeId",
                table: "Exercises");

            migrationBuilder.RenameColumn(
                name: "DifficultyLevelId",
                table: "Exercises",
                newName: "DifficultyId");

            migrationBuilder.RenameIndex(
                name: "IX_Exercises_DifficultyLevelId",
                table: "Exercises",
                newName: "IX_Exercises_DifficultyId");

            migrationBuilder.RenameColumn(
                name: "PatternId",
                table: "ExerciseMovementPatterns",
                newName: "MovementPatternId");

            migrationBuilder.RenameIndex(
                name: "IX_ExerciseMovementPatterns_PatternId",
                table: "ExerciseMovementPatterns",
                newName: "IX_ExerciseMovementPatterns_MovementPatternId");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Exercises",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<Guid>(
                name: "KineticChainTypeId",
                table: "Exercises",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<string>(
                name: "Instructions",
                table: "Exercises",
                type: "character varying(5000)",
                maxLength: 5000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Exercises",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "Exercises",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Exercises",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "ExerciseBodyParts",
                columns: table => new
                {
                    ExerciseId = table.Column<Guid>(type: "uuid", nullable: false),
                    BodyPartId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExerciseBodyParts", x => new { x.ExerciseId, x.BodyPartId });
                    table.ForeignKey(
                        name: "FK_ExerciseBodyParts_BodyParts_BodyPartId",
                        column: x => x.BodyPartId,
                        principalTable: "BodyParts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExerciseBodyParts_Exercises_ExerciseId",
                        column: x => x.ExerciseId,
                        principalTable: "Exercises",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExerciseMuscleGroups",
                columns: table => new
                {
                    ExerciseId = table.Column<Guid>(type: "uuid", nullable: false),
                    MuscleGroupId = table.Column<Guid>(type: "uuid", nullable: false),
                    MuscleRoleId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExerciseMuscleGroups", x => new { x.ExerciseId, x.MuscleGroupId });
                    table.ForeignKey(
                        name: "FK_ExerciseMuscleGroups_Exercises_ExerciseId",
                        column: x => x.ExerciseId,
                        principalTable: "Exercises",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExerciseMuscleGroups_MuscleGroups_MuscleGroupId",
                        column: x => x.MuscleGroupId,
                        principalTable: "MuscleGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExerciseMuscleGroups_MuscleRoles_MuscleRoleId",
                        column: x => x.MuscleRoleId,
                        principalTable: "MuscleRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Exercises_Name",
                table: "Exercises",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExerciseBodyParts_BodyPartId",
                table: "ExerciseBodyParts",
                column: "BodyPartId");

            migrationBuilder.CreateIndex(
                name: "IX_ExerciseMuscleGroups_MuscleGroupId",
                table: "ExerciseMuscleGroups",
                column: "MuscleGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_ExerciseMuscleGroups_MuscleRoleId",
                table: "ExerciseMuscleGroups",
                column: "MuscleRoleId");

            migrationBuilder.AddForeignKey(
                name: "FK_ExerciseMovementPatterns_MovementPatterns_MovementPatternId",
                table: "ExerciseMovementPatterns",
                column: "MovementPatternId",
                principalTable: "MovementPatterns",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Exercises_DifficultyLevels_DifficultyId",
                table: "Exercises",
                column: "DifficultyId",
                principalTable: "DifficultyLevels",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Exercises_KineticChainTypes_KineticChainTypeId",
                table: "Exercises",
                column: "KineticChainTypeId",
                principalTable: "KineticChainTypes",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ExerciseMovementPatterns_MovementPatterns_MovementPatternId",
                table: "ExerciseMovementPatterns");

            migrationBuilder.DropForeignKey(
                name: "FK_Exercises_DifficultyLevels_DifficultyId",
                table: "Exercises");

            migrationBuilder.DropForeignKey(
                name: "FK_Exercises_KineticChainTypes_KineticChainTypeId",
                table: "Exercises");

            migrationBuilder.DropTable(
                name: "ExerciseBodyParts");

            migrationBuilder.DropTable(
                name: "ExerciseMuscleGroups");

            migrationBuilder.DropIndex(
                name: "IX_Exercises_Name",
                table: "Exercises");

            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "Exercises");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Exercises");

            migrationBuilder.RenameColumn(
                name: "DifficultyId",
                table: "Exercises",
                newName: "DifficultyLevelId");

            migrationBuilder.RenameIndex(
                name: "IX_Exercises_DifficultyId",
                table: "Exercises",
                newName: "IX_Exercises_DifficultyLevelId");

            migrationBuilder.RenameColumn(
                name: "MovementPatternId",
                table: "ExerciseMovementPatterns",
                newName: "PatternId");

            migrationBuilder.RenameIndex(
                name: "IX_ExerciseMovementPatterns_MovementPatternId",
                table: "ExerciseMovementPatterns",
                newName: "IX_ExerciseMovementPatterns_PatternId");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Exercises",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<Guid>(
                name: "KineticChainTypeId",
                table: "Exercises",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Instructions",
                table: "Exercises",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(5000)",
                oldMaxLength: 5000);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Exercises",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(1000)",
                oldMaxLength: 1000);

            migrationBuilder.AddForeignKey(
                name: "FK_ExerciseMovementPatterns_MovementPatterns_PatternId",
                table: "ExerciseMovementPatterns",
                column: "PatternId",
                principalTable: "MovementPatterns",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

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
        }
    }
}
