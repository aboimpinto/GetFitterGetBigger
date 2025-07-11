using GetFitterGetBigger.Admin.Models.Dtos;

namespace GetFitterGetBigger.Admin.Builders
{
    /// <summary>
    /// Builder for creating ExerciseWeightTypeDto instances.
    /// Useful for testing and creating weight type instances.
    /// </summary>
    public class ExerciseWeightTypeDtoBuilder
    {
        private Guid _id = Guid.NewGuid();
        private string _code = "WEIGHT_REQUIRED";
        private string _name = "Weight Required";
        private string _description = "Weight Required Description";
        private bool _isActive = true;
        private int _displayOrder = 1;

        public ExerciseWeightTypeDtoBuilder WithId(Guid id)
        {
            _id = id;
            return this;
        }

        public ExerciseWeightTypeDtoBuilder WithCode(string code)
        {
            _code = code;
            return this;
        }

        public ExerciseWeightTypeDtoBuilder WithName(string name)
        {
            _name = name;
            return this;
        }

        public ExerciseWeightTypeDtoBuilder WithDescription(string description)
        {
            _description = description;
            return this;
        }

        public ExerciseWeightTypeDtoBuilder AsInactive()
        {
            _isActive = false;
            return this;
        }

        public ExerciseWeightTypeDtoBuilder WithDisplayOrder(int order)
        {
            _displayOrder = order;
            return this;
        }

        public static ExerciseWeightTypeDtoBuilder BodyweightOnly()
        {
            return new ExerciseWeightTypeDtoBuilder()
                .WithCode("BODYWEIGHT_ONLY")
                .WithName("Bodyweight Only")
                .WithDescription("Exercise uses bodyweight only - no additional weight needed");
        }

        public static ExerciseWeightTypeDtoBuilder NoWeight()
        {
            return new ExerciseWeightTypeDtoBuilder()
                .WithCode("NO_WEIGHT")
                .WithName("No Weight")
                .WithDescription("Exercise requires no weight - typically stretches or mobility work");
        }

        public static ExerciseWeightTypeDtoBuilder BodyweightOptional()
        {
            return new ExerciseWeightTypeDtoBuilder()
                .WithCode("BODYWEIGHT_OPTIONAL")
                .WithName("Bodyweight Optional")
                .WithDescription("Exercise can be performed with bodyweight or additional weight");
        }

        public static ExerciseWeightTypeDtoBuilder WeightRequired()
        {
            return new ExerciseWeightTypeDtoBuilder()
                .WithCode("WEIGHT_REQUIRED")
                .WithName("Weight Required")
                .WithDescription("Exercise requires external weight (dumbbells, barbells, etc.)");
        }

        public static ExerciseWeightTypeDtoBuilder MachineWeight()
        {
            return new ExerciseWeightTypeDtoBuilder()
                .WithCode("MACHINE_WEIGHT")
                .WithName("Machine Weight")
                .WithDescription("Exercise uses machine weight stack or setting");
        }

        public ExerciseWeightTypeDto Build()
        {
            return new ExerciseWeightTypeDto
            {
                Id = _id,
                Code = _code,
                Name = _name,
                Description = _description,
                IsActive = _isActive,
                DisplayOrder = _displayOrder
            };
        }
    }
}