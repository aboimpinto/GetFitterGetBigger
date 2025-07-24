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
    }
}