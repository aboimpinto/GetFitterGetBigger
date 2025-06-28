using GetFitterGetBigger.Admin.Models.Dtos;

namespace GetFitterGetBigger.Admin.Builders
{
    /// <summary>
    /// Builder for creating ExercisePagedResultDto instances.
    /// Useful for testing and creating paginated exercise results.
    /// </summary>
    public class ExercisePagedResultDtoBuilder
    {
        private List<ExerciseListDto> _items = new();
        private int _totalCount = 0;
        private int _pageNumber = 1;
        private int _pageSize = 10;

        public ExercisePagedResultDtoBuilder WithItems(List<ExerciseListDto> items)
        {
            _items = items;
            _totalCount = items.Count;
            return this;
        }

        public ExercisePagedResultDtoBuilder WithItems(params ExerciseListDto[] items)
        {
            _items = items.ToList();
            _totalCount = items.Length;
            return this;
        }

        public ExercisePagedResultDtoBuilder AddItem(ExerciseListDto item)
        {
            _items.Add(item);
            _totalCount = _items.Count;
            return this;
        }

        public ExercisePagedResultDtoBuilder WithEmptyResult()
        {
            _items = new List<ExerciseListDto>();
            _totalCount = 0;
            return this;
        }

        public ExercisePagedResultDtoBuilder WithTotalCount(int totalCount)
        {
            _totalCount = totalCount;
            return this;
        }

        public ExercisePagedResultDtoBuilder WithPageNumber(int pageNumber)
        {
            _pageNumber = pageNumber;
            return this;
        }

        public ExercisePagedResultDtoBuilder WithPageSize(int pageSize)
        {
            _pageSize = pageSize;
            return this;
        }

        public ExercisePagedResultDtoBuilder WithPagination(int pageNumber, int pageSize, int totalCount)
        {
            _pageNumber = pageNumber;
            _pageSize = pageSize;
            _totalCount = totalCount;
            return this;
        }

        public ExercisePagedResultDto Build()
        {
            return new ExercisePagedResultDto
            {
                Items = _items,
                TotalCount = _totalCount,
                PageNumber = _pageNumber,
                PageSize = _pageSize
            };
        }

        public static ExercisePagedResultDto BuildWithExercises(int count, int pageNumber = 1, int pageSize = 10)
        {
            var items = ExerciseListDtoBuilder.BuildList(count);
            return new ExercisePagedResultDtoBuilder()
                .WithItems(items)
                .WithPageNumber(pageNumber)
                .WithPageSize(pageSize)
                .WithTotalCount(count)
                .Build();
        }
    }
}