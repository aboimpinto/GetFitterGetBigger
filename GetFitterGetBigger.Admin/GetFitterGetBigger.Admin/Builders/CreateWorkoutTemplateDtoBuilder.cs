using GetFitterGetBigger.Admin.Models.Dtos;

namespace GetFitterGetBigger.Admin.Builders
{
    public class CreateWorkoutTemplateDtoBuilder
    {
        private CreateWorkoutTemplateDto _dto;

        public CreateWorkoutTemplateDtoBuilder()
        {
            _dto = new CreateWorkoutTemplateDto
            {
                Name = "Default Workout Template",
                Description = "A default workout template description",
                CategoryId = "workoutcategory-20000002-2000-4000-8000-200000000001",
                DifficultyId = "difficultylevel-9c7b59a4-bcd8-48a6-971a-cd67b0a7ab5a",
                EstimatedDurationMinutes = 60,
                IsPublic = true,
                Tags = new List<string> { "strength", "upper-body" }
            };
        }

        public CreateWorkoutTemplateDtoBuilder WithName(string name)
        {
            _dto.Name = name;
            return this;
        }

        public CreateWorkoutTemplateDtoBuilder WithDescription(string? description)
        {
            _dto.Description = description;
            return this;
        }

        public CreateWorkoutTemplateDtoBuilder WithCategoryId(string categoryId)
        {
            _dto.CategoryId = categoryId;
            return this;
        }

        public CreateWorkoutTemplateDtoBuilder WithDifficultyId(string difficultyId)
        {
            _dto.DifficultyId = difficultyId;
            return this;
        }

        public CreateWorkoutTemplateDtoBuilder WithEstimatedDuration(int minutes)
        {
            _dto.EstimatedDurationMinutes = minutes;
            return this;
        }

        public CreateWorkoutTemplateDtoBuilder WithIsPublic(bool isPublic)
        {
            _dto.IsPublic = isPublic;
            return this;
        }

        public CreateWorkoutTemplateDtoBuilder WithTags(params string[] tags)
        {
            _dto.Tags = tags.ToList();
            return this;
        }

        public CreateWorkoutTemplateDto Build()
        {
            return _dto;
        }
    }
}