using GetFitterGetBigger.Admin.Models.Dtos;

namespace GetFitterGetBigger.Admin.Services
{
    public interface IEquipmentStateService
    {
        event Action? OnChange;

        // List state
        IEnumerable<EquipmentDto> Equipment { get; }
        IEnumerable<EquipmentDto> FilteredEquipment { get; }
        bool IsLoading { get; }
        string? ErrorMessage { get; }

        // Selected equipment state
        EquipmentDto? SelectedEquipment { get; }
        bool IsLoadingEquipment { get; }

        // Form state
        bool IsCreating { get; }
        bool IsUpdating { get; }
        bool IsDeleting { get; }

        // Filter state
        string SearchTerm { get; set; }

        // Methods
        Task InitializeAsync();
        Task LoadEquipmentAsync();
        Task CreateEquipmentAsync(CreateEquipmentDto dto);
        Task UpdateEquipmentAsync(string id, UpdateEquipmentDto dto);
        Task DeleteEquipmentAsync(string id);
        void SelectEquipment(EquipmentDto? equipment);
        void ClearSelectedEquipment();
        void ClearError();
        void SetSearchTerm(string searchTerm);
    }
}