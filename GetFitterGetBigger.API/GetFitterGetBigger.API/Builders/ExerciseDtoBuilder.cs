using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models.Entities;

namespace GetFitterGetBigger.API.Builders;

public class ExerciseDtoBuilder
{
    private readonly Exercise _exercise;
    private readonly ExerciseDto _dto;

    public ExerciseDtoBuilder(Exercise exercise)
    {
        _exercise = exercise;
        _dto = new ExerciseDto();
    }

    public ExerciseDtoBuilder WithBasicInfo()
    {
        _dto.Id = _exercise.Id.ToString();
        _dto.Name = _exercise.Name;
        _dto.Description = _exercise.Description;
        _dto.VideoUrl = _exercise.VideoUrl;
        _dto.ImageUrl = _exercise.ImageUrl;
        _dto.IsUnilateral = _exercise.IsUnilateral;
        _dto.IsActive = _exercise.IsActive;
        return this;
    }

    public ExerciseDtoBuilder WithCoachNotes()
    {
        _dto.CoachNotes = _exercise.CoachNotes?
            .OrderBy(cn => cn.Order)
            .Select(cn => new CoachNoteDto
            {
                Id = cn.Id.ToString(),
                Text = cn.Text,
                Order = cn.Order
            })
            .ToList() ?? new List<CoachNoteDto>();
        return this;
    }

    public ExerciseDtoBuilder WithExerciseTypes()
    {
        _dto.ExerciseTypes = _exercise.ExerciseExerciseTypes?
            .Where(eet => eet.ExerciseType != null)
            .Select(eet => MapReferenceData(eet.ExerciseType!))
            .ToList() ?? new List<ReferenceDataDto>();
        return this;
    }

    public ExerciseDtoBuilder WithMuscleGroups()
    {
        _dto.MuscleGroups = _exercise.ExerciseMuscleGroups?
            .Where(emg => emg.MuscleGroup != null && emg.MuscleRole != null)
            .Select(emg => new MuscleGroupWithRoleDto
            {
                MuscleGroup = new ReferenceDataDto
                {
                    Id = emg.MuscleGroup!.Id.ToString(),
                    Value = emg.MuscleGroup.Name,
                    Description = emg.MuscleGroup.Name
                },
                Role = MapReferenceData(emg.MuscleRole!)
            })
            .ToList() ?? new List<MuscleGroupWithRoleDto>();
        return this;
    }

    public ExerciseDtoBuilder WithEquipment()
    {
        _dto.Equipment = _exercise.ExerciseEquipment?
            .Where(ee => ee.Equipment != null)
            .Select(ee => new ReferenceDataDto
            {
                Id = ee.Equipment!.Id.ToString(),
                Value = ee.Equipment.Name,
                Description = ee.Equipment.Name
            })
            .ToList() ?? new List<ReferenceDataDto>();
        return this;
    }

    public ExerciseDtoBuilder WithBodyParts()
    {
        _dto.BodyParts = _exercise.ExerciseBodyParts?
            .Where(ebp => ebp.BodyPart != null)
            .Select(ebp => MapReferenceData(ebp.BodyPart!))
            .ToList() ?? new List<ReferenceDataDto>();
        return this;
    }

    public ExerciseDtoBuilder WithMovementPatterns()
    {
        _dto.MovementPatterns = _exercise.ExerciseMovementPatterns?
            .Where(emp => emp.MovementPattern != null)
            .Select(emp => new ReferenceDataDto
            {
                Id = emp.MovementPattern!.Id.ToString(),
                Value = emp.MovementPattern.Name,
                Description = emp.MovementPattern.Description
            })
            .ToList() ?? new List<ReferenceDataDto>();
        return this;
    }

    public ExerciseDtoBuilder WithReferenceData()
    {
        _dto.Difficulty = _exercise.Difficulty != null 
            ? MapReferenceData(_exercise.Difficulty) 
            : null!;
            
        _dto.KineticChain = _exercise.KineticChain != null 
            ? MapReferenceData(_exercise.KineticChain) 
            : null;
            
        _dto.ExerciseWeightType = _exercise.ExerciseWeightType != null 
            ? MapReferenceData(_exercise.ExerciseWeightType) 
            : null;
            
        return this;
    }

    public ExerciseDto Build() => _dto;

    private static ReferenceDataDto MapReferenceData<T>(T data) where T : ReferenceDataBase
    {
        // Get the Id property using reflection since it's not in the base class
        var idProperty = data.GetType().GetProperty("Id");
        var id = idProperty?.GetValue(data)?.ToString() ?? string.Empty;
        
        return new ReferenceDataDto
        {
            Id = id,
            Value = data.Value,
            Description = data.Description
        };
    }
}