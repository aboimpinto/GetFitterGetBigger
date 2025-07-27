using GetFitterGetBigger.Admin.Models.Dtos;

namespace GetFitterGetBigger.Admin.Builders
{
    /// <summary>
    /// Builder class for creating UpdateWorkoutTemplateDto instances with fluent API support
    /// </summary>
    public class UpdateWorkoutTemplateDtoBuilder
    {
        private string _name = "Test Workout Template";
        private string? _description = "Test Description";
        private string _categoryId = "category-1";
        private string _difficultyId = "difficulty-1";
        private int _estimatedDurationMinutes = 30;
        private bool _isPublic = false;
        private List<string> _tags = new();

        /// <summary>
        /// Sets the name of the workout template
        /// </summary>
        /// <param name="name">The workout template name</param>
        /// <returns>The builder instance for method chaining</returns>
        public UpdateWorkoutTemplateDtoBuilder WithName(string name)
        {
            _name = name;
            return this;
        }

        /// <summary>
        /// Sets the description of the workout template
        /// </summary>
        /// <param name="description">The workout template description</param>
        /// <returns>The builder instance for method chaining</returns>
        public UpdateWorkoutTemplateDtoBuilder WithDescription(string? description)
        {
            _description = description;
            return this;
        }

        /// <summary>
        /// Sets the estimated duration of the workout in minutes
        /// </summary>
        /// <param name="minutes">The duration in minutes</param>
        /// <returns>The builder instance for method chaining</returns>
        public UpdateWorkoutTemplateDtoBuilder WithEstimatedDurationMinutes(int minutes)
        {
            _estimatedDurationMinutes = minutes;
            return this;
        }

        /// <summary>
        /// Sets the category identifier for the workout template
        /// </summary>
        /// <param name="categoryId">The category identifier</param>
        /// <returns>The builder instance for method chaining</returns>
        public UpdateWorkoutTemplateDtoBuilder WithCategoryId(string categoryId)
        {
            _categoryId = categoryId;
            return this;
        }

        /// <summary>
        /// Sets the difficulty level identifier for the workout template
        /// </summary>
        /// <param name="difficultyId">The difficulty level identifier</param>
        /// <returns>The builder instance for method chaining</returns>
        public UpdateWorkoutTemplateDtoBuilder WithDifficultyId(string difficultyId)
        {
            _difficultyId = difficultyId;
            return this;
        }

        /// <summary>
        /// Sets the tags associated with the workout template
        /// </summary>
        /// <param name="tags">Array of tags to associate with the template</param>
        /// <returns>The builder instance for method chaining</returns>
        public UpdateWorkoutTemplateDtoBuilder WithTags(params string[] tags)
        {
            _tags = tags.ToList();
            return this;
        }

        /// <summary>
        /// Sets whether the workout template is publicly accessible
        /// </summary>
        /// <param name="isPublic">True if the template is public, false otherwise</param>
        /// <returns>The builder instance for method chaining</returns>
        public UpdateWorkoutTemplateDtoBuilder WithIsPublic(bool isPublic)
        {
            _isPublic = isPublic;
            return this;
        }

        /// <summary>
        /// Builds and returns the configured UpdateWorkoutTemplateDto instance
        /// </summary>
        /// <returns>A new UpdateWorkoutTemplateDto with the configured values</returns>
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