using System;
using System.Collections.Generic;
using System.Linq;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Repositories.Interfaces;
using Moq;

namespace GetFitterGetBigger.API.Tests.Helpers
{
    /// <summary>
    /// Helper class for setting up ExerciseService test mocks with proper navigation property loading
    /// </summary>
    public static class ExerciseServiceTestHelper
    {
        /// <summary>
        /// Sets up the exercise repository mock to simulate navigation property loading after AddAsync
        /// </summary>
        public static void SetupAddAsyncWithNavigationLoading(Mock<IExerciseRepository> repositoryMock)
        {
            repositoryMock.Setup(r => r.AddAsync(It.IsAny<Exercise>()))
                .ReturnsAsync((Exercise exercise) =>
                {
                    // Simulate what the real repository does - return the exercise with loaded navigation properties
                    // In a real scenario, these would be loaded from the database
                    // For tests, we'll return the exercise as-is since the test data already has the collections populated
                    
                    // Create a new instance to simulate EF Core behavior
                    var savedExercise = exercise with
                    {
                        // Simulate that EF Core might assign server-generated values
                        Id = exercise.Id.IsEmpty ? ExerciseId.New() : exercise.Id
                    };
                    
                    // The collections are already populated from the test setup
                    return savedExercise;
                });
        }
        
        /// <summary>
        /// Sets up the exercise repository mock to simulate GetByIdAsync with navigation properties
        /// </summary>
        public static void SetupGetByIdAsyncWithNavigationLoading(
            Mock<IExerciseRepository> repositoryMock, 
            Exercise exercise)
        {
            repositoryMock.Setup(r => r.GetByIdAsync(exercise.Id))
                .ReturnsAsync(exercise);
        }
        
        /// <summary>
        /// Creates a mock ExerciseDto mapper result that includes all navigation properties
        /// This simulates what the real mapper would produce
        /// </summary>
        public static void VerifyNavigationPropertiesLoaded(Exercise exercise)
        {
            // This is a helper to ensure the exercise has all expected navigation properties
            // In real tests, you'd assert specific counts and values
            if (exercise.CoachNotes == null)
                throw new InvalidOperationException("CoachNotes collection should not be null");
            if (exercise.ExerciseExerciseTypes == null)
                throw new InvalidOperationException("ExerciseExerciseTypes collection should not be null");
            if (exercise.ExerciseMuscleGroups == null)
                throw new InvalidOperationException("ExerciseMuscleGroups collection should not be null");
            if (exercise.ExerciseEquipment == null)
                throw new InvalidOperationException("ExerciseEquipment collection should not be null");
            if (exercise.ExerciseBodyParts == null)
                throw new InvalidOperationException("ExerciseBodyParts collection should not be null");
            if (exercise.ExerciseMovementPatterns == null)
                throw new InvalidOperationException("ExerciseMovementPatterns collection should not be null");
        }
    }
}