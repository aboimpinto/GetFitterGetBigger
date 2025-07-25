namespace GetFitterGetBigger.Admin.Constants
{
    /// <summary>
    /// Constants for WorkoutState IDs used throughout the application
    /// </summary>
    public static class WorkoutStateIds
    {
        /// <summary>
        /// Draft state - initial state for new workout templates
        /// </summary>
        public const string Draft = "workoutstate-02000001-0000-0000-0000-000000000001";
        
        /// <summary>
        /// Production state - published and available for use
        /// </summary>
        public const string Production = "workoutstate-02000001-0000-0000-0000-000000000002";
        
        /// <summary>
        /// Archived state - no longer active but retained for history
        /// </summary>
        public const string Archived = "workoutstate-02000001-0000-0000-0000-000000000003";
    }
}