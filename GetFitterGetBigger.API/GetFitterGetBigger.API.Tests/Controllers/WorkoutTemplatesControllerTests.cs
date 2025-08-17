using FluentAssertions;
using GetFitterGetBigger.API.Controllers;
using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Tests.Services.Extensions;
using Microsoft.AspNetCore.Mvc;
using Moq.AutoMock;

namespace GetFitterGetBigger.API.Tests.Controllers;

public class WorkoutTemplatesControllerTests
{
    
    // Test constants for IDs and data consistency
    private const string TestTemplateId = "workouttemplate-550e8400-e29b-41d4-a716-446655440000";
    private const string TestCategoryId = "workoutcategory-550e8400-e29b-41d4-a716-446655440001";
    private const string TestDifficultyId = "difficultylevel-550e8400-e29b-41d4-a716-446655440002";
    private const string TestObjectiveId = "workoutobjective-550e8400-e29b-41d4-a716-446655440003";
    private const string TestWorkoutStateId = "workoutstate-02000001-0000-0000-0000-000000000002";
    private const string TestDraftStateId = "workoutstate-02000001-0000-0000-0000-000000000001";
    private const string CreatedTemplateId = "workouttemplate-650e8400-e29b-41d4-a716-446655440099";
    
    // Test data constants
    private const string TestWorkoutName = "Updated Workout";
    private const string TestDescription = "Updated description";
    private const string NewWorkoutName = "New Workout";
    private const string DuplicateName = "Duplicate Name";
    private const string InvalidWorkoutName = "Invalid Workout";
    private const string ExistingWorkoutName = "Existing Workout";
    
    
    
    #region UpdateWorkoutTemplate Tests
    
    [Fact]
    public async Task UpdateWorkoutTemplate_WhenSuccessful_ReturnsOkWithData()
    {
        // Arrange
        var automocker = new AutoMocker();
        var controller = automocker.CreateInstance<WorkoutTemplatesController>();
        
        var request = new UpdateWorkoutTemplateDto
        {
            Name = TestWorkoutName,
            Description = TestDescription,
            CategoryId = TestCategoryId,
            DifficultyId = TestDifficultyId,
            EstimatedDurationMinutes = 60,
            Tags = new List<string> { "strength", "cardio" },
            IsPublic = true,
            ObjectiveIds = new List<string> { TestObjectiveId }
        };
        
        var successDto = WorkoutTemplateDto.Empty;
        
        automocker.SetupWorkoutTemplateService_UpdateAsync_Success(successDto);
            
        // Act
        var result = await controller.UpdateWorkoutTemplate(TestTemplateId, request);
        
        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().NotBeNull();
        
        automocker.VerifyWorkoutTemplateService_UpdateAsync_CalledOnceWith(TestTemplateId, request);
    }
    
    [Fact]
    public async Task UpdateWorkoutTemplate_WhenNotFound_ReturnsNotFound()
    {
        // Arrange
        var automocker = new AutoMocker();
        var controller = automocker.CreateInstance<WorkoutTemplatesController>();
        
        var request = new UpdateWorkoutTemplateDto
        {
            Name = TestWorkoutName,
            Description = TestDescription,
            CategoryId = TestCategoryId,
            DifficultyId = TestDifficultyId,
            EstimatedDurationMinutes = 60
        };
        
        automocker.SetupWorkoutTemplateService_UpdateAsync_NotFound();
            
        // Act
        var result = await controller.UpdateWorkoutTemplate(TestTemplateId, request);
        
        // Assert
        result.Should().BeOfType<NotFoundResult>();
        
        automocker.VerifyWorkoutTemplateService_UpdateAsync_CalledOnceWith(TestTemplateId, request);
    }
    
    [Fact]
    public async Task UpdateWorkoutTemplate_WhenNameAlreadyExists_ReturnsConflict()
    {
        // Arrange
        var automocker = new AutoMocker();
        var controller = automocker.CreateInstance<WorkoutTemplatesController>();
        
        var request = new UpdateWorkoutTemplateDto
        {
            Name = DuplicateName,
            Description = "Some description",
            CategoryId = TestCategoryId,
            DifficultyId = TestDifficultyId,
            EstimatedDurationMinutes = 60
        };
        
        automocker.SetupWorkoutTemplateService_UpdateAsync_AlreadyExists(DuplicateName);
            
        // Act
        var result = await controller.UpdateWorkoutTemplate(TestTemplateId, request);
        
        // Assert
        var conflictResult = result.Should().BeOfType<ConflictObjectResult>().Subject;
        conflictResult.Value.Should().NotBeNull();
        
        automocker.VerifyWorkoutTemplateService_UpdateAsync_CalledOnceWith(TestTemplateId, request);
    }
    
