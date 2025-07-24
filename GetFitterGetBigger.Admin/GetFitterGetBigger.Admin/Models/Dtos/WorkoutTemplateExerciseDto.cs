namespace GetFitterGetBigger.Admin.Models.Dtos
{
    public class WorkoutTemplateExerciseDto
    {
        public string ExerciseId { get; set; } = string.Empty;
        public string ExerciseName { get; set; } = string.Empty;
        public int OrderIndex { get; set; }
        public int Sets { get; set; }
        public string TargetReps { get; set; } = string.Empty;
        public int RestSeconds { get; set; }
        public string? Notes { get; set; }
    }
}