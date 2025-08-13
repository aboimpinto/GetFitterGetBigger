using GetFitterGetBigger.API.Constants.ErrorMessages;
using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Repositories.Interfaces;
using GetFitterGetBigger.API.Services.Interfaces;
using GetFitterGetBigger.API.Services.Results;
using GetFitterGetBigger.API.Services.Validation;
using GetFitterGetBigger.API.Services.WorkoutTemplate.Models.DTOs;
using Olimpo.EntityFramework.Persistency;

namespace GetFitterGetBigger.API.Services.WorkoutTemplate.Features.Equipment;

/// <summary>
/// Service implementation for analyzing and managing equipment requirements for workout templates
/// </summary>
public class EquipmentRequirementsService(
    IUnitOfWorkProvider<FitnessDbContext> unitOfWorkProvider,
    IWorkoutTemplateExerciseService workoutTemplateExerciseService,
    IExerciseService exerciseService,
    ILogger<EquipmentRequirementsService> logger) : IEquipmentRequirementsService
{

    public async Task<ServiceResult<IEnumerable<EquipmentDto>>> GetRequiredEquipmentAsync(WorkoutTemplateId workoutTemplateId)
    {
        return await ServiceValidate.Build<IEnumerable<EquipmentDto>>()
            .EnsureNotEmpty(workoutTemplateId, WorkoutTemplateErrorMessages.InvalidIdFormat)
            .EnsureExistsAsync(
                async () => await WorkoutTemplateExistsAsync(workoutTemplateId),
                "WorkoutTemplate")
            .MatchAsync(
                whenValid: async () => await LoadRequiredEquipmentAsync(workoutTemplateId),
                whenInvalid: errors => ServiceResult<IEnumerable<EquipmentDto>>.Failure(
                    [], 
                    errors.FirstOrDefault() ?? ServiceError.ValidationFailed("Unknown error"))
            );
    }
    
    public async Task<ServiceResult<IEnumerable<EquipmentUsageDto>>> AnalyzeEquipmentUsageAsync(
        IEnumerable<WorkoutTemplateId> workoutTemplateIds)
    {
        var templateIds = workoutTemplateIds.ToList();
        
        return await ServiceValidate.Build<IEnumerable<EquipmentUsageDto>>()
            .Ensure(() => templateIds.Any(), ServiceError.ValidationFailed("At least one workout template ID is required"))
            .Ensure(() => templateIds.All(id => !id.IsEmpty), ServiceError.ValidationFailed("All workout template IDs must be valid"))
            .MatchAsync(
                whenValid: async () => await PerformEquipmentUsageAnalysisAsync(templateIds),
                whenInvalid: errors => ServiceResult<IEnumerable<EquipmentUsageDto>>.Failure(
                    [],
                    errors.FirstOrDefault() ?? ServiceError.ValidationFailed("Unknown error"))
            );
    }
    
    public async Task<ServiceResult<EquipmentAvailabilityDto>> CheckEquipmentAvailabilityAsync(
        WorkoutTemplateId workoutTemplateId,
        IEnumerable<EquipmentId> availableEquipmentIds)
    {
        // First get the required equipment
        var requiredEquipmentResult = await GetRequiredEquipmentAsync(workoutTemplateId);
        
        // Use pattern matching to handle the result
        return !requiredEquipmentResult.IsSuccess
            ? ServiceResult<EquipmentAvailabilityDto>.Failure(
                new EquipmentAvailabilityDto(),
                requiredEquipmentResult.Errors)
            : await CalculateEquipmentAvailabilityAsync(
                requiredEquipmentResult.Data.ToList(),
                availableEquipmentIds);
    }
    
    private async Task<ServiceResult<IEnumerable<EquipmentDto>>> LoadRequiredEquipmentAsync(WorkoutTemplateId id)
    {
        // Get all template exercises using WorkoutTemplateExerciseService
        var templateExercisesResult = await workoutTemplateExerciseService.GetByWorkoutTemplateAsync(id);
        
        var result = !templateExercisesResult.IsSuccess
            ? ServiceResult<IEnumerable<EquipmentDto>>.Failure([], templateExercisesResult.Errors)
            : await ProcessTemplateExercisesAsync(templateExercisesResult.Data);
            
        return result;
    }
    
    private async Task<ServiceResult<IEnumerable<EquipmentDto>>> ProcessTemplateExercisesAsync(WorkoutTemplateExerciseListDto sessionData)
    {
        var templateExercises = sessionData.WarmupExercises
            .Concat(sessionData.MainExercises)
            .Concat(sessionData.CooldownExercises);
            
        var result = !templateExercises.Any()
            ? ServiceResult<IEnumerable<EquipmentDto>>.Success([])
            : await ExtractEquipmentFromExercisesAsync(templateExercises);
            
        return result;
    }
    
    private async Task<ServiceResult<IEnumerable<EquipmentDto>>> ExtractEquipmentFromExercisesAsync(
        IEnumerable<WorkoutTemplateExerciseDto> templateExercises)
    {
        // Get all unique exercise IDs
        var exerciseIds = templateExercises
            .Select(te => ExerciseId.ParseOrEmpty(te.Exercise.Id))
            .Where(id => !id.IsEmpty)
            .Distinct()
            .ToList();
        
        // Get all exercises with their equipment using ExerciseService
        var equipmentSet = new HashSet<EquipmentDto>(new EquipmentDtoComparer());
        
        foreach (var exerciseId in exerciseIds)
        {
            var exerciseResult = await exerciseService.GetByIdAsync(exerciseId);
            if (!exerciseResult.IsSuccess)
            {
                // Log warning but continue - one exercise failing shouldn't fail the whole operation
                logger.LogWarning("Failed to get exercise {ExerciseId} for equipment extraction", exerciseId);
                continue;
            }
            
            var exerciseDto = exerciseResult.Data;
            if (!exerciseDto.IsEmpty && exerciseDto.Equipment?.Any() == true)
            {
                foreach (var equipmentRef in exerciseDto.Equipment)
                {
                    var equipmentDto = MapReferenceDataToEquipmentDto(equipmentRef);
                    equipmentSet.Add(equipmentDto);
                }
            }
        }
        
        return ServiceResult<IEnumerable<EquipmentDto>>.Success(equipmentSet.OrderBy(e => e.Name));
    }
    
    private async Task<ServiceResult<IEnumerable<EquipmentUsageDto>>> PerformEquipmentUsageAnalysisAsync(
        List<WorkoutTemplateId> templateIds)
    {
        var equipmentUsageMap = new Dictionary<string, EquipmentUsageInfo>();
        var totalTemplates = templateIds.Count;
        
        foreach (var templateId in templateIds)
        {
            var equipmentResult = await GetRequiredEquipmentAsync(templateId);
            if (!equipmentResult.IsSuccess)
            {
                logger.LogWarning("Failed to get equipment for template {TemplateId}", templateId);
                continue;
            }
            
            foreach (var equipment in equipmentResult.Data)
            {
                if (!equipmentUsageMap.ContainsKey(equipment.Id))
                {
                    equipmentUsageMap[equipment.Id] = new EquipmentUsageInfo 
                    { 
                        Equipment = equipment,
                        Count = 0
                    };
                }
                equipmentUsageMap[equipment.Id].Count++;
            }
        }
        
        var usageStats = equipmentUsageMap.Values.Select(info => new EquipmentUsageDto
        {
            Equipment = info.Equipment,
            UsageCount = info.Count,
            UsagePercentage = (decimal)info.Count / totalTemplates * 100
        }).OrderByDescending(u => u.UsagePercentage);
        
        return ServiceResult<IEnumerable<EquipmentUsageDto>>.Success(usageStats);
    }
    
    private async Task<ServiceResult<EquipmentAvailabilityDto>> CalculateEquipmentAvailabilityAsync(
        List<EquipmentDto> requiredEquipment,
        IEnumerable<EquipmentId> availableEquipmentIds)
    {
        var availableIdSet = availableEquipmentIds
            .Where(id => !id.IsEmpty)
            .Select(id => id.ToString())
            .ToHashSet();
        
        var availableEquipment = requiredEquipment
            .Where(e => availableIdSet.Contains(e.Id))
            .ToList();
            
        var missingEquipment = requiredEquipment
            .Where(e => !availableIdSet.Contains(e.Id))
            .ToList();
        
        var availabilityDto = new EquipmentAvailabilityDto
        {
            CanPerformWorkout = !missingEquipment.Any(),
            RequiredEquipment = requiredEquipment,
            AvailableEquipment = availableEquipment,
            MissingEquipment = missingEquipment,
            AvailabilityPercentage = requiredEquipment.Any() 
                ? (decimal)availableEquipment.Count / requiredEquipment.Count * 100 
                : 100
        };
        
        // TODO: In future, we could add logic to find alternative exercises
        // that can be performed with available equipment
        
        return await Task.FromResult(ServiceResult<EquipmentAvailabilityDto>.Success(availabilityDto));
    }
    
    private async Task<bool> WorkoutTemplateExistsAsync(WorkoutTemplateId id)
    {
        using var unitOfWork = unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IWorkoutTemplateRepository>();
        return await repository.ExistsAsync(id);
    }
    
    private static EquipmentDto MapReferenceDataToEquipmentDto(ReferenceDataDto equipmentRef)
    {
        return new()
        {
            Id = equipmentRef.Id,
            Name = equipmentRef.Value,
            IsActive = true, // Assume active since it's in the exercise
            CreatedAt = DateTime.UtcNow
        };
    }
    
    // Helper class for tracking equipment usage
    private class EquipmentUsageInfo
    {
        public EquipmentDto Equipment { get; set; } = new();
        public int Count { get; set; }
    }
    
    // Custom comparer to avoid duplicate equipment based on ID
    private class EquipmentDtoComparer : IEqualityComparer<EquipmentDto>
    {
        public bool Equals(EquipmentDto? x, EquipmentDto? y)
        {
            if (x == null && y == null) return true;
            if (x == null || y == null) return false;
            return x.Id == y.Id;
        }
        
        public int GetHashCode(EquipmentDto obj)
        {
            return obj.Id?.GetHashCode() ?? 0;
        }
    }
}