using GetFitterGetBigger.Admin.Models.Dtos;

namespace GetFitterGetBigger.Admin.Builders
{
    /// <summary>
    /// Builder for creating ExerciseListDto instances.
    /// Useful for testing and creating exercise list items.
    /// </summary>
    public class ExerciseListDtoBuilder
    {
        private string _id = Guid.NewGuid().ToString();
        private string _name = "Test Exercise";
        private string _description = "Test Description";
        private string _instructions = "Test Instructions";
        private List<CoachNoteDto> _coachNotes = new();
        private List<ExerciseTypeDto> _exerciseTypes = new();
        private string? _videoUrl = null;
        private string? _imageUrl = null;
        private bool _isUnilateral = false;
        private bool _isActive = true;
        private ReferenceDataDto? _difficulty = new() { Id = "1", Value = "Intermediate", Description = "Intermediate level" };
        private ReferenceDataDto? _kineticChain = null;
        private List<MuscleGroupListItemDto> _muscleGroups = new();
        private List<ReferenceDataDto> _equipment = new();
        private List<ReferenceDataDto> _movementPatterns = new();
        private List<ReferenceDataDto> _bodyParts = new();

        public ExerciseListDtoBuilder WithId(string id)
        {
            _id = id;
            return this;
        }

        public ExerciseListDtoBuilder WithName(string name)
        {
            _name = name;
            return this;
        }

        public ExerciseListDtoBuilder WithDescription(string description)
        {
            _description = description;
            return this;
        }

        public ExerciseListDtoBuilder WithInstructions(string instructions)
        {
            _instructions = instructions;
            return this;
        }

        public ExerciseListDtoBuilder WithCoachNotes(List<CoachNoteDto> coachNotes)
        {
            _coachNotes = coachNotes;
            return this;
        }

        public ExerciseListDtoBuilder WithCoachNotes(params (string text, int order)[] notes)
        {
            _coachNotes = notes.Select(n => new CoachNoteDto
            {
                Id = Guid.NewGuid().ToString(),
                Text = n.text,
                Order = n.order
            }).ToList();
            return this;
        }

        public ExerciseListDtoBuilder AddCoachNote(string text, int order = 0)
        {
            _coachNotes.Add(new CoachNoteDto
            {
                Id = Guid.NewGuid().ToString(),
                Text = text,
                Order = order
            });
            return this;
        }

        public ExerciseListDtoBuilder WithExerciseTypes(List<ExerciseTypeDto> exerciseTypes)
        {
            _exerciseTypes = exerciseTypes;
            return this;
        }

        public ExerciseListDtoBuilder WithExerciseTypes(params (string value, string description)[] types)
        {
            _exerciseTypes = types.Select(t => new ExerciseTypeDto
            {
                Id = $"type-{t.value.ToLower()}",
                Value = t.value,
                Description = t.description
            }).ToList();
            return this;
        }

        public ExerciseListDtoBuilder AddExerciseType(string value, string description = "")
        {
            _exerciseTypes.Add(new ExerciseTypeDto
            {
                Id = $"type-{value.ToLower()}",
                Value = value,
                Description = description
            });
            return this;
        }

        public ExerciseListDtoBuilder WithVideoUrl(string videoUrl)
        {
            _videoUrl = videoUrl;
            return this;
        }

        public ExerciseListDtoBuilder WithImageUrl(string imageUrl)
        {
            _imageUrl = imageUrl;
            return this;
        }

        public ExerciseListDtoBuilder AsUnilateral()
        {
            _isUnilateral = true;
            return this;
        }

        public ExerciseListDtoBuilder AsInactive()
        {
            _isActive = false;
            return this;
        }

        public ExerciseListDtoBuilder WithIsActive(bool isActive)
        {
            _isActive = isActive;
            return this;
        }

        public ExerciseListDtoBuilder WithDifficulty(string value, string? id = null)
        {
            _difficulty = new ReferenceDataDto
            {
                Id = id ?? $"difficulty-{value.ToLower()}",
                Value = value,
                Description = $"{value} level"
            };
            return this;
        }

