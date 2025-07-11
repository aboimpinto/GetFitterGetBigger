namespace GetFitterGetBigger.Admin.Models.Dtos
{
    public class CoachNoteDto
    {
        public string Id { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public int Order { get; set; }
    }

    public class CoachNoteCreateDto
    {
        public string? Id { get; set; }
        public string Text { get; set; } = string.Empty;
        public int Order { get; set; }
    }
}