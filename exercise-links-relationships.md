# Exercise Links Relationships

This document describes the relationships between Workout, Warmup, and Cooldown exercises for the GetFitterGetBigger application. These relationships will be implemented through the ExerciseLinks table once the feature is functional.

## Link Types
- **WARMUP**: Exercise that prepares the body for the main workout
- **COOLDOWN**: Exercise that helps recovery after the main workout
- **ALTERNATIVE_EASIER**: Easier variation of an exercise
- **ALTERNATIVE_HARDER**: More challenging variation of an exercise
- **ALTERNATIVE_SAME**: Similar difficulty variation

## Warmup -> Workout Relationships

### Chest/Push Exercises
| Warmup Exercise | Workout Exercise(s) | Notes |
|----------------|-------------------|--------|
| Arm Circles | Push-ups, Bench Press, Overhead Press | General shoulder warmup |
| Chest Fly with Bands | Barbell Bench Press, Dumbbell Bench Press, All push variations | Chest activation |
| Scapular Push-ups | Push-ups, All bench press variations | Shoulder blade activation |
| Jumping Jacks | Any upper body workout | General full-body warmup |

### Back/Pull Exercises
| Warmup Exercise | Workout Exercise(s) | Notes |
|----------------|-------------------|--------|
| Band Pull-aparts | Pull-ups, Barbell Row, Lat Pulldown | Upper back activation |
| Scapular Pulls | Pull-ups, Chin-ups, All pull-up variations | Scapular activation |
| Arm Circles | Pull-ups, Rows | Shoulder mobility |

### Leg Exercises
| Warmup Exercise | Workout Exercise(s) | Notes |
|----------------|-------------------|--------|
| Bodyweight Squats | Barbell Squat, All squat variations | Hip and knee warmup |
| Leg Swings | Romanian Deadlift, Squats, Lunges | Hip mobility |
| Walking Lunges | Barbell Squat, Bulgarian Split Squat | Dynamic leg warmup |
| Jumping Jacks | Any leg workout | General warmup |

### Shoulder Exercises
| Warmup Exercise | Workout Exercise(s) | Notes |
|----------------|-------------------|--------|
| Band Dislocations | Overhead Press, Lateral Raises | Shoulder mobility |
| YTW Raises | Overhead Press, Lateral Raises | Shoulder activation |
| Arm Circles | All shoulder exercises | General shoulder warmup |

### Arm Exercises
| Warmup Exercise | Workout Exercise(s) | Notes |
|----------------|-------------------|--------|
| Arm Circles | Barbell Curl, Tricep Dips | General arm warmup |
| Band Pull-aparts | Barbell Curl | Bicep activation |
| Scapular Push-ups | Tricep Dips | Tricep preparation |

### Core Exercises
| Warmup Exercise | Workout Exercise(s) | Notes |
|----------------|-------------------|--------|
| Cat-Cow Stretch | Plank, Russian Twists | Spinal mobility |
| Dead Bug | Plank, All core exercises | Core activation |

## Workout -> Cooldown Relationships

### Chest/Push Exercises
| Workout Exercise | Cooldown Exercise(s) | Notes |
|-----------------|---------------------|--------|
| Barbell Bench Press | Chest Doorway Stretch, Tricep Overhead Stretch | Chest and tricep stretching |
| Push-ups | Chest Doorway Stretch, Child Pose | Chest and shoulder recovery |
| All bench variations | Chest Doorway Stretch, Foam Roll Back | Chest and back recovery |

### Back/Pull Exercises
| Workout Exercise | Cooldown Exercise(s) | Notes |
|-----------------|---------------------|--------|
| Pull-ups | Lat Stretch, Bicep Wall Stretch | Lat and bicep stretching |
| Barbell Row | Lat Stretch, Foam Roll Back | Back recovery |
| All pull variations | Lat Stretch, Child Pose | Back and shoulder recovery |

### Leg Exercises
| Workout Exercise | Cooldown Exercise(s) | Notes |
|-----------------|---------------------|--------|
| Barbell Squat | Quad Stretch, Hamstring Stretch, Foam Roll Quads | Full leg recovery |
| Romanian Deadlift | Hamstring Stretch, Pigeon Pose | Hamstring and hip recovery |
| All squat variations | Quad Stretch, Foam Roll Quads | Quadriceps recovery |

