using GetFitterGetBigger.Admin.Models.Dtos;

namespace GetFitterGetBigger.Admin.Services
{
    public class EquipmentStateService : IEquipmentStateService
    {
        private readonly IEquipmentService _equipmentService;
        private List<EquipmentDto> _equipment = new();
        private EquipmentDto? _selectedEquipment;
        private string _searchTerm = string.Empty;
        private bool _isLoading;
        private bool _isCreating;
        private bool _isUpdating;
        private bool _isDeleting;
        private string? _errorMessage;

        public event Action? OnChange;

        public EquipmentStateService(IEquipmentService equipmentService)
        {
            _equipmentService = equipmentService;
        }

        // List state
        public IEnumerable<EquipmentDto> Equipment => _equipment;

        public IEnumerable<EquipmentDto> FilteredEquipment
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_searchTerm))
                    return _equipment;

                return _equipment.Where(e =>
                    e.Name.Contains(_searchTerm, StringComparison.OrdinalIgnoreCase));
            }
        }

        public bool IsLoading => _isLoading;
        public string? ErrorMessage => _errorMessage;

        // Selected equipment state
        public EquipmentDto? SelectedEquipment => _selectedEquipment;
        public bool IsLoadingEquipment => false; // Not used for equipment as we don't load individual items

        // Form state
        public bool IsCreating => _isCreating;
        public bool IsUpdating => _isUpdating;
        public bool IsDeleting => _isDeleting;

        // Filter state
        public string SearchTerm
        {
            get => _searchTerm;
            set
            {
                _searchTerm = value;
                NotifyStateChanged();
            }
        }

        public async Task InitializeAsync()
        {
            await LoadEquipmentAsync();
        }

        public async Task LoadEquipmentAsync()
        {
            try
            {
                _isLoading = true;
                _errorMessage = null;
                NotifyStateChanged();

                var equipment = await _equipmentService.GetEquipmentAsync();
                _equipment = equipment.OrderBy(e => e.Name).ToList();
            }
            catch (Exception ex)
            {
                _errorMessage = $"Failed to load equipment: {ex.Message}";
            }
            finally
            {
                _isLoading = false;
                NotifyStateChanged();
            }
        }

        public async Task CreateEquipmentAsync(CreateEquipmentDto dto)
        {
            try
            {
                _isCreating = true;
                _errorMessage = null;
                NotifyStateChanged();

                var created = await _equipmentService.CreateEquipmentAsync(dto);

                // Reload the list to get the updated data
                await LoadEquipmentAsync();

                // Select the newly created equipment
                _selectedEquipment = _equipment.FirstOrDefault(e => e.Id == created.Id);
            }
            catch (InvalidOperationException ex)
            {
                _errorMessage = ex.Message;
            }
            catch (Exception ex)
            {
                _errorMessage = $"Failed to create equipment: {ex.Message}";
            }
            finally
            {
                _isCreating = false;
                NotifyStateChanged();
            }
        }

        public async Task UpdateEquipmentAsync(string id, UpdateEquipmentDto dto)
        {
            try
            {
                _isUpdating = true;
                _errorMessage = null;
                NotifyStateChanged();

                var updated = await _equipmentService.UpdateEquipmentAsync(id, dto);

                // Reload the list to get the updated data
                await LoadEquipmentAsync();

                // Update selected equipment if it was the one being edited
                if (_selectedEquipment?.Id == id)
                {
                    _selectedEquipment = _equipment.FirstOrDefault(e => e.Id == id);
                }
            }
            catch (InvalidOperationException ex)
            {
                _errorMessage = ex.Message;
            }
            catch (Exception ex)
            {
                _errorMessage = $"Failed to update equipment: {ex.Message}";
            }
            finally
            {
                _isUpdating = false;
                NotifyStateChanged();
            }
        }

        public async Task DeleteEquipmentAsync(string id)
        {
            try
            {
                _isDeleting = true;
                _errorMessage = null;
                NotifyStateChanged();

                await _equipmentService.DeleteEquipmentAsync(id);

                // Reload the list to get the updated data
                await LoadEquipmentAsync();

                // Clear selected equipment if it was the one being deleted
                if (_selectedEquipment?.Id == id)
                {
                    _selectedEquipment = null;
                }
            }
            catch (InvalidOperationException ex)
            {
                _errorMessage = ex.Message;
            }
            catch (Exception ex)
            {
                _errorMessage = $"Failed to delete equipment: {ex.Message}";
            }
            finally
            {
                _isDeleting = false;
                NotifyStateChanged();
            }
        }

        public void SelectEquipment(EquipmentDto? equipment)
        {
            _selectedEquipment = equipment;
            NotifyStateChanged();
        }

        public void ClearSelectedEquipment()
        {
            _selectedEquipment = null;
            NotifyStateChanged();
        }

        public void ClearError()
        {
            _errorMessage = null;
            NotifyStateChanged();
        }

        public void SetSearchTerm(string searchTerm)
        {
            SearchTerm = searchTerm;
        }

        private void NotifyStateChanged() => OnChange?.Invoke();
    }
}