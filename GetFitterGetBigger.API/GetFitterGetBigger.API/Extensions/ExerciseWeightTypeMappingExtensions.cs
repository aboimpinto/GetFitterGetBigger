using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models.Entities;

namespace GetFitterGetBigger.API.Extensions;

/// <summary>
/// Extension methods for mapping ExerciseWeightType entities to DTOs
/// </summary>
public static class ExerciseWeightTypeMappingExtensions
{
    /// <summary>
    /// Maps an ExerciseWeightType entity to a ReferenceDataDto
    /// </summary>
    /// <param name="entity">The ExerciseWeightType entity to map</param>
    /// <returns>A ReferenceDataDto representation of the entity, or empty DTO if entity is empty</returns>
    public static ReferenceDataDto ToReferenceDataDto(this ExerciseWeightType entity)
    {
        if (entity.IsEmpty)
        {
            return new ReferenceDataDto
            {
                Id = string.Empty,
                Value = string.Empty,
                Description = null
            };
        }
        
        return new ReferenceDataDto
        {
            Id = entity.Id.ToString(),
            Value = entity.Value,
            Description = entity.Description
        };
    }
    
    /// <summary>
    /// Maps a collection of ExerciseWeightType entities to ReferenceDataDto objects
    /// </summary>
    /// <param name="entities">The collection of ExerciseWeightType entities to map</param>
    /// <returns>A collection of ReferenceDataDto objects</returns>
    public static IEnumerable<ReferenceDataDto> ToReferenceDataDtos(this IEnumerable<ExerciseWeightType> entities)
    {
        return entities.Select(e => e.ToReferenceDataDto());
    }
}