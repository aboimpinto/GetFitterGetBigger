# The Workout Template Model: Creating Reusable Workout Blueprints

## Introduction

With a comprehensive `Exercise` taxonomy established, we now turn to the structure that gives these exercises purpose and context: the Workout Template. A workout template is more than a random list of exercises; it is a structured, goal-oriented blueprint designed to elicit a specific physiological response. The challenge lies in creating a data model that is flexible enough to capture the vast diversity of training styles—from simple, full-body routines to complex, periodized programs—while remaining intuitive for the user.

This document focuses on the Workout Template—the reusable blueprint or plan for a workout that a user or coach designs. It addresses core requirements for workout phases (Warmup, Workout, Cooldown) and complex metric assignments through the integration of Exercises and ExerciseLinks.

For information about tracking actual workout performance, see the companion document on Workout Logging.

## Part I: The Anatomy of a Workout Template

A Workout Template is the core, reusable entity that defines the structure and intent of a workout session. It serves as the master plan from which individual workout logs are generated.

### 1.1 The `WorkoutTemplate` Entity

This is the top-level object for any workout plan. It contains the workout's metadata, defining its purpose and general characteristics.

*   **Name:** A clear, descriptive title (e.g., "Upper Body Strength Day," "HIIT Cardio Blast").
*   **Description:** A brief overview of the workout's focus and what the user can expect.
*   **Workout Objective:** A crucial piece of metadata that defines the primary goal of the session. This is best modeled as a link to a separate `WorkoutObjectives` table. Research into fitness applications and coaching principles reveals several common objectives [3, 4, 5]:
    *   Muscular Strength
    *   Hypertrophy (Muscle Growth)
    *   Power
    *   Muscular Endurance
    *   Cardiovascular Conditioning (e.g., HIIT, LISS)
    *   Flexibility & Mobility
    *   General Fitness / Maintenance
*   **Estimated Duration:** The approximate time in minutes required to complete the workout. This is a highly valuable piece of information for users planning their schedule.[6]
*   **Difficulty Level:** An enumerated type ('Beginner', 'Intermediate', 'Advanced') that helps users select appropriate routines.[7]

### 1.2 The `WorkoutTemplateExercise` Entity: Structuring the Flow

This is the heart of the workout model, linking exercises from our library to a specific template. It defines not just *what* exercise to do, but *when* and *how*. This entity directly addresses your requirement for Warmup, Workout, and Cooldown phases.

*   **`WorkoutTemplateID`:** Foreign key linking to the parent `WorkoutTemplate`.
*   **`ExerciseID`:** Foreign key linking to the specific exercise in the `Exercises` table.
*   **`Phase`:** An enumerated type (`Warmup`, `Main`, `Cooldown`) to segment the workout as you requested.[8]
*   **`Order`:** An integer to define the sequence of exercises within each phase. This is critical for logical workout flow (e.g., performing large compound lifts before isolation exercises).[9, 10]
*   **`Notes`:** Optional text for specific instructions or cues for this exercise within this particular workout (e.g., "Focus on explosive movement," "Use a 2-second pause at the bottom").

### 1.3 Defining Performance Metrics: The `SetGroup`

Your request to model complex metric combinations (e.g., "time and reps") highlights a key challenge. A single exercise within a workout might have multiple set structures. For example, a Barbell Squat could be prescribed as "3 sets of 8-10 reps" followed by "1 set of 20 reps."

To handle this cleanly, we introduce a `SetGroup` entity. Each `WorkoutTemplateExercise` can have one or more `SetGroup`s associated with it.

**`SetGroup` Entity:**
*   **`WorkoutTemplateExerciseID`:** Foreign key linking back to the exercise entry in the workout.
*   **`Order`:** The sequence of this set group for the given exercise.
*   **`TargetSets`:** The number of sets to be performed (e.g., 3).
*   **`TargetReps`:** The number of repetitions per set (e.g., 10). Can be a range (e.g., "8-12").
*   **`TargetWeight`:** The prescribed weight. This can be stored as a specific value, a percentage of 1-Rep Max (1RM), or based on Rate of Perceived Exertion (RPE).[11]
*   **`TargetDurationSeconds`:** The target time for timed sets (e.g., 60 for a plank).
*   **`RestBetweenSetsSeconds`:** The prescribed rest period after each set, a crucial variable in workout programming.[10, 12]
*   **`ExecutionProtocol`:** An enumerated type to handle your complex metric combinations.
    *   **`Standard`:** Perform the work (reps/duration), then rest. This covers your "Reps," "Reps and Weight," and "Time" cases.
    *   **`AMRAP` (As Many Reps As Possible):** The `TargetDurationSeconds` defines a window, and the user's goal is to complete as many `TargetReps` as possible.
    *   **`EMOM` (Every Minute On The Minute):** This covers your "time and reps" scenario. The user performs `TargetReps` at the start of a minute and rests for the remainder of that minute. `TargetDurationSeconds` would define the total length (e.g., 600 for a 10-minute EMOM).
    *   **`ForTime`:** The user's goal is to complete the prescribed sets and reps as quickly as possible.

This `SetGroup` structure provides the flexibility to define virtually any combination of performance metrics for an exercise within a workout.

## Conclusion

The Workout Template model provides a flexible and powerful system for creating reusable workout blueprints. By defining clear structures for exercises, phases, and performance metrics through the `WorkoutTemplate`, `WorkoutTemplateExercise`, and `SetGroup` entities, we create a foundation that can accommodate diverse training styles and objectives.

This template-based approach, combined with the proper integration of Exercises and ExerciseLinks, ensures that workout creation remains intuitive while supporting complex training methodologies. The model's flexibility in handling various execution protocols (Standard, AMRAP, EMOM, ForTime) and metric combinations makes it suitable for users ranging from beginners to advanced athletes.

For advanced training concepts like supersets and specialized set types, see the Advanced Training Concepts documentation. For multi-week programming and progressive overload, refer to the Training Programs documentation. To understand how these templates are used to track actual workout performance, see the Workout Logging documentation.