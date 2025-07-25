using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Services.Commands.WorkoutTemplateExercises;

namespace GetFitterGetBigger.API.Tests.TestBuilders.ServiceCommands;

public class AddExerciseToTemplateCommandBuilder
{
    private WorkoutTemplateId _workoutTemplateId = WorkoutTemplateId.New();
    private ExerciseId _exerciseId = ExerciseId.New();
    private string _zone = "Main";
    private int? _sequenceOrder = null;
    private UserId _userId = UserId.New();
    private string? _notes = null;
    
    public AddExerciseToTemplateCommandBuilder WithWorkoutTemplateId(WorkoutTemplateId id)
    {
        _workoutTemplateId = id;
        return this;
    }
    
    public AddExerciseToTemplateCommandBuilder WithExerciseId(ExerciseId id)
    {
        _exerciseId = id;
        return this;
    }
    
    public AddExerciseToTemplateCommandBuilder WithZone(string zone)
    {
        _zone = zone;
        return this;
    }
    
    public AddExerciseToTemplateCommandBuilder WithSequenceOrder(int? order)
    {
        _sequenceOrder = order;
        return this;
    }
    
    public AddExerciseToTemplateCommandBuilder WithUserId(UserId id)
    {
        _userId = id;
        return this;
    }
    
    public AddExerciseToTemplateCommandBuilder WithNotes(string? notes)
    {
        _notes = notes;
        return this;
    }
    
    public AddExerciseToTemplateCommand Build()
    {
        return new AddExerciseToTemplateCommand
        {
            WorkoutTemplateId = _workoutTemplateId,
            ExerciseId = _exerciseId,
            Zone = _zone,
            SequenceOrder = _sequenceOrder,
            UserId = _userId,
            Notes = _notes
        };
    }
}