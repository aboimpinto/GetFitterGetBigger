using System.Collections.Generic;
using System.Linq;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Services.Commands;

namespace GetFitterGetBigger.API.Tests.TestBuilders.ServiceCommands;

/// <summary>
/// Builder for creating UpdateExerciseCommand objects for testing
/// </summary>
public class UpdateExerciseCommandBuilder
{
    private string _name = "Test Exercise";
    private string _description = "Test Description";
    private DifficultyLevelId _difficultyId = DifficultyLevelId.New();
    private KineticChainTypeId _kineticChainId = KineticChainTypeId.New();
    private ExerciseWeightTypeId _exerciseWeightTypeId = ExerciseWeightTypeId.New();
    private List<ExerciseTypeId> _exerciseTypeIds = new() { ExerciseTypeId.New() };
    private List<MuscleGroupAssignment> _muscleGroups = new();
    private List<CoachNoteCommand> _coachNotes = new();
    private List<EquipmentId> _equipmentIds = new();
    private List<BodyPartId> _bodyPartIds = new();
    private List<MovementPatternId> _movementPatternIds = new();
    private string? _videoUrl;
    private string? _imageUrl;
    private bool _isUnilateral = false;
    private bool _isActive = true;

    public static UpdateExerciseCommandBuilder ForWorkoutExercise()
    {
        return new UpdateExerciseCommandBuilder();
    }

    public static UpdateExerciseCommandBuilder ForRestExercise()
    {
        return new UpdateExerciseCommandBuilder()
            .WithExerciseTypes(ExerciseTypeId.New()) // Rest type
            .WithKineticChainId(null)
            .WithExerciseWeightTypeId(null);
    }

    public UpdateExerciseCommandBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public UpdateExerciseCommandBuilder WithDescription(string description)
    {
        _description = description;
        return this;
    }

    public UpdateExerciseCommandBuilder WithDifficultyId(DifficultyLevelId difficultyId)
    {
        _difficultyId = difficultyId;
        return this;
    }

    public UpdateExerciseCommandBuilder WithKineticChainId(KineticChainTypeId? kineticChainId)
    {
        _kineticChainId = kineticChainId ?? KineticChainTypeId.Empty;
        return this;
    }

    public UpdateExerciseCommandBuilder WithExerciseWeightTypeId(ExerciseWeightTypeId? exerciseWeightTypeId)
    {
        _exerciseWeightTypeId = exerciseWeightTypeId ?? ExerciseWeightTypeId.Empty;
        return this;
    }

    public UpdateExerciseCommandBuilder WithExerciseTypes(params ExerciseTypeId[] exerciseTypeIds)
    {
        _exerciseTypeIds = new List<ExerciseTypeId>(exerciseTypeIds);
        return this;
    }

    public UpdateExerciseCommandBuilder WithMuscleGroups(params (MuscleGroupId muscleGroupId, MuscleRoleId muscleRoleId)[] muscleGroups)
    {
        _muscleGroups = muscleGroups.Select(mg => new MuscleGroupAssignment
        {
            MuscleGroupId = mg.muscleGroupId,
            MuscleRoleId = mg.muscleRoleId
        }).ToList();
        return this;
    }

    public UpdateExerciseCommandBuilder WithCoachNotes(params (string text, int order)[] coachNotes)
    {
        _coachNotes = coachNotes.Select(cn => new CoachNoteCommand
        {
            Text = cn.text,
            Order = cn.order
        }).ToList();
        return this;
    }

    public UpdateExerciseCommandBuilder WithEquipmentIds(params EquipmentId[] equipmentIds)
    {
        _equipmentIds = new List<EquipmentId>(equipmentIds);
        return this;
    }

    public UpdateExerciseCommandBuilder WithBodyPartIds(params BodyPartId[] bodyPartIds)
    {
        _bodyPartIds = new List<BodyPartId>(bodyPartIds);
        return this;
    }

    public UpdateExerciseCommandBuilder WithMovementPatternIds(params MovementPatternId[] movementPatternIds)
    {
        _movementPatternIds = new List<MovementPatternId>(movementPatternIds);
        return this;
    }

    public UpdateExerciseCommandBuilder WithVideoUrl(string? videoUrl)
    {
        _videoUrl = videoUrl;
        return this;
    }

    public UpdateExerciseCommandBuilder WithImageUrl(string? imageUrl)
    {
        _imageUrl = imageUrl;
        return this;
    }

    public UpdateExerciseCommandBuilder WithIsUnilateral(bool isUnilateral)
    {
        _isUnilateral = isUnilateral;
        return this;
    }

    public UpdateExerciseCommandBuilder WithIsActive(bool isActive)
    {
        _isActive = isActive;
        return this;
    }

    public UpdateExerciseCommand Build()
    {
        return new UpdateExerciseCommand
        {
            Name = _name,
            Description = _description,
            DifficultyId = _difficultyId,
            KineticChainId = _kineticChainId,
            ExerciseWeightTypeId = _exerciseWeightTypeId,
            ExerciseTypeIds = _exerciseTypeIds,
            MuscleGroups = _muscleGroups,
            CoachNotes = _coachNotes,
            EquipmentIds = _equipmentIds,
            BodyPartIds = _bodyPartIds,
            MovementPatternIds = _movementPatternIds,
            VideoUrl = _videoUrl,
            ImageUrl = _imageUrl,
            IsUnilateral = _isUnilateral,
            IsActive = _isActive
        };
    }
}