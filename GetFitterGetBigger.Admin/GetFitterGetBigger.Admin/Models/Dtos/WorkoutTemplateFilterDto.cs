namespace GetFitterGetBigger.Admin.Models.Dtos
{
    public class WorkoutTemplateFilterDto
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? NamePattern { get; set; }
        public string? CategoryId { get; set; }
        public string? DifficultyId { get; set; }
        public string? StateId { get; set; }
        public bool? IsPublic { get; set; }
    }
}