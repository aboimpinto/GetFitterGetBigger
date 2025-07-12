using System;

namespace GetFitterGetBigger.API.Tests.TestBuilders;

/// <summary>
/// Standard test IDs for consistency across unit tests.
/// These are just static IDs for testing - no database required.
/// </summary>
public static class TestIds
{
    // Exercise Type IDs
    public static class ExerciseTypeIds
    {
        public static readonly string Warmup = "exercisetype-a7c8d210-6e09-4b1a-89f7-1e2d3c4b5a6f";
        public static readonly string Workout = "exercisetype-b8d9e321-7f1a-5c2b-9a08-2f3e4d5c6b7a";
        public static readonly string Cooldown = "exercisetype-c9e0f432-8a2b-6d3c-ab19-3a4f5e6d7c8b";
        public static readonly string Rest = "exercisetype-d0f1a543-9b3c-7e4d-bc2a-4b5a6f7e8d9c";
        public static readonly string Superset = "exercisetype-e1a2b654-ac4d-8f5e-cd3b-5c6b7a8f9e0a";
    }
    
    // Difficulty Level IDs
    public static class DifficultyLevelIds
    {
        public static readonly string Beginner = "difficultylevel-11111111-1111-1111-1111-111111111111";
        public static readonly string Intermediate = "difficultylevel-22222222-2222-2222-2222-222222222222";
        public static readonly string Advanced = "difficultylevel-33333333-3333-3333-3333-333333333333";
        public static readonly string Expert = "difficultylevel-44444444-4444-4444-4444-444444444444";
    }
    
    // Muscle Group IDs
    public static class MuscleGroupIds
    {
        public static readonly string Chest = "musclegroup-a1b2c3d4-5e6f-7a8b-9c0d-1e2f3a4b5c6d";
        public static readonly string Back = "musclegroup-b2c3d4e5-6f7a-8b9c-0d1e-2f3a4b5c6d7e";
        public static readonly string Shoulders = "musclegroup-c3d4e5f6-7a8b-9c0d-1e2f-3a4b5c6d7e8f";
        public static readonly string Biceps = "musclegroup-d4e5f6a7-8b9c-0d1e-2f3a-4b5c6d7e8f9a";
        public static readonly string Triceps = "musclegroup-e5f6a7b8-9c0d-1e2f-3a4b-5c6d7e8f9a0b";
        public static readonly string Quadriceps = "musclegroup-f6a7b8c9-0d1e-2f3a-4b5c-6d7e8f9a0b1c";
        public static readonly string Hamstrings = "musclegroup-a7b8c9d0-1e2f-3a4b-5c6d-7e8f9a0b1c2d";
        public static readonly string Glutes = "musclegroup-b8c9d0e1-2f3a-4b5c-6d7e-8f9a0b1c2d3e";
        public static readonly string Calves = "musclegroup-c9d0e1f2-3a4b-5c6d-7e8f-9a0b1c2d3e4f";
        public static readonly string Core = "musclegroup-d0e1f2a3-4b5c-6d7e-8f9a-0b1c2d3e4f5a";
    }
    
    // Muscle Role IDs
    public static class MuscleRoleIds
    {
        public static readonly string Primary = "musclerole-11111111-1111-1111-1111-111111111111";
        public static readonly string Secondary = "musclerole-22222222-2222-2222-2222-222222222222";
        public static readonly string Stabilizer = "musclerole-33333333-3333-3333-3333-333333333333";
    }
    
    // Body Part IDs
    public static class BodyPartIds
    {
        public static readonly string UpperBody = "bodypart-a1111111-1111-1111-1111-111111111111";
        public static readonly string LowerBody = "bodypart-b2222222-2222-2222-2222-222222222222";
        public static readonly string Core = "bodypart-c3333333-3333-3333-3333-333333333333";
        public static readonly string FullBody = "bodypart-d4444444-4444-4444-4444-444444444444";
        // Aliases for compatibility
        public static readonly string Chest = UpperBody;
        public static readonly string Back = UpperBody;
        public static readonly string Shoulders = UpperBody;
        public static readonly string Legs = LowerBody;
    }
    
    // Equipment IDs
    public static class EquipmentIds
    {
        public static readonly string Barbell = "equipment-a1111111-1111-1111-1111-111111111111";
        public static readonly string Dumbbell = "equipment-b2222222-2222-2222-2222-222222222222";
        public static readonly string Cable = "equipment-c3333333-3333-3333-3333-333333333333";
        public static readonly string Bodyweight = "equipment-d4444444-4444-4444-4444-444444444444";
        public static readonly string Machine = "equipment-e5555555-5555-5555-5555-555555555555";
    }
    
    // Kinetic Chain IDs
    public static class KineticChainIds
    {
        public static readonly string Open = "kineticchaintype-11111111-1111-1111-1111-111111111111";
        public static readonly string Closed = "kineticchaintype-22222222-2222-2222-2222-222222222222";
    }
    
    // Movement Pattern IDs
    public static class MovementPatternIds
    {
        public static readonly string Push = "movementpattern-11111111-1111-1111-1111-111111111111";
        public static readonly string Pull = "movementpattern-22222222-2222-2222-2222-222222222222";
        public static readonly string Squat = "movementpattern-33333333-3333-3333-3333-333333333333";
        public static readonly string Hinge = "movementpattern-44444444-4444-4444-4444-444444444444";
        public static readonly string Lunge = "movementpattern-55555555-5555-5555-5555-555555555555";
        public static readonly string Carry = "movementpattern-66666666-6666-6666-6666-666666666666";
    }
    
    // Exercise Weight Type IDs
    public static class ExerciseWeightTypeIds
    {
        public static readonly string BodyweightOnly = "exerciseweighttype-a1f3e2d4-5b6c-4d7e-8f9a-0b1c2d3e4f5a";
        public static readonly string BodyweightOptional = "exerciseweighttype-b2e4d3c5-6a7b-5c8d-9e0f-1a2b3c4d5e6f";
        public static readonly string WeightRequired = "exerciseweighttype-c3d5c4b6-7b8c-6d9e-0f1a-2b3c4d5e6f7a";
        public static readonly string MachineWeight = "exerciseweighttype-d4c6b5a7-8c9d-7e0f-1a2b-3c4d5e6f7a8b";
        public static readonly string NoWeight = "exerciseweighttype-e5b7a698-9d0e-8f1a-2b3c-4d5e6f7a8b9c";
    }
    
    // Metric Type IDs
    public static class MetricTypeIds
    {
        public static readonly string Repetitions = "metrictype-11111111-1111-1111-1111-111111111111";
        public static readonly string Time = "metrictype-22222222-2222-2222-2222-222222222222";
        public static readonly string Distance = "metrictype-33333333-3333-3333-3333-333333333333";
        public static readonly string Weight = "metrictype-44444444-4444-4444-4444-444444444444";
    }
}