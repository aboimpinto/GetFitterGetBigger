using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GetFitterGetBigger.API.Migrations
{
    /// <inheritdoc />
    public partial class UpdateExerciseLinksForFourWaySystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add the new enum column as nullable
            migrationBuilder.AddColumn<int>(
                name: "LinkTypeEnum",
                table: "ExerciseLinks",
                type: "integer",
                nullable: true);

            // Migrate existing data from string to enum
            // "Warmup" -> 0 (WARMUP), "Cooldown" -> 1 (COOLDOWN)
            migrationBuilder.Sql(@"
                UPDATE ""ExerciseLinks"" 
                SET ""LinkTypeEnum"" = 0 
                WHERE ""LinkType"" = 'Warmup';
            ");
            
            migrationBuilder.Sql(@"
                UPDATE ""ExerciseLinks"" 
                SET ""LinkTypeEnum"" = 1 
                WHERE ""LinkType"" = 'Cooldown';
            ");

            // Add index for enum-based queries on SourceExerciseId + LinkTypeEnum
            migrationBuilder.CreateIndex(
                name: "IX_ExerciseLinks_SourceExerciseId_LinkTypeEnum",
                table: "ExerciseLinks",
                columns: new[] { "SourceExerciseId", "LinkTypeEnum" });

            // Add index for enum-based queries on TargetExerciseId + LinkTypeEnum
            migrationBuilder.CreateIndex(
                name: "IX_ExerciseLinks_TargetExerciseId_LinkTypeEnum", 
                table: "ExerciseLinks",
                columns: new[] { "TargetExerciseId", "LinkTypeEnum" });

            // Add unique constraint for enum-based operations (allows null values)
            // This supports both existing string-based and new enum-based unique constraints
            migrationBuilder.CreateIndex(
                name: "IX_ExerciseLinks_Source_Target_TypeEnum_Unique",
                table: "ExerciseLinks",
                columns: new[] { "SourceExerciseId", "TargetExerciseId", "LinkTypeEnum" },
                unique: true,
                filter: "\"LinkTypeEnum\" IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop the unique index for enum-based operations
            migrationBuilder.DropIndex(
                name: "IX_ExerciseLinks_Source_Target_TypeEnum_Unique",
                table: "ExerciseLinks");

            // Drop the index for TargetExerciseId + LinkTypeEnum
            migrationBuilder.DropIndex(
                name: "IX_ExerciseLinks_TargetExerciseId_LinkTypeEnum",
                table: "ExerciseLinks");

            // Drop the index for SourceExerciseId + LinkTypeEnum  
            migrationBuilder.DropIndex(
                name: "IX_ExerciseLinks_SourceExerciseId_LinkTypeEnum",
                table: "ExerciseLinks");

            // Drop the enum column (data migration is not reversible by design)
            migrationBuilder.DropColumn(
                name: "LinkTypeEnum",
                table: "ExerciseLinks");
        }
    }
}
