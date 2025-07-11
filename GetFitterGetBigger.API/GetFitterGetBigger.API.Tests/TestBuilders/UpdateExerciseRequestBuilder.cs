using GetFitterGetBigger.API.DTOs;

namespace GetFitterGetBigger.API.Tests.TestBuilders;

/// <summary>
/// Builder pattern for UpdateExerciseRequest to simplify test data creation
/// and ensure all validation rules are met by default
/// </summary>
public class UpdateExerciseRequestBuilder
{
    private readonly UpdateExerciseRequest _request;

    public UpdateExerciseRequestBuilder()
    {
        _request = new UpdateExerciseRequest
        {
            Name = "Updated Test Exercise",
            Description = "Updated Test Description",
            IsUnilateral = false,
            IsActive = true,
            DifficultyId = TestConstants.DifficultyLevelIds.Beginner,
            KineticChainId = TestConstants.KineticChainTypeIds.Compound,
            ExerciseWeightTypeId = TestConstants.ExerciseWeightTypeIds.WeightRequired,
            ExerciseTypeIds = new List<string> { TestConstants.ExerciseTypeIds.Workout },
            MuscleGroups = new List<MuscleGroupWithRoleRequest>
            {
                new MuscleGroupWithRoleRequest
                {
                    MuscleGroupId = TestConstants.MuscleGroupIds.Chest,
                    MuscleRoleId = TestConstants.MuscleRoleIds.Primary
                }
            },
            EquipmentIds = new List<string> { TestConstants.EquipmentIds.Barbell },
            BodyPartIds = new List<string> { TestConstants.BodyPartIds.Chest },
            MovementPatternIds = new List<string> { TestConstants.MovementPatternIds.Push }
        };
    }

    /// <summary>
    /// Creates a REST exercise update (no kinetic chain, no muscle groups, no weight type)
    /// </summary>
    public static UpdateExerciseRequestBuilder ForRestExercise()
    {
        var builder = new UpdateExerciseRequestBuilder();
        builder._request.ExerciseTypeIds = new List<string> { TestConstants.ExerciseTypeIds.Rest };
        builder._request.KineticChainId = null;
        builder._request.ExerciseWeightTypeId = null;
        builder._request.MuscleGroups = new List<MuscleGroupWithRoleRequest>();
        builder._request.EquipmentIds = new List<string>();
        builder._request.BodyPartIds = new List<string>();
        builder._request.MovementPatternIds = new List<string>();
        return builder;
    }

    /// <summary>
    /// Creates a non-REST exercise update with all required fields
    /// </summary>
    public static UpdateExerciseRequestBuilder ForWorkoutExercise()
    {
        return new UpdateExerciseRequestBuilder(); // Default is already workout
    }

    public UpdateExerciseRequestBuilder WithName(string name)
    {
        _request.Name = name;
        return this;
    }

    public UpdateExerciseRequestBuilder WithDescription(string description)
    {
        _request.Description = description;
        return this;
    }

    public UpdateExerciseRequestBuilder WithExerciseTypes(params string[] exerciseTypeIds)
    {
        _request.ExerciseTypeIds = exerciseTypeIds.ToList();
        return this;
    }

    public UpdateExerciseRequestBuilder WithDifficultyId(string difficultyId)
    {
        _request.DifficultyId = difficultyId;
        return this;
    }

    public UpdateExerciseRequestBuilder WithKineticChainId(string? kineticChainId)
    {
        _request.KineticChainId = kineticChainId;
        return this;
    }

    public UpdateExerciseRequestBuilder WithExerciseWeightTypeId(string? exerciseWeightTypeId)
    {
        _request.ExerciseWeightTypeId = exerciseWeightTypeId;
        return this;
    }

    public UpdateExerciseRequestBuilder WithMuscleGroups(params (string MuscleGroupId, string MuscleRoleId)[] muscleGroups)
    {
        _request.MuscleGroups = muscleGroups.Select(mg => new MuscleGroupWithRoleRequest
        {
            MuscleGroupId = mg.MuscleGroupId,
            MuscleRoleId = mg.MuscleRoleId
        }).ToList();
        return this;
    }

    public UpdateExerciseRequestBuilder WithCoachNotes(params (string Text, int Order)[] notes)
    {
        _request.CoachNotes = notes.Select(note => new CoachNoteRequest
        {
            Text = note.Text,
            Order = note.Order
        }).ToList();
        return this;
    }

    public UpdateExerciseRequestBuilder WithCoachNotes(params (string Id, string Text, int Order)[] notes)
    {
        _request.CoachNotes = notes.Select(note => new CoachNoteRequest
        {
            Id = note.Id,
            Text = note.Text,
            Order = note.Order
        }).ToList();
        return this;
    }

    public UpdateExerciseRequestBuilder WithVideoUrl(string videoUrl)
    {
        _request.VideoUrl = videoUrl;
        return this;
    }

    public UpdateExerciseRequestBuilder WithImageUrl(string imageUrl)
    {
        _request.ImageUrl = imageUrl;
        return this;
    }

    public UpdateExerciseRequestBuilder WithIsUnilateral(bool? isUnilateral)
    {
        _request.IsUnilateral = isUnilateral;
        return this;
    }

    public UpdateExerciseRequestBuilder WithIsActive(bool? isActive)
    {
        _request.IsActive = isActive;
        return this;
    }

    public UpdateExerciseRequestBuilder WithEquipmentIds(params string[] equipmentIds)
    {
        _request.EquipmentIds = equipmentIds.ToList();
        return this;
    }

    public UpdateExerciseRequestBuilder WithBodyPartIds(params string[] bodyPartIds)
    {
        _request.BodyPartIds = bodyPartIds.ToList();
        return this;
    }

    public UpdateExerciseRequestBuilder WithMovementPatternIds(params string[] movementPatternIds)
    {
        _request.MovementPatternIds = movementPatternIds.ToList();
        return this;
    }

    public UpdateExerciseRequest Build()
    {
        return _request;
    }

    /// <summary>
    /// Implicit conversion to UpdateExerciseRequest for convenience
    /// </summary>
    public static implicit operator UpdateExerciseRequest(UpdateExerciseRequestBuilder builder)
    {
        return builder.Build();
    }
}