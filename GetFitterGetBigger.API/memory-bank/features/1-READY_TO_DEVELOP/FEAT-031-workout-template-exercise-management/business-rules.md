# Business Rules: Workout Template Exercise Management

This document consolidates all business rules and clarifications for FEAT-031.

## Template Lifecycle

**IMPORTANT**: WorkoutStateId in the WorkoutTemplate entity IS the template lifecycle state mechanism.

### States (via WorkoutState entity)
1. **Draft**
   - Full CRUD operations allowed
   - Exercises can be added, removed, modified
   - Metadata can be changed
   - Template structure can be reorganized

2. **Production**
   - Template is locked for structural changes
   - Only description and media can be updated
   - Cannot add/remove/reorder exercises
   - Cannot modify exercise metadata
   - Used for active workout plans

3. **Archived**
   - Template cannot be modified at all
   - Still accessible for historical reference
   - Can be cloned to create new draft template

### State Transitions
- Draft → Production (when PT publishes template)
- Production → Archived (when PT retires template)
- Archived → Draft (only via cloning, creates new template)

### State Validation
- Templates can only move to Production if all exercises are Active
- If an exercise becomes Archived while template is in Draft, PT must replace it before publishing
- Production templates remain valid even if exercises become Archived (historical accuracy)

## Exercise Lifecycle

### Exercise States
1. **Active**
   - Can be added to draft templates
   - Visible in exercise selection lists
   - Full functionality available

2. **Archived**
   - Cannot be added to new templates
   - Remains in existing templates
   - Hidden from exercise selection lists
   - Core properties cannot change

3. **Deleted (Soft Delete)**
   - Marked with IsDeleted flag
   - Never physically removed from database
   - Maintains referential integrity

### Exercise Modification Rules
- **Before Production**: Exercise properties can be modified
- **After Production**: Core properties locked, only description/media can change
- **Archiving Impact**: PT must archive exercise and create new one for major changes
- **Template Impact**: Archiving exercise may trigger template archiving

## Round Management

### Round Rules
1. **Numbering**: Always sequential starting from 1
2. **Gaps**: Not allowed - rounds must be renumbered after deletion
3. **Empty Rounds**: Allowed during draft/customization phase
4. **Round Limit**: No hard limit (UI may impose soft limits for usability)
5. **Deletion Impact**: When deleting round N, rounds N+1, N+2... become N, N+1...

### Round Operations
- **Add Round**: Created with next sequential number
- **Delete Round**: Triggers renumbering of subsequent rounds
- **Copy Round**: Creates NEW round at target number with new GUIDs for all exercises (target round doesn't exist before copy)
- **Reorder Within Round**: Updates OrderInRound for affected exercises

## Auto-Linking Rules

### Core Principles
1. **Smart but Simple**: System doesn't track manual vs auto additions
2. **Orphan Detection**: Based on current usage, not history
3. **No Metadata Inheritance**: Auto-added exercises start with empty `{}`
4. **Multiple Rounds**: Same warmup can exist in different rounds

### Key Scenarios
1. **Manual First**: If PT manually adds warmup, auto-link skips it
2. **Auto First**: If auto-link adds warmup, it's removed when orphaned
3. **Shared Warmups**: Removed only when no workout exercise needs them

## Validation Rules

### Metadata Validation
- **Validation Layer**: Controller/Service validates metadata (implementation detail)
- **Strategy Pattern**: Different validators per protocol
- **Rejection**: Invalid metadata causes exercise addition to fail
- **Empty Default**: Auto-added exercises use `{}` until configured
- **Validation Timing**: On save operation (when adding/updating exercises)

### Template Validation
- **Draft Only**: Modifications only allowed in draft state
- **Exercise State**: Cannot add archived exercises to templates
- **Metadata Required**: All exercises must have metadata (even if `{}`)

## Permission Model (Current Phase)

### Current Implementation
- Single PT with full access to all templates
- No sharing or collaboration features
- No role-based access control
- No concurrent editing concerns (single PT cannot edit same workout on multiple devices simultaneously)

### Future Considerations (Out of Scope)
- Multiple PT support
- Template sharing mechanisms
- Role-based permissions
- Audit trail requirements
- Concurrent editing handling

## Performance Considerations

### Caching Strategy
- **Draft Templates**: Not cached (frequent changes)
- **Production Templates**: Cached with invalidation on change
- **Archived Templates**: Cached indefinitely
- **Exercise Data**: 5-minute cache
- **ExerciseLinks**: 10-minute cache

### Query Optimization
- Use indexes for all foreign keys
- Composite indexes for common query patterns
- Filtered indexes for phase-specific queries
- JSON indexing for future metadata queries

### Pagination and Limits
- **Large Template Support**: Future feature (out of current scope)
- **Current Focus**: Basic exercise management functionality
- **Round Limits**: UI may impose soft limits for usability

## Error Handling

### Concurrency
- Not expected in current single-PT model
- Future: Optimistic concurrency with version checks

### Data Integrity
- Soft delete prevents referential integrity issues
- Cascade rules not needed due to soft delete
- Archive strategy maintains historical accuracy

### Edge Cases
- **Circular References**: Prevented by checking existing exercises
- **Missing Exercise**: Skip with warning log
- **Invalid Metadata**: Reject with specific error message
- **Template State Mismatch**: Reject modifications with state error

## Migration Notes

### Code-First Approach
- Entity classes define schema
- Migrations generated from entities
- All migrations merged before deployment
- No manual SQL scripts needed

### Rollback Strategy
- Not required for new feature development
- Migrations tested in development environment
- Production deployment includes migration validation

## Implementation Priorities

1. **Phase 1**: Core CRUD with Draft templates only
2. **Phase 2**: Auto-linking logic
3. **Phase 3**: Template lifecycle states
4. **Phase 4**: Performance optimization
5. **Future**: Multi-PT support and permissions