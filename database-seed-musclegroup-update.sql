-- GetFitterGetBigger Muscle Group Update Script
-- This script adds missing primary, secondary, and stabilizer muscle groups to exercises
-- It completes the muscle group assignments for all exercises in the database

-- ========================================
-- PART 1: ORIGINAL WORKOUT EXERCISES
-- ========================================

-- Push-ups: Primary: Pectoralis Major, Secondary: Triceps, Anterior Deltoid, Stabilizer: Core
INSERT INTO "ExerciseMuscleGroups" ("ExerciseId", "MuscleGroupId", "MuscleRoleId") VALUES
('950e8400-e29b-41d4-a716-446655440002', '550e8400-e29b-41d4-a716-446655440001', '5d8e9a7b-3c2d-4f6a-9b8c-1e5d7f3a2c9b'), -- Primary: Pectoralis Major
('950e8400-e29b-41d4-a716-446655440002', '550e8400-e29b-41d4-a716-446655440014', '8c7d6b5a-4e3f-2a1b-9c8d-7e6f5d4c3b2a'), -- Secondary: Triceps
('950e8400-e29b-41d4-a716-446655440002', '550e8400-e29b-41d4-a716-446655440010', '8c7d6b5a-4e3f-2a1b-9c8d-7e6f5d4c3b2a'), -- Secondary: Anterior Deltoid
('950e8400-e29b-41d4-a716-446655440002', '550e8400-e29b-41d4-a716-446655440016', '1a2b3c4d-5e6f-7a8b-9c0d-1e2f3a4b5c6d'); -- Stabilizer: Rectus Abdominis

-- Barbell Row: Primary: Latissimus Dorsi, Secondary: Rhomboids, Trapezius, Biceps
INSERT INTO "ExerciseMuscleGroups" ("ExerciseId", "MuscleGroupId", "MuscleRoleId") VALUES
('950e8400-e29b-41d4-a716-446655440004', '550e8400-e29b-41d4-a716-446655440003', '5d8e9a7b-3c2d-4f6a-9b8c-1e5d7f3a2c9b'), -- Primary: Latissimus Dorsi
('950e8400-e29b-41d4-a716-446655440004', '550e8400-e29b-41d4-a716-446655440005', '8c7d6b5a-4e3f-2a1b-9c8d-7e6f5d4c3b2a'), -- Secondary: Rhomboids
('950e8400-e29b-41d4-a716-446655440004', '550e8400-e29b-41d4-a716-446655440004', '8c7d6b5a-4e3f-2a1b-9c8d-7e6f5d4c3b2a'), -- Secondary: Trapezius
('950e8400-e29b-41d4-a716-446655440004', '550e8400-e29b-41d4-a716-446655440013', '8c7d6b5a-4e3f-2a1b-9c8d-7e6f5d4c3b2a'); -- Secondary: Biceps

-- Romanian Deadlift: Primary: Hamstrings, Secondary: Glutes, Lower Back
INSERT INTO "ExerciseMuscleGroups" ("ExerciseId", "MuscleGroupId", "MuscleRoleId") VALUES
('950e8400-e29b-41d4-a716-446655440006', '550e8400-e29b-41d4-a716-446655440007', '5d8e9a7b-3c2d-4f6a-9b8c-1e5d7f3a2c9b'), -- Primary: Hamstrings
('950e8400-e29b-41d4-a716-446655440006', '550e8400-e29b-41d4-a716-446655440008', '8c7d6b5a-4e3f-2a1b-9c8d-7e6f5d4c3b2a'), -- Secondary: Glutes
('950e8400-e29b-41d4-a716-446655440006', '550e8400-e29b-41d4-a716-446655440018', '1a2b3c4d-5e6f-7a8b-9c0d-1e2f3a4b5c6d'); -- Stabilizer: Lower Back

-- Overhead Press: Primary: Anterior Deltoid, Secondary: Lateral Deltoid, Triceps, Stabilizer: Core
INSERT INTO "ExerciseMuscleGroups" ("ExerciseId", "MuscleGroupId", "MuscleRoleId") VALUES
('950e8400-e29b-41d4-a716-446655440007', '550e8400-e29b-41d4-a716-446655440010', '5d8e9a7b-3c2d-4f6a-9b8c-1e5d7f3a2c9b'), -- Primary: Anterior Deltoid
('950e8400-e29b-41d4-a716-446655440007', '550e8400-e29b-41d4-a716-446655440011', '8c7d6b5a-4e3f-2a1b-9c8d-7e6f5d4c3b2a'), -- Secondary: Lateral Deltoid
('950e8400-e29b-41d4-a716-446655440007', '550e8400-e29b-41d4-a716-446655440014', '8c7d6b5a-4e3f-2a1b-9c8d-7e6f5d4c3b2a'), -- Secondary: Triceps
('950e8400-e29b-41d4-a716-446655440007', '550e8400-e29b-41d4-a716-446655440016', '1a2b3c4d-5e6f-7a8b-9c0d-1e2f3a4b5c6d'); -- Stabilizer: Rectus Abdominis

