using GetFitterGetBigger.Admin.Models.Dtos;

namespace GetFitterGetBigger.Admin.Builders
{
    /// <summary>
    /// Builder for creating MuscleGroupApiDto instances.
    /// Used when preparing muscle group data for API calls.
    /// </summary>
    public class MuscleGroupApiDtoBuilder
    {
        private string _muscleGroupId = string.Empty;
        private string _muscleRoleId = string.Empty;

        public MuscleGroupApiDtoBuilder WithMuscleGroupId(string muscleGroupId)
        {
            _muscleGroupId = muscleGroupId;
            return this;
        }

        public MuscleGroupApiDtoBuilder WithMuscleRoleId(string muscleRoleId)
        {
            _muscleRoleId = muscleRoleId;
            return this;
        }

        public MuscleGroupApiDto Build()
        {
            return new MuscleGroupApiDto
            {
                MuscleGroupId = _muscleGroupId,
                MuscleRoleId = _muscleRoleId
            };
        }

        /// <summary>
        /// Creates a MuscleGroupApiDto from a muscle group assignment and a role lookup.
        /// Useful when converting from form data to API format.
        /// </summary>
        public static MuscleGroupApiDto FromAssignment(
            MuscleGroupRoleAssignmentDto assignment,
            Func<string, string?> roleNameToIdLookup)
        {
            var roleId = roleNameToIdLookup(assignment.Role) ?? string.Empty;
            return new MuscleGroupApiDtoBuilder()
                .WithMuscleGroupId(assignment.MuscleGroupId)
                .WithMuscleRoleId(roleId)
                .Build();
        }
    }
}