# Training Programs and Progressive Overload

## Introduction

A single workout is just one piece of a larger fitness journey. The ultimate goal is to create structured Training Programs—collections of workouts organized over time to achieve specific adaptations. This document outlines how to model multi-week programs that implement progressive overload, the fundamental principle driving long-term fitness improvements.

## The Training Program Model

### Core Concept

A Training Program is a structured plan that:
- Organizes multiple workout templates into a coherent schedule
- Spans a defined time period (typically 4-12 weeks)
- Implements progression strategies to ensure continuous adaptation
- Adapts to user performance and feedback

### The `TrainingProgram` Entity

The top-level container for a complete training plan.

**Key Attributes:**
- **`ProgramID`:** Unique identifier
- **`Name`:** Descriptive title (e.g., "12-Week Strength Builder")
- **`Description`:** Overview of program goals and methodology
- **`ProgramType`:** Enumeration of program focus:
  - `Strength`
  - `Hypertrophy`
  - `PowerBuilding`
  - `Endurance`
  - `GeneralFitness`
  - `SportSpecific`
- **`Duration`:** Total length in weeks
- **`DaysPerWeek`:** Training frequency
- **`ExperienceLevel`:** Target audience (`Beginner`, `Intermediate`, `Advanced`)
- **`Equipment Required`:** List of necessary equipment

### The `ProgramSchedule` Entity

Links workout templates to specific days within the program.

**Key Attributes:**
- **`ProgramID`:** Foreign key to TrainingProgram
- **`WeekNumber`:** Which week of the program (1-N)
- **`DayNumber`:** Which day of the week (1-7)
- **`WorkoutTemplateID`:** The workout to perform
- **`IsOptional`:** Boolean for optional/bonus workouts
- **`AlternativeWorkoutTemplateID`:** Alternative option for flexibility

### Example Program Structure

```
Week 1:
  Day 1: Upper Body Strength A
  Day 2: Lower Body Strength A
  Day 3: Rest
  Day 4: Upper Body Hypertrophy
  Day 5: Lower Body Hypertrophy
  Day 6: Conditioning (Optional)
  Day 7: Rest

Week 2:
  Day 1: Upper Body Strength B
  Day 2: Lower Body Strength B
  ... (pattern continues with variations)
```

## Progressive Overload Implementation

### Overview

Progressive overload is the gradual increase of stress placed on the body during training. It's the cornerstone principle that drives adaptation and improvement.

### Progression Strategies

#### 1. Linear Progression
Increase load by a fixed amount each week.

**Implementation:**
```
Week 1: 100 lbs × 3 sets × 10 reps
Week 2: 105 lbs × 3 sets × 10 reps
Week 3: 110 lbs × 3 sets × 10 reps
```

#### 2. Double Progression
Increase reps first, then weight.

**Implementation:**
```
Week 1: 100 lbs × 3 sets × 8 reps
Week 2: 100 lbs × 3 sets × 9 reps
Week 3: 100 lbs × 3 sets × 10 reps
Week 4: 105 lbs × 3 sets × 8 reps
```

#### 3. Undulating Progression
Vary intensity and volume throughout the week.

**Implementation:**
```
Monday: Heavy (85% 1RM × 5 reps)
Wednesday: Medium (75% 1RM × 8 reps)
Friday: Light (65% 1RM × 12 reps)
```

### The `ProgressionRule` Entity

Defines how exercises should progress throughout the program.

**Key Attributes:**
- **`ProgramID`:** Foreign key to TrainingProgram
- **`ExerciseID`:** Specific exercise or null for global rules
- **`ProgressionType`:** Enumeration:
  - `LinearWeight`
  - `LinearReps`
  - `DoubleProgression`
  - `Undulating`
  - `Autoregulated`
- **`WeightIncrement`:** Amount to increase (lbs/kg or percentage)
- **`RepIncrement`:** Reps to add before weight increase
- **`DeloadWeek`:** Which weeks are deload weeks
- **`PerformanceThreshold`:** Success criteria for progression

### Autoregulation Features

#### RPE-Based Progression
Adjust loads based on Rate of Perceived Exertion rather than fixed percentages.

**Implementation:**
- Target RPE for each set (e.g., "3 sets @ RPE 8")
- Automatic load adjustment based on previous session RPE

