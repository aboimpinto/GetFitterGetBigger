using GetFitterGetBigger.Admin.Models.Dtos;
using GetFitterGetBigger.Admin.Services.Stores;
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
            ListStore.OnChange += StateHasChanged;
        }

        internal IEnumerable<WorkoutTemplateDto> GetSortedTemplates()
        {
            if (ListStore.CurrentPage?.Items == null)
                return Enumerable.Empty<WorkoutTemplateDto>();

            var templates = ListStore.CurrentPage.Items.AsEnumerable();

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
            var filter = ListStore.CurrentFilter;
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
                
                ListStore.ClearError();
                await ListStore.RefreshAsync();
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

        internal async Task HandleTemplateStateChange(WorkoutTemplateDto template, ReferenceDataDto newState)
        {
            var changeStateDto = new ChangeWorkoutStateDto
            {
                WorkoutStateId = newState.Id
            };

            await ListStore.ChangeTemplateStateAsync(template.Id, changeStateDto);
        }

        internal async Task GoToPage(int page)
        {
            var filter = ListStore.CurrentFilter;
            filter.Page = page;
            await ListStore.LoadTemplatesAsync(filter);
        }

        internal async Task GoToPreviousPage()
        {
            if (ListStore.CurrentFilter.Page > 1)
            {
                await GoToPage(ListStore.CurrentFilter.Page - 1);
            }
        }

        internal async Task GoToNextPage()
        {
            if (ListStore.CurrentPage != null && ListStore.CurrentFilter.Page < ListStore.CurrentPage.TotalPages)
            {
                await GoToPage(ListStore.CurrentFilter.Page + 1);
            }
        }

        public void Dispose()
        {
            ListStore.OnChange -= StateHasChanged;
        }
    }
}