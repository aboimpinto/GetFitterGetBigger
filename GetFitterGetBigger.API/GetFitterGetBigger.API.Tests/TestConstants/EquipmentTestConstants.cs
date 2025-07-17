namespace GetFitterGetBigger.API.Tests.TestConstants;

/// <summary>
/// Test constants for Equipment-related tests
/// </summary>
public static class EquipmentTestConstants
{
    public static class CacheKeys
    {
        public const string AllCacheKey = "Equipment:all";
        public const string ByIdCacheKey = "equipment:id:";
        public const string ByNameCacheKey = "equipment:name:";
    }
    
    public static class ErrorMessages
    {
        public const string EmptyId = "ID cannot be empty";
        public const string InvalidIdFormat = "Invalid ID format. Expected format: 'equipment-{guid}', got: ";
        public const string EmptyName = "Name cannot be empty";
        public const string RequestCannotBeNull = "Request cannot be null";
        public const string DuplicateName = "Equipment with the name '{0}' already exists";
        public const string NotFound = "Equipment not found";
        public const string FailedToLoad = "Failed to load Equipment";
        public const string InUse = "Cannot delete equipment that is in use by exercises";
    }
    
    public static class TestData
    {
        public const string BarbellId = "equipment-11111111-1111-1111-1111-111111111111";
        public const string DumbbellId = "equipment-22222222-2222-2222-2222-222222222222";
        public const string CableMachineId = "equipment-33333333-3333-3333-3333-333333333333";
        public const string ResistanceBandId = "equipment-44444444-4444-4444-4444-444444444444";
        public const string PullUpBarId = "equipment-55555555-5555-5555-5555-555555555555";
        public const string KettlebellId = "equipment-66666666-6666-6666-6666-666666666666";
        public const string InactiveId = "equipment-77777777-7777-7777-7777-777777777777";
        public const string NonExistentId = "equipment-99999999-9999-9999-9999-999999999999";
        
        public const string BarbellName = "Barbell";
        public const string DumbbellName = "Dumbbell";
        public const string CableMachineName = "Cable Machine";
        public const string ResistanceBandName = "Resistance Band";
        public const string PullUpBarName = "Pull-up Bar";
        public const string KettlebellName = "Kettlebell";
        public const string InactiveName = "Inactive Equipment";
        public const string NewEquipmentName = "New Equipment";
        public const string UpdatedEquipmentName = "Updated Equipment";
    }
}