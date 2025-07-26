using GetFitterGetBigger.Admin.Models.Dtos;
using GetFitterGetBigger.Admin.Models.ReferenceData;
using System.Text.Json;

namespace GetFitterGetBigger.Admin.Services.Strategies.ReferenceTableStrategies
{
    public class MuscleGroupsStrategy : BaseReferenceTableStrategy<MuscleGroups>
    {
        public override string Endpoint => "/api/ReferenceTables/MuscleGroups";
        public override string CacheKey => "RefData_MuscleGroups";
        
        public override Task<IEnumerable<ReferenceDataDto>> TransformDataAsync(string jsonResponse)
        {
            // MuscleGroups endpoint returns MuscleGroupDto, not ReferenceDataDto
            var muscleGroups = JsonSerializer.Deserialize<IEnumerable<MuscleGroupDto>>(
                jsonResponse, 
                JsonOptions);
                
            if (muscleGroups == null)
                return Task.FromResult(Enumerable.Empty<ReferenceDataDto>());
                
            // Transform to ReferenceDataDto
            var result = muscleGroups.Select(mg => new ReferenceDataDto
            {
                Id = mg.Id,
                Value = mg.Name,
                Description = $"Body Part: {mg.BodyPartName ?? "Unknown"}"
            }).ToList();
            
            return Task.FromResult<IEnumerable<ReferenceDataDto>>(result);
        }
    }
}