using FluentAssertions;
using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Repositories.Interfaces;
using GetFitterGetBigger.API.Services.Exercise;
using GetFitterGetBigger.API.Services.Interfaces;
using GetFitterGetBigger.API.Services.Results;
using GetFitterGetBigger.API.Services.WorkoutTemplate.Features.Equipment;
using GetFitterGetBigger.API.Services.WorkoutTemplate.Models.DTOs;
using GetFitterGetBigger.API.Tests.Services.Extensions;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.AutoMock;
using Olimpo.EntityFramework.Persistency;
using Xunit;

namespace GetFitterGetBigger.API.Tests.Services.WorkoutTemplate.Features.Equipment;

public class EquipmentRequirementsServiceTests
{
    [Fact]
    public async Task GetRequiredEquipmentAsync_WithEmptyId_ReturnsFailure()
    {
        // Arrange
        var autoMocker = new AutoMocker();
        var testee = autoMocker.CreateInstance<EquipmentRequirementsService>();
        var emptyId = WorkoutTemplateId.Empty;
        
        // Act
        var result = await testee.GetRequiredEquipmentAsync(emptyId);
        
        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle();
        result.Errors.First().Should().Contain("Invalid");
    }
    
    [Fact]
    public async Task GetRequiredEquipmentAsync_WithNonExistentTemplate_ReturnsFailure()
    {
        // Arrange
        var autoMocker = new AutoMocker();
        var testee = autoMocker.CreateInstance<EquipmentRequirementsService>();
        var templateId = WorkoutTemplateId.ParseOrEmpty("workouttemplate-00000000-0000-0000-0000-000000000001");
        
        // Setup repository
        autoMocker.SetupWorkoutTemplateExists(templateId, exists: false);
        
        // Act
        var result = await testee.GetRequiredEquipmentAsync(templateId);
        
        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle();
        result.Errors.First().ToLower().Should().Contain("not found");
    }
    
    [Fact]
    public async Task GetRequiredEquipmentAsync_WithTemplateHavingNoExercises_ReturnsEmptyList()
    {
        // Arrange
        var autoMocker = new AutoMocker();
        var testee = autoMocker.CreateInstance<EquipmentRequirementsService>();
        var templateId = WorkoutTemplateId.ParseOrEmpty("workouttemplate-00000000-0000-0000-0000-000000000001");
        
        // Setup mocks
        autoMocker.SetupWorkoutTemplateExists(templateId, exists: true);
        
        var emptyExerciseCollection = new WorkoutTemplateExerciseListDto
        {
            WarmupExercises = [],
            MainExercises = [],
            CooldownExercises = []
        };
        
        autoMocker.GetMock<IWorkoutTemplateExerciseService>()
            .Setup(x => x.GetByWorkoutTemplateAsync(templateId))
            .ReturnsAsync(ServiceResult<WorkoutTemplateExerciseListDto>.Success(emptyExerciseCollection));
        
        // Act
        var result = await testee.GetRequiredEquipmentAsync(templateId);
        
        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeEmpty();
    }
    
