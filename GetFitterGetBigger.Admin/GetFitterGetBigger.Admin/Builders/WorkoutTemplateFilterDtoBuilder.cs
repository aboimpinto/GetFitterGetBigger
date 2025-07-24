using GetFitterGetBigger.Admin.Models.Dtos;

namespace GetFitterGetBigger.Admin.Builders
{
    public class WorkoutTemplateFilterDtoBuilder
    {
        private WorkoutTemplateFilterDto _dto;

        public WorkoutTemplateFilterDtoBuilder()
        {
            _dto = new WorkoutTemplateFilterDto
            {
                Page = 1,
                PageSize = 10,
                NamePattern = null,
                CategoryId = null,
                DifficultyId = null,
                StateId = null,
                IsPublic = null
            };
        }

        public WorkoutTemplateFilterDtoBuilder WithPage(int page)
        {
            _dto.Page = page;
            return this;
        }

        public WorkoutTemplateFilterDtoBuilder WithPageSize(int pageSize)
        {
            _dto.PageSize = pageSize;
            return this;
        }

        public WorkoutTemplateFilterDtoBuilder WithNamePattern(string? namePattern)
        {
            _dto.NamePattern = namePattern;
            return this;
        }

        public WorkoutTemplateFilterDtoBuilder WithCategoryId(string? categoryId)
        {
            _dto.CategoryId = categoryId;
            return this;
        }

        public WorkoutTemplateFilterDtoBuilder WithDifficultyId(string? difficultyId)
        {
            _dto.DifficultyId = difficultyId;
            return this;
        }

        public WorkoutTemplateFilterDtoBuilder WithStateId(string? stateId)
        {
            _dto.StateId = stateId;
            return this;
        }

        public WorkoutTemplateFilterDtoBuilder WithIsPublic(bool? isPublic)
        {
            _dto.IsPublic = isPublic;
            return this;
        }

        public WorkoutTemplateFilterDto Build()
        {
            return _dto;
        }
    }
}