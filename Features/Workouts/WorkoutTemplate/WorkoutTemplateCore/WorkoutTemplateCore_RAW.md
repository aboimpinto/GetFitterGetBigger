# Workout Template Core: Creating Reusable Workout Blueprints

## Introduction

The Workout Template Core feature represents the heart of the workout management system within the GetFitterGetBigger ecosystem. This feature provides the comprehensive infrastructure for creating, managing, and executing structured workout blueprints that serve as the foundation for actual workout logging and client training programs.

A workout template is a reusable blueprint that defines exercises organized to achieve specific fitness goals. The template serves as a container that brings exercises together based on a chosen workout category (such as Upper Body or Lower Body), targeting specific objectives (like Muscular Strength or Endurance) through a defined execution protocol (Standard, AMRAP, EMOM, etc.).

This document focuses on the core entities and business logic required to model comprehensive workout templates that can be seamlessly executed across mobile, web, and desktop client applications.

## Part I: The Anatomy of a Workout Template

### 1.1 The WorkoutTemplate Entity

The WorkoutTemplate serves as the master blueprint for any workout session. It contains the workout's metadata, defines its purpose through category and objective selection, and establishes the framework within which exercises are organized and executed.

**Business Purpose:**
A workout template encapsulates the trainer's expertise and programming knowledge into a reusable format. It ensures consistency across training sessions, enables progressive overload tracking, and provides clear guidance for both trainers and clients.

**WorkoutTemplate Entity:**
- **Identifier:** Unique identifier for the template
- **Name:** Clear, descriptive title (e.g., "Upper Body Strength Day," "HIIT Cardio Blast")
- **Description:** Comprehensive overview of the workout's focus and any special instructions
- **Workout Category:** Reference to the type of workout (Upper Body, Lower Body, Full Body, etc.)
- **Workout Objective:** Reference to the training goal (Muscular Strength, Endurance, Power, etc.)
- **Execution Protocol:** Reference to how the workout will be executed (Standard, AMRAP, EMOM, etc.)
- **Estimated Duration:** Approximate time in minutes to complete the workout
- **Difficulty Level:** Beginner, Intermediate, or Advanced
- **Public Status:** Whether the template is available to all users or private
- **Creator:** User who created the template (typically a Personal Trainer)
- **Creation Timestamp:** When the template was created
- **Last Modified:** When the template was last updated
- **Version:** Version number for template evolution tracking
- **Active Status:** Whether the template is currently available for use
- **Tags:** Searchable tags for enhanced discovery

**Template Goal Definition:**
The combination of Workout Category, Workout Objective, and Execution Protocol defines the workout's purpose and structure, guiding exercise selection and organization.

### 1.2 The WorkoutTemplateExercise Entity: Building the Exercise Flow

This entity represents exercises added to the workout template, maintaining their sequence and organization within different workout zones.

**Business Purpose:**
The WorkoutTemplateExercise entity enables trainers to construct workouts by selecting appropriate exercises from the exercise library. The system supports intelligent exercise suggestions based on the chosen workout category and can automatically manage warmup/cooldown associations.

**WorkoutTemplateExercise Entity:**
- **Identifier:** Unique identifier for this exercise entry
- **Workout Template:** Reference to the parent workout template
- **Exercise:** Reference to the specific exercise from the exercise library
- **Zone:** Where this exercise belongs in the workout flow
  - Warmup - Preparatory exercises
  - Main - Primary training exercises
  - Cooldown - Recovery exercises
- **Sequence Order:** The position of this exercise within its zone
- **Exercise Notes:** Optional instructions specific to this exercise in this workout context

**Exercise Intelligence:**
- When an exercise is added to the Main zone, if it has associated warmup or cooldown exercises defined, these are automatically suggested for the appropriate zones
- UI provides category-based exercise suggestions, prioritizing complementary push/pull exercises
- Equipment requirements from selected exercises are automatically aggregated at the template level

### 1.3 The SetConfiguration Entity: Defining Performance Parameters

The SetConfiguration entity models how each exercise should be performed within the workout, supporting various training protocols through the selected execution protocol.

**Business Purpose:**
Modern training requires flexibility in set and rep schemes. The SetConfiguration provides this flexibility while maintaining structure based on the chosen execution protocol.

**SetConfiguration Entity:**
- **Identifier:** Unique identifier for this configuration
- **Workout Template Exercise:** Reference to the exercise entry
- **Configuration Order:** Sequence when multiple configurations exist for one exercise
- **Execution Protocol:** Reference to how these sets should be executed
- **Target Sets:** Number of sets to perform (interpreted based on protocol)
- **Target Reps:** Target repetitions (can be a range like "8-12")
- **Target Duration:** Time-based target in seconds (for timed exercises)
- **Intensity Guideline:** Subjective intensity instruction (e.g., "Leave 2 reps in reserve")

**Protocol-Based Interpretation:**
The selected Execution Protocol determines how the configuration values are interpreted:
- **Standard:** Traditional sets and reps with defined rest periods
- **AMRAP:** As Many Rounds/Reps As Possible within a time limit
- **EMOM:** Every Minute On the Minute execution
- Other protocols as defined in the reference data

