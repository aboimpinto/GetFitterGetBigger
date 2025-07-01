using GetFitterGetBigger.Admin.Models.Dtos;

namespace GetFitterGetBigger.Admin.Services
{
    public interface IMuscleGroupsStateService
    {
        event Action? OnChange;
        
        // List state
        IEnumerable<MuscleGroupDto> MuscleGroups { get; }
        IEnumerable<MuscleGroupDto> FilteredMuscleGroups { get; }
        bool IsLoading { get; }
        string? ErrorMessage { get; }
        
        // Selected muscle group state
        MuscleGroupDto? SelectedMuscleGroup { get; }
        bool IsLoadingMuscleGroup { get; }
        
        // Form state
        bool IsCreating { get; }
        bool IsUpdating { get; }
        bool IsDeleting { get; }
        
        // Filter state
        string SearchTerm { get; set; }
        string? SelectedBodyPartId { get; set; }
        
        // Body parts state (for dropdowns)
        IEnumerable<ReferenceDataDto> BodyParts { get; }
        bool IsLoadingBodyParts { get; }
        
        // Methods
        Task InitializeAsync();
        Task LoadMuscleGroupsAsync();
        Task LoadBodyPartsAsync();
        Task CreateMuscleGroupAsync(CreateMuscleGroupDto dto);
        Task UpdateMuscleGroupAsync(string id, UpdateMuscleGroupDto dto);
        Task DeleteMuscleGroupAsync(string id);
        void SelectMuscleGroup(MuscleGroupDto? muscleGroup);
        void ClearSelectedMuscleGroup();
        void ClearError();
        void SetSearchTerm(string searchTerm);
        void SetSelectedBodyPart(string? bodyPartId);
    }
}