-- Lateral Raises: Primary: Lateral Deltoid, Secondary: Anterior Deltoid, Posterior Deltoid
INSERT INTO "ExerciseMuscleGroups" ("ExerciseId", "MuscleGroupId", "MuscleRoleId") VALUES
('950e8400-e29b-41d4-a716-446655440008', '550e8400-e29b-41d4-a716-446655440011', '5d8e9a7b-3c2d-4f6a-9b8c-1e5d7f3a2c9b'), -- Primary: Lateral Deltoid
('950e8400-e29b-41d4-a716-446655440008', '550e8400-e29b-41d4-a716-446655440010', '8c7d6b5a-4e3f-2a1b-9c8d-7e6f5d4c3b2a'), -- Secondary: Anterior Deltoid
('950e8400-e29b-41d4-a716-446655440008', '550e8400-e29b-41d4-a716-446655440012', '8c7d6b5a-4e3f-2a1b-9c8d-7e6f5d4c3b2a'); -- Secondary: Posterior Deltoid

-- Barbell Curl: Primary: Biceps, Secondary: Forearms
INSERT INTO "ExerciseMuscleGroups" ("ExerciseId", "MuscleGroupId", "MuscleRoleId") VALUES
('950e8400-e29b-41d4-a716-446655440009', '550e8400-e29b-41d4-a716-446655440013', '5d8e9a7b-3c2d-4f6a-9b8c-1e5d7f3a2c9b'), -- Primary: Biceps
('950e8400-e29b-41d4-a716-446655440009', '550e8400-e29b-41d4-a716-446655440015', '8c7d6b5a-4e3f-2a1b-9c8d-7e6f5d4c3b2a'); -- Secondary: Forearms

-- Tricep Dips: Primary: Triceps, Secondary: Anterior Deltoid, Pectoralis Major (lower)
INSERT INTO "ExerciseMuscleGroups" ("ExerciseId", "MuscleGroupId", "MuscleRoleId") VALUES
('950e8400-e29b-41d4-a716-446655440010', '550e8400-e29b-41d4-a716-446655440014', '5d8e9a7b-3c2d-4f6a-9b8c-1e5d7f3a2c9b'), -- Primary: Triceps
('950e8400-e29b-41d4-a716-446655440010', '550e8400-e29b-41d4-a716-446655440010', '8c7d6b5a-4e3f-2a1b-9c8d-7e6f5d4c3b2a'), -- Secondary: Anterior Deltoid
('950e8400-e29b-41d4-a716-446655440010', '550e8400-e29b-41d4-a716-446655440001', '8c7d6b5a-4e3f-2a1b-9c8d-7e6f5d4c3b2a'); -- Secondary: Pectoralis Major

-- Plank: Primary: Rectus Abdominis, Secondary: Obliques, Stabilizer: Lower Back
INSERT INTO "ExerciseMuscleGroups" ("ExerciseId", "MuscleGroupId", "MuscleRoleId") VALUES
('950e8400-e29b-41d4-a716-446655440011', '550e8400-e29b-41d4-a716-446655440016', '5d8e9a7b-3c2d-4f6a-9b8c-1e5d7f3a2c9b'), -- Primary: Rectus Abdominis
('950e8400-e29b-41d4-a716-446655440011', '550e8400-e29b-41d4-a716-446655440017', '8c7d6b5a-4e3f-2a1b-9c8d-7e6f5d4c3b2a'), -- Secondary: Obliques
('950e8400-e29b-41d4-a716-446655440011', '550e8400-e29b-41d4-a716-446655440018', '1a2b3c4d-5e6f-7a8b-9c0d-1e2f3a4b5c6d'); -- Stabilizer: Lower Back

-- Russian Twists: Primary: Obliques, Secondary: Rectus Abdominis
INSERT INTO "ExerciseMuscleGroups" ("ExerciseId", "MuscleGroupId", "MuscleRoleId") VALUES
('950e8400-e29b-41d4-a716-446655440012', '550e8400-e29b-41d4-a716-446655440017', '5d8e9a7b-3c2d-4f6a-9b8c-1e5d7f3a2c9b'), -- Primary: Obliques
('950e8400-e29b-41d4-a716-446655440012', '550e8400-e29b-41d4-a716-446655440016', '8c7d6b5a-4e3f-2a1b-9c8d-7e6f5d4c3b2a'); -- Secondary: Rectus Abdominis

-- ========================================
-- PART 2: WARMUP EXERCISES - Add Secondary/Stabilizers
-- ========================================

-- Arm Circles: Secondary: Trapezius
INSERT INTO "ExerciseMuscleGroups" ("ExerciseId", "MuscleGroupId", "MuscleRoleId") VALUES
('950e8400-e29b-41d4-a716-446655440101', '550e8400-e29b-41d4-a716-446655440004', '8c7d6b5a-4e3f-2a1b-9c8d-7e6f5d4c3b2a'); -- Secondary: Trapezius

-- Jumping Jacks: Secondary: Calves, Deltoids
INSERT INTO "ExerciseMuscleGroups" ("ExerciseId", "MuscleGroupId", "MuscleRoleId") VALUES
('950e8400-e29b-41d4-a716-446655440102', '550e8400-e29b-41d4-a716-446655440009', '8c7d6b5a-4e3f-2a1b-9c8d-7e6f5d4c3b2a'), -- Secondary: Calves
('950e8400-e29b-41d4-a716-446655440102', '550e8400-e29b-41d4-a716-446655440011', '8c7d6b5a-4e3f-2a1b-9c8d-7e6f5d4c3b2a'); -- Secondary: Lateral Deltoid

