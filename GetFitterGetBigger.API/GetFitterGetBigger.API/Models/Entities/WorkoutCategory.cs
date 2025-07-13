using System;
using System.Text.RegularExpressions;
using GetFitterGetBigger.API.Models.SpecializedIds;

namespace GetFitterGetBigger.API.Models.Entities;

public record WorkoutCategory : ReferenceDataBase
{
    public WorkoutCategoryId Id { get; init; }
    public string Icon { get; init; } = string.Empty;
    public string Color { get; init; } = string.Empty;
    public string? PrimaryMuscleGroups { get; init; }
    
    private WorkoutCategory() { }
    
    public static class Handler
    {
        private static readonly Regex HexColorRegex = new(@"^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$", RegexOptions.Compiled);
        
        public static WorkoutCategory CreateNew(
            string value,
            string? description,
            string icon,
            string color,
            string? primaryMuscleGroups,
            int displayOrder,
            bool isActive = true)
        {
            ValidateParameters(value, icon, color);
                
            return new()
            {
                Id = WorkoutCategoryId.New(),
                Value = value,
                Description = description,
                Icon = icon,
                Color = color,
                PrimaryMuscleGroups = primaryMuscleGroups,
                DisplayOrder = displayOrder,
                IsActive = isActive
            };
        }
        
        public static WorkoutCategory Create(
            WorkoutCategoryId id,
            string value,
            string? description,
            string icon,
            string color,
            string? primaryMuscleGroups,
            int displayOrder,
            bool isActive = true)
        {
            ValidateParameters(value, icon, color);
            
            return new()
            {
                Id = id,
                Value = value,
                Description = description,
                Icon = icon,
                Color = color,
                PrimaryMuscleGroups = primaryMuscleGroups,
                DisplayOrder = displayOrder,
                IsActive = isActive
            };
        }
            
        public static WorkoutCategory Update(
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
            
            ValidateParameters(newValue, newIcon, newColor);
            
            return category with
            {
                Value = newValue,
                Description = description ?? category.Description,
                Icon = newIcon,
                Color = newColor,
                PrimaryMuscleGroups = primaryMuscleGroups ?? category.PrimaryMuscleGroups,
                DisplayOrder = displayOrder ?? category.DisplayOrder,
                IsActive = isActive ?? category.IsActive
            };
        }
            
        public static WorkoutCategory Deactivate(WorkoutCategory category) =>
            category with { IsActive = false };
            
        private static void ValidateParameters(string value, string icon, string color)
        {
            if (string.IsNullOrEmpty(value))
                throw new ArgumentException("Value cannot be empty", nameof(value));
            
            if (value.Length > 100)
                throw new ArgumentException("Value cannot exceed 100 characters", nameof(value));
                
            if (string.IsNullOrEmpty(icon))
                throw new ArgumentException("Icon is required", nameof(icon));
                
            if (icon.Length > 50)
                throw new ArgumentException("Icon cannot exceed 50 characters", nameof(icon));
                
            if (string.IsNullOrEmpty(color))
                throw new ArgumentException("Color is required", nameof(color));
                
            if (!HexColorRegex.IsMatch(color))
                throw new ArgumentException("Color must be a valid hex color code", nameof(color));
        }
    }
}