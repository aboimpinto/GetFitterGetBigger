using GetFitterGetBigger.Admin.Models.Dtos;

namespace GetFitterGetBigger.Admin.Builders
{
    /// <summary>
    /// Builder for creating ExerciseDto instances.
    /// Useful for testing and data transformation scenarios.
    /// </summary>
    public class ExerciseDtoBuilder
    {
        private string _id = Guid.NewGuid().ToString();
        private string _name = "Test Exercise";
        private string _description = "Test Description";
        private string _instructions = "Test Instructions";
        private List<CoachNoteDto> _coachNotes = new();
        private List<ExerciseTypeDto> _exerciseTypes = new();
        private ReferenceDataDto? _difficulty = new() { Id = "1", Value = "Intermediate", Description = "Intermediate level" };
        private ReferenceDataDto? _kineticChain = null;
        private ExerciseWeightTypeDto? _weightType = null;
        private decimal? _defaultWeight = null;
        private bool _isUnilateral = false;
        private bool _isActive = true;
        private string? _imageUrl = null;
        private string? _videoUrl = null;
        private List<MuscleGroupWithRoleDto> _muscleGroups = new();
        private List<ReferenceDataDto> _equipment = new();
        private List<ReferenceDataDto> _bodyParts = new();
        private List<ReferenceDataDto> _movementPatterns = new();
        private int _warmupLinkCount = 0;
        private int _cooldownLinkCount = 0;

        public ExerciseDtoBuilder WithId(string id)
        {
            _id = id;
            return this;
        }

        public ExerciseDtoBuilder WithName(string name)
        {
            _name = name;
            return this;
        }

        public ExerciseDtoBuilder WithDescription(string description)
        {
            _description = description;
            return this;
        }

        public ExerciseDtoBuilder WithInstructions(string instructions)
        {
            _instructions = instructions;
            return this;
        }

        public ExerciseDtoBuilder WithCoachNotes(List<CoachNoteDto> coachNotes)
        {
            _coachNotes = coachNotes;
            return this;
        }

        public ExerciseDtoBuilder WithCoachNotes(params (string text, int order)[] notes)
        {
            _coachNotes = notes.Select(n => new CoachNoteDto
            {
                Id = Guid.NewGuid().ToString(),
                Text = n.text,
                Order = n.order
            }).ToList();
            return this;
        }

        public ExerciseDtoBuilder WithCoachNote(string text, int order = 0)
        {
            _coachNotes.Add(new CoachNoteDto
            {
                Id = Guid.NewGuid().ToString(),
                Text = text,
                Order = order
            });
            return this;
        }

        public ExerciseDtoBuilder WithExerciseTypes(List<ExerciseTypeDto> exerciseTypes)
        {
            _exerciseTypes = exerciseTypes;
            return this;
        }

        public ExerciseDtoBuilder WithExerciseTypes(params (string value, string description)[] types)
        {
            _exerciseTypes = types.Select(t => new ExerciseTypeDto
            {
                Id = $"type-{t.value.ToLower()}",
                Value = t.value,
                Description = t.description
            }).ToList();
            return this;
        }

        public ExerciseDtoBuilder WithExerciseType(string value, string description = "")
        {
            _exerciseTypes.Add(new ExerciseTypeDto
            {
                Id = Guid.NewGuid().ToString(),
                Value = value,
                Description = description
            });
            return this;
        }

        public ExerciseDtoBuilder WithDifficulty(ReferenceDataDto difficulty)
        {
            _difficulty = difficulty;
            return this;
        }

        public ExerciseDtoBuilder WithDifficulty(string value, string? id = null)
        {
            _difficulty = new ReferenceDataDto
            {
                Id = id ?? $"difficulty-{value.ToLower()}",
                Value = value,
                Description = $"{value} level"
            };
            return this;
        }

        public ExerciseDtoBuilder WithKineticChain(string? value, string? description = null)
        {
            if (value == null)
            {
                _kineticChain = null;
            }
            else
            {
                _kineticChain = new ReferenceDataDto
                {
                    Id = $"kineticchain-{value.ToLower()}",
                    Value = value,
                    Description = description ?? $"{value} movement"
                };
            }
            return this;
        }

        public ExerciseDtoBuilder WithWeightType(ExerciseWeightTypeDto? weightType)
        {
            _weightType = weightType;
            return this;
        }

