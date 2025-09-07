using GetFitterGetBigger.API.Models.SpecializedIds;

namespace GetFitterGetBigger.API.Constants;

/// <summary>
/// Centralized constants for ExecutionProtocol values and IDs.
/// These values must match the seeded data in the database migrations.
/// </summary>
public static class ExecutionProtocolConstants
{
    // String value constants
    
    /// <summary>
    /// Reps and Sets protocol value - Traditional workout with fixed sets and repetitions
    /// </summary>
    public const string RepsAndSets = "Reps and Sets";
    
    /// <summary>
    /// Superset protocol value - Two exercises performed back-to-back
    /// </summary>
    public const string Superset = "Superset";
    
    /// <summary>
    /// Drop Set protocol value - Reducing weight after reaching failure
    /// </summary>
    public const string DropSet = "Drop Set";
    
    /// <summary>
    /// AMRAP protocol value - As Many Reps As Possible
    /// </summary>
    public const string Amrap = "AMRAP";
    
    // Code constants
    
    /// <summary>
    /// Code for Reps and Sets protocol
    /// </summary>
    public const string RepsAndSetsCode = "REPS_AND_SETS";
    
    /// <summary>
    /// Code for Superset protocol
    /// </summary>
    public const string SupersetCode = "SUPERSET";
    
    /// <summary>
    /// Code for Drop Set protocol
    /// </summary>
    public const string DropSetCode = "DROP_SET";
    
    /// <summary>
    /// Code for AMRAP protocol
    /// </summary>
    public const string AmrapCode = "AMRAP";
    
    // ID constants for database references
    
    /// <summary>
    /// REPS_AND_SETS ID (executionprotocol-30000003-3000-4000-8000-300000000001).
    /// Traditional workout with fixed sets and repetitions.
    /// This is the default ExecutionProtocol for new WorkoutTemplates.
    /// </summary>
    public const string RepsAndSetsIdString = "executionprotocol-30000003-3000-4000-8000-300000000001";
    
    /// <summary>
    /// REPS_AND_SETS ID as ExecutionProtocolId.
    /// Traditional workout with fixed sets and repetitions.
    /// </summary>
    public static readonly ExecutionProtocolId RepsAndSetsId = 
        ExecutionProtocolId.ParseOrEmpty(RepsAndSetsIdString);
    
    /// <summary>
    /// SUPERSET ID (executionprotocol-30000003-3000-4000-8000-300000000002).
    /// Two exercises performed back-to-back without rest.
    /// </summary>
    public const string SupersetIdString = "executionprotocol-30000003-3000-4000-8000-300000000002";
    
    /// <summary>
    /// SUPERSET ID as ExecutionProtocolId.
    /// Two exercises performed back-to-back without rest.
    /// </summary>
    public static readonly ExecutionProtocolId SupersetId = 
        ExecutionProtocolId.ParseOrEmpty(SupersetIdString);
    
    /// <summary>
    /// DROP_SET ID (executionprotocol-30000003-3000-4000-8000-300000000003).
    /// Reducing weight after reaching failure.
    /// </summary>
    public const string DropSetIdString = "executionprotocol-30000003-3000-4000-8000-300000000003";
    
    /// <summary>
    /// DROP_SET ID as ExecutionProtocolId.
    /// Reducing weight after reaching failure.
    /// </summary>
    public static readonly ExecutionProtocolId DropSetId = 
        ExecutionProtocolId.ParseOrEmpty(DropSetIdString);
    
    /// <summary>
    /// AMRAP ID (executionprotocol-30000003-3000-4000-8000-300000000004).
    /// As Many Reps As Possible in a given time.
    /// </summary>
    public const string AmrapIdString = "executionprotocol-30000003-3000-4000-8000-300000000004";
    
    /// <summary>
    /// AMRAP ID as ExecutionProtocolId.
    /// As Many Reps As Possible in a given time.
    /// </summary>
    public static readonly ExecutionProtocolId AmrapId = 
        ExecutionProtocolId.ParseOrEmpty(AmrapIdString);
}