    [Fact]
    public async Task GetRequiredEquipmentAsync_WithExercisesHavingEquipment_ReturnsUniqueEquipment()
    {
        // Arrange
        var autoMocker = new AutoMocker();
        var testee = autoMocker.CreateInstance<EquipmentRequirementsService>();
        var templateId = WorkoutTemplateId.ParseOrEmpty("workouttemplate-00000000-0000-0000-0000-000000000001");
        var exerciseId1 = ExerciseId.ParseOrEmpty("exercise-00000000-0000-0000-0000-000000000001");
        var exerciseId2 = ExerciseId.ParseOrEmpty("exercise-00000000-0000-0000-0000-000000000002");
        
        // Setup mocks
        autoMocker.SetupWorkoutTemplateExists(templateId, exists: true);
        
        // Setup template exercises
        var exerciseCollection = new WorkoutTemplateExerciseListDto
        {
            WarmupExercises = [],
            MainExercises = 
            [
                new WorkoutTemplateExerciseDto 
                { 
                    Exercise = new ExerciseDto { Id = exerciseId1.ToString() }
                },
                new WorkoutTemplateExerciseDto 
                { 
                    Exercise = new ExerciseDto { Id = exerciseId2.ToString() }
                }
            ],
            CooldownExercises = []
        };
        
        autoMocker.GetMock<IWorkoutTemplateExerciseService>()
            .Setup(x => x.GetByWorkoutTemplateAsync(templateId))
            .ReturnsAsync(ServiceResult<WorkoutTemplateExerciseListDto>.Success(exerciseCollection));
        
        // Setup exercises with equipment
        var exercise1 = new ExerciseDto
        {
            Id = exerciseId1.ToString(),
            Equipment = 
            [
                new ReferenceDataDto { Id = "equipment-001", Value = "Barbell" },
                new ReferenceDataDto { Id = "equipment-002", Value = "Bench" }
            ]
        };
        
        var exercise2 = new ExerciseDto
        {
            Id = exerciseId2.ToString(),
            Equipment = 
            [
                new ReferenceDataDto { Id = "equipment-001", Value = "Barbell" }, // Duplicate
                new ReferenceDataDto { Id = "equipment-003", Value = "Dumbbell" }
            ]
        };
        
        autoMocker.GetMock<IExerciseService>()
            .Setup(x => x.GetByIdAsync(exerciseId1))
            .ReturnsAsync(ServiceResult<ExerciseDto>.Success(exercise1));
            
        autoMocker.GetMock<IExerciseService>()
            .Setup(x => x.GetByIdAsync(exerciseId2))
            .ReturnsAsync(ServiceResult<ExerciseDto>.Success(exercise2));
        
        // Act
        var result = await testee.GetRequiredEquipmentAsync(templateId);
        
        // Assert
        result.IsSuccess.Should().BeTrue();
        var equipment = result.Data.ToList();
        equipment.Should().HaveCount(3); // Barbell, Bench, Dumbbell (no duplicates)
        equipment.Should().Contain(e => e.Name == "Barbell");
        equipment.Should().Contain(e => e.Name == "Bench");
        equipment.Should().Contain(e => e.Name == "Dumbbell");
    }
    
    [Fact]
    public async Task CheckEquipmentAvailabilityAsync_WithAllEquipmentAvailable_ReturnsCanPerform()
    {
        // Arrange
        var autoMocker = new AutoMocker();
        var testee = autoMocker.CreateInstance<EquipmentRequirementsService>();
        var templateId = WorkoutTemplateId.ParseOrEmpty("workouttemplate-00000000-0000-0000-0000-000000000001");
        var availableEquipmentIds = new[]
        {
            EquipmentId.ParseOrEmpty("equipment-00000000-0000-0000-0000-000000000001"),
            EquipmentId.ParseOrEmpty("equipment-00000000-0000-0000-0000-000000000002")
        };
        
        // Setup mocks
        autoMocker.SetupWorkoutTemplateExists(templateId, exists: true);
        
        // Setup required equipment
        var exerciseId = ExerciseId.ParseOrEmpty("exercise-00000000-0000-0000-0000-000000000001");
        var exerciseCollection = new WorkoutTemplateExerciseListDto
        {
            WarmupExercises = [],
            MainExercises = 
            [
                new WorkoutTemplateExerciseDto 
                { 
                    Exercise = new ExerciseDto { Id = exerciseId.ToString() }
                }
            ],
            CooldownExercises = []
        };
        
        autoMocker.GetMock<IWorkoutTemplateExerciseService>()
            .Setup(x => x.GetByWorkoutTemplateAsync(templateId))
            .ReturnsAsync(ServiceResult<WorkoutTemplateExerciseListDto>.Success(exerciseCollection));
        
        var exercise = new ExerciseDto
        {
            Id = exerciseId.ToString(),
            Equipment = 
            [
                new ReferenceDataDto { Id = "equipment-00000000-0000-0000-0000-000000000001", Value = "Barbell" },
                new ReferenceDataDto { Id = "equipment-00000000-0000-0000-0000-000000000002", Value = "Bench" }
            ]
        };
        
        autoMocker.GetMock<IExerciseService>()
            .Setup(x => x.GetByIdAsync(exerciseId))
            .ReturnsAsync(ServiceResult<ExerciseDto>.Success(exercise));
        
        // Act
        var result = await testee.CheckEquipmentAvailabilityAsync(templateId, availableEquipmentIds);
        
        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.CanPerformWorkout.Should().BeTrue();
        result.Data.AvailabilityPercentage.Should().Be(100);
        result.Data.MissingEquipment.Should().BeEmpty();
    }
    
