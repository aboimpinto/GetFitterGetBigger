using System.Collections.Generic;
using System.Linq;
using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models.Entities;
using System;

namespace GetFitterGetBigger.API.Tests.TestBuilders;

/// <summary>
/// Builder for creating CreateExerciseRequest objects for testing
/// </summary>
public class CreateExerciseRequestBuilder
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
    private bool _isUnilateral = false;

    public static CreateExerciseRequestBuilder ForWorkoutExercise()
    {
        return new CreateExerciseRequestBuilder();
    }

    public static CreateExerciseRequestBuilder ForRestExercise()
    {
        return new CreateExerciseRequestBuilder()
            .WithExerciseTypes(SeedDataBuilder.StandardIds.ExerciseTypeIds.Rest)
            .WithKineticChainId(null)
            .WithExerciseWeightTypeId(null);
    }

    public CreateExerciseRequestBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public CreateExerciseRequestBuilder WithDescription(string description)
    {
        _description = description;
        return this;
    }

    public CreateExerciseRequestBuilder WithDifficultyId(string difficultyId)
    {
        _difficultyId = difficultyId;
        return this;
    }

    public CreateExerciseRequestBuilder WithDifficulty(DifficultyLevel difficulty)
    {
        _difficultyId = difficulty.Id.ToString();
        return this;
    }

    public CreateExerciseRequestBuilder WithKineticChainId(string? kineticChainId)
    {
        _kineticChainId = kineticChainId;
        return this;
    }

    public CreateExerciseRequestBuilder WithKineticChain(KineticChainType? kineticChain)
    {
        _kineticChainId = kineticChain?.Id.ToString();
        return this;
    }

    public CreateExerciseRequestBuilder WithExerciseWeightTypeId(string? exerciseWeightTypeId)
    {
        _exerciseWeightTypeId = exerciseWeightTypeId;
        return this;
    }

    public CreateExerciseRequestBuilder WithWeightType(ExerciseWeightType? weightType)
    {
        _exerciseWeightTypeId = weightType?.Id.ToString();
        return this;
    }

    public CreateExerciseRequestBuilder WithExerciseTypes(params string[] exerciseTypeIds)
    {
        _exerciseTypeIds = new List<string>(exerciseTypeIds);
        return this;
    }

    public CreateExerciseRequestBuilder WithExerciseTypes(IEnumerable<ExerciseType> exerciseTypes)
    {
        _exerciseTypeIds = exerciseTypes.Select(et => et.Id.ToString()).ToList();
        return this;
    }

    public CreateExerciseRequestBuilder WithMuscleGroups(params (string muscleGroupId, string muscleRoleId)[] muscleGroups)
    {
        _muscleGroups = muscleGroups.Select(mg => new MuscleGroupWithRoleRequest
        {
            MuscleGroupId = mg.muscleGroupId,
            MuscleRoleId = mg.muscleRoleId
        }).ToList();
        return this;
    }

    public CreateExerciseRequestBuilder AddMuscleGroup(MuscleGroup muscle, MuscleRole role)
    {
        _muscleGroups.Add(new MuscleGroupWithRoleRequest
        {
            MuscleGroupId = muscle.Id.ToString(),
            MuscleRoleId = role.Id.ToString()
        });
        return this;
    }

    public CreateExerciseRequestBuilder WithCoachNotes(params (string text, int order)[] coachNotes)
    {
        _coachNotes = coachNotes.Select(cn => new CoachNoteRequest
        {
            Text = cn.text,
            Order = cn.order
        }).ToList();
        return this;
    }

    public CreateExerciseRequestBuilder AddCoachNote(string text, int order)
    {
        _coachNotes.Add(new CoachNoteRequest
        {
            Text = text,
            Order = order
        });
        return this;
    }

    public CreateExerciseRequestBuilder WithEquipmentIds(params string[] equipmentIds)
    {
        _equipmentIds = new List<string>(equipmentIds);
        return this;
    }

    public CreateExerciseRequestBuilder WithBodyPartIds(params string[] bodyPartIds)
    {
        _bodyPartIds = new List<string>(bodyPartIds);
        return this;
    }

    public CreateExerciseRequestBuilder WithMovementPatternIds(params string[] movementPatternIds)
    {
        _movementPatternIds = new List<string>(movementPatternIds);
        return this;
    }

    public CreateExerciseRequestBuilder WithVideoUrl(string? videoUrl)
    {
        _videoUrl = videoUrl;
        return this;
    }

    public CreateExerciseRequestBuilder WithImageUrl(string? imageUrl)
    {
        _imageUrl = imageUrl;
        return this;
    }

    public CreateExerciseRequestBuilder WithIsUnilateral(bool isUnilateral)
    {
        _isUnilateral = isUnilateral;
        return this;
    }

    public CreateExerciseRequest Build()
    {
        return new CreateExerciseRequest
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
            IsUnilateral = _isUnilateral
        };
    }
}