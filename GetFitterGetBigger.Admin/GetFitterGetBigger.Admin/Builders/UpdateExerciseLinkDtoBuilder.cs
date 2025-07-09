using GetFitterGetBigger.Admin.Models.Dtos;

namespace GetFitterGetBigger.Admin.Builders
{
    /// <summary>
    /// Builder for creating UpdateExerciseLinkDto instances.
    /// Useful for testing and data transformation scenarios.
    /// </summary>
    public class UpdateExerciseLinkDtoBuilder
    {
        private int _displayOrder = 0;
        private bool _isActive = true;

        public UpdateExerciseLinkDtoBuilder WithDisplayOrder(int displayOrder)
        {
            _displayOrder = displayOrder;
            return this;
        }

        public UpdateExerciseLinkDtoBuilder AsInactive()
        {
            _isActive = false;
            return this;
        }

        public UpdateExerciseLinkDtoBuilder AsActive()
        {
            _isActive = true;
            return this;
        }

        public UpdateExerciseLinkDto Build()
        {
            return new UpdateExerciseLinkDto
            {
                DisplayOrder = _displayOrder,
                IsActive = _isActive
            };
        }
    }
}