-- Jump Rope: Secondary: Quadriceps, Forearms
INSERT INTO "ExerciseMuscleGroups" ("ExerciseId", "MuscleGroupId", "MuscleRoleId") VALUES
('950e8400-e29b-41d4-a716-446655440103', '550e8400-e29b-41d4-a716-446655440006', '8c7d6b5a-4e3f-2a1b-9c8d-7e6f5d4c3b2a'), -- Secondary: Quadriceps
('950e8400-e29b-41d4-a716-446655440103', '550e8400-e29b-41d4-a716-446655440015', '8c7d6b5a-4e3f-2a1b-9c8d-7e6f5d4c3b2a'); -- Secondary: Forearms

-- Chest Fly with Bands: Secondary: Anterior Deltoid
INSERT INTO "ExerciseMuscleGroups" ("ExerciseId", "MuscleGroupId", "MuscleRoleId") VALUES
('950e8400-e29b-41d4-a716-446655440104', '550e8400-e29b-41d4-a716-446655440010', '8c7d6b5a-4e3f-2a1b-9c8d-7e6f5d4c3b2a'); -- Secondary: Anterior Deltoid

-- Scapular Push-ups: Secondary: Serratus, Stabilizer: Core
INSERT INTO "ExerciseMuscleGroups" ("ExerciseId", "MuscleGroupId", "MuscleRoleId") VALUES
('950e8400-e29b-41d4-a716-446655440105', '550e8400-e29b-41d4-a716-446655440004', '8c7d6b5a-4e3f-2a1b-9c8d-7e6f5d4c3b2a'), -- Secondary: Trapezius
('950e8400-e29b-41d4-a716-446655440105', '550e8400-e29b-41d4-a716-446655440016', '1a2b3c4d-5e6f-7a8b-9c0d-1e2f3a4b5c6d'); -- Stabilizer: Rectus Abdominis

-- Band Pull-aparts: Secondary: Posterior Deltoid, Trapezius
INSERT INTO "ExerciseMuscleGroups" ("ExerciseId", "MuscleGroupId", "MuscleRoleId") VALUES
('950e8400-e29b-41d4-a716-446655440106', '550e8400-e29b-41d4-a716-446655440012', '8c7d6b5a-4e3f-2a1b-9c8d-7e6f5d4c3b2a'), -- Secondary: Posterior Deltoid
('950e8400-e29b-41d4-a716-446655440106', '550e8400-e29b-41d4-a716-446655440004', '8c7d6b5a-4e3f-2a1b-9c8d-7e6f5d4c3b2a'); -- Secondary: Trapezius

-- Scapular Pulls: Secondary: Rhomboids
INSERT INTO "ExerciseMuscleGroups" ("ExerciseId", "MuscleGroupId", "MuscleRoleId") VALUES
('950e8400-e29b-41d4-a716-446655440107', '550e8400-e29b-41d4-a716-446655440005', '8c7d6b5a-4e3f-2a1b-9c8d-7e6f5d4c3b2a'); -- Secondary: Rhomboids

-- Bodyweight Squats: Secondary: Glutes, Stabilizer: Core
INSERT INTO "ExerciseMuscleGroups" ("ExerciseId", "MuscleGroupId", "MuscleRoleId") VALUES
('950e8400-e29b-41d4-a716-446655440108', '550e8400-e29b-41d4-a716-446655440008', '8c7d6b5a-4e3f-2a1b-9c8d-7e6f5d4c3b2a'), -- Secondary: Glutes
('950e8400-e29b-41d4-a716-446655440108', '550e8400-e29b-41d4-a716-446655440016', '1a2b3c4d-5e6f-7a8b-9c0d-1e2f3a4b5c6d'); -- Stabilizer: Rectus Abdominis

-- Leg Swings: Secondary: Hip Flexors
INSERT INTO "ExerciseMuscleGroups" ("ExerciseId", "MuscleGroupId", "MuscleRoleId") VALUES
('950e8400-e29b-41d4-a716-446655440109', '550e8400-e29b-41d4-a716-446655440016', '8c7d6b5a-4e3f-2a1b-9c8d-7e6f5d4c3b2a'); -- Secondary: Core (hip flexors)

-- Walking Lunges: Secondary: Glutes, Stabilizer: Core
INSERT INTO "ExerciseMuscleGroups" ("ExerciseId", "MuscleGroupId", "MuscleRoleId") VALUES
('950e8400-e29b-41d4-a716-446655440110', '550e8400-e29b-41d4-a716-446655440008', '8c7d6b5a-4e3f-2a1b-9c8d-7e6f5d4c3b2a'), -- Secondary: Glutes
('950e8400-e29b-41d4-a716-446655440110', '550e8400-e29b-41d4-a716-446655440016', '1a2b3c4d-5e6f-7a8b-9c0d-1e2f3a4b5c6d'); -- Stabilizer: Rectus Abdominis

-- Band Dislocations: Secondary: Posterior Deltoid
INSERT INTO "ExerciseMuscleGroups" ("ExerciseId", "MuscleGroupId", "MuscleRoleId") VALUES
('950e8400-e29b-41d4-a716-446655440111', '550e8400-e29b-41d4-a716-446655440012', '8c7d6b5a-4e3f-2a1b-9c8d-7e6f5d4c3b2a'); -- Secondary: Posterior Deltoid

