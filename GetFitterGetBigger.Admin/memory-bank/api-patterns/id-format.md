# ID Format Pattern

## Overview
All entity IDs in the GetFitterGetBigger API use a prefixed string format instead of plain GUIDs.

## Format
`<entity-type>-<guid>`

## Examples
- Exercise: `exercise-ddeeff00-1122-3344-5566-778899001122`
- Muscle Group: `musclegroup-ddeeff00-1122-3344-5566-778899001122`
- Difficulty: `difficulty-ddeeff00-1122-3344-5566-778899001122`
- Equipment: `equipment-ddeeff00-1122-3344-5566-778899001122`

## Implementation Guidelines

### DTOs
- All ID properties should be of type `string`, not `Guid`
- Initialize with `string.Empty` as default value
- Example:
  ```csharp
  public string Id { get; set; } = string.Empty;
  public string DifficultyId { get; set; } = string.Empty;
  ```

### Validation
- Use `string.IsNullOrWhiteSpace()` to check for empty IDs
- Do NOT attempt to parse IDs as GUIDs
- Example:
  ```csharp
  if (string.IsNullOrWhiteSpace(model.DifficultyId))
      validationErrors["Difficulty"] = "Difficulty level is required";
  ```

### Form Binding
- Bind directly to string properties
- No conversion needed for dropdown values
- Example:
  ```razor
  <select value="@model.DifficultyId" @onchange="@(e => model.DifficultyId = e.Value?.ToString() ?? string.Empty)">
  ```

### API Calls
- Pass IDs as strings in URLs
- No GUID parsing or conversion needed
- Example:
  ```csharp
  var requestUrl = $"{_apiBaseUrl}/api/exercises/{id}";
  ```

## Rationale
This format allows the API to:
- Identify entity types from the ID itself
- Maintain type safety across different entities
- Provide better debugging and logging capabilities
- Support future ID format changes without breaking clients