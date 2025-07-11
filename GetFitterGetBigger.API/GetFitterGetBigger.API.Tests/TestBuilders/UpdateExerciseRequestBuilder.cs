using System.Collections.Generic;
using System.Linq;
using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;
using System;

namespace GetFitterGetBigger.API.Tests.TestBuilders;

/// <summary>
/// Builder for creating UpdateExerciseRequest objects for testing
/// </summary>
public class UpdateExerciseRequestBuilder
{
    private string _name = "Test Exercise";
    private string _description = "Test Description";
    private string _difficultyId = SeedDataBuilder.StandardIds.DifficultyLevelIds.Beginner;
    private string? _kineticChainId = SeedDataBuilder.StandardIds.KineticChainTypeIds.Compound;
    private string? _exerciseWeightTypeId = SeedDataBuilder.StandardIds.ExerciseWeightTypeIds.WeightRequired;
    private List<string> _exerciseTypeIds = new() { SeedDataBuilder.StandardIds.ExerciseTypeIds.Workout };
    private List<MuscleGroupWithRoleRequest> _muscleGroups = new();
    private List<string> _equipmentIds = new();
    private List<string> _bodyPartIds = new();
    private List<string> _movementPatternIds = new();
    private List<CoachNoteRequest> _coachNotes = new();
    private string? _videoUrl;
    private string? _imageUrl;
    private bool? _isUnilateral = false;
    private bool? _isActive = true;

    public static UpdateExerciseRequestBuilder ForWorkoutExercise()
    {
        return new UpdateExerciseRequestBuilder();
    }

    public static UpdateExerciseRequestBuilder ForRestExercise()
    {
        return new UpdateExerciseRequestBuilder()
            .WithExerciseTypes(SeedDataBuilder.StandardIds.ExerciseTypeIds.Rest)
            .WithKineticChainId(null)
            .WithExerciseWeightTypeId(null);
    }

    public UpdateExerciseRequestBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public UpdateExerciseRequestBuilder WithDescription(string description)
    {
        _description = description;
        return this;
    }

    public UpdateExerciseRequestBuilder WithDifficultyId(string difficultyId)
    {
        _difficultyId = difficultyId;
        return this;
    }

    public UpdateExerciseRequestBuilder WithDifficulty(DifficultyLevel difficulty)
    {
        _difficultyId = difficulty.Id.ToString();
        return this;
    }

    public UpdateExerciseRequestBuilder WithKineticChainId(string? kineticChainId)
    {
        _kineticChainId = kineticChainId;
        return this;
    }

    public UpdateExerciseRequestBuilder WithKineticChain(KineticChainType? kineticChain)
    {
        _kineticChainId = kineticChain?.Id.ToString();
        return this;
    }

    public UpdateExerciseRequestBuilder WithExerciseWeightTypeId(string? exerciseWeightTypeId)
    {
        _exerciseWeightTypeId = exerciseWeightTypeId;
        return this;
    }

    public UpdateExerciseRequestBuilder WithWeightType(ExerciseWeightType? weightType)
    {
        _exerciseWeightTypeId = weightType?.Id.ToString();
        return this;
    }

    public UpdateExerciseRequestBuilder WithExerciseTypes(params string[] exerciseTypeIds)
    {
        _exerciseTypeIds = new List<string>(exerciseTypeIds);
        return this;
    }

    public UpdateExerciseRequestBuilder WithExerciseTypes(IEnumerable<ExerciseType> exerciseTypes)
    {
        _exerciseTypeIds = exerciseTypes.Select(et => et.Id.ToString()).ToList();
        return this;
    }

    public UpdateExerciseRequestBuilder WithMuscleGroups(params (string muscleGroupId, string muscleRoleId)[] muscleGroups)
    {
        _muscleGroups = muscleGroups.Select(mg => new MuscleGroupWithRoleRequest
        {
            MuscleGroupId = mg.muscleGroupId,
            MuscleRoleId = mg.muscleRoleId
        }).ToList();
        return this;
    }