#### Performance-Based Adjustments
Modify future workouts based on actual performance.

**Rules:**
- If all sets completed with 2+ reps in reserve: Increase weight
- If failed to complete prescribed reps: Maintain or decrease weight
- If technique breakdown noted: Reduce weight and focus on form

## Periodization Models

### Linear Periodization
Progressive increase in intensity with decrease in volume.

**Phases:**
1. **Hypertrophy Phase** (Weeks 1-4): High volume, moderate intensity
2. **Strength Phase** (Weeks 5-8): Moderate volume, high intensity
3. **Peak/Deload** (Week 9): Low volume, very high intensity or rest

### Block Periodization
Focused blocks targeting specific adaptations.

**Blocks:**
1. **Accumulation:** High volume, moderate intensity
2. **Intensification:** Moderate volume, high intensity
3. **Realization:** Low volume, peak intensity

### Daily Undulating Periodization (DUP)
Vary training stimulus within each week.

**Example:**
- Monday: Strength (3-5 reps @ 85-90%)
- Wednesday: Power (6-8 reps @ 70-80%)
- Friday: Hypertrophy (10-12 reps @ 65-75%)

## Program Customization and Adaptation

### User-Specific Modifications

#### The `UserProgramEnrollment` Entity
Tracks a user's participation in a program.

**Key Attributes:**
- **`UserID`:** The enrolled user
- **`ProgramID`:** The program they're following
- **`StartDate`:** When they began
- **`CurrentWeek`:** Their progress through the program
- **`CompletionStatus`:** Active, Paused, Completed, Abandoned
- **`CustomModifications`:** JSON field for user-specific adjustments

#### Adaptive Features
1. **Injury Modifications:** Substitute exercises for injured body parts
2. **Equipment Substitutions:** Alternative exercises based on available equipment
3. **Schedule Flexibility:** Compress or extend weeks based on user availability

### Progress Tracking Integration

#### Automatic Progression Calculation
Based on logged workout data:
1. Analyze completion rates for prescribed sets/reps
2. Calculate average RPE across working sets
3. Apply progression rules to generate next workout's targets

#### Performance Metrics
- **Strength Gains:** Track 1RM estimates over program duration
- **Volume Progression:** Monitor total volume trends
- **Adherence Rate:** Percentage of scheduled workouts completed
- **Recovery Metrics:** Track session-to-session performance changes

## Implementation Considerations

### Database Schema

```sql
-- Core program structure
CREATE TABLE TrainingPrograms (
    ProgramID UUID PRIMARY KEY,
    Name VARCHAR(255),
    Duration INT,
    DaysPerWeek INT,
    -- ... other fields
);

CREATE TABLE ProgramSchedule (
    ScheduleID UUID PRIMARY KEY,
    ProgramID UUID REFERENCES TrainingPrograms,
    WeekNumber INT,
    DayNumber INT,
    WorkoutTemplateID UUID REFERENCES WorkoutTemplates,
    -- ... other fields
);

CREATE TABLE ProgressionRules (
    RuleID UUID PRIMARY KEY,
    ProgramID UUID REFERENCES TrainingPrograms,
    ExerciseID UUID REFERENCES Exercises,
    ProgressionType VARCHAR(50),
    -- ... progression parameters
);
```

### Business Logic

1. **Workout Generation Service:**
   - Takes program template and user history
   - Applies progression rules
   - Generates specific workout with adjusted targets

2. **Progress Analysis Service:**
   - Evaluates workout logs against program expectations
   - Identifies when to progress, maintain, or deload
   - Provides feedback and recommendations

3. **Program Recommendation Engine:**
   - Analyzes user goals, experience, and equipment
   - Suggests appropriate programs
   - Tracks program success rates for continuous improvement

## Conclusion

Training Programs with integrated progressive overload transform individual workouts into comprehensive fitness journeys. By structuring workouts over time and systematically increasing training demands, the platform guides users toward their goals while preventing plateaus and overtraining.

This system's flexibility accommodates various periodization models and progression strategies while maintaining the simplicity needed for user engagement. The integration with workout logging enables intelligent, responsive programming that adapts to each user's unique progress and needs, making "GetFitterGetBigger" a true training partner rather than just a workout tracker.