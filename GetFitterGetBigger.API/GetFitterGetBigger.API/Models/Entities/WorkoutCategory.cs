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
            var newValue = value ?? category.Value;
            var newIcon = icon ?? category.Icon;
            var newColor = color ?? category.Color;
            var newDisplayOrder = displayOrder ?? category.DisplayOrder;
            
            return Validate.For<WorkoutCategory>()
                .EnsureNotEmpty(newValue, WorkoutCategoryErrorMessages.ValueCannotBeEmpty)
                .EnsureMaxLength(newValue, 100, WorkoutCategoryErrorMessages.ValueExceedsMaxLength)
                .EnsureNotEmpty(newIcon, WorkoutCategoryErrorMessages.IconIsRequired)
                .EnsureMaxLength(newIcon, 50, WorkoutCategoryErrorMessages.IconExceedsMaxLength)
                .EnsureNotEmpty(newColor, WorkoutCategoryErrorMessages.ColorIsRequired)
                .Ensure(() => HexColorRegex.IsMatch(newColor), WorkoutCategoryErrorMessages.InvalidHexColorCode)
                .EnsureMinValue(newDisplayOrder, 0, WorkoutCategoryErrorMessages.DisplayOrderMustBeNonNegative)
                .OnSuccess(() => category with
                {
                    Value = newValue,
                    Description = description is null ? category.Description : description,
                    Icon = newIcon,
                    Color = newColor,
                    PrimaryMuscleGroups = primaryMuscleGroups is null ? category.PrimaryMuscleGroups : primaryMuscleGroups,
                    DisplayOrder = newDisplayOrder,
                    IsActive = isActive is null ? category.IsActive : isActive.Value
                });
        }
            
        public static EntityResult<WorkoutCategory> Deactivate(WorkoutCategory category) =>
            EntityResult<WorkoutCategory>.Success(category with { IsActive = false });
    }
}