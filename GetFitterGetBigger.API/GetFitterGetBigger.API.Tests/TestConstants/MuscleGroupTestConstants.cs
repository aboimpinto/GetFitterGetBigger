namespace GetFitterGetBigger.API.Tests.TestConstants;

/// <summary>
/// Test constants for MuscleGroup-related tests
/// </summary>
public static class MuscleGroupTestConstants
{
    /// <summary>
    /// Test data constants for MuscleGroup entities
    /// </summary>
    public static class TestData
    {
        // Valid MuscleGroup IDs
        public const string BicepsId = "musclegroup-11111111-1111-1111-1111-111111111111";
        public const string TricepsId = "musclegroup-22222222-2222-2222-2222-222222222222";
        public const string ChestId = "musclegroup-33333333-3333-3333-3333-333333333333";
        public const string BackId = "musclegroup-44444444-4444-4444-4444-444444444444";
        
        // Valid MuscleGroup Names
        public const string BicepsName = "Biceps";
        public const string TricepsName = "Triceps";
        public const string ChestName = "Chest";
        public const string BackName = "Back";
        
        // Valid BodyPart IDs
        public const string ArmsBodyPartId = "bodypart-aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa";
        public const string TorsoBodyPartId = "bodypart-bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb";
        
        // Valid BodyPart Names
        public const string ArmsBodyPartName = "Arms";
        public const string TorsoBodyPartName = "Torso";
        
        // Test data for Create/Update operations
        public const string NewMuscleGroupName = "Deltoids";
        public const string UpdatedMuscleGroupName = "Shoulders";
        
        // Non-existent ID for NotFound scenarios
        public const string NonExistentId = "musclegroup-99999999-9999-9999-9999-999999999999";
        
        // Invalid format IDs
        public const string InvalidFormatId = "invalid-format";
        public const string InvalidBodyPartId = "invalid-bodypart-format";
        
        // Empty strings for validation scenarios
        public const string EmptyName = "";
        public const string EmptyBodyPartId = "";
    }
    
    /// <summary>
    /// Error message constants for test assertions
    /// </summary>
    public static class ErrorMessages
    {
        public const string NotFound = "MuscleGroup not found";
        public const string AlreadyExists = "MuscleGroup already exists";
        public const string DependencyExists = "Cannot delete MuscleGroup - it is being used";
        public const string InvalidFormat = "Invalid ID format";
        public const string ValidationFailed = "Validation failed";
        public const string InternalError = "Internal server error";
    }
}