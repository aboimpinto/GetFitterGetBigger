# FEAT-026: Workout Template Core

## Overview
The Workout Template Core feature provides the comprehensive infrastructure for creating, managing, and executing structured workout blueprints that serve as the foundation for actual workout logging and client training programs within the GetFitterGetBigger ecosystem.

## Business Context
A workout template is a reusable blueprint that defines exercises organized to achieve specific fitness goals. The template serves as a container that brings exercises together based on a chosen workout category (such as Upper Body or Lower Body), targeting specific objectives (like Muscular Strength or Endurance) through a defined execution protocol (Standard, AMRAP, EMOM, etc.).

This feature enables personal trainers to encapsulate their expertise and programming knowledge into reusable formats, ensuring consistency across training sessions, enabling progressive overload tracking, and providing clear guidance for both trainers and clients.

## Target Users
- **Primary**: Personal Trainers who need to create, manage, and assign structured workout programs to their clients
- **Secondary**: Clients who execute workout templates and track their performance against prescribed targets

## Core Requirements

### Data Model Requirements
The feature requires the following entities:
1. **WorkoutTemplate**: Main entity containing template metadata and configuration
2. **WorkoutTemplateExercise**: Links exercises to templates with zone and sequence information
3. **SetConfiguration**: Defines how exercises should be performed (sets, reps, duration)
4. **WorkoutState**: Reference entity for template lifecycle states (DRAFT, PRODUCTION, ARCHIVED)

### API Requirements
Complete CRUD operations for:
- Workout templates management
- Exercise addition/removal within templates
- Set configuration management
- State transitions with validation
- Exercise suggestions based on category and objectives
- Reference data endpoints for workout states

### Business Rules
1. A workout template must have a name, category, objective, and execution protocol
2. Exercises in the workout flow must follow the zone order: Warmup → Main → Cooldown
3. Each exercise within a zone must have a unique sequence order
4. When an exercise with warmup/cooldown associations is added to the Main zone, the system suggests adding those associations
5. Equipment requirements are automatically aggregated from all selected exercises
6. Rest periods are implemented as special exercise entries
7. Workout state transitions follow specific rules:
   - DRAFT → PRODUCTION: Deletes all associated execution logs
   - PRODUCTION → DRAFT: Only allowed if no execution logs exist
   - Any state → ARCHIVED: Preserves all execution logs for historical reference
8. Only workouts in PRODUCTION state can be executed by regular users
9. Archived workouts cannot be executed but historical data remains viewable
10. Exercise suggestions are based on the selected workout category and push/pull complementarity

### Validation Requirements
- **Name**: Required, 3-100 characters
- **Description**: Optional, max 1000 characters
- **Workout Category**: Required, must be valid reference
- **Workout Objective**: Required, must be valid reference
- **Execution Protocol**: Required, must be valid reference
- **Estimated Duration**: Required, 5-300 minutes
- **Difficulty Level**: Required, must be one of: Beginner, Intermediate, Advanced
- **Zone**: Required for exercises, must be one of: Warmup, Main, Cooldown
- **Sequence Order**: Required, must be unique within zone
- **Target Sets**: Required for set configuration, 1-100
- **Target Reps**: Required for rep-based exercises, can be range (e.g., "8-12")
- **Target Duration**: Required for time-based exercises, 1-3600 seconds

## Technical Implementation Requirements

### Database Schema
```sql
-- Core Tables
WorkoutTemplates
WorkoutTemplateExercises
SetConfigurations
WorkoutStates (Reference Table)

-- Relationships
WorkoutTemplates → WorkoutCategories
WorkoutTemplates → WorkoutObjectives
WorkoutTemplates → ExecutionProtocols
WorkoutTemplates → WorkoutStates
WorkoutTemplateExercises → Exercises
SetConfigurations → ExecutionProtocols
```

### Service Layer Requirements
- **WorkoutTemplateService**: Handles template CRUD, state transitions, validation
- **WorkoutTemplateExerciseService**: Manages exercises within templates
- **SetConfigurationService**: Handles set configuration logic
- **WorkoutStateService**: Reference data service for workout states
- All services must use ServiceResult pattern for error handling
- Follow the established UnitOfWork pattern (ReadOnly for queries, Writable for modifications)

### Caching Strategy
- **WorkoutStates**: Eternal cache (365 days) as reference data
- **Template Lists**: Cache with 5-minute TTL
- **Individual Templates**: Cache with 1-hour TTL
- Cache invalidation on any template modification

### Error Handling
| Code | Scenario | User Message |
|------|----------|--------------|
| 400  | Invalid request data | Please check your input and try again |
| 403  | Insufficient permissions | You don't have permission to perform this action |
| 404  | Template not found | The requested workout template was not found |
| 409  | State transition conflict | Cannot change state due to existing execution logs |

## Dependencies
- Exercise Management feature (for exercise library)
- User Management feature (for creator/trainer information)
- Reference Data: WorkoutCategory, WorkoutObjective, ExecutionProtocol

## Security Considerations
- **Data Privacy**: Templates marked as private are only visible to the creator
- **Audit Trail**: All template modifications are logged with timestamp and user ID
- **Role-Based Access**: Personal Trainer role required for create/update/delete operations

## Success Metrics
- Number of workout templates created per trainer
- Template reuse rate across multiple clients
- Workout completion rates for assigned templates
- Time saved in workout programming (measured in hours per week)

## Notes
- Authentication and authorization features are planned for future implementation
- The current implementation focuses on the fundamental workout template structure and management capabilities
- Future enhancements include template sharing marketplace, AI-powered suggestions, and performance analytics