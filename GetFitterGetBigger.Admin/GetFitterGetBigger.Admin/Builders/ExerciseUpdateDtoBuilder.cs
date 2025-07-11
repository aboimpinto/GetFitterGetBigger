using GetFitterGetBigger.Admin.Models.Dtos;

namespace GetFitterGetBigger.Admin.Builders
{
    /// <summary>
    /// Builder for creating ExerciseUpdateDto instances in production code.
    /// Provides a fluent interface for constructing exercise update DTOs.
    /// </summary>
    public class ExerciseUpdateDtoBuilder
    {
        private string _name = string.Empty;
        private string _description = string.Empty;
        private List<CoachNoteCreateDto> _coachNotes = new();
        private List<string> _exerciseTypeIds = new();
        private string _difficultyId = string.Empty;
        private string? _kineticChainId = null;
        private string? _weightTypeId = null;
        private bool _isUnilateral = false;
        private bool _isActive = true;
        private string? _imageUrl = null;
        private string? _videoUrl = null;
        private List<MuscleGroupApiDto> _muscleGroups = new();
        private List<string> _equipmentIds = new();
        private List<string> _bodyPartIds = new();
        private List<string> _movementPatternIds = new();

        public ExerciseUpdateDtoBuilder WithName(string name)
        {
            _name = name;
            return this;
        }

        public ExerciseUpdateDtoBuilder WithDescription(string description)
        {
            _description = description;
            return this;
        }


        public ExerciseUpdateDtoBuilder WithCoachNotes(params (string text, int order)[] notes)
        {
            _coachNotes = notes.Select(n => new CoachNoteCreateDto
            {
                Text = n.text,
                Order = n.order
            }).ToList();
            return this;
        }

        public ExerciseUpdateDtoBuilder WithCoachNotes(List<CoachNoteCreateDto> coachNotes)
        {
            _coachNotes = coachNotes;
            return this;
        }

        public ExerciseUpdateDtoBuilder AddCoachNote(string text, int order = 0)
        {
            _coachNotes.Add(new CoachNoteCreateDto
            {
                Text = text,
                Order = order
            });
            return this;
        }

        public ExerciseUpdateDtoBuilder WithExerciseTypes(params string[] typeIds)
        {
            _exerciseTypeIds = typeIds.ToList();
            return this;
        }

        public ExerciseUpdateDtoBuilder WithExerciseTypes(List<string> typeIds)
        {
            _exerciseTypeIds = typeIds;
            return this;
        }

        public ExerciseUpdateDtoBuilder AddExerciseType(string typeId)
        {
            _exerciseTypeIds.Add(typeId);
            return this;
        }

        public ExerciseUpdateDtoBuilder WithDifficultyId(string difficultyId)
        {
            _difficultyId = difficultyId;
            return this;
        }

        public ExerciseUpdateDtoBuilder WithKineticChainId(string? kineticChainId)
        {
            _kineticChainId = kineticChainId;
            return this;
        }

        public ExerciseUpdateDtoBuilder WithWeightTypeId(string? weightTypeId)
        {
            _weightTypeId = weightTypeId;
            return this;
        }

        public ExerciseUpdateDtoBuilder WithIsUnilateral(bool isUnilateral)
        {
            _isUnilateral = isUnilateral;
            return this;
        }

        public ExerciseUpdateDtoBuilder AsUnilateral()
        {
            _isUnilateral = true;
            return this;
        }

        public ExerciseUpdateDtoBuilder WithIsActive(bool isActive)
        {
            _isActive = isActive;
            return this;
        }

        public ExerciseUpdateDtoBuilder AsInactive()
        {
            _isActive = false;
            return this;
        }

        public ExerciseUpdateDtoBuilder WithImageUrl(string? imageUrl)
        {
            _imageUrl = imageUrl;
            return this;
        }

        public ExerciseUpdateDtoBuilder WithVideoUrl(string? videoUrl)
        {
            _videoUrl = videoUrl;
            return this;
        }

        public ExerciseUpdateDtoBuilder WithMuscleGroups(params MuscleGroupApiDto[] muscleGroups)
        {
            _muscleGroups = muscleGroups.ToList();
            return this;
        }

        public ExerciseUpdateDtoBuilder WithMuscleGroups(List<MuscleGroupApiDto> muscleGroups)
        {
            _muscleGroups = muscleGroups;
            return this;
        }

        public ExerciseUpdateDtoBuilder WithMuscleGroups(params (string muscleGroupId, string roleId)[] assignments)
        {
            _muscleGroups = assignments.Select(a => new MuscleGroupApiDto
            {
                MuscleGroupId = a.muscleGroupId,
                MuscleRoleId = a.roleId
            }).ToList();
            return this;
        }

