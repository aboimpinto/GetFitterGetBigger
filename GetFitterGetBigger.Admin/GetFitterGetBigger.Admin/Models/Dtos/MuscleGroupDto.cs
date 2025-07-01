using System.Text.Json.Serialization;

namespace GetFitterGetBigger.Admin.Models.Dtos
{
    public class MuscleGroupDto
    {
        [JsonPropertyName("id")]
        public required string Id { get; set; }

        [JsonPropertyName("name")]
        public required string Name { get; set; }

        [JsonPropertyName("bodyPartId")]
        public required string BodyPartId { get; set; }

        [JsonPropertyName("bodyPartName")]
        public string? BodyPartName { get; set; }

        [JsonPropertyName("isActive")]
        public bool IsActive { get; set; }

        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; }

        [JsonPropertyName("updatedAt")]
        public DateTime? UpdatedAt { get; set; }
    }

    public class CreateMuscleGroupDto
    {
        [JsonPropertyName("name")]
        public required string Name { get; set; }

        [JsonPropertyName("bodyPartId")]
        public required string BodyPartId { get; set; }
    }

    public class UpdateMuscleGroupDto
    {
        [JsonPropertyName("name")]
        public required string Name { get; set; }

        [JsonPropertyName("bodyPartId")]
        public required string BodyPartId { get; set; }
    }

    public class MuscleGroupPagedResultDto
    {
        [JsonPropertyName("items")]
        public List<MuscleGroupDto> Items { get; set; } = new();

        [JsonPropertyName("totalCount")]
        public int TotalCount { get; set; }

        [JsonPropertyName("pageNumber")]
        public int PageNumber { get; set; }

        [JsonPropertyName("pageSize")]
        public int PageSize { get; set; }

        [JsonPropertyName("totalPages")]
        public int TotalPages { get; set; }

        [JsonPropertyName("hasPreviousPage")]
        public bool HasPreviousPage { get; set; }

        [JsonPropertyName("hasNextPage")]
        public bool HasNextPage { get; set; }
    }
}