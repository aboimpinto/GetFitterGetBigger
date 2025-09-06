using GetFitterGetBigger.Admin.Models.Dtos;

namespace GetFitterGetBigger.Admin.Builders
{
    /// <summary>
    /// Builder for creating ExerciseTypeDto instances.
    /// Useful for testing and data transformation scenarios.
    /// </summary>
    public class ExerciseTypeDtoBuilder
    {
        private string _id = Guid.NewGuid().ToString();
        private string _value = "Workout";
        private string _description = "Workout exercise type";

        public ExerciseTypeDtoBuilder WithId(string id)
        {
            _id = id;
            return this;
        }

        public ExerciseTypeDtoBuilder WithValue(string value)
        {
            _value = value;
            return this;
        }

        public ExerciseTypeDtoBuilder WithDescription(string description)
        {
            _description = description;
            return this;
        }

        public ExerciseTypeDtoBuilder AsWorkout()
        {
            _value = "Workout";
            _description = "Workout exercise type";
            return this;
        }

        public ExerciseTypeDtoBuilder AsWarmup()
        {
            _value = "Warmup";
            _description = "Warmup exercise type";
            return this;
        }

        public ExerciseTypeDtoBuilder AsCooldown()
        {
            _value = "Cooldown";
            _description = "Cooldown exercise type";
            return this;
        }

        public ExerciseTypeDto Build()
        {
            return new ExerciseTypeDto
            {
                Id = _id,
                Value = _value,
                Description = _description
            };
        }
    }
}