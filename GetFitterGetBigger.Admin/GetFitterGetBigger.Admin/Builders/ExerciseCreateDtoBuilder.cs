using GetFitterGetBigger.Admin.Models.Dtos;

namespace GetFitterGetBigger.Admin.Builders
{
    /// <summary>
    /// Builder for creating ExerciseCreateDto instances in production code.
    /// Provides a fluent interface for constructing exercise creation DTOs.
    /// </summary>
    public class ExerciseCreateDtoBuilder
    {
        private string _name = string.Empty;
        private string _description = string.Empty;
        private string _instructions = string.Empty;
        private List<CoachNoteCreateDto> _coachNotes = new();
        private List<string> _exerciseTypeIds = new();
        private string _difficultyId = string.Empty;
        private string? _kineticChainId = null;
        private string? _weightTypeId = null;
        private decimal? _defaultWeight = null;
        private bool _isUnilateral = false;
        private bool _isActive = true;
        private string? _imageUrl = null;
        private string? _videoUrl = null;
        private List<MuscleGroupApiDto> _muscleGroups = new();
        private List<string> _equipmentIds = new();
        private List<string> _bodyPartIds = new();
        private List<string> _movementPatternIds = new();

        public ExerciseCreateDtoBuilder WithName(string name)
        {
            _name = name;
            return this;
        }

        public ExerciseCreateDtoBuilder WithDescription(string description)
        {
            _description = description;
            return this;
        }

        public ExerciseCreateDtoBuilder WithInstructions(string instructions)
        {
            _instructions = instructions;
            return this;
        }

        public ExerciseCreateDtoBuilder WithCoachNotes(params (string text, int order)[] notes)
        {
            _coachNotes = notes.Select(n => new CoachNoteCreateDto
            {
                Text = n.text,
                Order = n.order
            }).ToList();
            return this;
        }

        public ExerciseCreateDtoBuilder WithCoachNotes(List<CoachNoteCreateDto> coachNotes)
        {
            _coachNotes = coachNotes;
            return this;
        }

        public ExerciseCreateDtoBuilder AddCoachNote(string text, int order)
        {
            _coachNotes.Add(new CoachNoteCreateDto
            {
                Text = text,
                Order = order
            });
            return this;
        }

        public ExerciseCreateDtoBuilder WithExerciseTypes(params string[] typeIds)
        {
            _exerciseTypeIds = typeIds.ToList();
            return this;
        }

        public ExerciseCreateDtoBuilder WithExerciseTypes(List<string> typeIds)
        {
            _exerciseTypeIds = typeIds;
            return this;
        }

        public ExerciseCreateDtoBuilder WithDifficultyId(string difficultyId)
        {
            _difficultyId = difficultyId;
            return this;
        }

        public ExerciseCreateDtoBuilder WithKineticChainId(string? kineticChainId)
        {
            _kineticChainId = kineticChainId;
            return this;
        }

        public ExerciseCreateDtoBuilder WithWeightTypeId(string? weightTypeId)
        {
            _weightTypeId = weightTypeId;
            return this;
        }

        public ExerciseCreateDtoBuilder WithDefaultWeight(decimal? defaultWeight)
        {
            _defaultWeight = defaultWeight;
            return this;
        }

        public ExerciseCreateDtoBuilder AsUnilateral()
        {
            _isUnilateral = true;
            return this;
        }

        public ExerciseCreateDtoBuilder AsInactive()
        {
            _isActive = false;
            return this;
        }

        public ExerciseCreateDtoBuilder WithIsUnilateral(bool isUnilateral)
        {
            _isUnilateral = isUnilateral;
            return this;
        }

        public ExerciseCreateDtoBuilder WithIsActive(bool isActive)
        {
            _isActive = isActive;
            return this;
        }

        public ExerciseCreateDtoBuilder WithImageUrl(string? imageUrl)
        {
            _imageUrl = imageUrl;
            return this;
        }

        public ExerciseCreateDtoBuilder WithVideoUrl(string? videoUrl)
        {
            _videoUrl = videoUrl;
            return this;
        }