**Initial Implementation Note:**
The first execution protocol to be implemented is Standard. Other protocols will be visible in the UI with a "(Not Available)" tag and will default back to "Choose Execution Protocol" when selected.

## Part II: Workout Construction and Management

### 2.1 Exercise Selection and Organization

**Intelligent Exercise Suggestions:**
Based on the selected Workout Category, the system provides intelligent exercise suggestions:
- For Upper Body workouts, suggest upper body exercises
- When a push exercise is selected, prioritize the corresponding pull exercise
- Suggestions require validation by sports professionals

**Automatic Warmup/Cooldown Management:**
- Each exercise can have designated warmup and cooldown exercises
- When adding an exercise to the Main zone, associated warmups/cooldowns are automatically added if not present
- When removing a warmup/cooldown exercise linked to a main exercise, the system alerts the user
- Workouts can be saved with these alerts acknowledged

### 2.2 Equipment Management

**Automatic Equipment Aggregation:**
- Equipment requirements are defined at the exercise level
- When exercises are added to a workout, their equipment needs are automatically compiled
- The workout template maintains a comprehensive equipment list
- No manual equipment entry is required at the workout level

### 2.3 Rest Management

**Rest as an Exercise:**
- Rest periods are implemented as special exercises added to the workout
- Trainers add rest "exercises" where needed in the workout flow
- This approach provides maximum flexibility for rest placement
- No separate rest fields are needed throughout the system

### 2.4 Notes Management

**Hierarchical Note System:**
- Overall workout notes can be added at the template level
- Exercise-specific notes come from the exercise definition
- Additional context notes can be added for each exercise within the workout
- Notes remain contextual to their source (workout or exercise level)

## Part III: Business Rules and Validation

### 3.1 Template Validation Rules

**Zone Flow Validation:**
- Warmup exercises must precede Main exercises
- Cooldown exercises must follow Main exercises
- Each zone maintains its own sequence ordering

**Exercise Compatibility:**
- Exercises must be appropriate for the selected workout category
- System validates exercise selection against workout objectives
- Equipment availability is considered in exercise suggestions

**Execution Protocol Validation:**
- Selected protocol must be compatible with exercise types
- Configuration values must be appropriate for the chosen protocol
- UI adapts based on protocol selection

### 3.2 Safety and User Guidance

**Warmup/Cooldown Alerts:**
- System tracks warmup/cooldown associations
- Alerts users when removing linked exercises
- Allows workout saving with acknowledged alerts
- Maintains exercise safety recommendations

**Equipment Consistency:**
- Automatic equipment list compilation prevents missing equipment issues
- Equipment requirements clearly displayed before workout execution
- Alternative exercise suggestions based on available equipment

## Part IV: User Experience Considerations

### 4.1 Personal Trainer Interface

**Template Creation Workflow:**
1. Select workout category, objective, and execution protocol
2. System adapts UI based on execution protocol selection
3. Browse and select exercises with intelligent suggestions
4. Automatic warmup/cooldown population
5. Add rest exercises as needed
6. Review automatic equipment compilation
7. Add overall workout notes
8. Save and version the template

**Exercise Selection Features:**
- Category-based filtering and suggestions
- Push/pull exercise pairing recommendations
- Visual indicators for exercises with warmup/cooldown associations
- Equipment requirement visibility

### 4.2 Execution Protocol Interface

**Protocol-Specific UI Adaptation:**
- Standard protocol shows traditional sets/reps/rest interface
- Future protocols (AMRAP, EMOM, etc.) will show protocol-specific fields
- Clear indication of available vs. upcoming protocols
- Fallback handling for unavailable protocols

### 4.3 Client Application Integration

**Workout Execution Flow:**
- Clear zone separation (Warmup → Main → Cooldown)
- Equipment checklist before starting
- Exercise-specific notes and instructions
- Rest periods integrated as workout steps
- Progress tracking against template targets

## Part V: Data Relationships

**Core Relationships:**
```
WorkoutTemplate has many WorkoutTemplateExercise entries
WorkoutTemplateExercise has many SetConfiguration entries
WorkoutTemplate references WorkoutCategory (Reference Data)
WorkoutTemplate references WorkoutObjective (Reference Data)
WorkoutTemplate references ExecutionProtocol (Reference Data)
WorkoutTemplateExercise references Exercise (Exercise Feature)
SetConfiguration references ExecutionProtocol (Reference Data)
```

**Automatic Aggregations:**
- Equipment list compiled from selected exercises
- Warmup/cooldown associations managed through exercise relationships
- Rest periods integrated as exercise entries

## Conclusion

The Workout Template Core feature provides a flexible yet structured approach to workout creation. By treating workouts as goal-oriented containers for exercises, supporting intelligent exercise selection, and automatically managing equipment and warmup/cooldown relationships, the system enables trainers to create effective, safe workout programs.

The integration with execution protocols provides future extensibility while the initial Standard protocol implementation ensures immediate usability. The approach of treating rest as exercises and maintaining contextual notes ensures maximum flexibility while keeping the data model clean and intuitive.

This foundation supports professional workout programming while maintaining simplicity for both trainers creating templates and clients executing workouts.