    [Fact]
    public async Task UpdateWorkoutTemplate_WhenValidationFails_ReturnsConflict()
    {
        // Arrange
        var automocker = new AutoMocker();
        var controller = automocker.CreateInstance<WorkoutTemplatesController>();
        
        var request = new UpdateWorkoutTemplateDto
        {
            Name = InvalidWorkoutName,
            Description = "Some description",
            CategoryId = TestCategoryId,
            DifficultyId = TestDifficultyId,
            EstimatedDurationMinutes = 60
        };
        
        automocker.SetupWorkoutTemplateService_UpdateAsync_ValidationFailed("Invalid category ID");
            
        // Act
        var result = await controller.UpdateWorkoutTemplate(TestTemplateId, request);
        
        // Assert
        var conflictResult = result.Should().BeOfType<ConflictObjectResult>().Subject;
        conflictResult.Value.Should().NotBeNull();
        
        automocker.VerifyWorkoutTemplateService_UpdateAsync_CalledOnceWith(TestTemplateId, request);
    }
    
    #endregion
    
    #region CreateWorkoutTemplate Tests
    
    [Fact]
    public async Task CreateWorkoutTemplate_WhenSuccessful_ReturnsCreatedAtAction()
    {
        // Arrange
        var automocker = new AutoMocker();
        var controller = automocker.CreateInstance<WorkoutTemplatesController>();
        
        var request = new CreateWorkoutTemplateDto
        {
            Name = NewWorkoutName,
            Description = "New description",
            CategoryId = TestCategoryId,
            DifficultyId = TestDifficultyId,
            EstimatedDurationMinutes = 45,
            Tags = new List<string> { "beginner" },
            IsPublic = false,
            ObjectiveIds = new List<string>()
        };
        
        var createdDto = WorkoutTemplateDto.Empty with { Id = CreatedTemplateId };
        
        automocker.SetupWorkoutTemplateService_CreateAsync_Success(createdDto);
            
        // Act
        var result = await controller.CreateWorkoutTemplate(request);
        
        // Assert
        var createdResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.ActionName.Should().Be("GetWorkoutTemplate");
        var data = createdResult.Value.Should().BeOfType<WorkoutTemplateDto>().Subject;
        data.Should().NotBeNull();
        
        automocker.VerifyWorkoutTemplateService_CreateAsync_CalledOnceWith(request);
    }
    
    [Fact]
    public async Task CreateWorkoutTemplate_WhenNameAlreadyExists_ReturnsConflict()
    {
        // Arrange
        var automocker = new AutoMocker();
        var controller = automocker.CreateInstance<WorkoutTemplatesController>();
        
        var request = new CreateWorkoutTemplateDto
        {
            Name = ExistingWorkoutName,
            Description = "Description",
            CategoryId = TestCategoryId,
            DifficultyId = TestDifficultyId,
            EstimatedDurationMinutes = 45
        };
        
        automocker.SetupWorkoutTemplateService_CreateAsync_AlreadyExists(ExistingWorkoutName);
            
        // Act
        var result = await controller.CreateWorkoutTemplate(request);
        
        // Assert
        var conflictResult = result.Should().BeOfType<ConflictObjectResult>().Subject;
        conflictResult.Value.Should().NotBeNull();
        
        automocker.VerifyWorkoutTemplateService_CreateAsync_CalledOnceWith(request);
    }
    
    [Fact]
    public async Task CreateWorkoutTemplate_WhenValidationFails_ReturnsConflict()
    {
        // Arrange
        var automocker = new AutoMocker();
        var controller = automocker.CreateInstance<WorkoutTemplatesController>();
        
        var request = new CreateWorkoutTemplateDto
        {
            Name = InvalidWorkoutName,
            Description = "Description",
            CategoryId = TestCategoryId,
            DifficultyId = TestDifficultyId,
            EstimatedDurationMinutes = 45
        };
        
        automocker.SetupWorkoutTemplateService_CreateAsync_ValidationFailed("Invalid difficulty level");
            
        // Act
        var result = await controller.CreateWorkoutTemplate(request);
        
        // Assert
        var conflictResult = result.Should().BeOfType<ConflictObjectResult>().Subject;
        conflictResult.Value.Should().NotBeNull();
        
        automocker.VerifyWorkoutTemplateService_CreateAsync_CalledOnceWith(request);
    }
    
