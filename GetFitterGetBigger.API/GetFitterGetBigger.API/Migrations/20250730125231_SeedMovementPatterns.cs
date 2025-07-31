using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GetFitterGetBigger.API.Migrations
{
    /// <inheritdoc />
    public partial class SeedMovementPatterns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Insert Movement Patterns
            migrationBuilder.InsertData(
                table: "MovementPatterns",
                columns: new[] { "Id", "Value", "Description", "DisplayOrder", "IsActive" },
                values: new object[,]
                {
                    { new Guid("bbccddee-ff00-1122-3344-556677889900"), "Squat", "A lower-body, knee-dominant movement characterized by the simultaneous bending of the hips, knees, and ankles while maintaining a relatively upright torso. Examples: Barbell Back Squat, Goblet Squat, Air Squat.", 1, true },
                    { new Guid("a760cc8f-b32e-408b-b0ec-1ba053ee4bed"), "Hinge", "A lower-body, hip-dominant movement involving flexion and extension primarily at the hip joint with minimal knee bend. This pattern is fundamental for lifting objects from the floor and developing the posterior chain. Examples: Deadlift, Kettlebell Swing, Good Morning.", 2, true },
                    { new Guid("5c4b8fe7-a66e-4be2-9e3f-30c1de46e7ad"), "Lunge", "A unilateral (single-leg focused) movement pattern that challenges balance, stability, and strength in a split stance. It is a key pattern for locomotion and single-leg stability. Examples: Forward Lunge, Reverse Lunge, Bulgarian Split Squat.", 3, true },
                    { new Guid("99aabbcc-ddee-ff00-1122-334455667788"), "Horizontal Push", "Pushing forward, parallel to the ground. Examples: Bench Press, Push-up, Cable Chest Press.", 4, true },
                    { new Guid("71b77ae2-e8d2-4547-bd90-b7a69d975124"), "Vertical Push", "Pushing upward, overhead. Examples: Overhead Press, Dumbbell Shoulder Press, Handstand Push-up.", 5, true },
                    { new Guid("aabbccdd-eeff-0011-2233-445566778899"), "Horizontal Pull", "Pulling backward, parallel to the ground. Examples: Bent-Over Row, Seated Cable Row, Inverted Row.", 6, true },
                    { new Guid("efab6dba-4bcd-4381-9fd1-cbb86f1f2301"), "Vertical Pull", "Pulling downward from an overhead position. Examples: Pull-up, Chin-up, Lat Pulldown.", 7, true },
                    { new Guid("9019d05b-c822-4aa9-8181-751f16cfbc75"), "Rotation", "A core-focused pattern involving either generating rotational force (twisting) or resisting it. This is crucial for athletic power transfer and spinal stability. Examples: Medicine Ball Rotational Throw (Rotation), Pallof Press (Anti-Rotation), Bird-Dog (Anti-Rotation).", 8, true },
                    { new Guid("a2c67018-196d-45ff-b596-c2d8bc845c20"), "Gait/Carry", "A pattern of locomotion (walking or running) while under an external load. This is considered highly functional as it integrates core stability with grip strength and full-body coordination. Examples: Farmer's Walk, Suitcase Carry, Overhead Carry.", 9, true }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove the seeded movement patterns
            migrationBuilder.DeleteData(
                table: "MovementPatterns",
                keyColumn: "Id",
                keyValues: new object[]
                {
                    new Guid("bbccddee-ff00-1122-3344-556677889900"),
                    new Guid("a760cc8f-b32e-408b-b0ec-1ba053ee4bed"),
                    new Guid("5c4b8fe7-a66e-4be2-9e3f-30c1de46e7ad"),
                    new Guid("99aabbcc-ddee-ff00-1122-334455667788"),
                    new Guid("71b77ae2-e8d2-4547-bd90-b7a69d975124"),
                    new Guid("aabbccdd-eeff-0011-2233-445566778899"),
                    new Guid("efab6dba-4bcd-4381-9fd1-cbb86f1f2301"),
                    new Guid("9019d05b-c822-4aa9-8181-751f16cfbc75"),
                    new Guid("a2c67018-196d-45ff-b596-c2d8bc845c20")
                });
        }
    }
}
