using GetFitterGetBigger.Admin.Models.Dtos;
using GetFitterGetBigger.Admin.Models.ReferenceData;
using System.Text.Json;

namespace GetFitterGetBigger.Admin.Services.Strategies.ReferenceTableStrategies
{
    public class ExerciseTypesStrategy : BaseReferenceTableStrategy<ExerciseTypes>
    {
        public override string Endpoint => "/api/ReferenceTables/ExerciseTypes";
        public override string CacheKey => "RefData_ExerciseTypes";
        
        // Note: This returns ExerciseTypeDto, but we're keeping it as ReferenceDataDto
        // for consistency with the interface. The actual ExerciseTypeDto conversion
        // happens in a separate method if needed.
    }
}