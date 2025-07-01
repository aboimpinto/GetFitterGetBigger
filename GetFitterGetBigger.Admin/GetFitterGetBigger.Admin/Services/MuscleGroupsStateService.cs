using GetFitterGetBigger.Admin.Models.Dtos;

namespace GetFitterGetBigger.Admin.Services
{
    public class MuscleGroupsStateService : IMuscleGroupsStateService
    {
        private readonly IMuscleGroupsService _muscleGroupsService;
        private readonly IReferenceDataService _referenceDataService;
        private List<MuscleGroupDto> _muscleGroups = new();
        private List<ReferenceDataDto> _bodyParts = new();
        private MuscleGroupDto? _selectedMuscleGroup;
        private string _searchTerm = string.Empty;
        private string? _selectedBodyPartId;
        private bool _isLoading;
        private bool _isLoadingBodyParts;
        private bool _isCreating;
        private bool _isUpdating;
        private bool _isDeleting;
        private string? _errorMessage;

        public event Action? OnChange;

        public MuscleGroupsStateService(
            IMuscleGroupsService muscleGroupsService,
            IReferenceDataService referenceDataService)
        {
            _muscleGroupsService = muscleGroupsService;
            _referenceDataService = referenceDataService;
        }

        // List state
        public IEnumerable<MuscleGroupDto> MuscleGroups => _muscleGroups;
        
        public IEnumerable<MuscleGroupDto> FilteredMuscleGroups
        {
            get
            {
                var filtered = _muscleGroups.AsEnumerable();

                // Filter by search term
                if (!string.IsNullOrWhiteSpace(_searchTerm))
                {
                    filtered = filtered.Where(mg => 
                        mg.Name.Contains(_searchTerm, StringComparison.OrdinalIgnoreCase));
                }

                // Filter by body part
                if (!string.IsNullOrWhiteSpace(_selectedBodyPartId))
                {
                    filtered = filtered.Where(mg => mg.BodyPartId == _selectedBodyPartId);
                }

                return filtered.OrderBy(mg => mg.Name);
            }
        }

        public bool IsLoading => _isLoading;
        public string? ErrorMessage => _errorMessage;

        // Selected muscle group state
        public MuscleGroupDto? SelectedMuscleGroup => _selectedMuscleGroup;
        public bool IsLoadingMuscleGroup => false; // Not used for muscle groups as we don't load individual items

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

        public string? SelectedBodyPartId
        {
            get => _selectedBodyPartId;
            set
            {
                _selectedBodyPartId = value;
                NotifyStateChanged();
            }
        }

        // Body parts state
        public IEnumerable<ReferenceDataDto> BodyParts => _bodyParts;
        public bool IsLoadingBodyParts => _isLoadingBodyParts;

        public async Task InitializeAsync()
        {
            // Load both body parts and muscle groups in parallel
            var loadBodyPartsTask = LoadBodyPartsAsync();
            var loadMuscleGroupsTask = LoadMuscleGroupsAsync();
            
            await Task.WhenAll(loadBodyPartsTask, loadMuscleGroupsTask);
        }