-- YTW Raises: Secondary: Rhomboids, Trapezius
INSERT INTO "ExerciseMuscleGroups" ("ExerciseId", "MuscleGroupId", "MuscleRoleId") VALUES
('950e8400-e29b-41d4-a716-446655440112', '550e8400-e29b-41d4-a716-446655440005', '8c7d6b5a-4e3f-2a1b-9c8d-7e6f5d4c3b2a'), -- Secondary: Rhomboids
('950e8400-e29b-41d4-a716-446655440112', '550e8400-e29b-41d4-a716-446655440004', '8c7d6b5a-4e3f-2a1b-9c8d-7e6f5d4c3b2a'); -- Secondary: Trapezius

-- Cat-Cow Stretch: Secondary: Rectus Abdominis
INSERT INTO "ExerciseMuscleGroups" ("ExerciseId", "MuscleGroupId", "MuscleRoleId") VALUES
('950e8400-e29b-41d4-a716-446655440113', '550e8400-e29b-41d4-a716-446655440016', '8c7d6b5a-4e3f-2a1b-9c8d-7e6f5d4c3b2a'); -- Secondary: Rectus Abdominis

-- Dead Bug: Secondary: Obliques, Stabilizer: Lower Back
INSERT INTO "ExerciseMuscleGroups" ("ExerciseId", "MuscleGroupId", "MuscleRoleId") VALUES
('950e8400-e29b-41d4-a716-446655440114', '550e8400-e29b-41d4-a716-446655440017', '8c7d6b5a-4e3f-2a1b-9c8d-7e6f5d4c3b2a'), -- Secondary: Obliques
('950e8400-e29b-41d4-a716-446655440114', '550e8400-e29b-41d4-a716-446655440018', '1a2b3c4d-5e6f-7a8b-9c0d-1e2f3a4b5c6d'); -- Stabilizer: Lower Back

-- ========================================
-- PART 3: COOLDOWN EXERCISES - Primary muscles for stretches
-- ========================================

-- Note: Stretches don't have traditional muscle roles, but we assign the primary muscle being stretched

-- Chest Doorway Stretch: Primary: Pectoralis Major
INSERT INTO "ExerciseMuscleGroups" ("ExerciseId", "MuscleGroupId", "MuscleRoleId") VALUES
('950e8400-e29b-41d4-a716-446655440201', '550e8400-e29b-41d4-a716-446655440001', '5d8e9a7b-3c2d-4f6a-9b8c-1e5d7f3a2c9b'); -- Primary: Pectoralis Major

-- Lat Stretch: Primary: Latissimus Dorsi
INSERT INTO "ExerciseMuscleGroups" ("ExerciseId", "MuscleGroupId", "MuscleRoleId") VALUES
('950e8400-e29b-41d4-a716-446655440202', '550e8400-e29b-41d4-a716-446655440003', '5d8e9a7b-3c2d-4f6a-9b8c-1e5d7f3a2c9b'); -- Primary: Latissimus Dorsi

-- Quad Stretch: Primary: Quadriceps
INSERT INTO "ExerciseMuscleGroups" ("ExerciseId", "MuscleGroupId", "MuscleRoleId") VALUES
('950e8400-e29b-41d4-a716-446655440203', '550e8400-e29b-41d4-a716-446655440006', '5d8e9a7b-3c2d-4f6a-9b8c-1e5d7f3a2c9b'); -- Primary: Quadriceps

-- Hamstring Stretch: Primary: Hamstrings
INSERT INTO "ExerciseMuscleGroups" ("ExerciseId", "MuscleGroupId", "MuscleRoleId") VALUES
('950e8400-e29b-41d4-a716-446655440204', '550e8400-e29b-41d4-a716-446655440007', '5d8e9a7b-3c2d-4f6a-9b8c-1e5d7f3a2c9b'); -- Primary: Hamstrings

-- Shoulder Cross-body Stretch: Primary: Posterior Deltoid
INSERT INTO "ExerciseMuscleGroups" ("ExerciseId", "MuscleGroupId", "MuscleRoleId") VALUES
('950e8400-e29b-41d4-a716-446655440205', '550e8400-e29b-41d4-a716-446655440012', '5d8e9a7b-3c2d-4f6a-9b8c-1e5d7f3a2c9b'); -- Primary: Posterior Deltoid

-- Tricep Overhead Stretch: Primary: Triceps
INSERT INTO "ExerciseMuscleGroups" ("ExerciseId", "MuscleGroupId", "MuscleRoleId") VALUES
('950e8400-e29b-41d4-a716-446655440206', '550e8400-e29b-41d4-a716-446655440014', '5d8e9a7b-3c2d-4f6a-9b8c-1e5d7f3a2c9b'); -- Primary: Triceps

-- Bicep Wall Stretch: Primary: Biceps
INSERT INTO "ExerciseMuscleGroups" ("ExerciseId", "MuscleGroupId", "MuscleRoleId") VALUES
('950e8400-e29b-41d4-a716-446655440207', '550e8400-e29b-41d4-a716-446655440013', '5d8e9a7b-3c2d-4f6a-9b8c-1e5d7f3a2c9b'); -- Primary: Biceps

-- Child Pose: Primary: Lower Back, Secondary: Lats
INSERT INTO "ExerciseMuscleGroups" ("ExerciseId", "MuscleGroupId", "MuscleRoleId") VALUES
('950e8400-e29b-41d4-a716-446655440208', '550e8400-e29b-41d4-a716-446655440018', '5d8e9a7b-3c2d-4f6a-9b8c-1e5d7f3a2c9b'), -- Primary: Lower Back
('950e8400-e29b-41d4-a716-446655440208', '550e8400-e29b-41d4-a716-446655440003', '8c7d6b5a-4e3f-2a1b-9c8d-7e6f5d4c3b2a'); -- Secondary: Latissimus Dorsi