    #endregion
    
    #region GetWorkoutTemplate Tests
    
    [Fact]
    public async Task GetWorkoutTemplate_WhenFound_ReturnsOkWithData()
    {
        // Arrange
        var automocker = new AutoMocker();
        var controller = automocker.CreateInstance<WorkoutTemplatesController>();
        
        var dto = WorkoutTemplateDto.Empty with { Id = TestTemplateId };
        
        automocker.SetupWorkoutTemplateService_GetByIdAsync_Success(dto);
            
        // Act
        var result = await controller.GetWorkoutTemplate(TestTemplateId);
        
        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var data = okResult.Value.Should().BeOfType<WorkoutTemplateDto>().Subject;
        data.Id.Should().Be(TestTemplateId);
    }
    
    [Fact]
    public async Task GetWorkoutTemplate_WhenNotFound_ReturnsNotFound()
    {
        // Arrange
        var automocker = new AutoMocker();
        var controller = automocker.CreateInstance<WorkoutTemplatesController>();
        
        automocker.SetupWorkoutTemplateService_GetByIdAsync_NotFound();
            
        // Act
        var result = await controller.GetWorkoutTemplate(TestTemplateId);
        
        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }
    
    #endregion
    
    #region DeleteWorkoutTemplate Tests
    
    [Fact]
    public async Task DeleteWorkoutTemplate_WhenNotFound_ReturnsNotFound()
    {
        // Arrange
        var automocker = new AutoMocker();
        var controller = automocker.CreateInstance<WorkoutTemplatesController>();
        
        automocker.SetupWorkoutTemplateService_DeleteAsync_NotFound();
            
        // Act
        var result = await controller.DeleteWorkoutTemplate(TestTemplateId);
        
        // Assert
        result.Should().BeOfType<NotFoundResult>();
        
        automocker.VerifyWorkoutTemplateService_DeleteAsync_CalledOnceWith(TestTemplateId);
    }
    
    [Fact]
    public async Task DeleteWorkoutTemplate_WhenSuccessful_ReturnsNoContent()
    {
        // Arrange
        var automocker = new AutoMocker();
        var controller = automocker.CreateInstance<WorkoutTemplatesController>();
        
        automocker.SetupWorkoutTemplateService_DeleteAsync_Success();
            
        // Act
        var result = await controller.DeleteWorkoutTemplate(TestTemplateId);
        
        // Assert
        result.Should().BeOfType<NoContentResult>();
        
        automocker.VerifyWorkoutTemplateService_DeleteAsync_CalledOnceWith(TestTemplateId);
    }
    
    #endregion
    
    #region ChangeWorkoutTemplateState Tests
    
    [Fact]
    public async Task ChangeWorkoutTemplateState_WhenSuccessful_ReturnsOkWithData()
    {
        // Arrange
        var automocker = new AutoMocker();
        var controller = automocker.CreateInstance<WorkoutTemplatesController>();
        
        var request = new ChangeWorkoutStateDto
        {
            WorkoutStateId = TestWorkoutStateId
        };
        
        var updatedDto = WorkoutTemplateDto.Empty with 
        { 
            Id = TestTemplateId,
            WorkoutState = new ReferenceDataDto 
            { 
                Id = request.WorkoutStateId, 
                Value = "PRODUCTION", 
                Description = "Production state" 
            }
        };
        
        automocker.SetupWorkoutTemplateService_ChangeStateAsync_Success(updatedDto);
            
        // Act
        var result = await controller.ChangeWorkoutTemplateState(TestTemplateId, request);
        
        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var data = okResult.Value.Should().BeOfType<WorkoutTemplateDto>().Subject;
        data.WorkoutState.Id.Should().Be(request.WorkoutStateId);
        
        automocker.VerifyWorkoutTemplateService_ChangeStateAsync_CalledOnceWith(TestTemplateId, request.WorkoutStateId);
    }
    
