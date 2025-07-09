# Feature: Movement Patterns Data Update

## Feature ID: FEAT-021
## Created: 2025-01-08
## Status: SUBMITTED
## Target PI: PI-2025-Q1

## Description
Update the MovementPatterns reference table to include a comprehensive set of movement patterns used in fitness training. This includes updating existing entries and adding new movement patterns with detailed descriptions.

## Business Value
- Provides a complete taxonomy of movement patterns for exercise categorization
- Enables better exercise organization and filtering in the application
- Supports advanced training program design by categorizing exercises by movement pattern
- Improves user experience by offering more granular movement pattern selection

## User Stories
- As a Personal Trainer, I want to categorize exercises by specific movement patterns (e.g., Horizontal Push vs Vertical Push) so that I can create more balanced workout programs
- As a Client, I want to see exercises organized by detailed movement patterns so that I can understand which muscle groups and movement mechanics are being trained
- As an Admin, I want a comprehensive set of movement patterns in the system so that all exercises can be properly categorized

## Acceptance Criteria
- [ ] Existing movement patterns (Push, Pull, Squat) are updated with detailed descriptions
- [ ] New movement patterns are added: Hinge, Lunge, Carry, Rotation/Anti-Rotation
- [ ] Push and Pull patterns are split into subcategories: Horizontal Push, Vertical Push, Horizontal Pull, Vertical Pull
- [ ] All movement patterns have comprehensive descriptions including examples
- [ ] Database migration successfully updates all data without errors
- [ ] No existing exercises or relationships are broken by the update

## Technical Specifications
### Data to be Updated/Added:
1. **Update Existing:**
   - Squat: Update description to comprehensive version
   - Push: Convert to either Horizontal Push or Vertical Push
   - Pull: Convert to either Horizontal Pull or Vertical Pull

2. **Add New Patterns:**
   - Hinge
   - Lunge
   - Horizontal Push
   - Vertical Push
   - Horizontal Pull
   - Vertical Pull
   - Carry
   - Rotation/Anti-Rotation

### Migration Requirements:
- Use EF Core migration to update data
- Ensure idempotent operations (can be run multiple times safely)
- Preserve existing IDs where possible
- Handle the conversion of existing Push/Pull to their subcategories

## Dependencies
- Access to MovementPattern entity and repository
- Entity Framework Core migrations
- Existing MovementPattern table structure

## Notes
- This is a data-only update, no schema changes required
- Must carefully handle the conversion of existing Push/Pull patterns to avoid breaking existing exercise relationships
- Consider the order of operations in the migration to ensure data integrity