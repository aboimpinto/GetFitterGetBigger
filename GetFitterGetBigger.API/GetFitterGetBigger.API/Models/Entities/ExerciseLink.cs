using GetFitterGetBigger.API.Models.Enums;
using GetFitterGetBigger.API.Models.SpecializedIds;

namespace GetFitterGetBigger.API.Models.Entities;

public record ExerciseLink : IEmptyEntity<ExerciseLink>
{
    public ExerciseLinkId Id { get; init; }
    
    // IEntity implementation
    string IEntity.Id => Id.ToString();
    bool IEntity.IsActive => IsActive;
    
    public ExerciseId SourceExerciseId { get; init; }
    public ExerciseId TargetExerciseId { get; init; }
    
    // Backward compatibility - existing property for migration period
    public string LinkType { get; init; } = string.Empty; // "Warmup" or "Cooldown"
    
    // Enhanced functionality - new enum property for four-way linking
    public ExerciseLinkType? LinkTypeEnum { get; init; }
    
    // Computed property for unified access during migration period
    public ExerciseLinkType ActualLinkType
    {
        get
        {
            // If the new enum property has a value, use it
            if (LinkTypeEnum.HasValue)
                return LinkTypeEnum.Value;
            
            // Otherwise, convert the legacy string value to the enum
            return LinkType switch
            {
                "Warmup" => ExerciseLinkType.WARMUP,
                "Cooldown" => ExerciseLinkType.COOLDOWN,
                _ when Enum.TryParse<ExerciseLinkType>(LinkType, out var parsed) => parsed,
                // Default to COOLDOWN for unknown types (maintains backward compatibility)
                _ => ExerciseLinkType.COOLDOWN 
            };
        }
    }
    
    public int DisplayOrder { get; init; }
    public bool IsActive { get; init; } = true;
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
    
    // Navigation properties
    public Exercise? SourceExercise { get; init; }
    public Exercise? TargetExercise { get; init; }
    
    // Empty pattern implementation
    public static ExerciseLink Empty => new()
    {
        Id = ExerciseLinkId.Empty,
        SourceExerciseId = ExerciseId.Empty,
        TargetExerciseId = ExerciseId.Empty,
        LinkType = string.Empty,
        LinkTypeEnum = null,
        DisplayOrder = 0,
        IsActive = false,
        CreatedAt = DateTime.MinValue,
        UpdatedAt = DateTime.MinValue
    };
    
    public bool IsEmpty => Id.IsEmpty;
    
    // Private constructor to force usage of Handler
    private ExerciseLink() { }
    
    public static class Handler
    {
        /// <summary>
        /// Creates a new ExerciseLink using string LinkType (accepts enum values as strings: WARMUP, COOLDOWN, WORKOUT, ALTERNATIVE)
        /// </summary>
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
            
            // Validate against enum values (as strings)
            if (!Enum.TryParse<ExerciseLinkType>(linkType, out _))
            {
                throw new ArgumentException("Link type must be one of: WARMUP, COOLDOWN, WORKOUT, ALTERNATIVE", nameof(linkType));
            }
            
            if (displayOrder < 0)
            {
                throw new ArgumentException("Display order cannot be negative", nameof(displayOrder));
            }
            
            var now = DateTime.UtcNow;
            var linkTypeEnum = Enum.Parse<ExerciseLinkType>(linkType);
            
            return new ExerciseLink
            {
                Id = ExerciseLinkId.New(),
                SourceExerciseId = sourceExerciseId,
                TargetExerciseId = targetExerciseId,
                LinkType = linkType,
                LinkTypeEnum = linkTypeEnum, // Set the enum value for new system
                DisplayOrder = displayOrder,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            };
        }

        /// <summary>
        /// Creates a new ExerciseLink using enum LinkType (enhanced functionality)
        /// </summary>
        public static ExerciseLink CreateNew(
            ExerciseId sourceExerciseId,
            ExerciseId targetExerciseId,
            ExerciseLinkType linkType,
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
            
            if (!Enum.IsDefined(typeof(ExerciseLinkType), linkType))
            {
                throw new ArgumentException("Link type must be a valid ExerciseLinkType value", nameof(linkType));
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
                LinkType = linkType.ToString(), // Set string for backward compatibility
                LinkTypeEnum = linkType,
                DisplayOrder = displayOrder,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            };
        }
        
        /// <summary>
        /// Creates ExerciseLink from existing data (backward compatibility with string LinkType)
        /// </summary>
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
                LinkTypeEnum = null, // Will use computed property
                DisplayOrder = displayOrder,
                IsActive = isActive,
                CreatedAt = createdAt,
                UpdatedAt = updatedAt
            };
        }

        /// <summary>
        /// Creates ExerciseLink from existing data (enhanced with enum LinkType)
        /// </summary>
        public static ExerciseLink Create(
            ExerciseLinkId id,
            ExerciseId sourceExerciseId,
            ExerciseId targetExerciseId,
            string linkType,
            ExerciseLinkType? linkTypeEnum,
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
                LinkTypeEnum = linkTypeEnum,
                DisplayOrder = displayOrder,
                IsActive = isActive,
                CreatedAt = createdAt,
                UpdatedAt = updatedAt
            };
        }
    }
}