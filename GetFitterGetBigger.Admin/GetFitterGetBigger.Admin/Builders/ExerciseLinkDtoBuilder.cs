using GetFitterGetBigger.Admin.Models.Dtos;

namespace GetFitterGetBigger.Admin.Builders
{
    /// <summary>
    /// Builder for creating ExerciseLinkDto instances.
    /// Useful for testing and data transformation scenarios.
    /// </summary>
    public class ExerciseLinkDtoBuilder
    {
        private string _id = Guid.NewGuid().ToString();
        private string _sourceExerciseId = Guid.NewGuid().ToString();
        private string _targetExerciseId = Guid.NewGuid().ToString();
        private string _targetExerciseName = "Target Exercise";
        private string _linkType = "Warmup";
        private int _displayOrder = 0;
        private bool _isActive = true;
        private DateTime _createdAt = DateTime.UtcNow;
        private DateTime _updatedAt = DateTime.UtcNow;
        private ExerciseDto? _targetExercise = null;

        public ExerciseLinkDtoBuilder WithId(string id)
        {
            _id = id;
            return this;
        }

        public ExerciseLinkDtoBuilder WithSourceExerciseId(string sourceExerciseId)
        {
            _sourceExerciseId = sourceExerciseId;
            return this;
        }

        public ExerciseLinkDtoBuilder WithTargetExerciseId(string targetExerciseId)
        {
            _targetExerciseId = targetExerciseId;
            return this;
        }

        public ExerciseLinkDtoBuilder WithTargetExerciseName(string targetExerciseName)
        {
            _targetExerciseName = targetExerciseName;
            return this;
        }

        public ExerciseLinkDtoBuilder WithLinkType(string linkType)
        {
            _linkType = linkType;
            return this;
        }

        public ExerciseLinkDtoBuilder AsWarmup()
        {
            _linkType = "Warmup";
            return this;
        }

        public ExerciseLinkDtoBuilder AsCooldown()
        {
            _linkType = "Cooldown";
            return this;
        }

        public ExerciseLinkDtoBuilder AsAlternative()
        {
            _linkType = "Alternative";
            _displayOrder = 0; // Alternative links don't use display order
            return this;
        }

        public ExerciseLinkDtoBuilder WithDisplayOrder(int displayOrder)
        {
            _displayOrder = displayOrder;
            return this;
        }

        public ExerciseLinkDtoBuilder WithDisplayOrder(int? displayOrder)
        {
            _displayOrder = displayOrder ?? 0;
            return this;
        }

        public ExerciseLinkDtoBuilder AsInactive()
        {
            _isActive = false;
            return this;
        }

        public ExerciseLinkDtoBuilder WithIsActive(bool isActive)
        {
            _isActive = isActive;
            return this;
        }

        public ExerciseLinkDtoBuilder WithCreatedAt(DateTime createdAt)
        {
            _createdAt = createdAt;
            return this;
        }

        public ExerciseLinkDtoBuilder WithUpdatedAt(DateTime updatedAt)
        {
            _updatedAt = updatedAt;
            return this;
        }

        public ExerciseLinkDtoBuilder WithTargetExercise(ExerciseDto targetExercise)
        {
            _targetExercise = targetExercise;
            return this;
        }

        public ExerciseLinkDtoBuilder WithTargetExercise(ExerciseListDto targetExercise)
        {
            // Convert ExerciseListDto to ExerciseDto for the link
            _targetExercise = new ExerciseDto
            {
                Id = targetExercise.Id,
                Name = targetExercise.Name,
                Difficulty = targetExercise.Difficulty,
                MuscleGroups = targetExercise.MuscleGroups?.Select(mg => new MuscleGroupWithRoleDto
                {
                    MuscleGroup = mg.MuscleGroup,
                    Role = mg.Role
                }).ToList() ?? new List<MuscleGroupWithRoleDto>(),
                Equipment = targetExercise.Equipment?.ToList() ?? new List<ReferenceDataDto>(),
                ExerciseTypes = targetExercise.ExerciseTypes?.ToList() ?? new List<ExerciseTypeDto>()
            };
            _targetExerciseName = targetExercise.Name;
            _targetExerciseId = targetExercise.Id;
            return this;
        }

        public ExerciseLinkDtoBuilder WithTargetExercise(string name, string id)
        {
            _targetExerciseName = name;
            _targetExerciseId = id;
            _targetExercise = new ExerciseDto
            {
                Id = id,
                Name = name,
                MuscleGroups = new List<MuscleGroupWithRoleDto>(),
                Equipment = new List<ReferenceDataDto>(),
                ExerciseTypes = new List<ExerciseTypeDto>()
            };
            return this;
        }

        public ExerciseLinkDto Build()
        {
            return new ExerciseLinkDto
            {
                Id = _id,
                SourceExerciseId = _sourceExerciseId,
                TargetExerciseId = _targetExerciseId,
                TargetExerciseName = _targetExerciseName,
                LinkType = _linkType,
                DisplayOrder = _displayOrder,
                IsActive = _isActive,
                CreatedAt = _createdAt,
                UpdatedAt = _updatedAt,
                TargetExercise = _targetExercise
            };
        }
    }
}