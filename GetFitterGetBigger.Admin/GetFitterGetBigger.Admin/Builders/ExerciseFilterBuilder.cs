using GetFitterGetBigger.Admin.Models.Dtos;

namespace GetFitterGetBigger.Admin.Builders
{
    /// <summary>
    /// Builder for creating ExerciseFilterDto instances in production code.
    /// Provides a fluent interface for constructing exercise filters.
    /// </summary>
    public class ExerciseFilterBuilder
    {
        private readonly ExerciseFilterDto _filter;

        public ExerciseFilterBuilder()
        {
            _filter = new ExerciseFilterDto
            {
                Page = 1,
                PageSize = 10
            };
        }

        public ExerciseFilterBuilder WithName(string? name)
        {
            _filter.Name = name;
            return this;
        }

        public ExerciseFilterBuilder WithDifficulty(string? difficultyId)
        {
            _filter.DifficultyId = difficultyId;
            return this;
        }

        public ExerciseFilterBuilder WithMuscleGroups(IEnumerable<string>? muscleGroupIds)
        {
            _filter.MuscleGroupIds = muscleGroupIds?.ToList();
            return this;
        }

        public ExerciseFilterBuilder WithEquipment(IEnumerable<string>? equipmentIds)
        {
            _filter.EquipmentIds = equipmentIds?.ToList();
            return this;
        }

        public ExerciseFilterBuilder WithActiveStatus(bool? isActive)
        {
            _filter.IsActive = isActive;
            return this;
        }

        public ExerciseFilterBuilder OnlyActive()
        {
            _filter.IsActive = true;
            return this;
        }

        public ExerciseFilterBuilder WithPagination(int page, int pageSize)
        {
            _filter.Page = page;
            _filter.PageSize = pageSize;
            return this;
        }

        public ExerciseFilterDto Build()
        {
            return _filter;
        }

        /// <summary>
        /// Creates a default filter with standard pagination settings.
        /// </summary>
        public static ExerciseFilterDto Default()
        {
            return new ExerciseFilterBuilder().Build();
        }

        /// <summary>
        /// Creates a filter for searching by name.
        /// </summary>
        public static ExerciseFilterDto ForSearch(string searchTerm, int page = 1, int pageSize = 10)
        {
            return new ExerciseFilterBuilder()
                .WithName(searchTerm)
                .WithPagination(page, pageSize)
                .Build();
        }

        /// <summary>
        /// Creates a filter from URL query parameters typically used in Blazor components.
        /// </summary>
        public static ExerciseFilterDto FromQueryParameters(
            string? name = null,
            string? difficultyId = null,
            List<string>? muscleGroupIds = null,
            List<string>? equipmentIds = null,
            bool? isActive = null,
            int page = 1,
            int pageSize = 10)
        {
            return new ExerciseFilterBuilder()
                .WithName(name)
                .WithDifficulty(difficultyId)
                .WithMuscleGroups(muscleGroupIds)
                .WithEquipment(equipmentIds)
                .WithActiveStatus(isActive)
                .WithPagination(page, pageSize)
                .Build();
        }

        /// <summary>
        /// Creates a deep copy of an existing ExerciseFilterDto.
        /// </summary>
        public static ExerciseFilterDto CopyFrom(ExerciseFilterDto source)
        {
            return new ExerciseFilterBuilder()
                .WithName(source.Name)
                .WithDifficulty(source.DifficultyId)
                .WithMuscleGroups(source.MuscleGroupIds?.ToList())
                .WithEquipment(source.EquipmentIds?.ToList())
                .WithActiveStatus(source.IsActive)
                .WithPagination(source.Page, source.PageSize)
                .Build();
        }
    }
}