    [Fact]
    public async Task CheckEquipmentAvailabilityAsync_WithMissingEquipment_ReturnsCannotPerform()
    {
        // Arrange
        var autoMocker = new AutoMocker();
        var testee = autoMocker.CreateInstance<EquipmentRequirementsService>();
        var templateId = WorkoutTemplateId.ParseOrEmpty("workouttemplate-00000000-0000-0000-0000-000000000001");
        var availableEquipmentIds = new[]
        {
            EquipmentId.ParseOrEmpty("equipment-00000000-0000-0000-0000-000000000001") // Only barbell available
        };
        
        // Setup mocks
        autoMocker.SetupWorkoutTemplateExists(templateId, exists: true);
        
        // Setup required equipment
        var exerciseId = ExerciseId.ParseOrEmpty("exercise-00000000-0000-0000-0000-000000000001");
        var exerciseCollection = new WorkoutTemplateExerciseListDto
        {
            WarmupExercises = [],
            MainExercises = 
            [
                new WorkoutTemplateExerciseDto 
                { 
                    Exercise = new ExerciseDto { Id = exerciseId.ToString() }
                }
            ],
            CooldownExercises = []
        };
        
        autoMocker.GetMock<IWorkoutTemplateExerciseService>()
            .Setup(x => x.GetByWorkoutTemplateAsync(templateId))
            .ReturnsAsync(ServiceResult<WorkoutTemplateExerciseListDto>.Success(exerciseCollection));
            
        var exercise = new ExerciseDto
        {
            Id = exerciseId.ToString(),
            Equipment = 
            [
                new ReferenceDataDto { Id = "equipment-00000000-0000-0000-0000-000000000001", Value = "Barbell" },
                new ReferenceDataDto { Id = "equipment-00000000-0000-0000-0000-000000000002", Value = "Bench" } // This is missing
            ]
        };
        
        autoMocker.GetMock<IExerciseService>()
            .Setup(x => x.GetByIdAsync(exerciseId))
            .ReturnsAsync(ServiceResult<ExerciseDto>.Success(exercise));
        
        // Act
        var result = await testee.CheckEquipmentAvailabilityAsync(templateId, availableEquipmentIds);
        
        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.CanPerformWorkout.Should().BeFalse();
        result.Data.AvailabilityPercentage.Should().Be(50);
        result.Data.MissingEquipment.Should().ContainSingle();
        result.Data.MissingEquipment.First().Name.Should().Be("Bench");
    }
    
    [Fact]
    public async Task AnalyzeEquipmentUsageAsync_WithMultipleTemplates_ReturnsUsageStatistics()
    {
        // Arrange
        var autoMocker = new AutoMocker();
        var testee = autoMocker.CreateInstance<EquipmentRequirementsService>();
        var templateIds = new[]
        {
            WorkoutTemplateId.ParseOrEmpty("workouttemplate-00000000-0000-0000-0000-000000000001"),
            WorkoutTemplateId.ParseOrEmpty("workouttemplate-00000000-0000-0000-0000-000000000002")
        };
        
        // Setup unit of work for both templates  
        autoMocker.SetupMultipleWorkoutTemplateExists(templateIds);
            
        var exerciseCollection1 = new WorkoutTemplateExerciseListDto
        {
            MainExercises = 
            [
                new WorkoutTemplateExerciseDto 
                { 
                    Exercise = new ExerciseDto { Id = "exercise-00000000-0000-0000-0000-000000000001" }
                }
            ],
            WarmupExercises = [],
            CooldownExercises = []
        };
        
        autoMocker.GetMock<IWorkoutTemplateExerciseService>()
            .Setup(x => x.GetByWorkoutTemplateAsync(templateIds[0]))
            .ReturnsAsync(ServiceResult<WorkoutTemplateExerciseListDto>.Success(exerciseCollection1));
        
        var exerciseCollection2 = new WorkoutTemplateExerciseListDto
        {
            MainExercises = 
            [
                new WorkoutTemplateExerciseDto 
                { 
                    Exercise = new ExerciseDto { Id = "exercise-00000000-0000-0000-0000-000000000002" }
                }
            ],
            WarmupExercises = [],
            CooldownExercises = []
        };
        
        autoMocker.GetMock<IWorkoutTemplateExerciseService>()
            .Setup(x => x.GetByWorkoutTemplateAsync(templateIds[1]))
            .ReturnsAsync(ServiceResult<WorkoutTemplateExerciseListDto>.Success(exerciseCollection2));
        
        // Setup exercises
        var exercise1 = new ExerciseDto
        {
            Id = "exercise-00000000-0000-0000-0000-000000000001",
            Equipment = 
            [
                new ReferenceDataDto { Id = "equipment-00000000-0000-0000-0000-000000000001", Value = "Barbell" },
                new ReferenceDataDto { Id = "equipment-00000000-0000-0000-0000-000000000002", Value = "Bench" }
            ]
        };
        
        var exercise2 = new ExerciseDto
        {
            Id = "exercise-00000000-0000-0000-0000-000000000002",
            Equipment = 
            [
                new ReferenceDataDto { Id = "equipment-00000000-0000-0000-0000-000000000001", Value = "Barbell" }
            ]
        };
        
        autoMocker.GetMock<IExerciseService>()
            .Setup(x => x.GetByIdAsync(ExerciseId.ParseOrEmpty("exercise-00000000-0000-0000-0000-000000000001")))
            .ReturnsAsync(ServiceResult<ExerciseDto>.Success(exercise1));
            
        autoMocker.GetMock<IExerciseService>()
            .Setup(x => x.GetByIdAsync(ExerciseId.ParseOrEmpty("exercise-00000000-0000-0000-0000-000000000002")))
            .ReturnsAsync(ServiceResult<ExerciseDto>.Success(exercise2));
        
        // Act
        var result = await testee.AnalyzeEquipmentUsageAsync(templateIds);
        
        // Assert
        result.IsSuccess.Should().BeTrue();
        var usageStats = result.Data.ToList();
        
        // Barbell used in 2/2 templates = 100%
        var barbellUsage = usageStats.FirstOrDefault(u => u.Equipment.Name == "Barbell");
        barbellUsage.Should().NotBeNull();
        barbellUsage!.UsageCount.Should().Be(2);
        barbellUsage.UsagePercentage.Should().Be(100);
        barbellUsage.IsEssential.Should().BeTrue();
        
        // Bench used in 1/2 templates = 50%
        var benchUsage = usageStats.FirstOrDefault(u => u.Equipment.Name == "Bench");
        benchUsage.Should().NotBeNull();
        benchUsage!.UsageCount.Should().Be(1);
        benchUsage.UsagePercentage.Should().Be(50);
        benchUsage.IsEssential.Should().BeFalse();
    }
    
