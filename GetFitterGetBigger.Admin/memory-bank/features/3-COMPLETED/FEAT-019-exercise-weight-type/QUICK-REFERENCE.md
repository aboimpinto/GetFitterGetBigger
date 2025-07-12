# FEAT-019 Exercise Weight Type - Quick Reference

## Weight Type Values
1. **BODYWEIGHT_ONLY** - Uses bodyweight resistance only
2. **NO_WEIGHT** - No weight required (stretches, cardio)
3. **BODYWEIGHT_OPTIONAL** - Can add weight to bodyweight
4. **WEIGHT_REQUIRED** - External weight required
5. **MACHINE_WEIGHT** - Machine weight stack/settings

## Business Rules
- ❌ REST exercises → No weight type allowed
- ✅ Non-REST exercises → Weight type required

## Key Components

### Selectors
```razor
<ExerciseWeightTypeSelector 
    @bind-Value="model.ExerciseWeightTypeId"
    Required="true"
    Disabled="false" />
```

### Badges
```razor
<ExerciseWeightTypeBadge 
    WeightType="@exercise.WeightType" 
    Size="ExerciseWeightTypeBadge.BadgeSize.Small" />
```

### API Endpoints
```
GET /api/exerciseweighttypes
GET /api/exercises/{id} (includes weight type)
POST/PUT /api/exercises (accepts exerciseWeightTypeId)
```

### JSON Format
```json
// Request
{
  "exerciseWeightTypeId": "exerciseweighttype-{guid}"
}

// Response
{
  "exerciseWeightType": {
    "id": "exerciseweighttype-{guid}",
    "value": "Bodyweight Only",
    "description": "Uses bodyweight resistance only"
  }
}
```

## Common Issues & Solutions

### Weight type not showing in list?
Add JSON attributes to DTO:
```csharp
[JsonPropertyName("exerciseWeightType")]
[JsonConverter(typeof(ExerciseWeightTypeJsonConverter))]
public ExerciseWeightTypeDto? WeightType { get; set; }
```

### Validation not clearing?
Implement value change handler:
```csharp
private void OnWeightTypeChanged()
{
    if (validationErrors.ContainsKey("WeightType"))
    {
        validationErrors.Remove("WeightType");
        StateHasChanged();
    }
}
```

## Testing
- Unit tests: `/JsonConverters/ExerciseWeightTypeJsonConverterTests.cs`
- Integration tests: Check exercise CRUD operations
- Manual tests: Verify UI flows and validation