namespace GetFitterGetBigger.Admin.Models.Dtos
{
    public class WorkoutTemplateDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public ReferenceDataDto Category { get; set; } = null!;
        public ReferenceDataDto Difficulty { get; set; } = null!;
        public ReferenceDataDto WorkoutState { get; set; } = null!;
        public int EstimatedDurationMinutes { get; set; }
        public bool IsPublic { get; set; }
        public List<string> Tags { get; set; } = new();
        public List<WorkoutTemplateExerciseDto> Exercises { get; set; } = new();
        public List<ReferenceDataDto> Objectives { get; set; } = new();
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}