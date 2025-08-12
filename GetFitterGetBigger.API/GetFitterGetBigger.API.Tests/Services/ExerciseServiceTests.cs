using FluentAssertions;
using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Repositories.Interfaces;
using GetFitterGetBigger.API.Services.Implementations;
using GetFitterGetBigger.API.Services.Results;
using GetFitterGetBigger.API.Tests.TestBuilders;
using GetFitterGetBigger.API.Tests.TestBuilders.Domain;
using GetFitterGetBigger.API.Tests.Services.Extensions;
using GetFitterGetBigger.API.Mappers;
using Moq;
using Moq.AutoMock;

namespace GetFitterGetBigger.API.Tests.Services
{
    public class ExerciseServiceTests
    {
        [Fact]
        public async Task GetPagedAsync_WithNameFilter_ReturnsOnlyMatchingExercises()
        {
            // Arrange
            var automocker = new AutoMocker();
            var testee = automocker.CreateInstance<ExerciseService>();
            
            const string searchTerm = "Press";
            const string matchingExercise1 = "Bench Press";
            const string matchingExercise2 = "Overhead Press";
            
            var filterParams = new ExerciseFilterParams
            {
                Page = 1,
                PageSize = 10,
                Name = searchTerm
            };
            
            var exercises = new List<Exercise>
            {
                ExerciseBuilder.AWorkoutExercise()
                    .WithName(matchingExercise1)
                    .Build(),
                ExerciseBuilder.AWorkoutExercise()
                    .WithName(matchingExercise2)
                    .Build()
            };

            automocker
                .SetupExerciseUnitOfWork()
                .SetupExerciseGetPaged(exercises, exercises.Count);

            // Act
            var result = await testee.GetPagedAsync(filterParams.ToCommand());

            // Assert
            result.Should().NotBeNull();
            result.Data.TotalCount.Should().Be(exercises.Count);
            result.Data.Items.Should().HaveCount(exercises.Count);
            result.Data.Items.Should().OnlyContain(dto => dto.Name.Contains(searchTerm));
        }

        [Fact]
        public async Task GetByIdAsync_WithExistingId_ReturnsCorrectExercise()
        {
            // Arrange
            var automocker = new AutoMocker();
            var testee = automocker.CreateInstance<ExerciseService>();
            
            var exerciseId = ExerciseId.New();
            const string expectedName = "Bench Press";
            
            var exercise = ExerciseBuilder.AWorkoutExercise()
                .WithId(exerciseId)
                .WithName(expectedName)
                .Build();

            automocker
                .SetupExerciseUnitOfWork()
                .SetupExerciseGetById(exercise);

            // Act
            var result = await testee.GetByIdAsync(exerciseId);

            // Assert
            result.Should().NotBeNull();
            result.Data.Name.Should().Be(expectedName);
        }

        [Fact]
        public async Task GetByIdAsync_WithEmptyId_ReturnsEmptyDto()
        {
            // Arrange
            var automocker = new AutoMocker();
            var testee = automocker.CreateInstance<ExerciseService>();
            
            const string invalidIdString = "invalid-id";
            var parsedId = ExerciseId.ParseOrEmpty(invalidIdString);

            // Act
            var result = await testee.GetByIdAsync(parsedId);

            // Assert
            result.Should().NotBeNull();
            result.Data.IsEmpty.Should().BeTrue(because: "invalid ID should result in Empty ID which returns Empty DTO");
        }

        [Fact]
        public async Task CreateAsync_WithNewExerciseName_CreatesAndReturnsExercise()
        {
            // Arrange
            var automocker = new AutoMocker();
            var testee = automocker.CreateInstance<ExerciseService>();
            
            const string exerciseName = "Squat";
            
            var request = CreateExerciseRequestBuilder.ForWorkoutExercise()
                .WithName(exerciseName)
                .AddMuscleGroup(MuscleGroupTestBuilder.Chest(), MuscleRoleTestBuilder.Primary())
                .Build();

            var createdExercise = ExerciseBuilder.AWorkoutExercise()
                .WithName(exerciseName)
                .Build();

            automocker
                .SetupExerciseUnitOfWork()
                .SetupExerciseAdd(createdExercise)
                .SetupExerciseExists(exerciseName, false)
                .SetupExerciseTypeServiceAllExist();

            // Act
            var result = await testee.CreateAsync(request.ToCommand());

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Data.Name.Should().Be(exerciseName);
            
            automocker.VerifyWritableUnitOfWorkCommitOnce();
        }

        [Fact]
        public async Task CreateAsync_WithExistingExerciseName_ReturnsAlreadyExistsError()
        {
            // Arrange
            var automocker = new AutoMocker();
            var testee = automocker.CreateInstance<ExerciseService>();
            
            const string existingExerciseName = "Bench Press";
            
            var request = CreateExerciseRequestBuilder.ForWorkoutExercise()
                .WithName(existingExerciseName)
                .AddMuscleGroup(MuscleGroupTestBuilder.Chest(), MuscleRoleTestBuilder.Primary())
                .Build();

            automocker
                .SetupExerciseUnitOfWork()
                .SetupExerciseExists(existingExerciseName, true)
                .SetupExerciseTypeServiceAllExist();

            // Act
            var result = await testee.CreateAsync(request.ToCommand());

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Data.IsEmpty.Should().BeTrue();
            result.PrimaryErrorCode.Should().Be(ServiceErrorCode.AlreadyExists);
        }

