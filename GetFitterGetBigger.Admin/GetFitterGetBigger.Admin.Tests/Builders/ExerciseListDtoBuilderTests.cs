using FluentAssertions;
using GetFitterGetBigger.Admin.Builders;
using GetFitterGetBigger.Admin.Models.Dtos;
using Xunit;

namespace GetFitterGetBigger.Admin.Tests.Builders
{
    public class ExerciseListDtoBuilderTests
    {
        [Fact]
        public void Build_WithDefaults_ReturnsValidExerciseListDto()
        {
            // Act
            var result = new ExerciseListDtoBuilder().Build();

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().NotBeNullOrEmpty();
            result.Name.Should().Be("Test Exercise");
            result.Description.Should().Be("Test Description");
            result.Instructions.Should().Be("Test Instructions");
            result.IsActive.Should().BeTrue();
            result.IsUnilateral.Should().BeFalse();
            result.Difficulty.Should().NotBeNull();
            result.Difficulty!.Value.Should().Be("Intermediate");
            result.CoachNotes.Should().BeEmpty();
            result.ExerciseTypes.Should().BeEmpty();
            result.MuscleGroups.Should().BeEmpty();
            result.Equipment.Should().BeEmpty();
            result.BodyParts.Should().BeEmpty();
            result.MovementPatterns.Should().BeEmpty();
            result.WarmupLinkCount.Should().Be(0);
            result.CooldownLinkCount.Should().Be(0);
            result.HasLinks.Should().BeFalse();
        }

        [Fact]
        public void WithId_SetsId()
        {
            // Arrange
            var id = "exercise-123";

            // Act
            var result = new ExerciseListDtoBuilder()
                .WithId(id)
                .Build();

            // Assert
            result.Id.Should().Be(id);
        }

        [Fact]
        public void WithName_SetsName()
        {
            // Arrange
            var name = "Bench Press";

            // Act
            var result = new ExerciseListDtoBuilder()
                .WithName(name)
                .Build();

            // Assert
            result.Name.Should().Be(name);
        }

        [Fact]
        public void WithDescription_SetsDescription()
        {
            // Arrange
            var description = "A compound chest exercise";

            // Act
            var result = new ExerciseListDtoBuilder()
                .WithDescription(description)
                .Build();

            // Assert
            result.Description.Should().Be(description);
        }

        [Fact]
        public void WithInstructions_SetsInstructions()
        {
            // Arrange
            var instructions = "Lie on bench, lower bar to chest, press up";

            // Act
            var result = new ExerciseListDtoBuilder()
                .WithInstructions(instructions)
                .Build();

            // Assert
            result.Instructions.Should().Be(instructions);
        }

        [Fact]
        public void WithCoachNotes_UsingList_SetsCoachNotes()
        {
            // Arrange
            var notes = new List<CoachNoteDto>
            {
                new() { Id = "1", Text = "Note 1", Order = 1 },
                new() { Id = "2", Text = "Note 2", Order = 2 }
            };

            // Act
            var result = new ExerciseListDtoBuilder()
                .WithCoachNotes(notes)
                .Build();

            // Assert
            result.CoachNotes.Should().BeEquivalentTo(notes);
        }

        [Fact]
        public void WithCoachNotes_UsingParams_CreatesCoachNotes()
        {
            // Act
            var result = new ExerciseListDtoBuilder()
                .WithCoachNotes(("Keep back straight", 1), ("Control the weight", 2))
                .Build();

            // Assert
            result.CoachNotes.Should().HaveCount(2);
            result.CoachNotes[0].Text.Should().Be("Keep back straight");
            result.CoachNotes[0].Order.Should().Be(1);
            result.CoachNotes[1].Text.Should().Be("Control the weight");
            result.CoachNotes[1].Order.Should().Be(2);
        }

        [Fact]
        public void AddCoachNote_AddsNoteToList()
        {
            // Act
            var result = new ExerciseListDtoBuilder()
                .AddCoachNote("First note", 1)
                .AddCoachNote("Second note", 2)
                .Build();

            // Assert
            result.CoachNotes.Should().HaveCount(2);
            result.CoachNotes[0].Text.Should().Be("First note");
            result.CoachNotes[1].Text.Should().Be("Second note");
        }

        [Fact]
        public void WithExerciseTypes_UsingList_SetsExerciseTypes()
        {
            // Arrange
            var types = new List<ExerciseTypeDto>
            {
                new() { Id = "1", Value = "Workout", Description = "Regular workout" },
                new() { Id = "2", Value = "Warmup", Description = "Warmup exercise" }
            };

            // Act
            var result = new ExerciseListDtoBuilder()
                .WithExerciseTypes(types)
                .Build();

            // Assert
            result.ExerciseTypes.Should().BeEquivalentTo(types);
        }