### Shoulder Exercises
| Workout Exercise | Cooldown Exercise(s) | Notes |
|-----------------|---------------------|--------|
| Overhead Press | Shoulder Cross-body Stretch, Tricep Overhead Stretch | Shoulder and tricep recovery |
| Lateral Raises | Shoulder Cross-body Stretch | Shoulder recovery |

### Arm Exercises
| Workout Exercise | Cooldown Exercise(s) | Notes |
|-----------------|---------------------|--------|
| Barbell Curl | Bicep Wall Stretch | Bicep recovery |
| Tricep Dips | Tricep Overhead Stretch | Tricep recovery |

### Core Exercises
| Workout Exercise | Cooldown Exercise(s) | Notes |
|-----------------|---------------------|--------|
| Plank | Cobra Stretch, Child Pose | Core and back recovery |
| Russian Twists | Cobra Stretch, Child Pose | Core recovery |

## Alternative Exercise Relationships

### Push-up Progressions
| Base Exercise | Easier Alternatives | Harder Alternatives |
|--------------|-------------------|-------------------|
| Push-ups | Wall Push-ups, Incline Push-ups, Knee Push-ups | Decline Push-ups, Diamond Push-ups, TRX Push-ups, Weighted Push-ups |

### Bench Press Variations
| Base Exercise | Easier Alternatives | Harder/Different Alternatives |
|--------------|-------------------|----------------------------|
| Barbell Bench Press | Smith Machine Bench Press, Dumbbell Bench Press | Incline Barbell Press, Decline Barbell Press |

### Pull-up Progressions
| Base Exercise | Easier Alternatives | Harder Alternatives |
|--------------|-------------------|-------------------|
| Pull-ups | Assisted Pull-ups, Negative Pull-ups, Lat Pulldown | Wide-grip Pull-ups, Weighted Pull-ups |
| Pull-ups | Chin-ups (different grip) | Weighted Pull-ups |

### Squat Progressions
| Base Exercise | Easier Alternatives | Harder Alternatives |
|--------------|-------------------|-------------------|
| Barbell Squat | Goblet Squat, Box Squat, Leg Press, Bodyweight Squats | Front Squat, Bulgarian Split Squat, Pistol Squat |

### Plank Progressions
| Base Exercise | Easier Alternatives | Harder Alternatives |
|--------------|-------------------|-------------------|
| Plank | Knee Plank | Side Plank, Plank with Leg Lift, TRX Plank, Ab Wheel Rollout |

## Shared Warmup/Cooldown Exercises

Some exercises can serve multiple workout exercises:

### Universal Warmups
- **Jumping Jacks**: Can warm up for any workout
- **Arm Circles**: Good for any upper body exercise
- **Jump Rope**: Cardiovascular warmup for any workout

### Universal Cooldowns
- **Child Pose**: Recovery for back, shoulders, and core
- **Foam Roll Back**: Recovery for any back-involved exercise
- **Foam Roll Quads**: Recovery for any leg exercise

## Implementation Notes

When implementing the ExerciseLinks functionality:

1. **Link Type Enum**: Create an enum for link types (WARMUP, COOLDOWN, ALTERNATIVE_EASIER, ALTERNATIVE_HARDER, ALTERNATIVE_SAME)

2. **Bidirectional Relationships**: Some relationships should be bidirectional (e.g., if Exercise A is an alternative to Exercise B, then B is also an alternative to A)

3. **Multiple Links**: One exercise can have multiple warmups, cooldowns, and alternatives

4. **Difficulty Progression**: When suggesting alternatives, consider the user's current fitness level and progression goals

5. **Equipment Availability**: When suggesting alternatives, consider available equipment

6. **Muscle Group Matching**: Ensure alternatives target the same primary muscle groups

7. **Link Strength/Priority**: Consider adding a priority or strength field to indicate primary vs secondary relationships

## Future Enhancements

1. **Smart Recommendations**: Use user history to recommend appropriate warmups and cooldowns
2. **Progressive Overload**: Automatically suggest harder alternatives as users progress
3. **Injury Prevention**: Emphasize specific warmups for exercises with higher injury risk
4. **Recovery Tracking**: Track which cooldowns are most effective for recovery
5. **Custom Links**: Allow trainers to create custom exercise relationships for their clients