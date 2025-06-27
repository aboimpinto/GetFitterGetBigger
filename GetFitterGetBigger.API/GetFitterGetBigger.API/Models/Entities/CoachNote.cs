using System;
using GetFitterGetBigger.API.Models.SpecializedIds;

namespace GetFitterGetBigger.API.Models.Entities;

public record CoachNote
{
    public CoachNoteId Id { get; init; }
    public ExerciseId ExerciseId { get; init; }
    public string Text { get; init; } = string.Empty;
    public int Order { get; init; }
    
    // Navigation property
    public Exercise Exercise { get; init; } = null!;
    
    private CoachNote() { }
    
    public static class Handler
    {
        public static CoachNote CreateNew(
            ExerciseId exerciseId,
            string text,
            int order)
        {
            if (string.IsNullOrWhiteSpace(text))
                throw new ArgumentException("Text cannot be empty", nameof(text));
                
            if (text.Length > 1000)
                throw new ArgumentException("Text cannot exceed 1000 characters", nameof(text));
                
            if (order < 0)
                throw new ArgumentException("Order must be non-negative", nameof(order));
                
            return new()
            {
                Id = CoachNoteId.New(),
                ExerciseId = exerciseId,
                Text = text.Trim(),
                Order = order
            };
        }
        
        public static CoachNote Create(
            CoachNoteId id,
            ExerciseId exerciseId,
            string text,
            int order)
        {
            if (string.IsNullOrWhiteSpace(text))
                throw new ArgumentException("Text cannot be empty", nameof(text));
                
            if (text.Length > 1000)
                throw new ArgumentException("Text cannot exceed 1000 characters", nameof(text));
                
            if (order < 0)
                throw new ArgumentException("Order must be non-negative", nameof(order));
                
            return new()
            {
                Id = id,
                ExerciseId = exerciseId,
                Text = text.Trim(),
                Order = order
            };
        }
        
        public static CoachNote UpdateText(CoachNote original, string newText)
        {
            if (string.IsNullOrWhiteSpace(newText))
                throw new ArgumentException("Text cannot be empty", nameof(newText));
                
            if (newText.Length > 1000)
                throw new ArgumentException("Text cannot exceed 1000 characters", nameof(newText));
                
            return original with { Text = newText.Trim() };
        }
        
        public static CoachNote UpdateOrder(CoachNote original, int newOrder)
        {
            if (newOrder < 0)
                throw new ArgumentException("Order must be non-negative", nameof(newOrder));
                
            return original with { Order = newOrder };
        }
    }
}