        public ExerciseUpdateDtoBuilder AddMuscleGroup(string muscleGroupId, string roleId)
        {
            _muscleGroups.Add(new MuscleGroupApiDto
            {
                MuscleGroupId = muscleGroupId,
                MuscleRoleId = roleId
            });
            return this;
        }

        public ExerciseUpdateDtoBuilder WithEquipment(params string[] equipmentIds)
        {
            _equipmentIds = equipmentIds.ToList();
            return this;
        }

        public ExerciseUpdateDtoBuilder WithBodyParts(params string[] bodyPartIds)
        {
            _bodyPartIds = bodyPartIds.ToList();
            return this;
        }

        public ExerciseUpdateDtoBuilder WithMovementPatterns(params string[] patternIds)
        {
            _movementPatternIds = patternIds.ToList();
            return this;
        }

        public ExerciseUpdateDto Build()
        {
            return new ExerciseUpdateDto
            {
                Name = _name,
                Description = _description,
                CoachNotes = _coachNotes,
                ExerciseTypeIds = _exerciseTypeIds,
                DifficultyId = _difficultyId,
                KineticChainId = _kineticChainId,
                ExerciseWeightTypeId = _weightTypeId,
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
        /// Creates an ExerciseUpdateDto from an existing ExerciseCreateDto.
        /// Useful when you have a create DTO and need to convert it to an update DTO.
        /// </summary>
        public static ExerciseUpdateDto FromCreateDto(ExerciseCreateDto createDto)
        {
            return new ExerciseUpdateDtoBuilder()
                .WithName(createDto.Name)
                .WithDescription(createDto.Description)
                .WithDifficultyId(createDto.DifficultyId)
                .WithKineticChainId(createDto.KineticChainId)
                .WithWeightTypeId(createDto.ExerciseWeightTypeId)
                .WithIsUnilateral(createDto.IsUnilateral)
                .WithIsActive(createDto.IsActive)
                .WithImageUrl(createDto.ImageUrl)
                .WithVideoUrl(createDto.VideoUrl)
                .WithCoachNotes(createDto.CoachNotes)
                .WithExerciseTypes(createDto.ExerciseTypeIds)
                .WithMuscleGroups(createDto.MuscleGroups)
                .WithEquipment(createDto.EquipmentIds.ToArray())
                .WithBodyParts(createDto.BodyPartIds.ToArray())
                .WithMovementPatterns(createDto.MovementPatternIds.ToArray())
                .Build();
        }

        /// <summary>
        /// Creates an ExerciseUpdateDto from an existing ExerciseDto.
        /// Useful for edit scenarios where you need to map from the retrieved DTO.
        /// </summary>
        public static ExerciseUpdateDto FromExerciseDto(ExerciseDto exercise)
        {
            var builder = new ExerciseUpdateDtoBuilder()
                .WithName(exercise.Name)
                .WithDescription(exercise.Description)
                .WithDifficultyId(exercise.Difficulty?.Id ?? string.Empty)
                .WithKineticChainId(exercise.KineticChain?.Id)
                .WithWeightTypeId(exercise.WeightType != null ? $"exerciseweighttype-{exercise.WeightType.Id}" : null)
                .WithIsUnilateral(exercise.IsUnilateral)
                .WithIsActive(exercise.IsActive)
                .WithImageUrl(exercise.ImageUrl)
                .WithVideoUrl(exercise.VideoUrl);

            // Convert CoachNoteDto to CoachNoteCreateDto
            if (exercise.CoachNotes.Any())
            {
                builder.WithCoachNotes(exercise.CoachNotes.Select(cn => new CoachNoteCreateDto
                {
                    Id = cn.Id,
                    Text = cn.Text,
                    Order = cn.Order
                }).ToList());
            }

            // Convert ExerciseTypeDto to IDs
            if (exercise.ExerciseTypes.Any())
            {
                builder.WithExerciseTypes(exercise.ExerciseTypes.Select(et => et.Id).ToList());
            }

            // Convert muscle groups
            if (exercise.MuscleGroups.Any())
            {
                builder.WithMuscleGroups(exercise.MuscleGroups.Select(mg => new MuscleGroupApiDto
                {
                    MuscleGroupId = mg.MuscleGroup?.Id ?? string.Empty,
                    MuscleRoleId = mg.Role?.Id ?? string.Empty
                }).ToList());
            }

            // Convert equipment to IDs
            if (exercise.Equipment.Any())
            {
                builder.WithEquipment(exercise.Equipment.Select(e => e.Id).ToArray());
            }

            // Convert body parts to IDs
            if (exercise.BodyParts.Any())
            {
                builder.WithBodyParts(exercise.BodyParts.Select(bp => bp.Id).ToArray());
            }

            // Convert movement patterns to IDs
            if (exercise.MovementPatterns.Any())
            {
                builder.WithMovementPatterns(exercise.MovementPatterns.Select(mp => mp.Id).ToArray());
            }

            return builder.Build();
        }
    }
}