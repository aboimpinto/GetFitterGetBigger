using GetFitterGetBigger.API.Models.SpecializedIds;

namespace GetFitterGetBigger.API.Services.Commands;

public record GetExercisesCommand(
    int Page = 1,
    int PageSize = 10,
    string Name = "",
    string SearchTerm = "",
    DifficultyLevelId DifficultyLevelId = default,
    List<MuscleGroupId> MuscleGroupIds = null!,
    List<EquipmentId> EquipmentIds = null!,
    List<MovementPatternId> MovementPatternIds = null!,
    List<BodyPartId> BodyPartIds = null!,
    bool IncludeInactive = false,
    bool IsActive = false)
{
    public GetExercisesCommand() : this(
        Page: 1,
        PageSize: 10,
        Name: string.Empty,
        SearchTerm: string.Empty,
        DifficultyLevelId: DifficultyLevelId.Empty,
        MuscleGroupIds: new List<MuscleGroupId>(),
        EquipmentIds: new List<EquipmentId>(),
        MovementPatternIds: new List<MovementPatternId>(),
        BodyPartIds: new List<BodyPartId>(),
        IncludeInactive: false,
        IsActive: false)
    {
    }
}