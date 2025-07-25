using GetFitterGetBigger.Admin.Models.Dtos;
using Microsoft.AspNetCore.Components;

namespace GetFitterGetBigger.Admin.Components.WorkoutTemplates
{
    public partial class WorkoutTemplateList : ComponentBase, IDisposable
    {
        [Parameter] public EventCallback OnCreateNew { get; set; }
        [Parameter] public EventCallback<WorkoutTemplateDto> OnTemplateSelected { get; set; }
        [Parameter] public EventCallback<WorkoutTemplateDto> OnTemplateEdit { get; set; }
        [Parameter] public EventCallback<WorkoutTemplateDto> OnTemplateDuplicate { get; set; }
        [Parameter] public EventCallback<WorkoutTemplateDto> OnTemplateDelete { get; set; }

        private string sortBy = "updated";
        private bool isAscending = false;
        private bool isRetrying = false;

        protected override void OnInitialized()
        {
            StateService.OnChange += StateHasChanged;
        }

        internal IEnumerable<WorkoutTemplateDto> GetSortedTemplates()
        {
            if (StateService.CurrentPage?.Items == null)
                return Enumerable.Empty<WorkoutTemplateDto>();

            var templates = StateService.CurrentPage.Items.AsEnumerable();

            templates = sortBy switch
            {
                "name" => isAscending 
                    ? templates.OrderBy(t => t.Name) 
                    : templates.OrderByDescending(t => t.Name),
                "created" => isAscending 
                    ? templates.OrderBy(t => t.CreatedAt) 
                    : templates.OrderByDescending(t => t.CreatedAt),
                "updated" => isAscending 
                    ? templates.OrderBy(t => t.UpdatedAt) 
                    : templates.OrderByDescending(t => t.UpdatedAt),
                _ => templates.OrderByDescending(t => t.UpdatedAt)
            };

            return templates;
        }

        internal async Task HandleSortChange()
        {
            // Sorting is done client-side, just trigger a re-render
            await InvokeAsync(StateHasChanged);
        }

        internal void ToggleSortDirection()
        {
            isAscending = !isAscending;
            StateHasChanged();
        }

        internal bool HasActiveFilters()
        {
            var filter = StateService.CurrentFilter;
            return !string.IsNullOrWhiteSpace(filter.NamePattern) ||
                   !string.IsNullOrWhiteSpace(filter.CategoryId) ||
                   !string.IsNullOrWhiteSpace(filter.DifficultyId) ||
                   !string.IsNullOrWhiteSpace(filter.StateId) ||
                   filter.IsPublic.HasValue;
        }

        internal async Task HandleRetry()
        {
            try
            {
                isRetrying = true;
                StateHasChanged();
                
                StateService.ClearError();
                await StateService.RefreshCurrentPageAsync();
            }
            finally
            {
                isRetrying = false;
                StateHasChanged();
            }
        }

        internal async Task HandleCreateNew()
        {
            if (OnCreateNew.HasDelegate)
            {
                await OnCreateNew.InvokeAsync();
            }
        }

        internal async Task HandleTemplateClick(WorkoutTemplateDto template)
        {
            if (OnTemplateSelected.HasDelegate)
            {
                await OnTemplateSelected.InvokeAsync(template);
            }
        }

        internal async Task HandleTemplateEdit(WorkoutTemplateDto template)
        {
            if (OnTemplateEdit.HasDelegate)
            {
                await OnTemplateEdit.InvokeAsync(template);
            }
        }

        internal async Task HandleTemplateDuplicate(WorkoutTemplateDto template)
        {
            if (OnTemplateDuplicate.HasDelegate)
            {
                await OnTemplateDuplicate.InvokeAsync(template);
            }
        }

        internal async Task HandleTemplateDelete(WorkoutTemplateDto template)
        {
            if (OnTemplateDelete.HasDelegate)
            {
                await OnTemplateDelete.InvokeAsync(template);
            }
        }

        internal async Task HandleTemplateStateChange(ReferenceDataDto newState)
        {
            // The state change is handled by the WorkoutTemplateCard's StateTransitionButton
            // We just need to refresh the list after the state change
            await StateService.RefreshCurrentPageAsync();
        }

        internal async Task GoToPage(int page)
        {
            var filter = StateService.CurrentFilter;
            filter.Page = page;
            await StateService.LoadWorkoutTemplatesAsync(filter);
        }

        internal async Task GoToPreviousPage()
        {
            if (StateService.CurrentFilter.Page > 1)
            {
                await GoToPage(StateService.CurrentFilter.Page - 1);
            }
        }

        internal async Task GoToNextPage()
        {
            if (StateService.CurrentPage != null && StateService.CurrentFilter.Page < StateService.CurrentPage.TotalPages)
            {
                await GoToPage(StateService.CurrentFilter.Page + 1);
            }
        }

        public void Dispose()
        {
            StateService.OnChange -= StateHasChanged;
        }
    }
}