        [Fact]
        public void WithExerciseTypes_UsingParams_CreatesExerciseTypes()
        {
            // Act
            var result = new ExerciseListDtoBuilder()
                .WithExerciseTypes(("Workout", "Regular workout"), ("Cooldown", "Cooldown exercise"))
                .Build();

            // Assert
            result.ExerciseTypes.Should().HaveCount(2);
            result.ExerciseTypes[0].Value.Should().Be("Workout");
            result.ExerciseTypes[0].Id.Should().Be("type-workout");
            result.ExerciseTypes[1].Value.Should().Be("Cooldown");
            result.ExerciseTypes[1].Id.Should().Be("type-cooldown");
        }

        [Fact]
        public void AddExerciseType_AddsTypeToList()
        {
            // Act
            var result = new ExerciseListDtoBuilder()
                .AddExerciseType("Workout", "Regular workout")
                .AddExerciseType("Warmup")
                .Build();

            // Assert
            result.ExerciseTypes.Should().HaveCount(2);
            result.ExerciseTypes[0].Value.Should().Be("Workout");
            result.ExerciseTypes[0].Description.Should().Be("Regular workout");
            result.ExerciseTypes[1].Value.Should().Be("Warmup");
            result.ExerciseTypes[1].Description.Should().Be("");
        }

        [Fact]
        public void WithVideoUrl_SetsVideoUrl()
        {
            // Arrange
            var url = "https://example.com/video.mp4";

            // Act
            var result = new ExerciseListDtoBuilder()
                .WithVideoUrl(url)
                .Build();

            // Assert
            result.VideoUrl.Should().Be(url);
        }

        [Fact]
        public void WithImageUrl_SetsImageUrl()
        {
            // Arrange
            var url = "https://example.com/image.jpg";

            // Act
            var result = new ExerciseListDtoBuilder()
                .WithImageUrl(url)
                .Build();

            // Assert
            result.ImageUrl.Should().Be(url);
        }

        [Fact]
        public void AsUnilateral_SetsIsUnilateralTrue()
        {
            // Act
            var result = new ExerciseListDtoBuilder()
                .AsUnilateral()
                .Build();

            // Assert
            result.IsUnilateral.Should().BeTrue();
        }

        [Fact]
        public void AsInactive_SetsIsActiveFalse()
        {
            // Act
            var result = new ExerciseListDtoBuilder()
                .AsInactive()
                .Build();

            // Assert
            result.IsActive.Should().BeFalse();
        }

        [Fact]
        public void WithIsActive_SetsActiveStatus()
        {
            // Act
            var activeResult = new ExerciseListDtoBuilder()
                .WithIsActive(true)
                .Build();
            
            var inactiveResult = new ExerciseListDtoBuilder()
                .WithIsActive(false)
                .Build();

            // Assert
            activeResult.IsActive.Should().BeTrue();
            inactiveResult.IsActive.Should().BeFalse();
        }

        [Fact]
        public void WithDifficulty_SetsDifficulty()
        {
            // Act
            var result = new ExerciseListDtoBuilder()
                .WithDifficulty("Advanced", "adv-123")
                .Build();

            // Assert
            result.Difficulty.Should().NotBeNull();
            result.Difficulty!.Id.Should().Be("adv-123");
            result.Difficulty.Value.Should().Be("Advanced");
            result.Difficulty.Description.Should().Be("Advanced level");
        }

        [Fact]
        public void WithDifficulty_WithoutId_GeneratesId()
        {
            // Act
            var result = new ExerciseListDtoBuilder()
                .WithDifficulty("Beginner")
                .Build();

            // Assert
            result.Difficulty.Should().NotBeNull();
            result.Difficulty!.Id.Should().Be("difficulty-beginner");
            result.Difficulty.Value.Should().Be("Beginner");
        }

        [Fact]
        public void WithKineticChain_SetsKineticChain()
        {
            // Act
            var result = new ExerciseListDtoBuilder()
                .WithKineticChain("Compound", "Multi-joint movement")
                .Build();

            // Assert
            result.KineticChain.Should().NotBeNull();
            result.KineticChain!.Id.Should().Be("kineticchain-compound");
            result.KineticChain.Value.Should().Be("Compound");
            result.KineticChain.Description.Should().Be("Multi-joint movement");
        }

        [Fact]
        public void WithKineticChain_WithNull_SetsNull()
        {
            // Act
            var result = new ExerciseListDtoBuilder()
                .WithKineticChain(null)
                .Build();

            // Assert
            result.KineticChain.Should().BeNull();
        }