    [Fact]
    public async Task AnalyzeEquipmentUsageAsync_WithEmptyTemplateList_ReturnsFailure()
    {
        // Arrange
        var autoMocker = new AutoMocker();
        var testee = autoMocker.CreateInstance<EquipmentRequirementsService>();
        var emptyTemplateIds = Array.Empty<WorkoutTemplateId>();
        
        // Act
        var result = await testee.AnalyzeEquipmentUsageAsync(emptyTemplateIds);
        
        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle();
        result.Errors.First().ToLower().Should().Contain("at least one");
    }
}

// Extension methods for setting up mocks
public static class EquipmentRequirementsServiceTestExtensions
{
    public static void SetupWorkoutTemplateExists(this AutoMocker autoMocker, WorkoutTemplateId templateId, bool exists)
    {
        var readOnlyUnitOfWork = new Mock<IReadOnlyUnitOfWork<FitnessDbContext>>();
        var workoutTemplateRepository = new Mock<IWorkoutTemplateRepository>();
        
        workoutTemplateRepository
            .Setup(x => x.ExistsAsync(templateId))
            .ReturnsAsync(exists);
        
        readOnlyUnitOfWork
            .Setup(x => x.GetRepository<IWorkoutTemplateRepository>())
            .Returns(workoutTemplateRepository.Object);
            
        autoMocker.GetMock<IUnitOfWorkProvider<FitnessDbContext>>()
            .Setup(x => x.CreateReadOnly())
            .Returns(readOnlyUnitOfWork.Object);
    }
    
    public static void SetupMultipleWorkoutTemplateExists(this AutoMocker autoMocker, WorkoutTemplateId[] templateIds)
    {
        // Setup a single repository mock that can handle all template existence checks
        var workoutTemplateRepository = new Mock<IWorkoutTemplateRepository>();
        
        foreach (var templateId in templateIds)
        {
            workoutTemplateRepository
                .Setup(x => x.ExistsAsync(templateId))
                .ReturnsAsync(true);
        }
        
        // Setup unit of work to return the same repository instance for all calls
        var readOnlyUnitOfWork = new Mock<IReadOnlyUnitOfWork<FitnessDbContext>>();
        readOnlyUnitOfWork
            .Setup(x => x.GetRepository<IWorkoutTemplateRepository>())
            .Returns(workoutTemplateRepository.Object);
            
        autoMocker.GetMock<IUnitOfWorkProvider<FitnessDbContext>>()
            .Setup(x => x.CreateReadOnly())
            .Returns(readOnlyUnitOfWork.Object);
    }
}