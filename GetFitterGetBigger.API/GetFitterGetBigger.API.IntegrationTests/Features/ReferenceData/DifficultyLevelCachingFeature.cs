using Xunit;

namespace GetFitterGetBigger.API.IntegrationTests.Features.ReferenceData;

/// <summary>
/// Partial class to apply the Serial collection to the DifficultyLevelCaching feature.
/// This ensures tests in this feature run sequentially, avoiding cache contamination issues.
/// </summary>
[Collection("Serial")]
public partial class DifficultyLevelCachingFeature
{
    // The main implementation is in the generated .feature.cs file
    // This partial class only exists to apply the Collection attribute
}