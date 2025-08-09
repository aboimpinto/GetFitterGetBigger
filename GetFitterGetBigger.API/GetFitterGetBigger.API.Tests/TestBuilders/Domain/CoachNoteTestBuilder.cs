using System;
using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;

namespace GetFitterGetBigger.API.Tests.TestBuilders.Domain;

/// <summary>
/// Test builder for creating valid CoachNote entities with proper validation
/// </summary>
public class CoachNoteTestBuilder
{
    private string _text = "Focus on proper form throughout the movement";
    private int _order = 0;
    private ExerciseId? _exerciseId = null;
    private CoachNoteId? _id = null;

    private CoachNoteTestBuilder() { }

    /// <summary>
    /// Creates a builder for a new coach note (without ID)
    /// </summary>
    public static CoachNoteTestBuilder New() => new CoachNoteTestBuilder();

    /// <summary>
    /// Creates a builder for an existing coach note (with ID)
    /// </summary>
    public static CoachNoteTestBuilder Existing(CoachNoteId id) => new CoachNoteTestBuilder().WithId(id);

    /// <summary>
    /// Creates a builder for an existing coach note (with ID string)
    /// </summary>
    public static CoachNoteTestBuilder Existing(string idString)
    {
        if (string.IsNullOrWhiteSpace(idString))
        {
            throw new ArgumentException("Coach note ID cannot be empty for existing note", nameof(idString));
        }
        
        // Parse the ID to ensure it's valid
        if (!idString.StartsWith("coachnote-"))
        {
            throw new ArgumentException($"Invalid CoachNoteId format: '{idString}'. Expected format: 'coachnote-{{guid}}'");
        }
        
        var guidPart = idString["coachnote-".Length..];
        if (!Guid.TryParse(guidPart, out var guid))
        {
            throw new ArgumentException($"Invalid GUID in CoachNoteId: '{guidPart}'");
        }
        
        return new CoachNoteTestBuilder().WithId(CoachNoteId.From(guid));
    }

    /// <summary>
    /// Creates a builder for a form tip coach note
    /// </summary>
    public static CoachNoteTestBuilder FormTip(string tip) => new CoachNoteTestBuilder()
        .WithText(tip)
        .WithOrder(0);

    /// <summary>
    /// Creates a builder for a safety warning coach note
    /// </summary>
    public static CoachNoteTestBuilder SafetyWarning(string warning) => new CoachNoteTestBuilder()
        .WithText($"⚠️ Safety: {warning}")
        .WithOrder(0);

    /// <summary>
    /// Creates a builder for a progression tip coach note
    /// </summary>
    public static CoachNoteTestBuilder ProgressionTip(string tip) => new CoachNoteTestBuilder()
        .WithText($"Progression: {tip}")
        .WithOrder(1);

    public CoachNoteTestBuilder WithId(CoachNoteId id)
    {
        _id = id;
        return this;
    }

    public CoachNoteTestBuilder WithText(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new ArgumentException("Coach note text cannot be empty", nameof(text));
        }
        
        if (text.Length > 1000)
        {
            throw new ArgumentException("Coach note text cannot exceed 1000 characters", nameof(text));
        }
        
        _text = text;
        return this;
    }

    public CoachNoteTestBuilder WithOrder(int order)
    {
        if (order < 0)
        {
            throw new ArgumentException("Coach note order must be non-negative", nameof(order));
        }
        _order = order;
        return this;
    }

    public CoachNoteTestBuilder ForExercise(ExerciseId exerciseId)
    {
        _exerciseId = exerciseId;
        return this;
    }

    public CoachNoteTestBuilder ForExercise(string exerciseIdString)
    {
        if (!exerciseIdString.StartsWith("exercise-"))
        {
            throw new ArgumentException($"Invalid ExerciseId format: '{exerciseIdString}'. Expected format: 'exercise-{{guid}}'");
        }
        
        var guidPart = exerciseIdString["exercise-".Length..];
        if (!Guid.TryParse(guidPart, out var guid))
        {
            throw new ArgumentException($"Invalid GUID in ExerciseId: '{guidPart}'");
        }
        
        _exerciseId = ExerciseId.From(guid);
        return this;
    }

    /// <summary>
    /// Builds a CoachNote entity with validation
    /// </summary>
    public CoachNote Build()
    {
        if (_exerciseId == null)
        {
            throw new InvalidOperationException("ExerciseId is required. Use ForExercise() to set it.");
        }

        // If ID is provided, use Create method, otherwise use CreateNew
        if (_id.HasValue)
        {
            return CoachNote.Handler.Create(
                id: _id.Value,
                exerciseId: _exerciseId.Value,
                text: _text,
                order: _order
            );
        }
        else
        {
            return CoachNote.Handler.CreateNew(
                exerciseId: _exerciseId.Value,
                text: _text,
                order: _order
            );
        }
    }

    /// <summary>
    /// Builds a CoachNoteRequest DTO for use in API requests
    /// This is what you'd use when creating requests that include coach notes
    /// </summary>
    public CoachNoteRequest BuildRequest()
    {
        return new CoachNoteRequest
        {
            Id = _id?.ToString(),
            Text = _text,
            Order = _order
        };
    }

    /// <summary>
    /// Implicit conversion to CoachNote for convenience
    /// </summary>
    public static implicit operator CoachNote(CoachNoteTestBuilder builder)
    {
        return builder.Build();
    }
}