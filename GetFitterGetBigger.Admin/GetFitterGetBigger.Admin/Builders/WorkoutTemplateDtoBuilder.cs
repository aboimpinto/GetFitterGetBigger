using GetFitterGetBigger.Admin.Models.Dtos;

namespace GetFitterGetBigger.Admin.Builders
{
    public class WorkoutTemplateDtoBuilder
    {
        private WorkoutTemplateDto _dto;

        public WorkoutTemplateDtoBuilder()
        {
            _dto = new WorkoutTemplateDto
            {
                Id = $"workouttemplate-{Guid.NewGuid()}",
                Name = "Default Workout Template",
                Description = "A default workout template description",
                Category = new ReferenceDataDto 
                { 
                    Id = "workoutcategory-20000002-2000-4000-8000-200000000001",
                    Value = "Upper Body - Push",
                    Description = "Push exercises targeting chest, shoulders, triceps"
                },
                Difficulty = new ReferenceDataDto
                {
                    Id = "difficultylevel-9c7b59a4-bcd8-48a6-971a-cd67b0a7ab5a",
                    Value = "Intermediate",
                    Description = "Suitable for those with some fitness experience"
                },
                WorkoutState = new ReferenceDataDto
                {
                    Id = "workoutstate-02000001-0000-0000-0000-000000000001",
                    Value = "DRAFT",
                    Description = "Template under construction"
                },
                EstimatedDurationMinutes = 60,
                IsPublic = true,
                Tags = new List<string> { "strength", "upper-body" },
                Exercises = new List<WorkoutTemplateExerciseDto>(),
                Objectives = new List<ReferenceDataDto>(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }

        public WorkoutTemplateDtoBuilder WithId(string id)
        {
            _dto.Id = id;
            return this;
        }

        public WorkoutTemplateDtoBuilder WithName(string name)
        {
            _dto.Name = name;
            return this;
        }

        public WorkoutTemplateDtoBuilder WithDescription(string? description)
        {
            _dto.Description = description;
            return this;
        }

        public WorkoutTemplateDtoBuilder WithCategory(string id, string value, string? description = null)
        {
            _dto.Category = new ReferenceDataDto
            {
                Id = id,
                Value = value,
                Description = description
            };
            return this;
        }

        public WorkoutTemplateDtoBuilder WithDifficulty(string id, string value, string? description = null)
        {
            _dto.Difficulty = new ReferenceDataDto
            {
                Id = id,
                Value = value,
                Description = description
            };
            return this;
        }

        public WorkoutTemplateDtoBuilder WithWorkoutState(string id, string value, string? description = null)
        {
            _dto.WorkoutState = new ReferenceDataDto
            {
                Id = id,
                Value = value,
                Description = description
            };
            return this;
        }

        public WorkoutTemplateDtoBuilder WithEstimatedDuration(int minutes)
        {
            _dto.EstimatedDurationMinutes = minutes;
            return this;
        }

        public WorkoutTemplateDtoBuilder WithIsPublic(bool isPublic)
        {
            _dto.IsPublic = isPublic;
            return this;
        }

        public WorkoutTemplateDtoBuilder WithTags(params string[] tags)
        {
            _dto.Tags = tags.ToList();
            return this;
        }

        public WorkoutTemplateDtoBuilder WithExercises(params WorkoutTemplateExerciseDto[] exercises)
        {
            _dto.Exercises = exercises.ToList();
            return this;
        }

        public WorkoutTemplateDtoBuilder WithObjectives(params ReferenceDataDto[] objectives)
        {
            _dto.Objectives = objectives.ToList();
            return this;
        }

        public WorkoutTemplateDtoBuilder WithCreatedAt(DateTime createdAt)
        {
            _dto.CreatedAt = createdAt;
            return this;
        }

        public WorkoutTemplateDtoBuilder WithUpdatedAt(DateTime updatedAt)
        {
            _dto.UpdatedAt = updatedAt;
            return this;
        }

        public WorkoutTemplateDto Build()
        {
            return _dto;
        }
    }
}