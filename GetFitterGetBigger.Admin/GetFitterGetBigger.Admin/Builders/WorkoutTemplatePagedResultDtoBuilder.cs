using GetFitterGetBigger.Admin.Models.Dtos;

namespace GetFitterGetBigger.Admin.Builders
{
    public class WorkoutTemplatePagedResultDtoBuilder
    {
        private WorkoutTemplatePagedResultDto _dto;

        public WorkoutTemplatePagedResultDtoBuilder()
        {
            _dto = new WorkoutTemplatePagedResultDto
            {
                Items = new List<WorkoutTemplateDto>(),
                TotalCount = 0,
                PageNumber = 1,
                PageSize = 10,
                TotalPages = 1,
                HasPreviousPage = false,
                HasNextPage = false
            };
        }

        public WorkoutTemplatePagedResultDtoBuilder WithItems(params WorkoutTemplateDto[] items)
        {
            _dto.Items = items.ToList();
            _dto.TotalCount = items.Length;
            _dto.TotalPages = (int)Math.Ceiling(_dto.TotalCount / (double)_dto.PageSize);
            _dto.HasNextPage = _dto.PageNumber < _dto.TotalPages;
            _dto.HasPreviousPage = _dto.PageNumber > 1;
            return this;
        }

        public WorkoutTemplatePagedResultDtoBuilder WithPageNumber(int pageNumber)
        {
            _dto.PageNumber = pageNumber;
            _dto.HasPreviousPage = pageNumber > 1;
            _dto.HasNextPage = pageNumber < _dto.TotalPages;
            return this;
        }

        public WorkoutTemplatePagedResultDtoBuilder WithPageSize(int pageSize)
        {
            _dto.PageSize = pageSize;
            _dto.TotalPages = (int)Math.Ceiling(_dto.TotalCount / (double)pageSize);
            _dto.HasNextPage = _dto.PageNumber < _dto.TotalPages;
            return this;
        }

        public WorkoutTemplatePagedResultDtoBuilder WithTotalCount(int totalCount)
        {
            _dto.TotalCount = totalCount;
            _dto.TotalPages = (int)Math.Ceiling(totalCount / (double)_dto.PageSize);
            _dto.HasNextPage = _dto.PageNumber < _dto.TotalPages;
            return this;
        }

        public WorkoutTemplatePagedResultDto Build()
        {
            return _dto;
        }
    }
}