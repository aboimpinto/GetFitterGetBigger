using GetFitterGetBigger.Admin.Models.Dtos;

namespace GetFitterGetBigger.Admin.Builders
{
    /// <summary>
    /// Builder for creating ExerciseLinksResponseDto instances.
    /// Useful for testing and data transformation scenarios.
    /// </summary>
    public class ExerciseLinksResponseDtoBuilder
    {
        private string _exerciseId = Guid.NewGuid().ToString();
        private string _exerciseName = "Test Exercise";
        private List<ExerciseLinkDto> _links = new();
        private int _totalCount = 0;

        public ExerciseLinksResponseDtoBuilder WithExerciseId(string exerciseId)
        {
            _exerciseId = exerciseId;
            return this;
        }

        public ExerciseLinksResponseDtoBuilder WithExerciseName(string exerciseName)
        {
            _exerciseName = exerciseName;
            return this;
        }

        public ExerciseLinksResponseDtoBuilder WithLinks(params ExerciseLinkDto[] links)
        {
            _links = links.ToList();
            _totalCount = links.Length;
            return this;
        }

        public ExerciseLinksResponseDtoBuilder WithLinks(List<ExerciseLinkDto> links)
        {
            _links = links;
            _totalCount = links.Count;
            return this;
        }

        public ExerciseLinksResponseDtoBuilder WithWarmupLinks(params ExerciseLinkDto[] warmupLinks)
        {
            _links.AddRange(warmupLinks);
            _totalCount = _links.Count;
            return this;
        }

        public ExerciseLinksResponseDtoBuilder WithCooldownLinks(params ExerciseLinkDto[] cooldownLinks)
        {
            _links.AddRange(cooldownLinks);
            _totalCount = _links.Count;
            return this;
        }

        public ExerciseLinksResponseDtoBuilder WithTotalCount(int totalCount)
        {
            _totalCount = totalCount;
            return this;
        }

        public ExerciseLinksResponseDto Build()
        {
            return new ExerciseLinksResponseDto
            {
                ExerciseId = _exerciseId,
                ExerciseName = _exerciseName,
                Links = _links,
                TotalCount = _totalCount
            };
        }
    }
}