        public ExerciseDtoBuilder WithWeightType(ReferenceDataDto weightType)
        {
            _weightType = new ExerciseWeightTypeDto
            {
                Id = Guid.Parse(weightType.Id),
                Code = $"CODE_{weightType.Value.ToUpper().Replace(" ", "_")}",
                Name = weightType.Value,
                Description = weightType.Description ?? weightType.Value,
                IsActive = true,
                DisplayOrder = 1
            };
            return this;
        }

        public ExerciseDtoBuilder WithWeightType(string code, string name, string? description = null)
        {
            _weightType = new ExerciseWeightTypeDto
            {
                Id = Guid.NewGuid(),
                Code = code,
                Name = name,
                Description = description ?? name,
                IsActive = true,
                DisplayOrder = 1
            };
            return this;
        }

        public ExerciseDtoBuilder WithDefaultWeight(decimal? weight)
        {
            _defaultWeight = weight;
            return this;
        }

        public ExerciseDtoBuilder AsUnilateral()
        {
            _isUnilateral = true;
            return this;
        }

        public ExerciseDtoBuilder AsInactive()
        {
            _isActive = false;
            return this;
        }

        public ExerciseDtoBuilder WithImageUrl(string imageUrl)
        {
            _imageUrl = imageUrl;
            return this;
        }

        public ExerciseDtoBuilder WithVideoUrl(string videoUrl)
        {
            _videoUrl = videoUrl;
            return this;
        }

        public ExerciseDtoBuilder WithMuscleGroups(List<MuscleGroupWithRoleDto> muscleGroups)
        {
            _muscleGroups = muscleGroups;
            return this;
        }

        public ExerciseDtoBuilder WithMuscleGroups(params (string muscleName, string roleName)[] muscleGroups)
        {
            _muscleGroups = muscleGroups.Select(mg => new MuscleGroupWithRoleDto
            {
                MuscleGroup = new ReferenceDataDto { Id = Guid.NewGuid().ToString(), Value = mg.muscleName, Description = mg.muscleName },
                Role = new ReferenceDataDto { Id = Guid.NewGuid().ToString(), Value = mg.roleName, Description = mg.roleName }
            }).ToList();
            return this;
        }

        public ExerciseDtoBuilder WithMuscleGroup(string muscleName, string roleName)
        {
            _muscleGroups.Add(new MuscleGroupWithRoleDto
            {
                MuscleGroup = new ReferenceDataDto { Id = Guid.NewGuid().ToString(), Value = muscleName, Description = muscleName },
                Role = new ReferenceDataDto { Id = Guid.NewGuid().ToString(), Value = roleName, Description = roleName }
            });
            return this;
        }

        public ExerciseDtoBuilder WithEquipment(List<ReferenceDataDto> equipment)
        {
            _equipment = equipment;
            return this;
        }

        public ExerciseDtoBuilder WithBodyParts(List<ReferenceDataDto> bodyParts)
        {
            _bodyParts = bodyParts;
            return this;
        }

        public ExerciseDtoBuilder WithMovementPatterns(List<ReferenceDataDto> movementPatterns)
        {
            _movementPatterns = movementPatterns;
            return this;
        }

        public ExerciseDtoBuilder WithWarmupLinkCount(int count)
        {
            _warmupLinkCount = count;
            return this;
        }

        public ExerciseDtoBuilder WithCooldownLinkCount(int count)
        {
            _cooldownLinkCount = count;
            return this;
        }

        public ExerciseDtoBuilder WithLinkCounts(int warmupCount, int cooldownCount)
        {
            _warmupLinkCount = warmupCount;
            _cooldownLinkCount = cooldownCount;
            return this;
        }

        public ExerciseDto Build()
        {
            return new ExerciseDto
            {
                Id = _id,
                Name = _name,
                Description = _description,
                Instructions = _instructions,
                CoachNotes = _coachNotes,
                ExerciseTypes = _exerciseTypes,
                Difficulty = _difficulty,
                KineticChain = _kineticChain,
                WeightType = _weightType,
                DefaultWeight = _defaultWeight,
                IsUnilateral = _isUnilateral,
                IsActive = _isActive,
                ImageUrl = _imageUrl,
                VideoUrl = _videoUrl,
                MuscleGroups = _muscleGroups,
                Equipment = _equipment,
                BodyParts = _bodyParts,
                MovementPatterns = _movementPatterns,
                WarmupLinkCount = _warmupLinkCount,
                CooldownLinkCount = _cooldownLinkCount
            };
        }
    }
}