        public async Task LoadMuscleGroupsAsync()
        {
            try
            {
                _isLoading = true;
                _errorMessage = null;
                NotifyStateChanged();

                var muscleGroups = await _muscleGroupsService.GetMuscleGroupsAsync();
                _muscleGroups = muscleGroups.OrderBy(mg => mg.Name).ToList();

                // Enhance muscle groups with body part names if body parts are loaded
                if (_bodyParts.Any())
                {
                    foreach (var muscleGroup in _muscleGroups)
                    {
                        var bodyPart = _bodyParts.FirstOrDefault(bp => bp.Id == muscleGroup.BodyPartId);
                        if (bodyPart != null)
                        {
                            muscleGroup.BodyPartName = bodyPart.Value;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _errorMessage = $"Failed to load muscle groups: {ex.Message}";
            }
            finally
            {
                _isLoading = false;
                NotifyStateChanged();
            }
        }

        public async Task LoadBodyPartsAsync()
        {
            try
            {
                _isLoadingBodyParts = true;
                NotifyStateChanged();

                var bodyParts = await _referenceDataService.GetBodyPartsAsync();
                _bodyParts = bodyParts.OrderBy(bp => bp.Value).ToList();

                // If muscle groups are already loaded, enhance them with body part names
                if (_muscleGroups.Any())
                {
                    foreach (var muscleGroup in _muscleGroups)
                    {
                        var bodyPart = _bodyParts.FirstOrDefault(bp => bp.Id == muscleGroup.BodyPartId);
                        if (bodyPart != null)
                        {
                            muscleGroup.BodyPartName = bodyPart.Value;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _errorMessage = $"Failed to load body parts: {ex.Message}";
            }
            finally
            {
                _isLoadingBodyParts = false;
                NotifyStateChanged();
            }
        }

        public async Task CreateMuscleGroupAsync(CreateMuscleGroupDto dto)
        {
            try
            {
                _isCreating = true;
                _errorMessage = null;
                NotifyStateChanged();

                var created = await _muscleGroupsService.CreateMuscleGroupAsync(dto);
                
                // Reload the list to get the updated data
                await LoadMuscleGroupsAsync();
            }
            catch (InvalidOperationException ex)
            {
                _errorMessage = ex.Message;
            }
            catch (Exception ex)
            {
                _errorMessage = $"Failed to create muscle group: {ex.Message}";
            }
            finally
            {
                _isCreating = false;
                NotifyStateChanged();
            }
        }

        public async Task UpdateMuscleGroupAsync(string id, UpdateMuscleGroupDto dto)
        {
            try
            {
                _isUpdating = true;
                _errorMessage = null;
                NotifyStateChanged();

                var updated = await _muscleGroupsService.UpdateMuscleGroupAsync(id, dto);
                
                // Reload the list to get the updated data
                await LoadMuscleGroupsAsync();
                
                // Update selected muscle group if it was the one being updated
                if (_selectedMuscleGroup?.Id == id)
                {
                    _selectedMuscleGroup = _muscleGroups.FirstOrDefault(mg => mg.Id == id);
                }
            }
            catch (InvalidOperationException ex)
            {
                _errorMessage = ex.Message;
            }
            catch (Exception ex)
            {
                _errorMessage = $"Failed to update muscle group: {ex.Message}";
            }
            finally
            {
                _isUpdating = false;
                NotifyStateChanged();
            }
        }

        public async Task DeleteMuscleGroupAsync(string id)
        {
            try
            {
                _isDeleting = true;
                _errorMessage = null;
                NotifyStateChanged();

                await _muscleGroupsService.DeleteMuscleGroupAsync(id);
                
                // Remove from local list immediately for better UX
                _muscleGroups.RemoveAll(mg => mg.Id == id);
                
                // Clear selected if it was deleted
                if (_selectedMuscleGroup?.Id == id)
                {
                    _selectedMuscleGroup = null;
                }
                
                // Reload to ensure consistency
                await LoadMuscleGroupsAsync();
            }
            catch (InvalidOperationException ex)
            {
                _errorMessage = ex.Message;
                // Reload to restore the item that couldn't be deleted
                await LoadMuscleGroupsAsync();
            }
            catch (Exception ex)
            {
                _errorMessage = $"Failed to delete muscle group: {ex.Message}";
                // Reload to restore the item that couldn't be deleted
                await LoadMuscleGroupsAsync();
            }
            finally
            {
                _isDeleting = false;
                NotifyStateChanged();
            }
        }

        public void SelectMuscleGroup(MuscleGroupDto? muscleGroup)
        {
            _selectedMuscleGroup = muscleGroup;
            NotifyStateChanged();
        }

        public void ClearSelectedMuscleGroup()
        {
            _selectedMuscleGroup = null;
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

        public void SetSelectedBodyPart(string? bodyPartId)
        {
            SelectedBodyPartId = bodyPartId;
        }

        private void NotifyStateChanged() => OnChange?.Invoke();
    }
}