    [Fact]
    public async Task ChangeWorkoutTemplateState_WhenTemplateNotFound_ReturnsNotFound()
    {
        // Arrange
        var automocker = new AutoMocker();
        var controller = automocker.CreateInstance<WorkoutTemplatesController>();
        
        var request = new ChangeWorkoutStateDto
        {
            WorkoutStateId = TestWorkoutStateId
        };
        
        automocker.SetupWorkoutTemplateService_ChangeStateAsync_NotFound();
            
        // Act
        var result = await controller.ChangeWorkoutTemplateState(TestTemplateId, request);
        
        // Assert
        result.Should().BeOfType<NotFoundResult>();
        
        automocker.VerifyWorkoutTemplateService_ChangeStateAsync_CalledOnceWith(TestTemplateId, request.WorkoutStateId);
    }
    
    [Fact]
    public async Task ChangeWorkoutTemplateState_WhenDependencyExists_ReturnsConflictWithErrors()
    {
        // Arrange
        var automocker = new AutoMocker();
        var controller = automocker.CreateInstance<WorkoutTemplatesController>();
        
        var request = new ChangeWorkoutStateDto
        {
            WorkoutStateId = TestDraftStateId
        };
        
        automocker.SetupWorkoutTemplateService_ChangeStateAsync_DependencyExists();
            
        // Act
        var result = await controller.ChangeWorkoutTemplateState(TestTemplateId, request);
        
        // Assert
        var conflictResult = result.Should().BeOfType<ConflictObjectResult>().Subject;
        conflictResult.Value.Should().NotBeNull();
        
        // Verify the error structure is passed through
        var errorObject = conflictResult.Value;
        var errorsProp = errorObject.GetType().GetProperty("errors");
        errorsProp.Should().NotBeNull();
        
        automocker.VerifyWorkoutTemplateService_ChangeStateAsync_CalledOnceWith(TestTemplateId, request.WorkoutStateId);
    }
    
    [Fact]
    public async Task ChangeWorkoutTemplateState_WhenValidationFails_ReturnsBadRequestWithErrors()
    {
        // Arrange
        var automocker = new AutoMocker();
        var controller = automocker.CreateInstance<WorkoutTemplatesController>();
        
        var request = new ChangeWorkoutStateDto
        {
            WorkoutStateId = "workoutstate-invalid"
        };
        
        automocker.SetupWorkoutTemplateService_ChangeStateAsync_ValidationFailed();
            
        // Act
        var result = await controller.ChangeWorkoutTemplateState(TestTemplateId, request);
        
        // Assert
        var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.Value.Should().NotBeNull();
        
        // Verify the error structure is passed through
        var errorObject = badRequestResult.Value;
        var errorsProp = errorObject.GetType().GetProperty("errors");
        errorsProp.Should().NotBeNull();
        
        automocker.VerifyWorkoutTemplateService_ChangeStateAsync_CalledOnceWith(TestTemplateId, request.WorkoutStateId);
    }
    
    [Fact]
    public async Task ChangeWorkoutTemplateState_WhenInvalidFormat_ReturnsBadRequestWithErrors()
    {
        // Arrange
        var automocker = new AutoMocker();
        var controller = automocker.CreateInstance<WorkoutTemplatesController>();
        
        var request = new ChangeWorkoutStateDto
        {
            WorkoutStateId = "invalid-format-id"
        };
        
        automocker.SetupWorkoutTemplateService_ChangeStateAsync_InvalidFormat("Invalid WorkoutStateId format. Expected format: workoutstate-{guid}");
            
        // Act
        var result = await controller.ChangeWorkoutTemplateState(TestTemplateId, request);
        
        // Assert
        var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.Value.Should().NotBeNull();
        
        // Verify the error structure is passed through
        var errorObject = badRequestResult.Value;
        var errorsProp = errorObject.GetType().GetProperty("errors");
        errorsProp.Should().NotBeNull();
        
        automocker.VerifyWorkoutTemplateService_ChangeStateAsync_CalledOnceWith(TestTemplateId, request.WorkoutStateId);
    }
    
