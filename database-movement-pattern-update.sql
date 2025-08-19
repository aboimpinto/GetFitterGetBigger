-- GetFitterGetBigger Movement Pattern Update Script
-- This script adds missing movement patterns to all exercises
-- Movement Pattern IDs:
-- '750e8400-e29b-41d4-a716-446655440001' - Push
-- '750e8400-e29b-41d4-a716-446655440002' - Pull
-- '750e8400-e29b-41d4-a716-446655440003' - Squat
-- '750e8400-e29b-41d4-a716-446655440004' - Hinge
-- '750e8400-e29b-41d4-a716-446655440005' - Lunge
-- '750e8400-e29b-41d4-a716-446655440006' - Rotation
-- '750e8400-e29b-41d4-a716-446655440007' - Carry

-- ========================================
-- PART 1: ORIGINAL WORKOUT EXERCISES (some already have patterns)
-- ========================================

-- These are already in the seed file but let's ensure they're complete
-- Skipping: Bench Press, Push-ups, Pull-ups, Barbell Row, Barbell Squat, Romanian Deadlift, Overhead Press, Russian Twists

-- Lateral Raises - Neither push nor pull, but closest to Push pattern (lifting away from body)
INSERT INTO "ExerciseMovementPatterns" ("ExerciseId", "MovementPatternId") VALUES
('950e8400-e29b-41d4-a716-446655440008', '750e8400-e29b-41d4-a716-446655440001') -- Push
ON CONFLICT DO NOTHING;

-- Barbell Curl - Pull pattern (pulling weight toward body)
INSERT INTO "ExerciseMovementPatterns" ("ExerciseId", "MovementPatternId") VALUES
('950e8400-e29b-41d4-a716-446655440009', '750e8400-e29b-41d4-a716-446655440002') -- Pull
ON CONFLICT DO NOTHING;

-- Tricep Dips - Push pattern
INSERT INTO "ExerciseMovementPatterns" ("ExerciseId", "MovementPatternId") VALUES
('950e8400-e29b-41d4-a716-446655440010', '750e8400-e29b-41d4-a716-446655440001') -- Push
ON CONFLICT DO NOTHING;

-- Plank - None (isometric hold, no movement pattern)
-- No pattern needed for static holds

-- ========================================
-- PART 2: WARMUP EXERCISES
-- ========================================

-- Arm Circles - Rotation
INSERT INTO "ExerciseMovementPatterns" ("ExerciseId", "MovementPatternId") VALUES
('950e8400-e29b-41d4-a716-446655440101', '750e8400-e29b-41d4-a716-446655440006'); -- Rotation

-- Jumping Jacks - None (general cardio movement)
-- No specific pattern, it's a full-body cardio movement

-- Jump Rope - None (cardio/plyometric)
-- No specific pattern

-- Chest Fly with Bands - Push pattern (arms moving away from midline)
INSERT INTO "ExerciseMovementPatterns" ("ExerciseId", "MovementPatternId") VALUES
('950e8400-e29b-41d4-a716-446655440104', '750e8400-e29b-41d4-a716-446655440001'); -- Push

-- Scapular Push-ups - Push pattern
INSERT INTO "ExerciseMovementPatterns" ("ExerciseId", "MovementPatternId") VALUES
('950e8400-e29b-41d4-a716-446655440105', '750e8400-e29b-41d4-a716-446655440001'); -- Push

-- Band Pull-aparts - Pull pattern
INSERT INTO "ExerciseMovementPatterns" ("ExerciseId", "MovementPatternId") VALUES
('950e8400-e29b-41d4-a716-446655440106', '750e8400-e29b-41d4-a716-446655440002'); -- Pull

-- Scapular Pulls - Pull pattern
INSERT INTO "ExerciseMovementPatterns" ("ExerciseId", "MovementPatternId") VALUES
('950e8400-e29b-41d4-a716-446655440107', '750e8400-e29b-41d4-a716-446655440002'); -- Pull

-- Bodyweight Squats - Squat pattern
INSERT INTO "ExerciseMovementPatterns" ("ExerciseId", "MovementPatternId") VALUES
('950e8400-e29b-41d4-a716-446655440108', '750e8400-e29b-41d4-a716-446655440003'); -- Squat

-- Leg Swings - None (dynamic mobility, not a specific pattern)
-- No specific pattern

-- Walking Lunges - Lunge pattern
INSERT INTO "ExerciseMovementPatterns" ("ExerciseId", "MovementPatternId") VALUES
('950e8400-e29b-41d4-a716-446655440110', '750e8400-e29b-41d4-a716-446655440005'); -- Lunge

-- Band Dislocations - Rotation pattern
INSERT INTO "ExerciseMovementPatterns" ("ExerciseId", "MovementPatternId") VALUES
('950e8400-e29b-41d4-a716-446655440111', '750e8400-e29b-41d4-a716-446655440006'); -- Rotation

-- YTW Raises - Pull pattern (scapular retraction)
INSERT INTO "ExerciseMovementPatterns" ("ExerciseId", "MovementPatternId") VALUES
('950e8400-e29b-41d4-a716-446655440112', '750e8400-e29b-41d4-a716-446655440002'); -- Pull

-- Cat-Cow Stretch - None (spinal flexion/extension mobility)
-- No specific pattern for mobility work

-- Dead Bug - None (core stability exercise)
-- No specific pattern for core stability

-- ========================================
-- PART 3: COOLDOWN/STRETCHING EXERCISES
-- ========================================

-- Stretches generally don't have movement patterns as they're static holds
-- We'll skip all pure stretching exercises:
-- Chest Doorway Stretch, Lat Stretch, Quad Stretch, Hamstring Stretch,
-- Shoulder Cross-body Stretch, Tricep Overhead Stretch, Bicep Wall Stretch,
-- Child Pose, Pigeon Pose, Cobra Stretch

