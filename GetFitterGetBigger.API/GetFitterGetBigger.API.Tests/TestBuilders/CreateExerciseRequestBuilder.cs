using GetFitterGetBigger.API.DTOs;

namespace GetFitterGetBigger.API.Tests.TestBuilders;

/// <summary>
/// Builder pattern for CreateExerciseRequest to simplify test data creation
/// and ensure all validation rules are met by default
/// </summary>
public class CreateExerciseRequestBuilder
{
    private readonly CreateExerciseRequest _request;

    public CreateExerciseRequestBuilder()
    {
        _request = new CreateExerciseRequest
        {
            Name = "Test Exercise",
            Description = "Test Description",
            IsUnilateral = false,
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
            }
        };
    }

    /// <summary>
    /// Creates a REST exercise (no kinetic chain, no muscle groups, no weight type)
    /// </summary>
    public static CreateExerciseRequestBuilder ForRestExercise()
    {
        var builder = new CreateExerciseRequestBuilder();
        builder._request.ExerciseTypeIds = new List<string> { TestConstants.ExerciseTypeIds.Rest };
        builder._request.KineticChainId = null;
        builder._request.ExerciseWeightTypeId = null;
        builder._request.MuscleGroups = new List<MuscleGroupWithRoleRequest>();
        return builder;
    }

    /// <summary>
    /// Creates a non-REST exercise with all required fields
    /// </summary>
    public static CreateExerciseRequestBuilder ForWorkoutExercise()
    {
        return new CreateExerciseRequestBuilder(); // Default is already workout
    }

    public CreateExerciseRequestBuilder WithName(string name)
    {
        _request.Name = name;
        return this;
    }

    public CreateExerciseRequestBuilder WithDescription(string description)
    {
        _request.Description = description;
        return this;
    }

    public CreateExerciseRequestBuilder WithExerciseTypes(params string[] exerciseTypeIds)
    {
        _request.ExerciseTypeIds = exerciseTypeIds.ToList();
        return this;
    }

    public CreateExerciseRequestBuilder WithDifficultyId(string difficultyId)
    {
        _request.DifficultyId = difficultyId;
        return this;
    }

    public CreateExerciseRequestBuilder WithKineticChainId(string? kineticChainId)
    {
        _request.KineticChainId = kineticChainId;
        return this;
    }

    public CreateExerciseRequestBuilder WithExerciseWeightTypeId(string? exerciseWeightTypeId)
    {
        _request.ExerciseWeightTypeId = exerciseWeightTypeId;
        return this;
    }

    public CreateExerciseRequestBuilder WithMuscleGroups(params (string MuscleGroupId, string MuscleRoleId)[] muscleGroups)
    {
        _request.MuscleGroups = muscleGroups.Select(mg => new MuscleGroupWithRoleRequest
        {
            MuscleGroupId = mg.MuscleGroupId,
            MuscleRoleId = mg.MuscleRoleId
        }).ToList();
        return this;
    }

    public CreateExerciseRequestBuilder WithCoachNotes(params (string Text, int Order)[] notes)
    {
        _request.CoachNotes = notes.Select(note => new CoachNoteRequest
        {
            Text = note.Text,
            Order = note.Order
        }).ToList();
        return this;
    }

    public CreateExerciseRequestBuilder WithVideoUrl(string videoUrl)
    {
        _request.VideoUrl = videoUrl;
        return this;
    }

    public CreateExerciseRequestBuilder WithImageUrl(string imageUrl)
    {
        _request.ImageUrl = imageUrl;
        return this;
    }

    public CreateExerciseRequestBuilder WithIsUnilateral(bool isUnilateral)
    {
        _request.IsUnilateral = isUnilateral;
        return this;
    }

    public CreateExerciseRequest Build()
    {
        return _request;
    }

    /// <summary>
    /// Implicit conversion to CreateExerciseRequest for convenience
    /// </summary>
    public static implicit operator CreateExerciseRequest(CreateExerciseRequestBuilder builder)
    {
        return builder.Build();
    }
}