-- Foam Roll Back: Primary: All back muscles (Lats, Rhomboids, Trapezius)
INSERT INTO "ExerciseMuscleGroups" ("ExerciseId", "MuscleGroupId", "MuscleRoleId") VALUES
('950e8400-e29b-41d4-a716-446655440209', '550e8400-e29b-41d4-a716-446655440003', '5d8e9a7b-3c2d-4f6a-9b8c-1e5d7f3a2c9b'), -- Primary: Latissimus Dorsi
('950e8400-e29b-41d4-a716-446655440209', '550e8400-e29b-41d4-a716-446655440005', '5d8e9a7b-3c2d-4f6a-9b8c-1e5d7f3a2c9b'), -- Primary: Rhomboids
('950e8400-e29b-41d4-a716-446655440209', '550e8400-e29b-41d4-a716-446655440004', '5d8e9a7b-3c2d-4f6a-9b8c-1e5d7f3a2c9b'); -- Primary: Trapezius

-- Foam Roll Quads: Primary: Quadriceps
INSERT INTO "ExerciseMuscleGroups" ("ExerciseId", "MuscleGroupId", "MuscleRoleId") VALUES
('950e8400-e29b-41d4-a716-446655440210', '550e8400-e29b-41d4-a716-446655440006', '5d8e9a7b-3c2d-4f6a-9b8c-1e5d7f3a2c9b'); -- Primary: Quadriceps

-- Pigeon Pose: Primary: Glutes, Secondary: Hip Flexors
INSERT INTO "ExerciseMuscleGroups" ("ExerciseId", "MuscleGroupId", "MuscleRoleId") VALUES
('950e8400-e29b-41d4-a716-446655440211', '550e8400-e29b-41d4-a716-446655440008', '5d8e9a7b-3c2d-4f6a-9b8c-1e5d7f3a2c9b'), -- Primary: Glutes
('950e8400-e29b-41d4-a716-446655440211', '550e8400-e29b-41d4-a716-446655440006', '8c7d6b5a-4e3f-2a1b-9c8d-7e6f5d4c3b2a'); -- Secondary: Quadriceps (hip flexors)

-- Cobra Stretch: Primary: Rectus Abdominis
INSERT INTO "ExerciseMuscleGroups" ("ExerciseId", "MuscleGroupId", "MuscleRoleId") VALUES
('950e8400-e29b-41d4-a716-446655440212', '550e8400-e29b-41d4-a716-446655440016', '5d8e9a7b-3c2d-4f6a-9b8c-1e5d7f3a2c9b'); -- Primary: Rectus Abdominis

-- ========================================
-- PART 4: ALTERNATIVE EXERCISES - Add Secondary/Stabilizers
-- ========================================

-- Wall Push-ups: Secondary: Triceps, Anterior Deltoid
INSERT INTO "ExerciseMuscleGroups" ("ExerciseId", "MuscleGroupId", "MuscleRoleId") VALUES
('950e8400-e29b-41d4-a716-446655440301', '550e8400-e29b-41d4-a716-446655440014', '8c7d6b5a-4e3f-2a1b-9c8d-7e6f5d4c3b2a'), -- Secondary: Triceps
('950e8400-e29b-41d4-a716-446655440301', '550e8400-e29b-41d4-a716-446655440010', '8c7d6b5a-4e3f-2a1b-9c8d-7e6f5d4c3b2a'); -- Secondary: Anterior Deltoid

-- Incline Push-ups: Secondary: Triceps, Anterior Deltoid
INSERT INTO "ExerciseMuscleGroups" ("ExerciseId", "MuscleGroupId", "MuscleRoleId") VALUES
('950e8400-e29b-41d4-a716-446655440302', '550e8400-e29b-41d4-a716-446655440014', '8c7d6b5a-4e3f-2a1b-9c8d-7e6f5d4c3b2a'), -- Secondary: Triceps
('950e8400-e29b-41d4-a716-446655440302', '550e8400-e29b-41d4-a716-446655440010', '8c7d6b5a-4e3f-2a1b-9c8d-7e6f5d4c3b2a'); -- Secondary: Anterior Deltoid

-- Knee Push-ups: Secondary: Triceps, Anterior Deltoid
INSERT INTO "ExerciseMuscleGroups" ("ExerciseId", "MuscleGroupId", "MuscleRoleId") VALUES
('950e8400-e29b-41d4-a716-446655440303', '550e8400-e29b-41d4-a716-446655440014', '8c7d6b5a-4e3f-2a1b-9c8d-7e6f5d4c3b2a'), -- Secondary: Triceps
('950e8400-e29b-41d4-a716-446655440303', '550e8400-e29b-41d4-a716-446655440010', '8c7d6b5a-4e3f-2a1b-9c8d-7e6f5d4c3b2a'); -- Secondary: Anterior Deltoid

-- Decline Push-ups: Secondary: Triceps, Anterior Deltoid (upper chest focus)
INSERT INTO "ExerciseMuscleGroups" ("ExerciseId", "MuscleGroupId", "MuscleRoleId") VALUES
('950e8400-e29b-41d4-a716-446655440304', '550e8400-e29b-41d4-a716-446655440014', '8c7d6b5a-4e3f-2a1b-9c8d-7e6f5d4c3b2a'), -- Secondary: Triceps
('950e8400-e29b-41d4-a716-446655440304', '550e8400-e29b-41d4-a716-446655440010', '8c7d6b5a-4e3f-2a1b-9c8d-7e6f5d4c3b2a'); -- Secondary: Anterior Deltoid