-- Foam rolling exercises have a rolling/carry pattern but not traditional movement patterns
-- Foam Roll Back, Foam Roll Quads - No pattern needed

-- ========================================
-- PART 4: ALTERNATIVE EXERCISES
-- ========================================

-- Push-up Variations - All Push pattern
INSERT INTO "ExerciseMovementPatterns" ("ExerciseId", "MovementPatternId") VALUES
('950e8400-e29b-41d4-a716-446655440301', '750e8400-e29b-41d4-a716-446655440001'), -- Wall Push-ups - Push
('950e8400-e29b-41d4-a716-446655440302', '750e8400-e29b-41d4-a716-446655440001'), -- Incline Push-ups - Push
('950e8400-e29b-41d4-a716-446655440303', '750e8400-e29b-41d4-a716-446655440001'), -- Knee Push-ups - Push
('950e8400-e29b-41d4-a716-446655440304', '750e8400-e29b-41d4-a716-446655440001'), -- Decline Push-ups - Push
('950e8400-e29b-41d4-a716-446655440305', '750e8400-e29b-41d4-a716-446655440001'), -- Diamond Push-ups - Push
('950e8400-e29b-41d4-a716-446655440306', '750e8400-e29b-41d4-a716-446655440001'), -- TRX Push-ups - Push
('950e8400-e29b-41d4-a716-446655440307', '750e8400-e29b-41d4-a716-446655440001'); -- Weighted Push-ups - Push

-- Bench Press Variations - All Push pattern
INSERT INTO "ExerciseMovementPatterns" ("ExerciseId", "MovementPatternId") VALUES
('950e8400-e29b-41d4-a716-446655440308', '750e8400-e29b-41d4-a716-446655440001'), -- Dumbbell Bench Press - Push
('950e8400-e29b-41d4-a716-446655440309', '750e8400-e29b-41d4-a716-446655440001'), -- Incline Barbell Press - Push
('950e8400-e29b-41d4-a716-446655440310', '750e8400-e29b-41d4-a716-446655440001'), -- Decline Barbell Press - Push
('950e8400-e29b-41d4-a716-446655440311', '750e8400-e29b-41d4-a716-446655440001'); -- Smith Machine Bench Press - Push

-- Pull-up Variations - All Pull pattern
INSERT INTO "ExerciseMovementPatterns" ("ExerciseId", "MovementPatternId") VALUES
('950e8400-e29b-41d4-a716-446655440312', '750e8400-e29b-41d4-a716-446655440002'), -- Assisted Pull-ups - Pull
('950e8400-e29b-41d4-a716-446655440313', '750e8400-e29b-41d4-a716-446655440002'), -- Negative Pull-ups - Pull
('950e8400-e29b-41d4-a716-446655440314', '750e8400-e29b-41d4-a716-446655440002'), -- Chin-ups - Pull
('950e8400-e29b-41d4-a716-446655440315', '750e8400-e29b-41d4-a716-446655440002'), -- Wide-grip Pull-ups - Pull
('950e8400-e29b-41d4-a716-446655440316', '750e8400-e29b-41d4-a716-446655440002'), -- Weighted Pull-ups - Pull
('950e8400-e29b-41d4-a716-446655440317', '750e8400-e29b-41d4-a716-446655440002'); -- Lat Pulldown - Pull

-- Squat Variations
INSERT INTO "ExerciseMovementPatterns" ("ExerciseId", "MovementPatternId") VALUES
('950e8400-e29b-41d4-a716-446655440318', '750e8400-e29b-41d4-a716-446655440003'), -- Goblet Squat - Squat
('950e8400-e29b-41d4-a716-446655440319', '750e8400-e29b-41d4-a716-446655440003'), -- Front Squat - Squat
('950e8400-e29b-41d4-a716-446655440320', '750e8400-e29b-41d4-a716-446655440003'), -- Box Squat - Squat
('950e8400-e29b-41d4-a716-446655440321', '750e8400-e29b-41d4-a716-446655440005'), -- Bulgarian Split Squat - Lunge (single leg pattern)
('950e8400-e29b-41d4-a716-446655440322', '750e8400-e29b-41d4-a716-446655440003'), -- Pistol Squat - Squat
('950e8400-e29b-41d4-a716-446655440323', '750e8400-e29b-41d4-a716-446655440003'); -- Leg Press - Squat

-- Plank Variations - None (isometric/stability exercises don't have movement patterns)
-- Knee Plank, Side Plank, Plank with Leg Lift, TRX Plank - No pattern

-- Ab Wheel Rollout - Combined Push/Pull pattern (rollout is push, return is pull)
INSERT INTO "ExerciseMovementPatterns" ("ExerciseId", "MovementPatternId") VALUES
('950e8400-e29b-41d4-a716-446655440328', '750e8400-e29b-41d4-a716-446655440001'), -- Ab Wheel Rollout - Push
('950e8400-e29b-41d4-a716-446655440328', '750e8400-e29b-41d4-a716-446655440002'); -- Ab Wheel Rollout - Pull

-- ========================================
-- SUMMARY OF EXERCISES WITHOUT PATTERNS
-- ========================================
-- These exercises intentionally don't have movement patterns:
-- 1. Static/Isometric exercises (Planks, Side Planks)
-- 2. Pure stretching exercises (all stretches)
-- 3. Mobility exercises (Cat-Cow, Leg Swings)
-- 4. General cardio movements (Jumping Jacks, Jump Rope)
-- 5. Foam rolling exercises
-- 6. Core stability exercises without specific patterns (Dead Bug)

-- ========================================
-- Script completed successfully
-- ========================================