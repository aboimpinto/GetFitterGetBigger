# Feature: Exercise Kinetic Chain Implementation

## Feature ID: FEAT-017
## Created: 2025-07-07

## Status: COMPLETED

## Summary
Implement the Kinetic Chain field for exercises in the Admin application, allowing Personal Trainers to categorize exercises as either Compound (multi-muscle) or Isolation (single-muscle) movements.

## Background
The API has been updated (API's FEAT-019) to include a new required field called "Kinetic Chain" for all Exercise endpoints. This field helps categorize exercises based on their biomechanical movement patterns.

## Requirements

### 1. Exercise Form Updates
- Add a new required field "Kinetic Chain" to the Create Exercise form
- Add the same field to the Update Exercise form
- Field should be a dropdown/select component with two options:
  - Compound (ID: kineticchaintype-f5d5a2de-9c4e-4b87-b8c3-5d1e17d0b1f4)
  - Isolation (ID: kineticchaintype-2b3e7cb2-9a3e-4c9a-88d8-b7c019c90d1b)

### 2. Validation Rules
- For non-REST exercises: Kinetic Chain is REQUIRED
- For REST exercises: Kinetic Chain must be NULL/not set
- Display appropriate error messages:
  - "Kinetic chain is required for non-rest exercises"
  - "Kinetic chain must be null for rest exercises"

### 3. Exercise List Display
- Add a "Kinetic Chain" column to the exercises list table
- Display the kinetic chain type name (Compound/Isolation)
- For REST exercises, display "-" or "N/A"

### 4. Exercise Details View
- Display the Kinetic Chain type in the exercise details
- Show both the name and description of the kinetic chain type

### 5. Data Models
Update the following TypeScript interfaces/types:
- `Exercise` interface: Add `kineticChain?: ReferenceData`
- `CreateExerciseRequest`: Add `kineticChainId?: string`
- `UpdateExerciseRequest`: Add `kineticChainId?: string`

### 6. API Integration
- Fetch kinetic chain types from: `GET /api/referenceTables/kineticChainTypes`
- Include `kineticChainId` in POST/PUT requests for exercises
- Handle the `kineticChain` object in GET responses

## Acceptance Criteria
1. [x] Create Exercise form includes Kinetic Chain dropdown (required for non-REST)
2. [x] Update Exercise form includes Kinetic Chain dropdown with current value
3. [x] Validation prevents saving non-REST exercises without Kinetic Chain
4. [x] Validation prevents saving REST exercises with Kinetic Chain
5. [x] Exercise list displays Kinetic Chain column
6. [x] Exercise details show Kinetic Chain information
7. [x] All API requests include the kineticChainId field correctly
8. [x] Error handling displays appropriate messages for validation failures

## Technical Notes
- The Kinetic Chain types are reference data and should be cached after first fetch
- Follow the same pattern as the Difficulty Level implementation
- Ensure proper TypeScript typing throughout

## Time Estimate
Based on the API implementation taking 1h 13m, estimate: 2-3 hours

## Dependencies
- API's FEAT-019 must be completed and deployed
- Reference table endpoint for kinetic chain types must be available

## Testing Considerations
- Test creating exercises with both Compound and Isolation types
- Test validation for REST exercises (should not allow kinetic chain)
- Test editing existing exercises to add/change kinetic chain
- Verify proper display in list and detail views