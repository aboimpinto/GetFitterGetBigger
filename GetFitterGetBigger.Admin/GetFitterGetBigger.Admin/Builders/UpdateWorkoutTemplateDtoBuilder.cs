using GetFitterGetBigger.Admin.Models.Dtos;

namespace GetFitterGetBigger.Admin.Builders
{
    public class UpdateWorkoutTemplateDtoBuilder
    {
        private string _name = "Test Workout Template";
        private string? _description = "Test Description";
        private string _categoryId = "category-1";
        private string _difficultyId = "difficulty-1";
        private int _estimatedDurationMinutes = 30;
        private bool _isPublic = false;
        private List<string> _tags = new();

        public UpdateWorkoutTemplateDtoBuilder WithName(string name)
        {
            _name = name;
            return this;
        }

        public UpdateWorkoutTemplateDtoBuilder WithDescription(string? description)
        {
            _description = description;
            return this;
        }

        public UpdateWorkoutTemplateDtoBuilder WithEstimatedDurationMinutes(int minutes)
        {
            _estimatedDurationMinutes = minutes;
            return this;
        }

        public UpdateWorkoutTemplateDtoBuilder WithCategoryId(string categoryId)
        {
            _categoryId = categoryId;
            return this;
        }

        public UpdateWorkoutTemplateDtoBuilder WithDifficultyId(string difficultyId)
        {
            _difficultyId = difficultyId;
            return this;
        }

        public UpdateWorkoutTemplateDtoBuilder WithTags(params string[] tags)
        {
            _tags = tags.ToList();
            return this;
        }

        public UpdateWorkoutTemplateDtoBuilder WithIsPublic(bool isPublic)
        {
            _isPublic = isPublic;
            return this;
        }


        public UpdateWorkoutTemplateDto Build()
        {
            return new UpdateWorkoutTemplateDto
            {
                Name = _name,
                Description = _description,
                CategoryId = _categoryId,
                DifficultyId = _difficultyId,
                EstimatedDurationMinutes = _estimatedDurationMinutes,
                IsPublic = _isPublic,
                Tags = _tags
            };
        }
    }
}