-- Diamond Push-ups: Secondary: Pectoralis Major (inner)
INSERT INTO "ExerciseMuscleGroups" ("ExerciseId", "MuscleGroupId", "MuscleRoleId") VALUES
('950e8400-e29b-41d4-a716-446655440305', '550e8400-e29b-41d4-a716-446655440001', '8c7d6b5a-4e3f-2a1b-9c8d-7e6f5d4c3b2a'); -- Secondary: Pectoralis Major

-- TRX Push-ups: Secondary: Triceps, Stabilizer: Obliques
INSERT INTO "ExerciseMuscleGroups" ("ExerciseId", "MuscleGroupId", "MuscleRoleId") VALUES
('950e8400-e29b-41d4-a716-446655440306', '550e8400-e29b-41d4-a716-446655440014', '8c7d6b5a-4e3f-2a1b-9c8d-7e6f5d4c3b2a'), -- Secondary: Triceps
('950e8400-e29b-41d4-a716-446655440306', '550e8400-e29b-41d4-a716-446655440017', '1a2b3c4d-5e6f-7a8b-9c0d-1e2f3a4b5c6d'); -- Stabilizer: Obliques

-- Weighted Push-ups: Secondary: Triceps, Anterior Deltoid, Stabilizer: Core
INSERT INTO "ExerciseMuscleGroups" ("ExerciseId", "MuscleGroupId", "MuscleRoleId") VALUES
('950e8400-e29b-41d4-a716-446655440307', '550e8400-e29b-41d4-a716-446655440014', '8c7d6b5a-4e3f-2a1b-9c8d-7e6f5d4c3b2a'), -- Secondary: Triceps
('950e8400-e29b-41d4-a716-446655440307', '550e8400-e29b-41d4-a716-446655440010', '8c7d6b5a-4e3f-2a1b-9c8d-7e6f5d4c3b2a'), -- Secondary: Anterior Deltoid
('950e8400-e29b-41d4-a716-446655440307', '550e8400-e29b-41d4-a716-446655440016', '1a2b3c4d-5e6f-7a8b-9c0d-1e2f3a4b5c6d'); -- Stabilizer: Rectus Abdominis

-- Dumbbell Bench Press: Secondary: Triceps, Anterior Deltoid
INSERT INTO "ExerciseMuscleGroups" ("ExerciseId", "MuscleGroupId", "MuscleRoleId") VALUES
('950e8400-e29b-41d4-a716-446655440308', '550e8400-e29b-41d4-a716-446655440014', '8c7d6b5a-4e3f-2a1b-9c8d-7e6f5d4c3b2a'), -- Secondary: Triceps
('950e8400-e29b-41d4-a716-446655440308', '550e8400-e29b-41d4-a716-446655440010', '8c7d6b5a-4e3f-2a1b-9c8d-7e6f5d4c3b2a'); -- Secondary: Anterior Deltoid

-- Incline Barbell Press: Secondary: Triceps, Anterior Deltoid
INSERT INTO "ExerciseMuscleGroups" ("ExerciseId", "MuscleGroupId", "MuscleRoleId") VALUES
('950e8400-e29b-41d4-a716-446655440309', '550e8400-e29b-41d4-a716-446655440014', '8c7d6b5a-4e3f-2a1b-9c8d-7e6f5d4c3b2a'), -- Secondary: Triceps
('950e8400-e29b-41d4-a716-446655440309', '550e8400-e29b-41d4-a716-446655440010', '8c7d6b5a-4e3f-2a1b-9c8d-7e6f5d4c3b2a'); -- Secondary: Anterior Deltoid

-- Decline Barbell Press: Secondary: Triceps
INSERT INTO "ExerciseMuscleGroups" ("ExerciseId", "MuscleGroupId", "MuscleRoleId") VALUES
('950e8400-e29b-41d4-a716-446655440310', '550e8400-e29b-41d4-a716-446655440014', '8c7d6b5a-4e3f-2a1b-9c8d-7e6f5d4c3b2a'); -- Secondary: Triceps

-- Smith Machine Bench Press: Secondary: Triceps, Anterior Deltoid
INSERT INTO "ExerciseMuscleGroups" ("ExerciseId", "MuscleGroupId", "MuscleRoleId") VALUES
('950e8400-e29b-41d4-a716-446655440311', '550e8400-e29b-41d4-a716-446655440014', '8c7d6b5a-4e3f-2a1b-9c8d-7e6f5d4c3b2a'), -- Secondary: Triceps
('950e8400-e29b-41d4-a716-446655440311', '550e8400-e29b-41d4-a716-446655440010', '8c7d6b5a-4e3f-2a1b-9c8d-7e6f5d4c3b2a'); -- Secondary: Anterior Deltoid

-- Assisted Pull-ups: Secondary: Biceps
INSERT INTO "ExerciseMuscleGroups" ("ExerciseId", "MuscleGroupId", "MuscleRoleId") VALUES
('950e8400-e29b-41d4-a716-446655440312', '550e8400-e29b-41d4-a716-446655440013', '8c7d6b5a-4e3f-2a1b-9c8d-7e6f5d4c3b2a'); -- Secondary: Biceps

