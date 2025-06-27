namespace GetFitterGetBigger.Admin.Models.Dtos
{
    public class ExerciseDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Instructions { get; set; } = string.Empty;
        public ReferenceDataDto? Difficulty { get; set; }
        public bool IsUnilateral { get; set; }
        public string? ImageUrl { get; set; }
        public string? VideoUrl { get; set; }
        public List<MuscleGroupWithRoleDto> MuscleGroups { get; set; } = new();
        public List<ReferenceDataDto> Equipment { get; set; } = new();
        public List<ReferenceDataDto> BodyParts { get; set; } = new();
        public List<ReferenceDataDto> MovementPatterns { get; set; } = new();
    }

    public class MuscleGroupWithRoleDto
    {
        public ReferenceDataDto? MuscleGroup { get; set; }
        public ReferenceDataDto? Role { get; set; }
    }

    public class ExerciseCreateDto
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Instructions { get; set; } = string.Empty;
        public string DifficultyId { get; set; } = string.Empty;
        public bool IsUnilateral { get; set; }
        public string? ImageUrl { get; set; }
        public string? VideoUrl { get; set; }
        public List<MuscleGroupApiDto> MuscleGroups { get; set; } = new();
        public List<string> EquipmentIds { get; set; } = new();
        public List<string> BodyPartIds { get; set; } = new();
        public List<string> MovementPatternIds { get; set; } = new();
    }

    public class ExerciseUpdateDto : ExerciseCreateDto
    {
    }

    public class MuscleGroupApiDto
    {
        public string MuscleGroupId { get; set; } = string.Empty;
        public string MuscleRoleId { get; set; } = string.Empty;
    }

    public class MuscleGroupRoleAssignmentDto
    {
        public string MuscleGroupId { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }

    public class ExerciseListDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Instructions { get; set; } = string.Empty;
        public string? VideoUrl { get; set; }
        public string? ImageUrl { get; set; }
        public bool IsUnilateral { get; set; }
        public bool IsActive { get; set; }
        public ReferenceDataDto? Difficulty { get; set; }
        public List<MuscleGroupListItemDto> MuscleGroups { get; set; } = new();
        public List<ReferenceDataDto> Equipment { get; set; } = new();
        public List<ReferenceDataDto> MovementPatterns { get; set; } = new();
        public List<ReferenceDataDto> BodyParts { get; set; } = new();
    }

    public class MuscleGroupListItemDto
    {
        public ReferenceDataDto? MuscleGroup { get; set; }
        public ReferenceDataDto? Role { get; set; }
    }

    public class ExercisePagedResultDto
    {
        public List<ExerciseListDto> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    }

    public class ExerciseFilterDto
    {
        public string? Name { get; set; }
        public string? DifficultyId { get; set; }
        public List<string>? MuscleGroupIds { get; set; }
        public List<string>? EquipmentIds { get; set; }
        public bool? IsActive { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}