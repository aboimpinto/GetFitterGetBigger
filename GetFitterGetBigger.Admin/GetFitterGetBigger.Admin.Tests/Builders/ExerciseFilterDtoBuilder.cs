using GetFitterGetBigger.Admin.Models.Dtos;

namespace GetFitterGetBigger.Admin.Tests.Builders
{
    public class ExerciseFilterDtoBuilder
    {
        private string? _name = null;
        private string? _difficultyId = null;
        private List<string>? _muscleGroupIds = null;
        private List<string>? _equipmentIds = null;
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

        public ExerciseFilterDtoBuilder WithEquipmentIds(params string[] equipmentIds)
        {
            _equipmentIds = equipmentIds.ToList();
            return this;
        }

        public ExerciseFilterDtoBuilder WithIsActive(bool isActive)
        {
            _isActive = isActive;
            return this;
        }

        public ExerciseFilterDtoBuilder OnlyActive()
        {
            _isActive = true;
            return this;
        }

        public ExerciseFilterDtoBuilder OnlyInactive()
        {
            _isActive = false;
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
                IsActive = _isActive,
                Page = _page,
                PageSize = _pageSize
            };
        }

        public static ExerciseFilterDto BuildDefault()
        {
            return new ExerciseFilterDtoBuilder().Build();
        }

        public static ExerciseFilterDto BuildForSearch(string searchTerm)
        {
            return new ExerciseFilterDtoBuilder()
                .WithName(searchTerm)
                .Build();
        }
    }
}