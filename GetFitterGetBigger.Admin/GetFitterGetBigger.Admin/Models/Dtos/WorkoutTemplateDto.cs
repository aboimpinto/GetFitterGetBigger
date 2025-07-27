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

        /// <summary>
        /// Gets whether this instance represents an empty/not found template
        /// </summary>
        public bool IsEmpty => Id == string.Empty && Name == string.Empty;

        /// <summary>
        /// Gets an empty instance representing a not found workout template
        /// </summary>
        public static WorkoutTemplateDto Empty => new()
        {
            Id = string.Empty,
            Name = string.Empty,
            Description = null,
            Category = new ReferenceDataDto { Id = string.Empty, Value = string.Empty },
            Difficulty = new ReferenceDataDto { Id = string.Empty, Value = string.Empty },
            WorkoutState = new ReferenceDataDto { Id = string.Empty, Value = string.Empty },
            EstimatedDurationMinutes = 0,
            IsPublic = false,
            Tags = new List<string>(),
            Exercises = new List<WorkoutTemplateExerciseDto>(),
            Objectives = new List<ReferenceDataDto>(),
            CreatedAt = DateTime.MinValue,
            UpdatedAt = DateTime.MinValue
        };
    }
}