        [Fact]
        public void WithWeightType_UsingDto_SetsWeightType()
        {
            // Arrange
            var weightType = new ExerciseWeightTypeDto
            {
                Id = Guid.NewGuid(),
                Code = "BODYWEIGHT_ONLY",
                Name = "Bodyweight Only",
                Description = "Uses bodyweight resistance",
                IsActive = true,
                DisplayOrder = 1
            };

            // Act
            var result = new ExerciseListDtoBuilder()
                .WithWeightType(weightType)
                .Build();

            // Assert
            result.WeightType.Should().Be(weightType);
        }

        [Fact]
        public void WithWeightType_UsingParams_CreatesWeightType()
        {
            // Act
            var result = new ExerciseListDtoBuilder()
                .WithWeightType("WEIGHT_REQUIRED", "Weight Required", "External weight required")
                .Build();

            // Assert
            result.WeightType.Should().NotBeNull();
            result.WeightType!.Code.Should().Be("WEIGHT_REQUIRED");
            result.WeightType.Name.Should().Be("Weight Required");
            result.WeightType.Description.Should().Be("External weight required");
            result.WeightType.IsActive.Should().BeTrue();
            result.WeightType.DisplayOrder.Should().Be(1);
        }

        [Fact]
        public void WithMuscleGroups_UsingParams_CreatesMuscleGroups()
        {
            // Act
            var result = new ExerciseListDtoBuilder()
                .WithMuscleGroups(("Chest", "Primary"), ("Triceps", "Secondary"))
                .Build();

            // Assert
            result.MuscleGroups.Should().HaveCount(2);
            result.MuscleGroups[0].MuscleGroup!.Value.Should().Be("Chest");
            result.MuscleGroups[0].Role!.Value.Should().Be("Primary");
            result.MuscleGroups[1].MuscleGroup!.Value.Should().Be("Triceps");
            result.MuscleGroups[1].Role!.Value.Should().Be("Secondary");
        }

        [Fact]
        public void WithMuscleGroups_UsingDtos_SetsMuscleGroups()
        {
            // Arrange
            var muscleGroups = new[]
            {
                new MuscleGroupListItemDto
                {
                    MuscleGroup = new ReferenceDataDto { Id = "1", Value = "Chest" },
                    Role = new ReferenceDataDto { Id = "1", Value = "Primary" }
                }
            };

            // Act
            var result = new ExerciseListDtoBuilder()
                .WithMuscleGroups(muscleGroups)
                .Build();

            // Assert
            result.MuscleGroups.Should().BeEquivalentTo(muscleGroups);
        }

        [Fact]
        public void WithMuscleGroup_AddsSingleMuscleGroup()
        {
            // Act
            var result = new ExerciseListDtoBuilder()
                .WithMuscleGroup("Chest", "Primary")
                .WithMuscleGroup("Shoulders", "Secondary")
                .Build();

            // Assert
            result.MuscleGroups.Should().HaveCount(2);
            result.MuscleGroups[0].MuscleGroup!.Value.Should().Be("Chest");
            result.MuscleGroups[1].MuscleGroup!.Value.Should().Be("Shoulders");
        }

        [Fact]
        public void WithPrimaryMuscleGroups_AddsPrimaryMuscles()
        {
            // Act
            var result = new ExerciseListDtoBuilder()
                .WithPrimaryMuscleGroups("Chest", "Triceps", "Shoulders")
                .Build();

            // Assert
            result.MuscleGroups.Should().HaveCount(3);
            result.MuscleGroups.Should().AllSatisfy(mg => mg.Role!.Value.Should().Be("Primary"));
            result.MuscleGroups.Select(mg => mg.MuscleGroup!.Value)
                .Should().BeEquivalentTo(new[] { "Chest", "Triceps", "Shoulders" });
        }

        [Fact]
        public void WithEquipment_UsingStrings_CreatesEquipment()
        {
            // Act
            var result = new ExerciseListDtoBuilder()
                .WithEquipment("Barbell", "Bench")
                .Build();

            // Assert
            result.Equipment.Should().HaveCount(2);
            result.Equipment[0].Value.Should().Be("Barbell");
            result.Equipment[1].Value.Should().Be("Bench");
        }

        [Fact]
        public void WithEquipment_UsingDtos_SetsEquipment()
        {
            // Arrange
            var equipment = new[]
            {
                new ReferenceDataDto { Id = "1", Value = "Dumbbell" },
                new ReferenceDataDto { Id = "2", Value = "Cable" }
            };

            // Act
            var result = new ExerciseListDtoBuilder()
                .WithEquipment(equipment)
                .Build();

            // Assert
            result.Equipment.Should().BeEquivalentTo(equipment);
        }

