using System;
using GetFitterGetBigger.API.Models.SpecializedIds;

namespace GetFitterGetBigger.API.Models.Entities;

public record ExerciseLink
{
    public ExerciseLinkId Id { get; init; }
    public ExerciseId SourceExerciseId { get; init; }
    public ExerciseId TargetExerciseId { get; init; }
    public string LinkType { get; init; } = string.Empty; // "Warmup" or "Cooldown"
    public int DisplayOrder { get; init; }
    public bool IsActive { get; init; } = true;
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
    
    // Navigation properties
    public Exercise? SourceExercise { get; init; }
    public Exercise? TargetExercise { get; init; }
    
    // Private constructor to force usage of Handler
    private ExerciseLink() { }
    
    public static class Handler
    {
        public static ExerciseLink CreateNew(
            ExerciseId sourceExerciseId,
            ExerciseId targetExerciseId,
            string linkType,
            int displayOrder)
        {
            // Validation logic
            if (sourceExerciseId == default)
            {
                throw new ArgumentException("Source exercise ID cannot be empty", nameof(sourceExerciseId));
            }
            
            if (targetExerciseId == default)
            {
                throw new ArgumentException("Target exercise ID cannot be empty", nameof(targetExerciseId));
            }
            
            if (string.IsNullOrWhiteSpace(linkType))
            {
                throw new ArgumentException("Link type cannot be empty", nameof(linkType));
            }
            
            if (linkType != "Warmup" && linkType != "Cooldown")
            {
                throw new ArgumentException("Link type must be either 'Warmup' or 'Cooldown'", nameof(linkType));
            }
            
            if (displayOrder < 0)
            {
                throw new ArgumentException("Display order cannot be negative", nameof(displayOrder));
            }
            
            var now = DateTime.UtcNow;
            return new ExerciseLink
            {
                Id = ExerciseLinkId.New(),
                SourceExerciseId = sourceExerciseId,
                TargetExerciseId = targetExerciseId,
                LinkType = linkType,
                DisplayOrder = displayOrder,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            };
        }
        
        public static ExerciseLink Create(
            ExerciseLinkId id,
            ExerciseId sourceExerciseId,
            ExerciseId targetExerciseId,
            string linkType,
            int displayOrder,
            bool isActive,
            DateTime createdAt,
            DateTime updatedAt)
        {
            return new ExerciseLink
            {
                Id = id,
                SourceExerciseId = sourceExerciseId,
                TargetExerciseId = targetExerciseId,
                LinkType = linkType,
                DisplayOrder = displayOrder,
                IsActive = isActive,
                CreatedAt = createdAt,
                UpdatedAt = updatedAt
            };
        }
    }
}