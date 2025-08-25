using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models.Entities;

namespace GetFitterGetBigger.API.Builders;

public class ExerciseDtoBuilder
{
    private readonly Exercise _exercise;
    private string? _id;
    private string? _name;
    private string? _description;
    private string? _videoUrl;
    private string? _imageUrl;
    private bool _isUnilateral;
    private bool _isActive;
    private List<CoachNoteDto>? _coachNotes;
    private List<ReferenceDataDto>? _exerciseTypes;
    private List<MuscleGroupWithRoleDto>? _muscleGroups;
    private List<ReferenceDataDto>? _equipment;
    private List<ReferenceDataDto>? _bodyParts;
    private List<ReferenceDataDto>? _movementPatterns;
    private ReferenceDataDto? _difficulty;
    private ReferenceDataDto? _kineticChain;
    private ReferenceDataDto? _exerciseWeightType;

    public ExerciseDtoBuilder(Exercise exercise)
    {
        _exercise = exercise;
    }

    public ExerciseDtoBuilder WithBasicInfo()
    {
        _id = _exercise.Id.ToString();
        _name = _exercise.Name;
        _description = _exercise.Description;
        _videoUrl = _exercise.VideoUrl;
        _imageUrl = _exercise.ImageUrl;
        _isUnilateral = _exercise.IsUnilateral;
        _isActive = _exercise.IsActive;
        return this;
    }

    public ExerciseDtoBuilder WithCoachNotes()
    {
        _coachNotes = _exercise.CoachNotes?
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
        _exerciseTypes = _exercise.ExerciseExerciseTypes?
            .Where(eet => eet.ExerciseType != null)
            .Select(eet => MapReferenceData(eet.ExerciseType!))
            .ToList() ?? new List<ReferenceDataDto>();
        return this;
    }

    public ExerciseDtoBuilder WithMuscleGroups()
    {
        _muscleGroups = _exercise.ExerciseMuscleGroups?
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
        _equipment = _exercise.ExerciseEquipment?
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
        _bodyParts = _exercise.ExerciseBodyParts?
            .Where(ebp => ebp.BodyPart != null)
            .Select(ebp => MapReferenceData(ebp.BodyPart!))
            .ToList() ?? new List<ReferenceDataDto>();
        return this;
    }

    public ExerciseDtoBuilder WithMovementPatterns()
    {
        _movementPatterns = _exercise.ExerciseMovementPatterns?
            .Where(emp => emp.MovementPattern != null)
            .Select(emp => new ReferenceDataDto
            {
                Id = emp.MovementPattern!.Id.ToString(),
                Value = emp.MovementPattern.Value,
                Description = emp.MovementPattern.Description
            })
            .ToList() ?? new List<ReferenceDataDto>();
        return this;
    }

    public ExerciseDtoBuilder WithReferenceData()
    {
        _difficulty = _exercise.Difficulty != null 
            ? MapReferenceData(_exercise.Difficulty) 
            : null!;
            
        _kineticChain = _exercise.KineticChain != null 
            ? MapReferenceData(_exercise.KineticChain) 
            : null;
            
        _exerciseWeightType = _exercise.ExerciseWeightType != null 
            ? MapReferenceData(_exercise.ExerciseWeightType) 
            : null;
            
        return this;
    }

    public ExerciseDto Build() => new()
    {
        Id = _id ?? string.Empty,
        Name = _name ?? string.Empty,
        Description = _description ?? string.Empty,
        VideoUrl = _videoUrl,
        ImageUrl = _imageUrl,
        IsUnilateral = _isUnilateral,
        IsActive = _isActive,
        CoachNotes = _coachNotes ?? new List<CoachNoteDto>(),
        ExerciseTypes = _exerciseTypes ?? new List<ReferenceDataDto>(),
        MuscleGroups = _muscleGroups ?? new List<MuscleGroupWithRoleDto>(),
        Equipment = _equipment ?? new List<ReferenceDataDto>(),
        BodyParts = _bodyParts ?? new List<ReferenceDataDto>(),
        MovementPatterns = _movementPatterns ?? new List<ReferenceDataDto>(),
        Difficulty = _difficulty ?? ReferenceDataDto.Empty,
        KineticChain = _kineticChain,
        ExerciseWeightType = _exerciseWeightType
    };

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