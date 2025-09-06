using System;
using System.Text.RegularExpressions;
using GetFitterGetBigger.API.Constants;
using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Models.Results;
using GetFitterGetBigger.API.Models.Validation;

namespace GetFitterGetBigger.API.Models.Entities;

public record WorkoutCategory : ReferenceDataBase, IPureReference, IEmptyEntity<WorkoutCategory>, ICacheableEntity
{
    public WorkoutCategoryId WorkoutCategoryId { get; init; }
    
    public string Id => WorkoutCategoryId.ToString();
    public string Icon { get; init; } = string.Empty;
    public string Color { get; init; } = string.Empty;
    public string? PrimaryMuscleGroups { get; init; }
    
    public bool IsEmpty => WorkoutCategoryId.IsEmpty;
    
    private WorkoutCategory() { }
    
    public CacheStrategy GetCacheStrategy() => CacheStrategy.Eternal;
    
    public TimeSpan? GetCacheDuration() => null; // Eternal caching
    
    public static WorkoutCategory Empty { get; } = new()
    {
        WorkoutCategoryId = WorkoutCategoryId.Empty,
        Value = string.Empty,
        Description = null,
        Icon = string.Empty,
        Color = "#000000",
        PrimaryMuscleGroups = null,
        DisplayOrder = 0,
        IsActive = false
    };
    
    public static class Handler
    {
        private static readonly Regex HexColorRegex = new(@"^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$", RegexOptions.Compiled);
        
        public static EntityResult<WorkoutCategory> CreateNew(
            string value,
            string? description,
            string icon,
            string color,
            string? primaryMuscleGroups,
            int displayOrder,
            bool isActive = true)
        {
            return Create(
                WorkoutCategoryId.New(),
                value,
                description,
                icon,
                color,
                primaryMuscleGroups,
                displayOrder,
                isActive
            );
        }
        
        public static EntityResult<WorkoutCategory> Create(
            WorkoutCategoryId id,
            string value,
            string? description,
            string icon,
            string color,
            string? primaryMuscleGroups,
            int displayOrder,
            bool isActive = true)
        {
            return Validate.For<WorkoutCategory>()
                .Ensure(() => !id.IsEmpty, WorkoutCategoryErrorMessages.IdCannotBeEmpty)
                .EnsureNotEmpty(value, WorkoutCategoryErrorMessages.ValueCannotBeEmpty)
                .EnsureMaxLength(value, 100, WorkoutCategoryErrorMessages.ValueExceedsMaxLength)
                .EnsureNotEmpty(icon, WorkoutCategoryErrorMessages.IconIsRequired)
                .EnsureMaxLength(icon, 50, WorkoutCategoryErrorMessages.IconExceedsMaxLength)
                .EnsureNotEmpty(color, WorkoutCategoryErrorMessages.ColorIsRequired)
                .Ensure(() => HexColorRegex.IsMatch(color), WorkoutCategoryErrorMessages.InvalidHexColorCode)
                .EnsureMinValue(displayOrder, 0, WorkoutCategoryErrorMessages.DisplayOrderMustBeNonNegative)
                .OnSuccess(() => new WorkoutCategory
                {
                    WorkoutCategoryId = id,
                    Value = value,
                    Description = description,
                    Icon = icon,
                    Color = color,
                    PrimaryMuscleGroups = primaryMuscleGroups,
                    DisplayOrder = displayOrder,
                    IsActive = isActive
                });
        }
            
        public static EntityResult<WorkoutCategory> Update(
            WorkoutCategory category,
            string? value = null,
            string? description = null,
            string? icon = null,
            string? color = null,
            string? primaryMuscleGroups = null,
            int? displayOrder = null,
            bool? isActive = null)
        {
            return ValidateUpdateParameters(value, icon, color, displayOrder)
                .OnSuccess(() => CreateUpdatedCategory(category, value, description, icon, color, primaryMuscleGroups, displayOrder, isActive));
        }
        
        /// <summary>
        /// Validates the parameters provided for update. Only validates non-null parameters.
        /// Null parameters are treated as "keep original value".
        /// </summary>
        private static EntityValidation<WorkoutCategory> ValidateUpdateParameters(
            string? value,
            string? icon,
            string? color,
            int? displayOrder)
        {
            var validation = Validate.For<WorkoutCategory>();
            
            // Only validate parameters that are explicitly provided (not null)
            if (value != null)
            {
                validation = validation
                    .Ensure(() => !string.IsNullOrWhiteSpace(value), WorkoutCategoryErrorMessages.ValueCannotBeEmpty)
                    .EnsureMaxLength(value, 100, WorkoutCategoryErrorMessages.ValueExceedsMaxLength);
            }
            
            if (icon != null)
            {
                validation = validation
                    .Ensure(() => !string.IsNullOrWhiteSpace(icon), WorkoutCategoryErrorMessages.IconIsRequired)
                    .EnsureMaxLength(icon, 50, WorkoutCategoryErrorMessages.IconExceedsMaxLength);
            }
            
            if (color != null)
            {
                validation = validation
                    .Ensure(() => !string.IsNullOrWhiteSpace(color), WorkoutCategoryErrorMessages.ColorIsRequired);
                    
                // Only validate hex format if color is not empty/whitespace
                if (!string.IsNullOrWhiteSpace(color))
                {
                    validation = validation.Ensure(() => HexColorRegex.IsMatch(color), WorkoutCategoryErrorMessages.InvalidHexColorCode);
                }
            }
            
            if (displayOrder.HasValue)
            {
                validation = validation.EnsureMinValue(displayOrder.Value, 0, WorkoutCategoryErrorMessages.DisplayOrderMustBeNonNegative);
            }
            
            return validation;
        }
        
        /// <summary>
        /// Creates the updated WorkoutCategory with all new values, using provided values or keeping originals
        /// </summary>
        private static WorkoutCategory CreateUpdatedCategory(
            WorkoutCategory category,
            string? value,
            string? description, 
            string? icon,
            string? color,
            string? primaryMuscleGroups, 
            int? displayOrder,
            bool? isActive)
        {
            return category with
            {
                Value = value ?? category.Value,
                Description = description ?? category.Description,
                Icon = icon ?? category.Icon,
                Color = color ?? category.Color,
                PrimaryMuscleGroups = primaryMuscleGroups ?? category.PrimaryMuscleGroups,
                DisplayOrder = displayOrder ?? category.DisplayOrder,
                IsActive = isActive ?? category.IsActive
            };
        }
            
        public static EntityResult<WorkoutCategory> Deactivate(WorkoutCategory category) =>
            EntityResult<WorkoutCategory>.Success(category with { IsActive = false });
    }
}