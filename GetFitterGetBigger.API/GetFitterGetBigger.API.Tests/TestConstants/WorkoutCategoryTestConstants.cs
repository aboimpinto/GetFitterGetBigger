namespace GetFitterGetBigger.API.Tests.TestConstants;

public static class WorkoutCategoryTestConstants
{
    public static class CacheKeys
    {
        public const string AllCacheKey = "WorkoutCategory:all";
        public static string ValueCacheKey(string value) => $"WorkoutCategory:value:{value}";
        public static string IdCacheKey(string id) => $"WorkoutCategory:id:{id}";
    }
    
    public static class ErrorMessages
    {
        public const string InvalidIdFormat = "Invalid workout category ID format";
        public const string ValueCannotBeEmpty = "Value cannot be empty";
        public const string NotFound = "Workout category not found";
    }
    
    public static class Defaults
    {
        public const string EmptyId = "";
        public const string EmptyValue = "";
        public const string EmptyIcon = "";
        public const string DefaultColor = "#000000";
        public const int DefaultDisplayOrder = 0;
        public const bool DefaultIsActive = false;
    }
}