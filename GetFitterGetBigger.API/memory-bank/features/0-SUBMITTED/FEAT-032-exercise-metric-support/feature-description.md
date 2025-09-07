# Feature: Exercise Metric Support System

## Feature ID: FEAT-032
**Title**: Exercise Metric Support for Intelligent UI Input  
**Status**: SUBMITTED  
**Priority**: Medium  
**Created**: 2025-01-07  
**Author**: Paulo Aboim Pinto  
**Extracted From**: FEAT-031  

## Description

Define which metrics (repetitions, time, weight, distance) each exercise supports to provide intelligent UI input fields. This feature enhances the user experience by showing only relevant input fields based on what each exercise can actually track.

## Background

During the design of FEAT-031 (Workout Template Exercise Management), we identified the need for intelligent UI input fields based on exercise capabilities. However, this is a separate concern from workout template management and deserves its own feature to maintain clean separation of concerns.

## User Stories

### As a Personal Trainer
- I want the UI to show only relevant input fields for each exercise
- I want to avoid confusion by not seeing weight fields for bodyweight exercises
- I want appropriate units for time-based vs rep-based exercises

### As an API Developer
- I need to query which metrics an exercise supports
- I need to validate that submitted data matches supported metrics
- I need to maintain exercise-metric relationships

## Acceptance Criteria

### 1. Exercise Metric Support Relationship
- [ ] Link exercises to their supported MetricTypes
- [ ] Support multiple metrics per exercise (e.g., burpees can be reps OR time)
- [ ] Maintain this relationship in ExerciseMetricSupport table (existing)

### 2. API Endpoint for UI
- [ ] Create endpoint: GET `/api/exercises/{exerciseId}/supported-metrics`
- [ ] Return list of supported metrics with appropriate metadata
- [ ] Include ExerciseWeightType for weight-based exercises

### 3. Validation
- [ ] Validate submitted workout data matches supported metrics
- [ ] Provide clear error messages for unsupported metric submissions
- [ ] Allow flexibility for exercises that support multiple metrics

### 4. Migration
- [ ] Create migration to populate ExerciseMetricSupport for existing exercises
- [ ] Default mappings based on ExerciseType and ExerciseWeightType

## Technical Design

### API Response Example

**Endpoint**: `GET /api/exercises/{exerciseId}/supported-metrics`

**Response for Barbell Squat**:
```json
{
  "success": true,
  "data": {
    "exerciseId": 125,
    "exerciseName": "Barbell Squat",
    "supportedMetrics": [
      {
        "metricType": "REPETITIONS",
        "required": true
      },
      {
        "metricType": "WEIGHT",
        "required": true,
        "weightType": "BARBELL",
        "units": ["kg", "lbs"]
      }
    ]
  }
}
```

**Response for Running**:
```json
{
  "success": true,
  "data": {
    "exerciseId": 200,
    "exerciseName": "Running",
    "supportedMetrics": [
      {
        "metricType": "TIME",
        "required": false,
        "units": ["seconds", "minutes", "hours"]
      },
      {
        "metricType": "DISTANCE",
        "required": false,
        "units": ["meters", "kilometers", "miles"]
      }
    ],
    "note": "At least one metric must be provided"
  }
}
```

**Response for Plank**:
```json
{
  "success": true,
  "data": {
    "exerciseId": 150,
    "exerciseName": "Plank",
    "supportedMetrics": [
      {
        "metricType": "TIME",
        "required": true,
        "units": ["seconds", "minutes"]
      }
    ]
  }
}
```

### Integration with FEAT-031

When FEAT-031 adds an exercise to a workout template:
1. UI calls this endpoint to get supported metrics
2. UI shows appropriate input fields
3. User fills in the data
4. Data is saved as JSON metadata in WorkoutTemplateExercise

### Validation Service

```csharp
public interface IExerciseMetricValidationService
{
    Task<bool> ValidateMetricsAsync(int exerciseId, JsonDocument metadata);
    Task<ValidationResult> GetValidationErrorsAsync(int exerciseId, JsonDocument metadata);
}
```

## Implementation Approach

### Phase 1: Foundation
1. Design ExerciseMetricSupport enhancements
2. Create API endpoint for querying supported metrics
3. Implement basic validation service

### Phase 2: Migration
1. Analyze existing exercises
2. Create migration to populate default metric support
3. Handle special cases (REST, compound exercises)

### Phase 3: Integration
1. Update Admin UI to use the endpoint
2. Add validation to workout template exercise addition
3. Create comprehensive tests

## Dependencies
- MetricType entity (existing)
- ExerciseMetricSupport relationship (existing)
- Exercise entity (existing)
- Must coordinate with FEAT-031 for integration

## Success Metrics
- UI shows correct input fields 100% of the time
- Zero invalid metric submissions
- Improved user experience with less confusion
- Reduced support tickets about "missing fields"

## Notes
- This feature was extracted from FEAT-031 to maintain separation of concerns
- The validation is optional initially - can store any JSON metadata
- Future enhancement: suggested rep ranges based on exercise and difficulty
- Consider adding "preferred" metric for exercises that support multiple

## Questions to Resolve
1. Should we enforce validation strictly or allow flexibility?
2. How to handle exercises that can use different metrics in different contexts?
3. Should we version the metric support (for future changes)?

## Related Features
- FEAT-031: Workout Template Exercise Management (origin feature)
- Future: Exercise recommendation based on metrics
- Future: Automatic unit conversion