    [Fact]
    public async Task ChangeWorkoutTemplateState_WhenUnauthorized_ReturnsBadRequestWithErrors()
    {
        // Arrange
        var automocker = new AutoMocker();
        var controller = automocker.CreateInstance<WorkoutTemplatesController>();
        
        var request = new ChangeWorkoutStateDto
        {
            WorkoutStateId = TestWorkoutStateId
        };
        
        automocker.SetupWorkoutTemplateService_ChangeStateAsync_Unauthorized();
            
        // Act
        var result = await controller.ChangeWorkoutTemplateState(TestTemplateId, request);
        
        // Assert
        // According to the refactored code, Unauthorized falls into the default BadRequest case
        var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.Value.Should().NotBeNull();
        
        // Verify the error structure is passed through
        var errorObject = badRequestResult.Value;
        var errorsProp = errorObject.GetType().GetProperty("errors");
        errorsProp.Should().NotBeNull();
        
        automocker.VerifyWorkoutTemplateService_ChangeStateAsync_CalledOnceWith(TestTemplateId, request.WorkoutStateId);
    }
    
    [Fact]
    public async Task ChangeWorkoutTemplateState_WhenInternalError_ReturnsBadRequestWithErrors()
    {
        // Arrange
        var automocker = new AutoMocker();
        var controller = automocker.CreateInstance<WorkoutTemplatesController>();
        
        var request = new ChangeWorkoutStateDto
        {
            WorkoutStateId = TestWorkoutStateId
        };
        
        automocker.SetupWorkoutTemplateService_ChangeStateAsync_InternalError();
            
        // Act
        var result = await controller.ChangeWorkoutTemplateState(TestTemplateId, request);
        
        // Assert
        // According to the refactored code, InternalError falls into the default BadRequest case
        var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.Value.Should().NotBeNull();
        
        // Verify the error structure is passed through
        var errorObject = badRequestResult.Value;
        var errorsProp = errorObject.GetType().GetProperty("errors");
        errorsProp.Should().NotBeNull();
        
        automocker.VerifyWorkoutTemplateService_ChangeStateAsync_CalledOnceWith(TestTemplateId, request.WorkoutStateId);
    }
    
    #endregion
    
    #region DuplicateWorkoutTemplate Tests
    
    [Fact]
    public async Task DuplicateWorkoutTemplate_WhenSuccessful_ReturnsCreatedAtAction()
    {
        // Arrange
        var automocker = new AutoMocker();
        var controller = automocker.CreateInstance<WorkoutTemplatesController>();
        
        var request = new DuplicateWorkoutTemplateDto
        {
            NewName = "Copy of Test Workout"
        };
        
        var duplicatedDto = WorkoutTemplateDto.Empty with 
        { 
            Id = CreatedTemplateId,
            Name = request.NewName,
            WorkoutState = new ReferenceDataDto 
            { 
                Id = TestDraftStateId, 
                Value = "DRAFT", 
                Description = "Draft state" 
            }
        };
        
        automocker.SetupWorkoutTemplateService_DuplicateAsync_Success(duplicatedDto);
            
        // Act
        var result = await controller.DuplicateWorkoutTemplate(TestTemplateId, request);
        
        // Assert
        var createdResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.ActionName.Should().Be("GetWorkoutTemplate");
        createdResult.RouteValues.Should().ContainKey("id").WhoseValue.Should().Be(CreatedTemplateId);
        var data = createdResult.Value.Should().BeOfType<WorkoutTemplateDto>().Subject;
        data.Name.Should().Be(request.NewName);
        
        automocker.VerifyWorkoutTemplateService_DuplicateAsync_CalledOnceWith(TestTemplateId, request.NewName);
    }
    
    [Fact]
    public async Task DuplicateWorkoutTemplate_WhenTemplateNotFound_ReturnsNotFound()
    {
        // Arrange
        var automocker = new AutoMocker();
        var controller = automocker.CreateInstance<WorkoutTemplatesController>();
        
        var request = new DuplicateWorkoutTemplateDto
        {
            NewName = "Copy of Non-Existent Workout"
        };
        
        automocker.SetupWorkoutTemplateService_DuplicateAsync_NotFound();
            
        // Act
        var result = await controller.DuplicateWorkoutTemplate("workouttemplate-nonexistent", request);
        
        // Assert
        result.Should().BeOfType<NotFoundResult>();
        
        automocker.VerifyWorkoutTemplateService_DuplicateAsync_CalledOnceWith("workouttemplate-nonexistent", request.NewName);
    }
    