-- Negative Pull-ups: Secondary: Biceps
INSERT INTO "ExerciseMuscleGroups" ("ExerciseId", "MuscleGroupId", "MuscleRoleId") VALUES
('950e8400-e29b-41d4-a716-446655440313', '550e8400-e29b-41d4-a716-446655440013', '8c7d6b5a-4e3f-2a1b-9c8d-7e6f5d4c3b2a'); -- Secondary: Biceps

-- Chin-ups: Secondary: Latissimus Dorsi (Biceps is primary for chin-ups)
INSERT INTO "ExerciseMuscleGroups" ("ExerciseId", "MuscleGroupId", "MuscleRoleId") VALUES
('950e8400-e29b-41d4-a716-446655440314', '550e8400-e29b-41d4-a716-446655440003', '8c7d6b5a-4e3f-2a1b-9c8d-7e6f5d4c3b2a'); -- Secondary: Latissimus Dorsi

-- Wide-grip Pull-ups: Secondary: Biceps, Rhomboids
INSERT INTO "ExerciseMuscleGroups" ("ExerciseId", "MuscleGroupId", "MuscleRoleId") VALUES
('950e8400-e29b-41d4-a716-446655440315', '550e8400-e29b-41d4-a716-446655440013', '8c7d6b5a-4e3f-2a1b-9c8d-7e6f5d4c3b2a'), -- Secondary: Biceps
('950e8400-e29b-41d4-a716-446655440315', '550e8400-e29b-41d4-a716-446655440005', '8c7d6b5a-4e3f-2a1b-9c8d-7e6f5d4c3b2a'); -- Secondary: Rhomboids

-- Weighted Pull-ups: Secondary: Biceps, Stabilizer: Core
INSERT INTO "ExerciseMuscleGroups" ("ExerciseId", "MuscleGroupId", "MuscleRoleId") VALUES
('950e8400-e29b-41d4-a716-446655440316', '550e8400-e29b-41d4-a716-446655440013', '8c7d6b5a-4e3f-2a1b-9c8d-7e6f5d4c3b2a'), -- Secondary: Biceps
('950e8400-e29b-41d4-a716-446655440316', '550e8400-e29b-41d4-a716-446655440016', '1a2b3c4d-5e6f-7a8b-9c0d-1e2f3a4b5c6d'); -- Stabilizer: Rectus Abdominis

-- Lat Pulldown: Secondary: Biceps, Rhomboids
INSERT INTO "ExerciseMuscleGroups" ("ExerciseId", "MuscleGroupId", "MuscleRoleId") VALUES
('950e8400-e29b-41d4-a716-446655440317', '550e8400-e29b-41d4-a716-446655440013', '8c7d6b5a-4e3f-2a1b-9c8d-7e6f5d4c3b2a'), -- Secondary: Biceps
('950e8400-e29b-41d4-a716-446655440317', '550e8400-e29b-41d4-a716-446655440005', '8c7d6b5a-4e3f-2a1b-9c8d-7e6f5d4c3b2a'); -- Secondary: Rhomboids

-- Goblet Squat: Secondary: Glutes, Stabilizer: Core
INSERT INTO "ExerciseMuscleGroups" ("ExerciseId", "MuscleGroupId", "MuscleRoleId") VALUES
('950e8400-e29b-41d4-a716-446655440318', '550e8400-e29b-41d4-a716-446655440008', '8c7d6b5a-4e3f-2a1b-9c8d-7e6f5d4c3b2a'), -- Secondary: Glutes
('950e8400-e29b-41d4-a716-446655440318', '550e8400-e29b-41d4-a716-446655440016', '1a2b3c4d-5e6f-7a8b-9c0d-1e2f3a4b5c6d'); -- Stabilizer: Rectus Abdominis

-- Front Squat: Secondary: Glutes, Stabilizer: Core (more core than back squat)
INSERT INTO "ExerciseMuscleGroups" ("ExerciseId", "MuscleGroupId", "MuscleRoleId") VALUES
('950e8400-e29b-41d4-a716-446655440319', '550e8400-e29b-41d4-a716-446655440008', '8c7d6b5a-4e3f-2a1b-9c8d-7e6f5d4c3b2a'), -- Secondary: Glutes
('950e8400-e29b-41d4-a716-446655440319', '550e8400-e29b-41d4-a716-446655440016', '1a2b3c4d-5e6f-7a8b-9c0d-1e2f3a4b5c6d'); -- Stabilizer: Rectus Abdominis

-- Box Squat: Secondary: Glutes
INSERT INTO "ExerciseMuscleGroups" ("ExerciseId", "MuscleGroupId", "MuscleRoleId") VALUES
('950e8400-e29b-41d4-a716-446655440320', '550e8400-e29b-41d4-a716-446655440008', '8c7d6b5a-4e3f-2a1b-9c8d-7e6f5d4c3b2a'); -- Secondary: Glutes

-- Bulgarian Split Squat: Secondary: Glutes, Stabilizer: Core
INSERT INTO "ExerciseMuscleGroups" ("ExerciseId", "MuscleGroupId", "MuscleRoleId") VALUES
('950e8400-e29b-41d4-a716-446655440321', '550e8400-e29b-41d4-a716-446655440008', '8c7d6b5a-4e3f-2a1b-9c8d-7e6f5d4c3b2a'), -- Secondary: Glutes
('950e8400-e29b-41d4-a716-446655440321', '550e8400-e29b-41d4-a716-446655440016', '1a2b3c4d-5e6f-7a8b-9c0d-1e2f3a4b5c6d'); -- Stabilizer: Rectus Abdominis

