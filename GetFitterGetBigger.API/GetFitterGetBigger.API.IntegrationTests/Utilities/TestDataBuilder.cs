using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Tests.TestBuilders;

namespace GetFitterGetBigger.API.IntegrationTests.Utilities;

/// <summary>
/// Fluent test data builder for creating test objects in BDD scenarios
/// </summary>
public class TestDataBuilder
{
    /// <summary>
    /// Create a new exercise request builder
    /// </summary>
    public static CreateExerciseRequestBuilder Exercise()
    {
        return CreateExerciseRequestBuilder.ForWorkoutExercise();
    }
    
    /// <summary>
    /// Create a new rest exercise request builder
    /// </summary>
    public static CreateExerciseRequestBuilder RestExercise()
    {
        return CreateExerciseRequestBuilder.ForRestExercise();
    }
    
    /// <summary>
    /// Create a muscle group with role request
    /// </summary>
    public static MuscleGroupWithRoleRequest MuscleGroupWithRole(string muscleGroupId, string muscleRoleId)
    {
        return new MuscleGroupWithRoleRequest
        {
            MuscleGroupId = muscleGroupId,
            MuscleRoleId = muscleRoleId
        };
    }
    
    /// <summary>
    /// Create a coach note request
    /// </summary>
    public static CoachNoteRequest CoachNote(string text, int order)
    {
        return new CoachNoteRequest
        {
            Text = text,
            Order = order
        };
    }
    
    /// <summary>
    /// Create an authentication request
    /// </summary>
    public static AuthenticationRequest AuthRequest(string email)
    {
        return new AuthenticationRequest(email);
    }
    
    /// <summary>
    /// Common test data values
    /// </summary>
    public static class Common
    {
        public static class Emails
        {
            public const string PersonalTrainer = "pt@example.com";
            public const string Admin = "admin@example.com";
            public const string FreeUser = "user@example.com";
            public const string TestUser = "test@example.com";
        }
        
        public static class Roles
        {
            public const string PersonalTrainer = "PT-Tier";
            public const string Admin = "Admin-Tier";
            public const string Free = "Free-Tier";
        }
        
        public static class ExerciseNames
        {
            public const string BenchPress = "Bench Press";
            public const string Squat = "Squat";
            public const string Deadlift = "Deadlift";
            public const string BicepCurl = "Bicep Curl";
            public const string Rest = "Rest Period";
        }
        
        public static class EquipmentNames
        {
            public const string Barbell = "Barbell";
            public const string Dumbbell = "Dumbbell";
            public const string CableMachine = "Cable Machine";
            public const string BodyWeight = "Body Weight";
        }
        
        public static class MuscleGroupNames
        {
            public const string Chest = "Chest";
            public const string Back = "Back";
            public const string Legs = "Legs";
            public const string Arms = "Arms";
            public const string Shoulders = "Shoulders";
            public const string Core = "Core";
        }
        
        public static class BodyPartNames
        {
            public const string UpperBody = "Upper Body";
            public const string LowerBody = "Lower Body";
            public const string Core = "Core";
            public const string FullBody = "Full Body";
        }
    }
    
    /// <summary>
    /// Builder for complex test scenarios
    /// </summary>
    public class ScenarioBuilder
    {
        private readonly List<object> _entities = new();
        
        public ScenarioBuilder WithExercise(string name, Action<CreateExerciseRequestBuilder>? configure = null)
        {
            var builder = Exercise().WithName(name);
            configure?.Invoke(builder);
            _entities.Add(builder.Build());
            return this;
        }
        
        public List<object> Build()
        {
            return _entities;
        }
    }
    
    /// <summary>
    /// Create a scenario builder
    /// </summary>
    public static ScenarioBuilder Scenario()
    {
        return new ScenarioBuilder();
    }
}