        public ExerciseListDtoBuilder WithKineticChain(string? value, string? description = null)
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

        public ExerciseListDtoBuilder WithMuscleGroups(params (string muscleName, string roleName)[] muscleGroups)
        {
            _muscleGroups = muscleGroups.Select(mg => new MuscleGroupListItemDto
            {
                MuscleGroup = new ReferenceDataDto
                {
                    Id = Guid.NewGuid().ToString(),
                    Value = mg.muscleName,
                    Description = mg.muscleName
                },
                Role = new ReferenceDataDto
                {
                    Id = Guid.NewGuid().ToString(),
                    Value = mg.roleName,
                    Description = mg.roleName
                }
            }).ToList();
            return this;
        }

        public ExerciseListDtoBuilder WithMuscleGroups(params MuscleGroupListItemDto[] muscleGroups)
        {
            _muscleGroups = muscleGroups.ToList();
            return this;
        }

        public ExerciseListDtoBuilder WithMuscleGroup(string muscleName, string roleName)
        {
            _muscleGroups.Add(new MuscleGroupListItemDto
            {
                MuscleGroup = new ReferenceDataDto
                {
                    Id = Guid.NewGuid().ToString(),
                    Value = muscleName,
                    Description = muscleName
                },
                Role = new ReferenceDataDto
                {
                    Id = Guid.NewGuid().ToString(),
                    Value = roleName,
                    Description = roleName
                }
            });
            return this;
        }

        public ExerciseListDtoBuilder WithPrimaryMuscleGroups(params string[] muscleNames)
        {
            foreach (var muscleName in muscleNames)
            {
                _muscleGroups.Add(new MuscleGroupListItemDto
                {
                    MuscleGroup = new ReferenceDataDto
                    {
                        Id = Guid.NewGuid().ToString(),
                        Value = muscleName,
                        Description = muscleName
                    },
                    Role = new ReferenceDataDto
                    {
                        Id = "1",
                        Value = "Primary",
                        Description = "Primary"
                    }
                });
            }
            return this;
        }

        public ExerciseListDtoBuilder WithEquipment(params string[] equipment)
        {
            _equipment = equipment.Select(e => new ReferenceDataDto
            {
                Id = Guid.NewGuid().ToString(),
                Value = e,
                Description = e
            }).ToList();
            return this;
        }

        public ExerciseListDtoBuilder WithEquipment(params ReferenceDataDto[] equipment)
        {
            _equipment = equipment.ToList();
            return this;
        }

        public ExerciseListDtoBuilder WithBodyParts(params string[] bodyParts)
        {
            _bodyParts = bodyParts.Select(bp => new ReferenceDataDto
            {
                Id = Guid.NewGuid().ToString(),
                Value = bp,
                Description = bp
            }).ToList();
            return this;
        }

        public ExerciseListDtoBuilder WithMovementPatterns(params string[] patterns)
        {
            _movementPatterns = patterns.Select(p => new ReferenceDataDto
            {
                Id = Guid.NewGuid().ToString(),
                Value = p,
                Description = p
            }).ToList();
            return this;
        }

        public ExerciseListDto Build()
        {
            return new ExerciseListDto
            {
                Id = _id,
                Name = _name,
                Description = _description,
                Instructions = _instructions,
                CoachNotes = _coachNotes,
                ExerciseTypes = _exerciseTypes,
                VideoUrl = _videoUrl,
                ImageUrl = _imageUrl,
                IsUnilateral = _isUnilateral,
                IsActive = _isActive,
                Difficulty = _difficulty,
                KineticChain = _kineticChain,
                MuscleGroups = _muscleGroups,
                Equipment = _equipment,
                MovementPatterns = _movementPatterns,
                BodyParts = _bodyParts
            };
        }

        public static List<ExerciseListDto> BuildList(int count)
        {
            var exercises = new List<ExerciseListDto>();
            for (int i = 1; i <= count; i++)
            {
                exercises.Add(new ExerciseListDtoBuilder()
                    .WithName($"Exercise {i}")
                    .WithDescription($"Description for exercise {i}")
                    .Build());
            }
            return exercises;
        }
    }
}