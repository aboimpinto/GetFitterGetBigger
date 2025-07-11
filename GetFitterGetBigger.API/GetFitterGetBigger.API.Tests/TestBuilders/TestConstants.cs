namespace GetFitterGetBigger.API.Tests.TestBuilders;

/// <summary>
/// Centralized test constants to ensure consistency across all tests
/// These IDs should match the seeded data in test databases
/// </summary>
public static class TestConstants
{
    public static class ExerciseTypeIds
    {
        public const string Rest = "exercisetype-d4e5f6a7-8b9c-0d1e-2f3a-4b5c6d7e8f9a";
        public const string Workout = "exercisetype-b2c3d4e5-6f7a-8b9c-0d1e-2f3a4b5c6d7e";
        public const string Warmup = "exercisetype-a1b2c3d4-5e6f-7a8b-9c0d-1e2f3a4b5c6d";
        public const string Cooldown = "exercisetype-c3d4e5f6-7a8b-9c0d-1e2f-3a4b5c6d7e8f";
    }

    public static class DifficultyLevelIds
    {
        public const string Beginner = "difficultylevel-8a8adb1d-24d2-4979-a5a6-0d760e6da24b";
        public const string Intermediate = "difficultylevel-9c7b59a4-bcd8-48a6-971a-cd67b0a7ab5a";
        public const string Advanced = "difficultylevel-3e27f9a7-d5a5-4f8e-8a76-6de2d23c9a3c";
    }

    public static class KineticChainTypeIds
    {
        public const string Compound = "kineticchaintype-f5d5a2de-9c4e-4b87-b8c3-5d1e17d0b1f4";
        public const string Isolation = "kineticchaintype-2b3e7cb2-9a3e-4c9a-88d8-b7c019c90d1b";
        public const string OpenChain = "kineticchaintype-12345678-9abc-def0-1234-567890abcdef";
        public const string ClosedChain = "kineticchaintype-23456789-abcd-ef01-2345-6789abcdef01";
    }

    public static class ExerciseWeightTypeIds
    {
        public const string BodyweightOnly = "exerciseweighttype-a1f3e2d4-5b6c-4d7e-8f9a-0b1c2d3e4f5a";
        public const string BodyweightOptional = "exerciseweighttype-b2e4d3c5-6a7b-5c8d-9e0f-1a2b3c4d5e6f";
        public const string WeightRequired = "exerciseweighttype-c3d5c4b6-7b8c-6d9e-0f1a-2b3c4d5e6f7a";
        public const string MachineWeight = "exerciseweighttype-d4c6b5a7-8c9d-7e0f-1a2b-3c4d5e6f7a8b";
        public const string NoWeight = "exerciseweighttype-e5b7a698-9d0e-8f1a-2b3c-4d5e6f7a8b9c";
    }

    public static class MuscleGroupIds
    {
        public const string Chest = "musclegroup-ccddeeff-0011-2233-4455-667788990011"; // Pectoralis
        public const string Back = "musclegroup-ddeeff00-1122-3344-5566-778899001122"; // Latissimus Dorsi
        public const string Shoulders = "musclegroup-c3d4e5f6-7081-9012-3456-789abcdef012";
        public const string Arms = "musclegroup-d4e5f6a7-8192-a123-4567-89abcdef0123";
        public const string Legs = "musclegroup-eeff0011-2233-4455-6677-889900112233"; // Quadriceps
        public const string Core = "musclegroup-f6a7b8c9-a3b4-c345-6789-abcdef012345";
    }

    public static class MuscleRoleIds
    {
        public const string Primary = "musclerole-abcdef12-3456-7890-abcd-ef1234567890";
        public const string Secondary = "musclerole-11223344-5566-7788-99aa-bbccddeeff00";
        public const string Stabilizer = "musclerole-22334455-6677-8899-aabb-ccddeeff0011";
    }

    public static class EquipmentIds
    {
        public const string Barbell = "equipment-33445566-7788-99aa-bbcc-ddeeff001122";
        public const string Dumbbell = "equipment-44556677-8899-aabb-ccdd-eeff00112233";
        public const string Machine = "equipment-34567890-3456-3456-3456-345678901234";
        public const string Bodyweight = "equipment-45678901-4567-4567-4567-456789012345";
        public const string Kettlebell = "equipment-55667788-99aa-bbcc-ddee-ff0011223344";
    }

    public static class MovementPatternIds
    {
        public const string Push = "movementpattern-99aabbcc-ddee-ff00-1122-334455667788";
        public const string Pull = "movementpattern-aabbccdd-eeff-0011-2233-445566778899";
        public const string Squat = "movementpattern-bbccddee-ff00-1122-3344-556677889900";
        public const string Hinge = "movementpattern-44556677-8899-aabb-ccdd-eeff00112233";
    }

    public static class BodyPartIds
    {
        public const string Chest = "bodypart-7c5a2d6e-e87e-4c8a-9f1d-9eb734f3df3c";
        public const string Back = "bodypart-b2d89d5c-cb8a-4f5d-8a9e-2c3b76612c5a";
        public const string Legs = "bodypart-4a6f1b42-5c9b-4c4e-878a-b3d9f2c1f1f5";
        public const string Shoulders = "bodypart-d7e0e24c-f8d4-4b8a-b1e0-cf9c2e6b5d0a";
    }

    /// <summary>
    /// Generate a new GUID-based ID for the given entity type
    /// Use this when you need unique IDs that don't conflict with seeded data
    /// </summary>
    public static string NewId(string entityType)
    {
        return $"{entityType.ToLowerInvariant()}-{Guid.NewGuid()}";
    }
}