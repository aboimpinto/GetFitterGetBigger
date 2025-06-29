using GetFitterGetBigger.Admin.Models.Dtos;

namespace GetFitterGetBigger.Admin.Builders
{
    /// <summary>
    /// Builder for creating MuscleGroupRoleAssignmentDto instances.
    /// Used in forms for assigning muscle groups with their roles.
    /// </summary>
    public class MuscleGroupRoleAssignmentDtoBuilder
    {
        private string _muscleGroupId = string.Empty;
        private string _role = string.Empty;

        public MuscleGroupRoleAssignmentDtoBuilder WithMuscleGroupId(string muscleGroupId)
        {
            _muscleGroupId = muscleGroupId;
            return this;
        }

        public MuscleGroupRoleAssignmentDtoBuilder WithRole(string role)
        {
            _role = role;
            return this;
        }

        public MuscleGroupRoleAssignmentDto Build()
        {
            return new MuscleGroupRoleAssignmentDto
            {
                MuscleGroupId = _muscleGroupId,
                Role = _role
            };
        }

        /// <summary>
        /// Creates an empty MuscleGroupRoleAssignmentDto.
        /// Useful for adding new empty assignments in forms.
        /// </summary>
        public static MuscleGroupRoleAssignmentDto Empty()
        {
            return new MuscleGroupRoleAssignmentDtoBuilder().Build();
        }
    }
}