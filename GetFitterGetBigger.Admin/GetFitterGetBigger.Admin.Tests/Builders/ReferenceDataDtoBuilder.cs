using GetFitterGetBigger.Admin.Models.Dtos;

namespace GetFitterGetBigger.Admin.Tests.Builders
{
    public class ReferenceDataDtoBuilder
    {
        private string _id = "ref-1";
        private string _value = "Test Reference";
        private string _description = "Test Description";

        public ReferenceDataDtoBuilder WithId(string id)
        {
            _id = id;
            return this;
        }

        public ReferenceDataDtoBuilder WithValue(string value)
        {
            _value = value;
            return this;
        }

        public ReferenceDataDtoBuilder WithDescription(string description)
        {
            _description = description;
            return this;
        }

        public ReferenceDataDto Build()
        {
            return new ReferenceDataDto
            {
                Id = _id,
                Value = _value,
                Description = _description
            };
        }

        public static IEnumerable<ReferenceDataDto> BuildList(int count)
        {
            var list = new List<ReferenceDataDto>();
            for (int i = 1; i <= count; i++)
            {
                list.Add(new ReferenceDataDtoBuilder()
                    .WithId($"ref-{i}")
                    .WithValue($"Reference {i}")
                    .WithDescription($"Description for reference {i}")
                    .Build());
            }
            return list;
        }
    }
}