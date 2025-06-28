using GetFitterGetBigger.Admin.Models.Dtos;

namespace GetFitterGetBigger.Admin.Tests.Builders
{
    public class ExerciseCreateDtoBuilder
    {
        private string _name = "Test Exercise";
        private string _description = "Test Description";
        private string _instructions = "Test Instructions";
        private List<CoachNoteCreateDto> _coachNotes = new();
        private List<string> _exerciseTypeIds = new();
        private string _difficultyId = "difficulty-intermediate";
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

        public ExerciseCreateDtoBuilder WithDifficultyId(string difficultyId)
        {
            _difficultyId = difficultyId;
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

        public ExerciseCreateDtoBuilder WithImageUrl(string imageUrl)
        {
            _imageUrl = imageUrl;
            return this;
        }

        public ExerciseCreateDtoBuilder WithVideoUrl(string videoUrl)
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

        public ExerciseCreateDtoBuilder WithBodyParts(params string[] bodyPartIds)
        {
            _bodyPartIds = bodyPartIds.ToList();
            return this;
        }

        public ExerciseCreateDtoBuilder WithMovementPatterns(params string[] movementPatternIds)
        {
            _movementPatternIds = movementPatternIds.ToList();
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
    }
}