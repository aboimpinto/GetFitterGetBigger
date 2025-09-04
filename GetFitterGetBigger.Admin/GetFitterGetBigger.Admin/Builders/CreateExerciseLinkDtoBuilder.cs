using GetFitterGetBigger.Admin.Models.Dtos;

namespace GetFitterGetBigger.Admin.Builders
{
    /// <summary>
    /// Builder for creating CreateExerciseLinkDto instances.
    /// Useful for testing and data transformation scenarios.
    /// </summary>
    public class CreateExerciseLinkDtoBuilder
    {
        private string _targetExerciseId = Guid.NewGuid().ToString();
        private string _linkType = "Warmup";
        private int? _displayOrder = 1;

        public CreateExerciseLinkDtoBuilder WithTargetExerciseId(string targetExerciseId)
        {
            _targetExerciseId = targetExerciseId;
            return this;
        }

        public CreateExerciseLinkDtoBuilder WithLinkType(string linkType)
        {
            _linkType = linkType;
            return this;
        }

        public CreateExerciseLinkDtoBuilder AsWarmup()
        {
            _linkType = "Warmup";
            return this;
        }

        public CreateExerciseLinkDtoBuilder AsCooldown()
        {
            _linkType = "Cooldown";
            return this;
        }

        public CreateExerciseLinkDtoBuilder AsAlternative()
        {
            _linkType = "Alternative";
            _displayOrder = null; // Alternative links don't need display order
            return this;
        }

        public CreateExerciseLinkDtoBuilder WithDisplayOrder(int? displayOrder)
        {
            _displayOrder = displayOrder;
            return this;
        }

        public CreateExerciseLinkDto Build()
        {
            return new CreateExerciseLinkDto
            {
                TargetExerciseId = _targetExerciseId,
                LinkType = _linkType,
                DisplayOrder = _displayOrder
            };
        }
    }
}