-- Pistol Squat: Secondary: Glutes, Stabilizer: Core
INSERT INTO "ExerciseMuscleGroups" ("ExerciseId", "MuscleGroupId", "MuscleRoleId") VALUES
('950e8400-e29b-41d4-a716-446655440322', '550e8400-e29b-41d4-a716-446655440008', '8c7d6b5a-4e3f-2a1b-9c8d-7e6f5d4c3b2a'), -- Secondary: Glutes
('950e8400-e29b-41d4-a716-446655440322', '550e8400-e29b-41d4-a716-446655440016', '1a2b3c4d-5e6f-7a8b-9c0d-1e2f3a4b5c6d'); -- Stabilizer: Rectus Abdominis

-- Leg Press: Secondary: Glutes, Hamstrings
INSERT INTO "ExerciseMuscleGroups" ("ExerciseId", "MuscleGroupId", "MuscleRoleId") VALUES
('950e8400-e29b-41d4-a716-446655440323', '550e8400-e29b-41d4-a716-446655440008', '8c7d6b5a-4e3f-2a1b-9c8d-7e6f5d4c3b2a'), -- Secondary: Glutes
('950e8400-e29b-41d4-a716-446655440323', '550e8400-e29b-41d4-a716-446655440007', '8c7d6b5a-4e3f-2a1b-9c8d-7e6f5d4c3b2a'); -- Secondary: Hamstrings

-- Knee Plank: Secondary: Obliques
INSERT INTO "ExerciseMuscleGroups" ("ExerciseId", "MuscleGroupId", "MuscleRoleId") VALUES
('950e8400-e29b-41d4-a716-446655440324', '550e8400-e29b-41d4-a716-446655440017', '8c7d6b5a-4e3f-2a1b-9c8d-7e6f5d4c3b2a'); -- Secondary: Obliques

-- Side Plank: Secondary: Rectus Abdominis, Stabilizer: Deltoids
INSERT INTO "ExerciseMuscleGroups" ("ExerciseId", "MuscleGroupId", "MuscleRoleId") VALUES
('950e8400-e29b-41d4-a716-446655440325', '550e8400-e29b-41d4-a716-446655440016', '8c7d6b5a-4e3f-2a1b-9c8d-7e6f5d4c3b2a'), -- Secondary: Rectus Abdominis
('950e8400-e29b-41d4-a716-446655440325', '550e8400-e29b-41d4-a716-446655440011', '1a2b3c4d-5e6f-7a8b-9c0d-1e2f3a4b5c6d'); -- Stabilizer: Lateral Deltoid

-- Plank with Leg Lift: Secondary: Obliques, Glutes, Stabilizer: Lower Back
INSERT INTO "ExerciseMuscleGroups" ("ExerciseId", "MuscleGroupId", "MuscleRoleId") VALUES
('950e8400-e29b-41d4-a716-446655440326', '550e8400-e29b-41d4-a716-446655440017', '8c7d6b5a-4e3f-2a1b-9c8d-7e6f5d4c3b2a'), -- Secondary: Obliques
('950e8400-e29b-41d4-a716-446655440326', '550e8400-e29b-41d4-a716-446655440008', '8c7d6b5a-4e3f-2a1b-9c8d-7e6f5d4c3b2a'), -- Secondary: Glutes
('950e8400-e29b-41d4-a716-446655440326', '550e8400-e29b-41d4-a716-446655440018', '1a2b3c4d-5e6f-7a8b-9c0d-1e2f3a4b5c6d'); -- Stabilizer: Lower Back

-- TRX Plank: Secondary: Obliques, Stabilizer: Deltoids
INSERT INTO "ExerciseMuscleGroups" ("ExerciseId", "MuscleGroupId", "MuscleRoleId") VALUES
('950e8400-e29b-41d4-a716-446655440327', '550e8400-e29b-41d4-a716-446655440017', '8c7d6b5a-4e3f-2a1b-9c8d-7e6f5d4c3b2a'), -- Secondary: Obliques
('950e8400-e29b-41d4-a716-446655440327', '550e8400-e29b-41d4-a716-446655440010', '1a2b3c4d-5e6f-7a8b-9c0d-1e2f3a4b5c6d'); -- Stabilizer: Anterior Deltoid

-- Ab Wheel Rollout: Secondary: Obliques, Lats, Stabilizer: Lower Back
INSERT INTO "ExerciseMuscleGroups" ("ExerciseId", "MuscleGroupId", "MuscleRoleId") VALUES
('950e8400-e29b-41d4-a716-446655440328', '550e8400-e29b-41d4-a716-446655440017', '8c7d6b5a-4e3f-2a1b-9c8d-7e6f5d4c3b2a'), -- Secondary: Obliques
('950e8400-e29b-41d4-a716-446655440328', '550e8400-e29b-41d4-a716-446655440003', '8c7d6b5a-4e3f-2a1b-9c8d-7e6f5d4c3b2a'), -- Secondary: Latissimus Dorsi
('950e8400-e29b-41d4-a716-446655440328', '550e8400-e29b-41d4-a716-446655440018', '1a2b3c4d-5e6f-7a8b-9c0d-1e2f3a4b5c6d'); -- Stabilizer: Lower Back

-- ========================================
-- Script completed successfully
-- ========================================