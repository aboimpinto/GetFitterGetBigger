namespace GetFitterGetBigger.Admin.Models.Dtos
{
    public enum ExerciseType
    {
        Warmup,
        Workout,
        Cooldown,
        Rest
    }

    public class ExerciseTypeDto
    {
        public string Id { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}