        [Fact]
        public async Task UpdateAsync_WithNewName_UpdatesExerciseNameAndActiveStatus()
        {
            // Arrange
            var automocker = new AutoMocker();
            var testee = automocker.CreateInstance<ExerciseService>();
            
            var exerciseId = ExerciseId.New();
            const string originalName = "Bench Press";
            const string updatedName = "Incline Bench Press";
            
            var request = UpdateExerciseRequestBuilder.ForWorkoutExercise()
                .WithName(updatedName)
                .WithIsActive(true)
                .AddMuscleGroup(MuscleGroupTestBuilder.Chest(), MuscleRoleTestBuilder.Primary())
                .Build();

            var existingExercise = ExerciseBuilder.AWorkoutExercise()
                .WithId(exerciseId)
                .WithName(originalName)
                .AsInactive()
                .Build();
            
            var updatedExercise = ExerciseBuilder.AWorkoutExercise()
                .WithId(exerciseId)
                .WithName(updatedName)
                .AsActive()
                .Build();

            // Setup different returns for multiple calls
            var getByIdCallCount = 0;
            automocker.GetMock<IExerciseRepository>()
                .Setup(r => r.GetByIdAsync(It.IsAny<ExerciseId>()))
                .ReturnsAsync(() => {
                    getByIdCallCount++;
                    return getByIdCallCount == 1 ? existingExercise : updatedExercise;
                });

            automocker
                .SetupExerciseUnitOfWork()
                .SetupExerciseUpdate()
                .SetupExerciseExists(updatedName, false)
                .SetupExerciseTypeServiceAllExist();

            // Act
            var result = await testee.UpdateAsync(exerciseId, request.ToCommand());

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Data.Name.Should().Be(updatedName);
            
            automocker.VerifyWritableUnitOfWorkCommitOnce();
        }

        [Fact]
        public async Task DeleteAsync_WithExistingExercise_PerformsSoftDelete()
        {
            // Arrange
            var automocker = new AutoMocker();
            var testee = automocker.CreateInstance<ExerciseService>();
            
            var exerciseId = ExerciseId.New();
            var exercise = ExerciseBuilder.AWorkoutExercise()
                .WithId(exerciseId)
                .Build();

            automocker
                .SetupExerciseUnitOfWork()
                .SetupExerciseGetById(exercise)
                .SetupExerciseSoftDelete();

            // Act
            var result = await testee.DeleteAsync(exerciseId);

            // Assert
            result.IsSuccess.Should().BeTrue();
            
            automocker
                .VerifyExerciseSoftDeleteOnce(exerciseId)
                .VerifyExerciseUpdateNeverCalled();
        }

        [Fact]
        public async Task DeleteAsync_WithNonExistentExercise_ReturnsNotFoundError()
        {
            // Arrange
            var automocker = new AutoMocker();
            var testee = automocker.CreateInstance<ExerciseService>();
            
            var exerciseId = ExerciseId.New();

            automocker
                .SetupExerciseUnitOfWork()
                .SetupExerciseGetById(Exercise.Empty);

            // Act
            var result = await testee.DeleteAsync(exerciseId);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.PrimaryErrorCode.Should().Be(ServiceErrorCode.NotFound);
            
            automocker.VerifyExerciseSoftDeleteNeverCalled();
        }

        [Fact]
        public async Task CreateAsync_RestExerciseWithWeightType_ReturnsValidationError()
        {
            // Arrange
            var automocker = new AutoMocker();
            var testee = automocker.CreateInstance<ExerciseService>();
            
            const string restExerciseName = "Rest Period";
            
            var request = CreateExerciseRequestBuilder.ForRestExercise()
                .WithName(restExerciseName)
                .WithExerciseWeightTypeId(TestIds.ExerciseWeightTypeIds.WeightRequired) // Invalid for REST
                .Build();

            automocker
                .SetupExerciseUnitOfWork()
                .SetupExerciseTypeServiceIsRestType(true)
                .SetupExerciseTypeServiceAllExist();

            // Act
            var result = await testee.CreateAsync(request.ToCommand());
            
            // Assert
            result.IsSuccess.Should().BeFalse(because: "REST exercises cannot have weight types");
            result.PrimaryErrorCode.Should().Be(ServiceErrorCode.ValidationFailed);
        }

