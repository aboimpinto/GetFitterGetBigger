namespace GetFitterGetBigger.Admin.Models.Dtos
{
    public class WorkoutTemplatePagedResultDto
    {
        public List<WorkoutTemplateDto> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public bool HasPreviousPage { get; set; }
        public bool HasNextPage { get; set; }

        /// <summary>
        /// Creates an empty paged result with default values
        /// </summary>
        public static WorkoutTemplatePagedResultDto Empty() => new()
        {
            Items = new List<WorkoutTemplateDto>(),
            TotalCount = 0,
            PageNumber = 1,
            PageSize = 10,
            TotalPages = 0,
            HasPreviousPage = false,
            HasNextPage = false
        };
    }
}