    [Fact]
    public async Task DuplicateWorkoutTemplate_WhenNewNameAlreadyExists_ReturnsConflictWithErrors()
    {
        // Arrange
        var automocker = new AutoMocker();
        var controller = automocker.CreateInstance<WorkoutTemplatesController>();
        
        var request = new DuplicateWorkoutTemplateDto
        {
            NewName = ExistingWorkoutName
        };
        
        automocker.SetupWorkoutTemplateService_DuplicateAsync_AlreadyExists(ExistingWorkoutName);
            
        // Act
        var result = await controller.DuplicateWorkoutTemplate(TestTemplateId, request);
        
        // Assert
        var conflictResult = result.Should().BeOfType<ConflictObjectResult>().Subject;
        conflictResult.Value.Should().NotBeNull();
        
        // Verify the error structure is passed through
        var errorObject = conflictResult.Value;
        var errorsProp = errorObject.GetType().GetProperty("errors");
        errorsProp.Should().NotBeNull();
        
        automocker.VerifyWorkoutTemplateService_DuplicateAsync_CalledOnceWith(TestTemplateId, request.NewName);
    }
    
    [Fact]
    public async Task DuplicateWorkoutTemplate_WhenValidationFails_ReturnsConflictWithErrors()
    {
        // Arrange
        var automocker = new AutoMocker();
        var controller = automocker.CreateInstance<WorkoutTemplatesController>();
        
        var request = new DuplicateWorkoutTemplateDto
        {
            NewName = "" // Empty name to trigger validation failure
        };
        
        automocker.SetupWorkoutTemplateService_DuplicateAsync_ValidationFailed("Name cannot be empty");
            
        // Act
        var result = await controller.DuplicateWorkoutTemplate(TestTemplateId, request);
        
        // Assert
        var conflictResult = result.Should().BeOfType<ConflictObjectResult>().Subject;
        conflictResult.Value.Should().NotBeNull();
        
        // Verify the error structure is passed through
        var errorObject = conflictResult.Value;
        var errorsProp = errorObject.GetType().GetProperty("errors");
        errorsProp.Should().NotBeNull();
        
        automocker.VerifyWorkoutTemplateService_DuplicateAsync_CalledOnceWith(TestTemplateId, request.NewName);
    }
    
    [Fact]
    public async Task DuplicateWorkoutTemplate_WithInvalidTemplateIdFormat_HandlesGracefully()
    {
        // Arrange
        var automocker = new AutoMocker();
        var controller = automocker.CreateInstance<WorkoutTemplatesController>();
        
        var request = new DuplicateWorkoutTemplateDto
        {
            NewName = "Copy of Workout"
        };
        
        // Setup to return NotFound when an empty/invalid ID is parsed
        automocker.SetupWorkoutTemplateService_DuplicateAsync_NotFound();
            
        // Act
        var result = await controller.DuplicateWorkoutTemplate("invalid-format", request);
        
        // Assert
        // When ParseOrEmpty receives an invalid format, it returns Empty, which should result in NotFound
        result.Should().BeOfType<NotFoundResult>();
        
        automocker.VerifyWorkoutTemplateService_DuplicateAsync_CalledOnceWith("invalid-format", request.NewName);
    }
    
    [Fact]
    public async Task DuplicateWorkoutTemplate_WithSpecialCharactersInNewName_ProcessesCorrectly()
    {
        // Arrange
        var automocker = new AutoMocker();
        var controller = automocker.CreateInstance<WorkoutTemplatesController>();
        
        var specialName = "Copy of Workout - Advanced (v2.0) & More!";
        var request = new DuplicateWorkoutTemplateDto
        {
            NewName = specialName
        };
        
        var duplicatedDto = WorkoutTemplateDto.Empty with 
        { 
            Id = CreatedTemplateId,
            Name = specialName
        };
        
        automocker.SetupWorkoutTemplateService_DuplicateAsync_Success(duplicatedDto);
            
        // Act
        var result = await controller.DuplicateWorkoutTemplate(TestTemplateId, request);
        
        // Assert
        var createdResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        var data = createdResult.Value.Should().BeOfType<WorkoutTemplateDto>().Subject;
        data.Name.Should().Be(specialName);
        
        automocker.VerifyWorkoutTemplateService_DuplicateAsync_CalledOnceWith(TestTemplateId, specialName);
    }
    
    #endregion
}