        [Fact]
        public void WithBodyParts_CreatesBodyParts()
        {
            // Act
            var result = new ExerciseListDtoBuilder()
                .WithBodyParts("Upper Body", "Arms")
                .Build();

            // Assert
            result.BodyParts.Should().HaveCount(2);
            result.BodyParts[0].Value.Should().Be("Upper Body");
            result.BodyParts[1].Value.Should().Be("Arms");
        }

        [Fact]
        public void WithMovementPatterns_CreatesMovementPatterns()
        {
            // Act
            var result = new ExerciseListDtoBuilder()
                .WithMovementPatterns("Push", "Horizontal")
                .Build();

            // Assert
            result.MovementPatterns.Should().HaveCount(2);
            result.MovementPatterns[0].Value.Should().Be("Push");
            result.MovementPatterns[1].Value.Should().Be("Horizontal");
        }

        [Fact]
        public void WithWarmupLinkCount_SetsWarmupCount()
        {
            // Act
            var result = new ExerciseListDtoBuilder()
                .WithWarmupLinkCount(3)
                .Build();

            // Assert
            result.WarmupLinkCount.Should().Be(3);
            result.HasLinks.Should().BeTrue();
        }

        [Fact]
        public void WithCooldownLinkCount_SetsCooldownCount()
        {
            // Act
            var result = new ExerciseListDtoBuilder()
                .WithCooldownLinkCount(2)
                .Build();

            // Assert
            result.CooldownLinkCount.Should().Be(2);
            result.HasLinks.Should().BeTrue();
        }

        [Fact]
        public void WithLinkCounts_SetsBothCounts()
        {
            // Act
            var result = new ExerciseListDtoBuilder()
                .WithLinkCounts(4, 5)
                .Build();

            // Assert
            result.WarmupLinkCount.Should().Be(4);
            result.CooldownLinkCount.Should().Be(5);
            result.HasLinks.Should().BeTrue();
        }

        [Fact]
        public void BuildList_CreatesMultipleExercises()
        {
            // Act
            var result = ExerciseListDtoBuilder.BuildList(3);

            // Assert
            result.Should().HaveCount(3);
            result[0].Name.Should().Be("Exercise 1");
            result[0].Description.Should().Be("Description for exercise 1");
            result[1].Name.Should().Be("Exercise 2");
            result[1].Description.Should().Be("Description for exercise 2");
            result[2].Name.Should().Be("Exercise 3");
            result[2].Description.Should().Be("Description for exercise 3");
        }

        [Fact]
        public void ComplexBuilder_ChainsMultipleMethods()
        {
            // Act
            var result = new ExerciseListDtoBuilder()
                .WithName("Bench Press")
                .WithDescription("Compound chest exercise")
                .WithInstructions("Lie on bench and press")
                .AsUnilateral()
                .WithVideoUrl("https://example.com/bench-press.mp4")
                .WithImageUrl("https://example.com/bench-press.jpg")
                .WithDifficulty("Intermediate")
                .WithKineticChain("Compound")
                .WithWeightType("WEIGHT_REQUIRED", "Weight Required")
                .WithPrimaryMuscleGroups("Chest")
                .WithMuscleGroup("Triceps", "Secondary")
                .WithMuscleGroup("Shoulders", "Secondary")
                .WithEquipment("Barbell", "Bench")
                .WithBodyParts("Upper Body")
                .WithMovementPatterns("Push", "Horizontal")
                .AddExerciseType("Workout")
                .AddCoachNote("Keep shoulders back", 1)
                .AddCoachNote("Control the descent", 2)
                .WithLinkCounts(2, 1)
                .Build();

            // Assert
            result.Name.Should().Be("Bench Press");
            result.Description.Should().Be("Compound chest exercise");
            result.Instructions.Should().Be("Lie on bench and press");
            result.IsUnilateral.Should().BeTrue();
            result.VideoUrl.Should().Be("https://example.com/bench-press.mp4");
            result.ImageUrl.Should().Be("https://example.com/bench-press.jpg");
            result.Difficulty!.Value.Should().Be("Intermediate");
            result.KineticChain!.Value.Should().Be("Compound");
            result.WeightType!.Code.Should().Be("WEIGHT_REQUIRED");
            result.MuscleGroups.Should().HaveCount(3);
            result.Equipment.Should().HaveCount(2);
            result.BodyParts.Should().HaveCount(1);
            result.MovementPatterns.Should().HaveCount(2);
            result.ExerciseTypes.Should().HaveCount(1);
            result.CoachNotes.Should().HaveCount(2);
            result.WarmupLinkCount.Should().Be(2);
            result.CooldownLinkCount.Should().Be(1);
            result.HasLinks.Should().BeTrue();
        }
    }
}