# Workout Workflow Feature

This document describes the workflow for a user engaging with a workout within the application.

## 1. Workout Selection and Initial Summary

Once a user selects a specific workout from the available options, the application navigates to a workout summary screen. This screen provides the user with an overview of the chosen workout, including:

* **Expected Time to Finish:** An estimation of the total duration of the workout.
* **Number of Rounds:** The total count of rounds in the workout.
* **Exercises per Round:** A list of exercises included in each round.
* **Number of Reps:** The prescribed number of repetitions for each RepBaseSet exercise.
* **Duration for TimeBaseSet:** The prescribed time for each TimeBaseSet exercise or rest period.

This initial summary allows the user to mentally prepare for the workout and understand its structure before commencing.

## 2. Starting the Workout

After reviewing the summary, the user can start the workout. The application will then guide the user through each round and set sequentially. During the workout, the screen will display relevant information about the **current exercise**, such as:

* Name of the exercise.
* Number of repetitions to perform (for RepBaseSet).
* Remaining time (for TimeBaseSet).
* Current round and set number.
* Visual aids or instructions for the exercise (if applicable).

## 3. Set Types

The workout is composed of different types of sets:

### 3.1. RepBaseSet

* **Description:** In a RepBaseSet, the user performs a specific number of repetitions for a given exercise.
* **Completion:** The set is considered complete once the user has performed all the prescribed repetitions.
* **Advancement:** To move to the next set, round, or to finish the workout, the user **must manually click a button**.

### 3.2. TimeBaseSet

* **Description:** In a TimeBaseSet, the user performs a specific exercise (or rests) for a predetermined duration.
* **Functionality:** This set type can be used for continuous exercise over a period or serve as a timed rest period between other sets or rounds.
* **Completion:** The set is considered complete once the timer for the set reaches zero.
* **Advancement:** The application **automatically advances** to the next set, next round, or finishes the workout when the time for the TimeBaseSet expires. There is no manual intervention required from the user to proceed.

## 4. Workout Progression

The application manages the flow from one set to the next and from one round to the next.

* **Within a Round:**
    * If a **RepBaseSet** is completed, the user clicks a button to proceed to the next set within the current round.
    * If a **TimeBaseSet** is completed, the app automatically proceeds to the next set within the current round.
* **End of a Round:**
    * If a **RepBaseSet** is the last set of a round, the user clicks a button to proceed to the next round (if any) or to finish the workout.
    * If a **TimeBaseSet** is the last set of a round, the app automatically proceeds to the next round (if any) or to finish the workout.
* **End of Workout:**
    * If a **RepBaseSet** is the last set of the last round, the user clicks a button to finish the workout.
    * If a **TimeBaseSet** is the last set of the last round, the app automatically finishes the workout.

## 5. Workout Completion and Final Summary

Upon completion of the entire workout (either by manual confirmation after the last RepBaseSet or automatically after the last TimeBaseSet), the application navigates to a workout summary page. This page displays detailed statistics and information about the completed session, which may include:

* **Total Time Taken:** The actual duration it took the user to complete the workout.
* **Time per Set:** The duration spent on each individual set.
* **Time per Round:** The total time spent on each round.
* **Number of Reps Done:** A log of repetitions completed for each RepBaseSet exercise.
* **Average Heart Rate (if tracked):**
* **Calories Burned (if estimated):**
* **Date and Time of Workout:**
* Other relevant statistical information or performance metrics.

This summary provides the user with valuable feedback on their performance and tracks their progress over time.