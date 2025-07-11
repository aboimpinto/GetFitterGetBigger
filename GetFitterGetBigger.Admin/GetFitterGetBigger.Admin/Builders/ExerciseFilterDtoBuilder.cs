using GetFitterGetBigger.Admin.Models.Dtos;

namespace GetFitterGetBigger.Admin.Builders
{
    /// <summary>
    /// Builder for creating ExerciseFilterDto instances.
    /// Provides a fluent interface for constructing exercise filter parameters.
    /// </summary>
    public class ExerciseFilterDtoBuilder
    {
        private string? _name = null;
        private string? _difficultyId = null;
        private List<string>? _muscleGroupIds = null;
        private List<string>? _equipmentIds = null;
        private List<string>? _weightTypeIds = null;
        private bool? _isActive = null;
        private int _page = 1;
        private int _pageSize = 10;

        public ExerciseFilterDtoBuilder WithName(string name)
        {
            _name = name;
            return this;
        }

        public ExerciseFilterDtoBuilder WithDifficultyId(string difficultyId)
        {
            _difficultyId = difficultyId;
            return this;
        }

        public ExerciseFilterDtoBuilder WithMuscleGroupIds(params string[] muscleGroupIds)
        {
            _muscleGroupIds = muscleGroupIds.ToList();
            return this;
        }

        public ExerciseFilterDtoBuilder WithMuscleGroupIds(List<string> muscleGroupIds)
        {
            _muscleGroupIds = muscleGroupIds;
            return this;
        }

        public ExerciseFilterDtoBuilder WithEquipmentIds(params string[] equipmentIds)
        {
            _equipmentIds = equipmentIds.ToList();
            return this;
        }

        public ExerciseFilterDtoBuilder WithEquipmentIds(List<string> equipmentIds)
        {
            _equipmentIds = equipmentIds;
            return this;
        }

        public ExerciseFilterDtoBuilder WithWeightTypeIds(params string[] weightTypeIds)
        {
            _weightTypeIds = weightTypeIds.ToList();
            return this;
        }

        public ExerciseFilterDtoBuilder WithWeightTypeIds(List<string> weightTypeIds)
        {
            _weightTypeIds = weightTypeIds;
            return this;
        }

        public ExerciseFilterDtoBuilder WithIsActive(bool isActive)
        {
            _isActive = isActive;
            return this;
        }

        public ExerciseFilterDtoBuilder WithPage(int page)
        {
            _page = page;
            return this;
        }

        public ExerciseFilterDtoBuilder WithPageSize(int pageSize)
        {
            _pageSize = pageSize;
            return this;
        }

        public ExerciseFilterDtoBuilder WithPagination(int page, int pageSize)
        {
            _page = page;
            _pageSize = pageSize;
            return this;
        }

        public ExerciseFilterDto Build()
        {
            return new ExerciseFilterDto
            {
                Name = _name,
                DifficultyId = _difficultyId,
                MuscleGroupIds = _muscleGroupIds,
                EquipmentIds = _equipmentIds,
                WeightTypeIds = _weightTypeIds,
                IsActive = _isActive,
                Page = _page,
                PageSize = _pageSize
            };
        }

        /// <summary>
        /// Creates a default filter with no criteria.
        /// </summary>
        public static ExerciseFilterDto Default()
        {
            return new ExerciseFilterDtoBuilder().Build();
        }

        /// <summary>
        /// Creates a filter for active exercises only.
        /// </summary>
        public static ExerciseFilterDto ActiveOnly()
        {
            return new ExerciseFilterDtoBuilder()
                .WithIsActive(true)
                .Build();
        }
    }
}