    public UpdateExerciseRequestBuilder AddMuscleGroup(MuscleGroup muscle, MuscleRole role)
    {
        _muscleGroups.Add(new MuscleGroupWithRoleRequest
        {
            MuscleGroupId = muscle.Id.ToString(),
            MuscleRoleId = role.Id.ToString()
        });
        return this;
    }
    
    public UpdateExerciseRequestBuilder AddMuscleGroup(string muscleGroupId, string muscleRoleId)
    {
        _muscleGroups.Add(new MuscleGroupWithRoleRequest
        {
            MuscleGroupId = muscleGroupId,
            MuscleRoleId = muscleRoleId
        });
        return this;
    }

    public UpdateExerciseRequestBuilder WithCoachNotes(params (string text, int order)[] coachNotes)
    {
        _coachNotes = coachNotes.Select(cn => new CoachNoteRequest
        {
            Text = cn.text,
            Order = cn.order
        }).ToList();
        return this;
    }

    public UpdateExerciseRequestBuilder AddCoachNote(string? id, string text, int order)
    {
        _coachNotes.Add(new CoachNoteRequest
        {
            Id = id,
            Text = text,
            Order = order
        });
        return this;
    }

    public UpdateExerciseRequestBuilder AddCoachNote(CoachNoteId id, string text, int order)
    {
        _coachNotes.Add(new CoachNoteRequest
        {
            Id = id.ToString(),
            Text = text,
            Order = order
        });
        return this;
    }

    public UpdateExerciseRequestBuilder AddCoachNote(string text, int order)
    {
        _coachNotes.Add(new CoachNoteRequest
        {
            Text = text,
            Order = order
        });
        return this;
    }

    public UpdateExerciseRequestBuilder AddCoachNoteWithInvalidFormat(string text, int order)
    {
        _coachNotes.Add(new CoachNoteRequest
        {
            Id = "invalid-id",
            Text = text,
            Order = order
        });
        return this;
    }

    public UpdateExerciseRequestBuilder AddCoachNoteWithMalformedId(string text, int order)
    {
        _coachNotes.Add(new CoachNoteRequest
        {
            Id = "coachnote-not-a-guid",
            Text = text,
            Order = order
        });
        return this;
    }

    public UpdateExerciseRequestBuilder WithEquipmentIds(params string[] equipmentIds)
    {
        _equipmentIds = new List<string>(equipmentIds);
        return this;
    }

    public UpdateExerciseRequestBuilder WithBodyPartIds(params string[] bodyPartIds)
    {
        _bodyPartIds = new List<string>(bodyPartIds);
        return this;
    }

    public UpdateExerciseRequestBuilder WithMovementPatternIds(params string[] movementPatternIds)
    {
        _movementPatternIds = new List<string>(movementPatternIds);
        return this;
    }

    public UpdateExerciseRequestBuilder WithVideoUrl(string? videoUrl)
    {
        _videoUrl = videoUrl;
        return this;
    }

    public UpdateExerciseRequestBuilder WithImageUrl(string? imageUrl)
    {
        _imageUrl = imageUrl;
        return this;
    }

    public UpdateExerciseRequestBuilder WithIsUnilateral(bool? isUnilateral)
    {
        _isUnilateral = isUnilateral;
        return this;
    }

    public UpdateExerciseRequestBuilder WithIsActive(bool? isActive)
    {
        _isActive = isActive;
        return this;
    }

    public UpdateExerciseRequest Build()
    {
        return new UpdateExerciseRequest
        {
            Name = _name,
            Description = _description,
            DifficultyId = _difficultyId,
            KineticChainId = _kineticChainId,
            ExerciseWeightTypeId = _exerciseWeightTypeId,
            ExerciseTypeIds = _exerciseTypeIds,
            MuscleGroups = _muscleGroups,
            EquipmentIds = _equipmentIds,
            BodyPartIds = _bodyPartIds,
            MovementPatternIds = _movementPatternIds,
            CoachNotes = _coachNotes,
            VideoUrl = _videoUrl,
            ImageUrl = _imageUrl,
            IsUnilateral = _isUnilateral,
            IsActive = _isActive
        };
    }
}