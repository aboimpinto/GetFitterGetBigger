using System;
using GetFitterGetBigger.API.Models.SpecializedIds;

namespace GetFitterGetBigger.API.Tests.Constants;

/// <summary>
/// Test constants for ExerciseWeightType tests to eliminate magic strings
/// </summary>
public static class ExerciseWeightTypeTestConstants
{
    #region Standard Exercise Weight Type Codes
    
    public const string BodyweightOnlyCode = "BODYWEIGHT_ONLY";
    public const string BodyweightOptionalCode = "BODYWEIGHT_OPTIONAL";
    public const string WeightRequiredCode = "WEIGHT_REQUIRED";
    public const string MachineWeightCode = "MACHINE_WEIGHT";
    public const string NoWeightCode = "NO_WEIGHT";
    
    #endregion
    
    #region Standard Exercise Weight Type Values
    
    public const string BodyweightOnlyValue = "Bodyweight Only";
    public const string BodyweightOptionalValue = "Bodyweight Optional";
    public const string WeightRequiredValue = "Weight Required";
    public const string MachineWeightValue = "Machine Weight";
    public const string NoWeightValue = "No Weight";
    
    #endregion
    
    #region Standard Exercise Weight Type Descriptions
    
    public const string BodyweightOnlyDescription = "Exercises that cannot have external weight added";
    public const string BodyweightOptionalDescription = "Exercises that can be performed with or without additional weight";
    public const string WeightRequiredDescription = "Exercises that must have external weight specified";
    public const string MachineWeightDescription = "Exercises performed on machines with weight stacks";
    public const string NoWeightDescription = "Exercises that do not use weight as a metric";
    
    #endregion
    
    #region Test-Specific Constants
    
    public const string TestCode = "TEST_CODE";
    public const string TestValue = "Test Value";
    public const string TestDescription = "Description";
    
    public const string InactiveTypeCode = "INACTIVE_TYPE";
    public const string InactiveTypeValue = "Inactive Type";
    public const string InactiveTypeDescription = "This type is inactive";
    
    public const string NonExistentCode = "NON_EXISTENT_CODE";
    public const string NonExistentValue = "NonExistent";
    
    // Test variation codes
    public const string CodeA = "CODE_A";
    public const string CodeB = "CODE_B";
    public const string CodeC = "CODE_C";
    public const string ValueA = "Value A";
    public const string ValueB = "Value B";
    public const string ValueC = "Value C";
    public const string DescriptionA = "Description 1";
    public const string DescriptionB = "Description 2";
    public const string DescriptionC = "Description 3";
    
    public const string InactiveCode = "INACTIVE_CODE";
    public const string InactiveValue = "Inactive Value";
    
    // Case sensitivity test
    public const string LowercaseBodyweightOnlyCode = "bodyweight_only";
    public const string MixedCaseMachineWeightValue = "MACHINE weight";
    
    #endregion
    
    #region Standard IDs
    
    public static readonly ExerciseWeightTypeId BodyweightOnlyId = ExerciseWeightTypeId.From(Guid.Parse("a1f3e2d4-5b6c-4d7e-8f9a-0b1c2d3e4f5a"));
    public static readonly ExerciseWeightTypeId BodyweightOptionalId = ExerciseWeightTypeId.From(Guid.Parse("b2e4d3c5-6a7b-5c8d-9e0f-1a2b3c4d5e6f"));
    public static readonly ExerciseWeightTypeId WeightRequiredId = ExerciseWeightTypeId.From(Guid.Parse("c3d5c4b6-7b8c-6d9e-0f1a-2b3c4d5e6f7a"));
    public static readonly ExerciseWeightTypeId MachineWeightId = ExerciseWeightTypeId.From(Guid.Parse("d4c6b5a7-8c9d-7e0f-1a2b-3c4d5e6f7a8b"));
    public static readonly ExerciseWeightTypeId NoWeightId = ExerciseWeightTypeId.From(Guid.Parse("e5b7a698-9d0e-8f1a-2b3c-4d5e6f7a8b9c"));
    public static readonly ExerciseWeightTypeId InactiveTypeId = ExerciseWeightTypeId.From(Guid.Parse("f6c8b7a9-0e1d-9f2a-3b4c-5d6e7f8a9b0c"));
    
    // Test-specific IDs
    public static readonly ExerciseWeightTypeId TestIdA = ExerciseWeightTypeId.From(Guid.Parse("a1f3e2d4-5b6c-4d7e-8f9a-0b1c2d3e4f5a"));
    public static readonly ExerciseWeightTypeId TestIdB = ExerciseWeightTypeId.From(Guid.Parse("b2e4d3c5-6a7b-5c8d-9e0f-1a2b3c4d5e6f"));
    public static readonly ExerciseWeightTypeId TestIdC = ExerciseWeightTypeId.From(Guid.Parse("c3d5c4b6-7b8c-6d9e-0f1a-2b3c4d5e6f7a"));
    public static readonly Guid TestFixedGuid = Guid.Parse("12345678-1234-1234-1234-123456789012");
    public static readonly ExerciseWeightTypeId TestIdFixed = ExerciseWeightTypeId.From(TestFixedGuid);
    
    #endregion
    
    #region Expected DTO IDs (for mapping tests)
    
    public const string BodyweightOnlyDtoId = "exerciseweighttype-a1f3e2d4-5b6c-4d7e-8f9a-0b1c2d3e4f5a";
    public const string BodyweightOptionalDtoId = "exerciseweighttype-b2e4d3c5-6a7b-5c8d-9e0f-1a2b3c4d5e6f";
    public const string WeightRequiredDtoId = "exerciseweighttype-c3d5c4b6-7b8c-6d9e-0f1a-2b3c4d5e6f7a";
    public const string TestFixedDtoId = "exerciseweighttype-12345678-1234-1234-1234-123456789012";
    
    #endregion
}