        [Fact]
        public async Task CreateAsync_WorkoutExerciseWithoutWeightType_CreatesSuccessfully()
        {
            // Arrange
            var automocker = new AutoMocker();
            var testee = automocker.CreateInstance<ExerciseService>();
            
            const string exerciseName = "Push-up";
            
            var request = CreateExerciseRequestBuilder.ForWorkoutExercise()
                .WithName(exerciseName)
                .WithExerciseWeightTypeId(null) // Optional for non-REST exercises
                .AddMuscleGroup(MuscleGroupTestBuilder.Chest(), MuscleRoleTestBuilder.Primary())
                .Build();

            var createdExercise = ExerciseBuilder.AWorkoutExercise()
                .WithName(exerciseName)
                .Build();

            automocker
                .SetupExerciseUnitOfWork()
                .SetupExerciseTypeServiceIsRestType(false)
                .SetupExerciseTypeServiceAllExist()
                .SetupExerciseExists(exerciseName, false)
                .SetupExerciseAdd(createdExercise);

            // Act
            var result = await testee.CreateAsync(request.ToCommand());
            
            // Assert
            result.IsSuccess.Should().BeTrue(because: "weight type is optional for non-REST exercises");
            result.Data.Should().NotBeNull();
            result.Data.IsEmpty.Should().BeFalse();
        }

        [Fact]
        public async Task CreateAsync_WorkoutExerciseWithWeightType_CreatesSuccessfully()
        {
            // Arrange
            var automocker = new AutoMocker();
            var testee = automocker.CreateInstance<ExerciseService>();
            
            const string exerciseName = "Barbell Squat";
            var weightTypeId = "exerciseweighttype-" + Guid.NewGuid();
            
            var request = CreateExerciseRequestBuilder.ForWorkoutExercise()
                .WithName(exerciseName)
                .WithExerciseWeightTypeId(weightTypeId)
                .AddMuscleGroup(MuscleGroupTestBuilder.Chest(), MuscleRoleTestBuilder.Primary())
                .Build();

            var createdExercise = ExerciseBuilder.AWorkoutExercise()
                .WithName(exerciseName)
                .Build();

            automocker
                .SetupExerciseUnitOfWork()
                .SetupExerciseAdd(createdExercise)
                .SetupExerciseExists(exerciseName, false)
                .SetupExerciseTypeServiceIsRestType(false)
                .SetupExerciseTypeServiceAllExist();

            // Act
            var result = await testee.CreateAsync(request.ToCommand());

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Data.Name.Should().Be(exerciseName);
            
            automocker.VerifyWritableUnitOfWorkCommitOnce();
        }

        [Fact]
        public async Task UpdateAsync_RestExerciseWithWeightType_ReturnsValidationError()
        {
            // Arrange
            var automocker = new AutoMocker();
            var testee = automocker.CreateInstance<ExerciseService>();
            
            var exerciseId = ExerciseId.New();
            const string restExerciseName = "Active Rest";
            var invalidWeightTypeId = "exerciseweighttype-" + Guid.NewGuid();
            
            var request = UpdateExerciseRequestBuilder.ForRestExercise()
                .WithName(restExerciseName)
                .WithExerciseTypes(TestIds.ExerciseTypeIds.Rest)
                .WithExerciseWeightTypeId(invalidWeightTypeId) // Invalid for REST
                .Build();

            var existingExercise = ExerciseBuilder.AWorkoutExercise()
                .WithId(exerciseId)
                .Build();

            automocker
                .SetupExerciseUnitOfWork()
                .SetupExerciseGetById(existingExercise)
                .SetupExerciseUpdate()
                .SetupExerciseTypeServiceIsRestType(true);

            // Act
            var result = await testee.UpdateAsync(exerciseId, request.ToCommand());
            
            // Assert
            result.IsSuccess.Should().BeFalse(because: "REST exercises cannot have weight types");
            result.PrimaryErrorCode.Should().Be(ServiceErrorCode.ValidationFailed);
        }

        [Fact]
        public async Task UpdateAsync_WorkoutExerciseWithoutWeightType_UpdatesSuccessfully()
        {
            // Arrange
            var automocker = new AutoMocker();
            var testee = automocker.CreateInstance<ExerciseService>();
            
            var exerciseId = ExerciseId.New();
            const string originalName = "Pull-up";
            const string updatedName = "Wide-Grip Pull-up";
            
            var request = UpdateExerciseRequestBuilder.ForWorkoutExercise()
                .WithName(updatedName)
                .WithExerciseWeightTypeId(null) // Optional for non-REST exercises
                .AddMuscleGroup(MuscleGroupTestBuilder.Chest(), MuscleRoleTestBuilder.Primary())
                .Build();

            var existingExercise = ExerciseBuilder.AWorkoutExercise()
                .WithId(exerciseId)
                .WithName(originalName)
                .Build();

            automocker
                .SetupExerciseUnitOfWork()
                .SetupExerciseGetById(existingExercise)
                .SetupExerciseUpdate()
                .SetupExerciseTypeServiceIsRestType(false)
                .SetupExerciseTypeServiceAllExist()
                .SetupExerciseExists(updatedName, false);

            // Act
            var result = await testee.UpdateAsync(exerciseId, request.ToCommand());
            
            // Assert
            result.IsSuccess.Should().BeTrue(because: "weight type is optional for non-REST exercises");
            result.Data.Should().NotBeNull();
            result.Data.IsEmpty.Should().BeFalse();
        }
    }
}