        public ExerciseCreateDtoBuilder WithMuscleGroups(params (string muscleGroupId, string roleId)[] muscleGroups)
        {
            _muscleGroups = muscleGroups.Select(mg => new MuscleGroupApiDto
            {
                MuscleGroupId = mg.muscleGroupId,
                MuscleRoleId = mg.roleId
            }).ToList();
            return this;
        }

        public ExerciseCreateDtoBuilder WithMuscleGroups(List<MuscleGroupApiDto> muscleGroups)
        {
            _muscleGroups = muscleGroups;
            return this;
        }

        public ExerciseCreateDtoBuilder AddMuscleGroup(string muscleGroupId, string roleId)
        {
            _muscleGroups.Add(new MuscleGroupApiDto
            {
                MuscleGroupId = muscleGroupId,
                MuscleRoleId = roleId
            });
            return this;
        }

        public ExerciseCreateDtoBuilder WithEquipment(params string[] equipmentIds)
        {
            _equipmentIds = equipmentIds.ToList();
            return this;
        }

        public ExerciseCreateDtoBuilder WithEquipment(List<string> equipmentIds)
        {
            _equipmentIds = equipmentIds;
            return this;
        }

        public ExerciseCreateDtoBuilder WithBodyParts(params string[] bodyPartIds)
        {
            _bodyPartIds = bodyPartIds.ToList();
            return this;
        }

        public ExerciseCreateDtoBuilder WithBodyParts(List<string> bodyPartIds)
        {
            _bodyPartIds = bodyPartIds;
            return this;
        }

        public ExerciseCreateDtoBuilder WithMovementPatterns(params string[] movementPatternIds)
        {
            _movementPatternIds = movementPatternIds.ToList();
            return this;
        }

        public ExerciseCreateDtoBuilder WithMovementPatterns(List<string> movementPatternIds)
        {
            _movementPatternIds = movementPatternIds;
            return this;
        }

        public ExerciseCreateDto Build()
        {
            return new ExerciseCreateDto
            {
                Name = _name,
                Description = _description,
                Instructions = _instructions,
                CoachNotes = _coachNotes,
                ExerciseTypeIds = _exerciseTypeIds,
                DifficultyId = _difficultyId,
                KineticChainId = _kineticChainId,
                WeightTypeId = _weightTypeId,
                DefaultWeight = _defaultWeight,
                IsUnilateral = _isUnilateral,
                IsActive = _isActive,
                ImageUrl = _imageUrl,
                VideoUrl = _videoUrl,
                MuscleGroups = _muscleGroups,
                EquipmentIds = _equipmentIds,
                BodyPartIds = _bodyPartIds,
                MovementPatternIds = _movementPatternIds
            };
        }

        /// <summary>
        /// Creates an ExerciseCreateDto from an existing ExerciseDto.
        /// Useful for edit scenarios where you need to map from the retrieved DTO.
        /// </summary>
        public static ExerciseCreateDto FromExerciseDto(ExerciseDto exercise)
        {
            return new ExerciseCreateDtoBuilder()
                .WithName(exercise.Name)
                .WithDescription(exercise.Description)
                .WithInstructions(exercise.Instructions)
                .WithDifficultyId(exercise.Difficulty?.Id ?? string.Empty)
                .WithKineticChainId(exercise.KineticChain?.Id)
                .WithWeightTypeId(exercise.WeightType?.Id.ToString())
                .WithDefaultWeight(exercise.DefaultWeight)
                .WithIsUnilateral(exercise.IsUnilateral)
                .WithIsActive(exercise.IsActive)
                .WithImageUrl(exercise.ImageUrl)
                .WithVideoUrl(exercise.VideoUrl)
                .WithCoachNotes(exercise.CoachNotes.Select(cn => new CoachNoteCreateDto
                {
                    Text = cn.Text,
                    Order = cn.Order
                }).ToList())
                .WithExerciseTypes(exercise.ExerciseTypes.Select(et => et.Id).ToList())
                .Build();
        }
    }
}