-- GetFitterGetBigger Database Seed Script
-- This script will clear all existing data and insert seed data
-- It respects foreign key constraints for proper deletion and insertion order

-- ========================================
-- PART 1: DELETE ALL DATA
-- ========================================
-- Delete in reverse dependency order to respect foreign keys

-- Delete workout log related data
DELETE FROM "WorkoutLogSets";
DELETE FROM "WorkoutLogs";

-- Delete workout template related data
DELETE FROM "SetConfigurations";
DELETE FROM "WorkoutTemplateExercises";
DELETE FROM "WorkoutTemplateObjectives";
DELETE FROM "WorkoutMuscles";
DELETE FROM "WorkoutTemplates";

-- Delete exercise link data
DELETE FROM "ExerciseLinks";

-- Delete exercise junction tables
DELETE FROM "ExerciseTargetedMuscles";
DELETE FROM "ExerciseMuscleGroups";
DELETE FROM "ExerciseMovementPatterns";
DELETE FROM "ExerciseMetricSupport";
DELETE FROM "ExerciseExerciseTypes";
DELETE FROM "ExerciseEquipment";
DELETE FROM "ExerciseBodyParts";

-- Delete coach notes
DELETE FROM "CoachNotes";

-- Delete exercises
DELETE FROM "Exercises";

-- Delete muscle groups (depends on BodyParts)
DELETE FROM "MuscleGroups";

-- Delete user related data
DELETE FROM "Claims";
DELETE FROM "Users";

-- Delete reference tables (no dependencies)
DELETE FROM "Equipment";
DELETE FROM "MetricTypes";
DELETE FROM "MovementPatterns";
DELETE FROM "MuscleRoles";
DELETE FROM "ExerciseTypes";
DELETE FROM "ExerciseWeightTypes";
DELETE FROM "ExecutionProtocols";
DELETE FROM "KineticChainTypes";
DELETE FROM "DifficultyLevels";
DELETE FROM "BodyParts";
DELETE FROM "WorkoutCategories";
DELETE FROM "WorkoutObjectives";
DELETE FROM "WorkoutStates";

-- ========================================
-- PART 2: INSERT SEED DATA
-- ========================================
-- Insert in dependency order

-- Insert BodyParts
INSERT INTO "BodyParts" ("Id", "Value", "Description", "DisplayOrder", "IsActive") VALUES
('7c5a2d6e-e87e-4c8a-9f1d-9eb734f3df3c', 'Chest', NULL, 1, true),
('b2d89d5c-cb8a-4f5d-8a9e-2c3b76612c5a', 'Back', NULL, 2, true),
('4a6f1b42-5c9b-4c4e-878a-b3d9f2c1f1f5', 'Legs', NULL, 3, true),
('d7e0e24c-f8d4-4b8a-b1e0-cf9c2e6b5d0a', 'Shoulders', NULL, 4, true),
('9c5f1b4e-2b8a-4c9d-8e7f-c5a9e2d7b8c1', 'Arms', NULL, 5, true),
('3e9f8a7d-6c5b-4a3e-8d2f-1b7c9a6d5e4c', 'Core', NULL, 6, true);

-- Insert DifficultyLevels
INSERT INTO "DifficultyLevels" ("Id", "Value", "Description", "DisplayOrder", "IsActive") VALUES
('8a8adb1d-24d2-4979-a5a6-0d760e6da24b', 'Beginner', 'Suitable for those new to fitness', 1, true),
('9c7b59a4-bcd8-48a6-971a-cd67b0a7ab5a', 'Intermediate', 'Suitable for those with some fitness experience', 2, true),
('3e27f9a7-d5a5-4f8e-8a76-6de2d23c9a3c', 'Advanced', 'Suitable for those with significant fitness experience', 3, true);

-- Insert ExerciseTypes
INSERT INTO "ExerciseTypes" ("ExerciseTypeId", "Value", "Description", "DisplayOrder", "IsActive") VALUES
('a1b2c3d4-5e6f-7a8b-9c0d-1e2f3a4b5c6d', 'Warmup', 'Exercises performed to prepare the body for more intense activity', 1, true),
('b2c3d4e5-6f7a-8b9c-0d1e-2f3a4b5c6d7e', 'Workout', 'Main exercises that form the core of the training session', 2, true),
('c3d4e5f6-7a8b-9c0d-1e2f-3a4b5c6d7e8f', 'Cooldown', 'Exercises performed to help the body recover after intense activity', 3, true),
('d4e5f6a7-8b9c-0d1e-2f3a-4b5c6d7e8f9a', 'Rest', 'Periods of rest between exercises or sets', 4, true);

-- Insert ExerciseWeightTypes
INSERT INTO "ExerciseWeightTypes" ("Id", "Code", "Value", "Description", "DisplayOrder", "IsActive") VALUES
('a1f3e2d4-5b6c-4d7e-8f9a-0b1c2d3e4f5a', 'BODYWEIGHT_ONLY', 'Bodyweight Only', 'Exercises that cannot have external weight added', 1, true),
('b2e4d3c5-6a7b-5c8d-9e0f-1a2b3c4d5e6f', 'BODYWEIGHT_OPTIONAL', 'Bodyweight Optional', 'Exercises that can be performed with or without additional weight', 2, true),
('c3d5c4b6-7b8c-6d9e-0f1a-2b3c4d5e6f7a', 'WEIGHT_REQUIRED', 'Weight Required', 'Exercises that must have external weight specified', 3, true),
('d4c6b5a7-8c9d-7e0f-1a2b-3c4d5e6f7a8b', 'MACHINE_WEIGHT', 'Machine Weight', 'Exercises performed on machines with weight stacks', 4, true),
('e5b7a698-9d0e-8f1a-2b3c-4d5e6f7a8b9c', 'NO_WEIGHT', 'No Weight', 'Exercises that do not use weight as a metric', 5, true);

-- Insert KineticChainTypes
INSERT INTO "KineticChainTypes" ("Id", "Value", "Description", "DisplayOrder", "IsActive") VALUES
('f5d5a2de-9c4e-4b87-b8c3-5d1e17d0b1f4', 'Compound', 'Exercises that work multiple muscle groups', 1, true),
('2b3e7cb2-9a3e-4c9a-88d8-b7c019c90d1b', 'Isolation', 'Exercises that work a single muscle group', 2, true);

-- Insert MuscleRoles
INSERT INTO "MuscleRoles" ("MuscleRoleId", "Value", "Description", "DisplayOrder", "IsActive") VALUES
('5d8e9a7b-3c2d-4f6a-9b8c-1e5d7f3a2c9b', 'Primary', 'The main muscle targeted by the exercise', 1, true),
('8c7d6b5a-4e3f-2a1b-9c8d-7e6f5d4c3b2a', 'Secondary', 'A muscle that assists in the exercise', 2, true),
('1a2b3c4d-5e6f-7a8b-9c0d-1e2f3a4b5c6d', 'Stabilizer', 'A muscle that helps stabilize the body during the exercise', 3, true);

-- Insert ExecutionProtocols
INSERT INTO "ExecutionProtocols" ("ExecutionProtocolId", "Code", "Value", "Description", "IntensityLevel", "RepBase", "TimeBase", "RestPattern", "DisplayOrder", "IsActive") VALUES
('30000003-3000-4000-8000-300000000001', 'STANDARD', 'Standard', 'Standard protocol with balanced rep and time components', 'Moderate to High', true, true, '60-90 seconds between sets', 1, true),
('30000003-3000-4000-8000-300000000002', 'SUPERSET', 'Superset', 'Perform exercises back-to-back without rest', 'High', true, false, 'Rest after completing both exercises', 2, true),
('30000003-3000-4000-8000-300000000003', 'DROP_SET', 'Drop Set', 'Reduce weight after reaching failure', 'Very High', true, false, 'Minimal rest between drops', 3, true),
('30000003-3000-4000-8000-300000000004', 'AMRAP', 'AMRAP', 'As Many Reps As Possible in given time', 'High', false, true, 'Fixed rest periods', 4, true);

-- Insert WorkoutCategories
INSERT INTO "WorkoutCategories" ("WorkoutCategoryId", "Value", "Description", "PrimaryMuscleGroups", "Icon", "Color", "DisplayOrder", "IsActive") VALUES
('20000002-2000-4000-8000-200000000001', 'Upper Body - Push', 'Push exercises targeting chest, shoulders, and triceps', 'Chest,Shoulders,Triceps', '💪', '#FF5722', 1, true),
('20000002-2000-4000-8000-200000000002', 'Upper Body - Pull', 'Pull exercises targeting back and biceps', 'Back,Biceps', '🏋️', '#4CAF50', 2, true),
('20000002-2000-4000-8000-200000000003', 'Lower Body', 'Lower body exercises for legs and glutes', 'Quadriceps,Hamstrings,Glutes,Calves', '🦵', '#2196F3', 3, true),
('20000002-2000-4000-8000-200000000004', 'Core', 'Core stability and strength exercises', 'Abs,Obliques,Lower Back', '🎯', '#9C27B0', 4, true),
('20000002-2000-4000-8000-200000000005', 'Full Body', 'Compound exercises engaging multiple muscle groups', 'Multiple', '🏃', '#FF9800', 5, true);

-- Insert WorkoutObjectives
INSERT INTO "WorkoutObjectives" ("WorkoutObjectiveId", "Value", "Description", "DisplayOrder", "IsActive") VALUES
('10000001-1000-4000-8000-100000000001', 'Muscular Strength', 'Build maximum strength through heavy loads and low repetitions', 1, true),
('10000001-1000-4000-8000-100000000002', 'Muscular Hypertrophy', 'Increase muscle size through moderate loads and volume', 2, true),
('10000001-1000-4000-8000-100000000003', 'Muscular Endurance', 'Improve ability to sustain effort over time', 3, true),
('10000001-1000-4000-8000-100000000004', 'Power Development', 'Develop explosive strength and speed', 4, true);

-- Insert WorkoutStates
INSERT INTO "WorkoutStates" ("Id", "Value", "Description", "DisplayOrder", "IsActive") VALUES
('02000001-0000-0000-0000-000000000001', 'DRAFT', 'Template under construction', 1, true),
('02000001-0000-0000-0000-000000000002', 'PRODUCTION', 'Active template for use', 2, true),
('02000001-0000-0000-0000-000000000003', 'ARCHIVED', 'Retired template', 3, true);

-- Insert sample MuscleGroups (you can add more as needed)
INSERT INTO "MuscleGroups" ("Id", "Name", "BodyPartId", "IsActive", "CreatedAt", "UpdatedAt") VALUES
-- Chest muscles
('550e8400-e29b-41d4-a716-446655440001', 'Pectoralis Major', '7c5a2d6e-e87e-4c8a-9f1d-9eb734f3df3c', true, NOW(), NOW()),
('550e8400-e29b-41d4-a716-446655440002', 'Pectoralis Minor', '7c5a2d6e-e87e-4c8a-9f1d-9eb734f3df3c', true, NOW(), NOW()),
-- Back muscles
('550e8400-e29b-41d4-a716-446655440003', 'Latissimus Dorsi', 'b2d89d5c-cb8a-4f5d-8a9e-2c3b76612c5a', true, NOW(), NOW()),
('550e8400-e29b-41d4-a716-446655440004', 'Trapezius', 'b2d89d5c-cb8a-4f5d-8a9e-2c3b76612c5a', true, NOW(), NOW()),
('550e8400-e29b-41d4-a716-446655440005', 'Rhomboids', 'b2d89d5c-cb8a-4f5d-8a9e-2c3b76612c5a', true, NOW(), NOW()),
-- Leg muscles
('550e8400-e29b-41d4-a716-446655440006', 'Quadriceps', '4a6f1b42-5c9b-4c4e-878a-b3d9f2c1f1f5', true, NOW(), NOW()),
('550e8400-e29b-41d4-a716-446655440007', 'Hamstrings', '4a6f1b42-5c9b-4c4e-878a-b3d9f2c1f1f5', true, NOW(), NOW()),
('550e8400-e29b-41d4-a716-446655440008', 'Glutes', '4a6f1b42-5c9b-4c4e-878a-b3d9f2c1f1f5', true, NOW(), NOW()),
('550e8400-e29b-41d4-a716-446655440009', 'Calves', '4a6f1b42-5c9b-4c4e-878a-b3d9f2c1f1f5', true, NOW(), NOW()),
-- Shoulder muscles
('550e8400-e29b-41d4-a716-446655440010', 'Anterior Deltoid', 'd7e0e24c-f8d4-4b8a-b1e0-cf9c2e6b5d0a', true, NOW(), NOW()),
('550e8400-e29b-41d4-a716-446655440011', 'Lateral Deltoid', 'd7e0e24c-f8d4-4b8a-b1e0-cf9c2e6b5d0a', true, NOW(), NOW()),
('550e8400-e29b-41d4-a716-446655440012', 'Posterior Deltoid', 'd7e0e24c-f8d4-4b8a-b1e0-cf9c2e6b5d0a', true, NOW(), NOW()),
-- Arm muscles
('550e8400-e29b-41d4-a716-446655440013', 'Biceps', '9c5f1b4e-2b8a-4c9d-8e7f-c5a9e2d7b8c1', true, NOW(), NOW()),
('550e8400-e29b-41d4-a716-446655440014', 'Triceps', '9c5f1b4e-2b8a-4c9d-8e7f-c5a9e2d7b8c1', true, NOW(), NOW()),
('550e8400-e29b-41d4-a716-446655440015', 'Forearms', '9c5f1b4e-2b8a-4c9d-8e7f-c5a9e2d7b8c1', true, NOW(), NOW()),
-- Core muscles
('550e8400-e29b-41d4-a716-446655440016', 'Rectus Abdominis', '3e9f8a7d-6c5b-4a3e-8d2f-1b7c9a6d5e4c', true, NOW(), NOW()),
('550e8400-e29b-41d4-a716-446655440017', 'Obliques', '3e9f8a7d-6c5b-4a3e-8d2f-1b7c9a6d5e4c', true, NOW(), NOW()),
('550e8400-e29b-41d4-a716-446655440018', 'Lower Back', '3e9f8a7d-6c5b-4a3e-8d2f-1b7c9a6d5e4c', true, NOW(), NOW());

-- Insert sample Equipment
INSERT INTO "Equipment" ("EquipmentId", "Name", "IsActive", "CreatedAt", "UpdatedAt") VALUES
('650e8400-e29b-41d4-a716-446655440001', 'Barbell', true, NOW(), NOW()),
('650e8400-e29b-41d4-a716-446655440002', 'Dumbbell', true, NOW(), NOW()),
('650e8400-e29b-41d4-a716-446655440003', 'Cable Machine', true, NOW(), NOW()),
('650e8400-e29b-41d4-a716-446655440004', 'Pull-up Bar', true, NOW(), NOW()),
('650e8400-e29b-41d4-a716-446655440005', 'Bench', true, NOW(), NOW()),
('650e8400-e29b-41d4-a716-446655440006', 'Smith Machine', true, NOW(), NOW()),
('650e8400-e29b-41d4-a716-446655440007', 'Leg Press Machine', true, NOW(), NOW()),
('650e8400-e29b-41d4-a716-446655440008', 'Resistance Bands', true, NOW(), NOW()),
('650e8400-e29b-41d4-a716-446655440009', 'Kettlebell', true, NOW(), NOW()),
('650e8400-e29b-41d4-a716-446655440010', 'Medicine Ball', true, NOW(), NOW());

-- Insert sample MovementPatterns
INSERT INTO "MovementPatterns" ("Id", "Value", "Description", "DisplayOrder", "IsActive") VALUES
('750e8400-e29b-41d4-a716-446655440001', 'Push', 'Pushing movements away from the body', 1, true),
('750e8400-e29b-41d4-a716-446655440002', 'Pull', 'Pulling movements toward the body', 2, true),
('750e8400-e29b-41d4-a716-446655440003', 'Squat', 'Squatting movement pattern', 3, true),
('750e8400-e29b-41d4-a716-446655440004', 'Hinge', 'Hip hinge movement pattern', 4, true),
('750e8400-e29b-41d4-a716-446655440005', 'Lunge', 'Lunging movement pattern', 5, true),
('750e8400-e29b-41d4-a716-446655440006', 'Rotation', 'Rotational movement pattern', 6, true),
('750e8400-e29b-41d4-a716-446655440007', 'Carry', 'Carrying/locomotion pattern', 7, true);

-- Insert sample MetricTypes
INSERT INTO "MetricTypes" ("MetricTypeId", "Value", "Description", "DisplayOrder", "IsActive") VALUES
('850e8400-e29b-41d4-a716-446655440001', 'Reps', 'Number of repetitions', 1, true),
('850e8400-e29b-41d4-a716-446655440002', 'Weight', 'Weight used in kilograms', 2, true),
('850e8400-e29b-41d4-a716-446655440003', 'Time', 'Duration in seconds', 3, true),
('850e8400-e29b-41d4-a716-446655440004', 'Distance', 'Distance in meters', 4, true),
('850e8400-e29b-41d4-a716-446655440005', 'Heart Rate', 'Heart rate in BPM', 5, true);

-- Insert sample Exercises
INSERT INTO "Exercises" ("Id", "Name", "Description", "DifficultyId", "KineticChainId", "ExerciseWeightTypeId", "IsUnilateral", "IsActive", "ImageUrl", "VideoUrl") VALUES
-- Chest exercises
('950e8400-e29b-41d4-a716-446655440001', 'Barbell Bench Press', 'Classic chest building exercise using a barbell on a bench', '9c7b59a4-bcd8-48a6-971a-cd67b0a7ab5a', 'f5d5a2de-9c4e-4b87-b8c3-5d1e17d0b1f4', 'c3d5c4b6-7b8c-6d9e-0f1a-2b3c4d5e6f7a', false, true, NULL, NULL),
('950e8400-e29b-41d4-a716-446655440002', 'Push-ups', 'Bodyweight exercise for chest, shoulders, and triceps', '8a8adb1d-24d2-4979-a5a6-0d760e6da24b', 'f5d5a2de-9c4e-4b87-b8c3-5d1e17d0b1f4', 'b2e4d3c5-6a7b-5c8d-9e0f-1a2b3c4d5e6f', false, true, NULL, NULL),
-- Back exercises
('950e8400-e29b-41d4-a716-446655440003', 'Pull-ups', 'Bodyweight pulling exercise for back and biceps', '9c7b59a4-bcd8-48a6-971a-cd67b0a7ab5a', 'f5d5a2de-9c4e-4b87-b8c3-5d1e17d0b1f4', 'b2e4d3c5-6a7b-5c8d-9e0f-1a2b3c4d5e6f', false, true, NULL, NULL),
('950e8400-e29b-41d4-a716-446655440004', 'Barbell Row', 'Compound pulling exercise for back development', '9c7b59a4-bcd8-48a6-971a-cd67b0a7ab5a', 'f5d5a2de-9c4e-4b87-b8c3-5d1e17d0b1f4', 'c3d5c4b6-7b8c-6d9e-0f1a-2b3c4d5e6f7a', false, true, NULL, NULL),
-- Leg exercises
('950e8400-e29b-41d4-a716-446655440005', 'Barbell Squat', 'Fundamental lower body exercise', '9c7b59a4-bcd8-48a6-971a-cd67b0a7ab5a', 'f5d5a2de-9c4e-4b87-b8c3-5d1e17d0b1f4', 'c3d5c4b6-7b8c-6d9e-0f1a-2b3c4d5e6f7a', false, true, NULL, NULL),
('950e8400-e29b-41d4-a716-446655440006', 'Romanian Deadlift', 'Hip hinge exercise for hamstrings and glutes', '9c7b59a4-bcd8-48a6-971a-cd67b0a7ab5a', 'f5d5a2de-9c4e-4b87-b8c3-5d1e17d0b1f4', 'c3d5c4b6-7b8c-6d9e-0f1a-2b3c4d5e6f7a', false, true, NULL, NULL),
-- Shoulder exercises
('950e8400-e29b-41d4-a716-446655440007', 'Overhead Press', 'Vertical pressing movement for shoulders', '9c7b59a4-bcd8-48a6-971a-cd67b0a7ab5a', 'f5d5a2de-9c4e-4b87-b8c3-5d1e17d0b1f4', 'c3d5c4b6-7b8c-6d9e-0f1a-2b3c4d5e6f7a', false, true, NULL, NULL),
('950e8400-e29b-41d4-a716-446655440008', 'Lateral Raises', 'Isolation exercise for lateral deltoids', '8a8adb1d-24d2-4979-a5a6-0d760e6da24b', '2b3e7cb2-9a3e-4c9a-88d8-b7c019c90d1b', 'c3d5c4b6-7b8c-6d9e-0f1a-2b3c4d5e6f7a', false, true, NULL, NULL),
-- Arm exercises
('950e8400-e29b-41d4-a716-446655440009', 'Barbell Curl', 'Isolation exercise for biceps', '8a8adb1d-24d2-4979-a5a6-0d760e6da24b', '2b3e7cb2-9a3e-4c9a-88d8-b7c019c90d1b', 'c3d5c4b6-7b8c-6d9e-0f1a-2b3c4d5e6f7a', false, true, NULL, NULL),
('950e8400-e29b-41d4-a716-446655440010', 'Tricep Dips', 'Compound exercise for triceps', '9c7b59a4-bcd8-48a6-971a-cd67b0a7ab5a', 'f5d5a2de-9c4e-4b87-b8c3-5d1e17d0b1f4', 'b2e4d3c5-6a7b-5c8d-9e0f-1a2b3c4d5e6f', false, true, NULL, NULL),
-- Core exercises
('950e8400-e29b-41d4-a716-446655440011', 'Plank', 'Isometric core stability exercise', '8a8adb1d-24d2-4979-a5a6-0d760e6da24b', '2b3e7cb2-9a3e-4c9a-88d8-b7c019c90d1b', 'a1f3e2d4-5b6c-4d7e-8f9a-0b1c2d3e4f5a', false, true, NULL, NULL),
('950e8400-e29b-41d4-a716-446655440012', 'Russian Twists', 'Rotational core exercise', '8a8adb1d-24d2-4979-a5a6-0d760e6da24b', '2b3e7cb2-9a3e-4c9a-88d8-b7c019c90d1b', 'b2e4d3c5-6a7b-5c8d-9e0f-1a2b3c4d5e6f', false, true, NULL, NULL);

-- Insert ExerciseBodyParts
INSERT INTO "ExerciseBodyParts" ("ExerciseId", "BodyPartId") VALUES
-- Bench Press - Chest
('950e8400-e29b-41d4-a716-446655440001', '7c5a2d6e-e87e-4c8a-9f1d-9eb734f3df3c'),
-- Push-ups - Chest, Arms
('950e8400-e29b-41d4-a716-446655440002', '7c5a2d6e-e87e-4c8a-9f1d-9eb734f3df3c'),
('950e8400-e29b-41d4-a716-446655440002', '9c5f1b4e-2b8a-4c9d-8e7f-c5a9e2d7b8c1'),
-- Pull-ups - Back, Arms
('950e8400-e29b-41d4-a716-446655440003', 'b2d89d5c-cb8a-4f5d-8a9e-2c3b76612c5a'),
('950e8400-e29b-41d4-a716-446655440003', '9c5f1b4e-2b8a-4c9d-8e7f-c5a9e2d7b8c1'),
-- Barbell Row - Back
('950e8400-e29b-41d4-a716-446655440004', 'b2d89d5c-cb8a-4f5d-8a9e-2c3b76612c5a'),
-- Barbell Squat - Legs
('950e8400-e29b-41d4-a716-446655440005', '4a6f1b42-5c9b-4c4e-878a-b3d9f2c1f1f5'),
-- Romanian Deadlift - Legs, Back
('950e8400-e29b-41d4-a716-446655440006', '4a6f1b42-5c9b-4c4e-878a-b3d9f2c1f1f5'),
('950e8400-e29b-41d4-a716-446655440006', 'b2d89d5c-cb8a-4f5d-8a9e-2c3b76612c5a'),
-- Overhead Press - Shoulders
('950e8400-e29b-41d4-a716-446655440007', 'd7e0e24c-f8d4-4b8a-b1e0-cf9c2e6b5d0a'),
-- Lateral Raises - Shoulders
('950e8400-e29b-41d4-a716-446655440008', 'd7e0e24c-f8d4-4b8a-b1e0-cf9c2e6b5d0a'),
-- Barbell Curl - Arms
('950e8400-e29b-41d4-a716-446655440009', '9c5f1b4e-2b8a-4c9d-8e7f-c5a9e2d7b8c1'),
-- Tricep Dips - Arms
('950e8400-e29b-41d4-a716-446655440010', '9c5f1b4e-2b8a-4c9d-8e7f-c5a9e2d7b8c1'),
-- Plank - Core
('950e8400-e29b-41d4-a716-446655440011', '3e9f8a7d-6c5b-4a3e-8d2f-1b7c9a6d5e4c'),
-- Russian Twists - Core
('950e8400-e29b-41d4-a716-446655440012', '3e9f8a7d-6c5b-4a3e-8d2f-1b7c9a6d5e4c');

-- Insert ExerciseEquipment
INSERT INTO "ExerciseEquipment" ("ExerciseId", "EquipmentId") VALUES
-- Bench Press - Barbell, Bench
('950e8400-e29b-41d4-a716-446655440001', '650e8400-e29b-41d4-a716-446655440001'),
('950e8400-e29b-41d4-a716-446655440001', '650e8400-e29b-41d4-a716-446655440005'),
-- Pull-ups - Pull-up Bar
('950e8400-e29b-41d4-a716-446655440003', '650e8400-e29b-41d4-a716-446655440004'),
-- Barbell Row - Barbell
('950e8400-e29b-41d4-a716-446655440004', '650e8400-e29b-41d4-a716-446655440001'),
-- Barbell Squat - Barbell
('950e8400-e29b-41d4-a716-446655440005', '650e8400-e29b-41d4-a716-446655440001'),
-- Romanian Deadlift - Barbell
('950e8400-e29b-41d4-a716-446655440006', '650e8400-e29b-41d4-a716-446655440001'),
-- Overhead Press - Barbell
('950e8400-e29b-41d4-a716-446655440007', '650e8400-e29b-41d4-a716-446655440001'),
-- Lateral Raises - Dumbbell
('950e8400-e29b-41d4-a716-446655440008', '650e8400-e29b-41d4-a716-446655440002'),
-- Barbell Curl - Barbell
('950e8400-e29b-41d4-a716-446655440009', '650e8400-e29b-41d4-a716-446655440001'),
-- Tricep Dips - Bench
('950e8400-e29b-41d4-a716-446655440010', '650e8400-e29b-41d4-a716-446655440005'),
-- Russian Twists - Medicine Ball
('950e8400-e29b-41d4-a716-446655440012', '650e8400-e29b-41d4-a716-446655440010');

-- Insert ExerciseExerciseTypes
INSERT INTO "ExerciseExerciseTypes" ("ExerciseId", "ExerciseTypeId") VALUES
-- All exercises are workout type
('950e8400-e29b-41d4-a716-446655440001', 'b2c3d4e5-6f7a-8b9c-0d1e-2f3a4b5c6d7e'),
('950e8400-e29b-41d4-a716-446655440002', 'b2c3d4e5-6f7a-8b9c-0d1e-2f3a4b5c6d7e'),
('950e8400-e29b-41d4-a716-446655440003', 'b2c3d4e5-6f7a-8b9c-0d1e-2f3a4b5c6d7e'),
('950e8400-e29b-41d4-a716-446655440004', 'b2c3d4e5-6f7a-8b9c-0d1e-2f3a4b5c6d7e'),
('950e8400-e29b-41d4-a716-446655440005', 'b2c3d4e5-6f7a-8b9c-0d1e-2f3a4b5c6d7e'),
('950e8400-e29b-41d4-a716-446655440006', 'b2c3d4e5-6f7a-8b9c-0d1e-2f3a4b5c6d7e'),
('950e8400-e29b-41d4-a716-446655440007', 'b2c3d4e5-6f7a-8b9c-0d1e-2f3a4b5c6d7e'),
('950e8400-e29b-41d4-a716-446655440008', 'b2c3d4e5-6f7a-8b9c-0d1e-2f3a4b5c6d7e'),
('950e8400-e29b-41d4-a716-446655440009', 'b2c3d4e5-6f7a-8b9c-0d1e-2f3a4b5c6d7e'),
('950e8400-e29b-41d4-a716-446655440010', 'b2c3d4e5-6f7a-8b9c-0d1e-2f3a4b5c6d7e'),
('950e8400-e29b-41d4-a716-446655440011', 'b2c3d4e5-6f7a-8b9c-0d1e-2f3a4b5c6d7e'),
('950e8400-e29b-41d4-a716-446655440012', 'b2c3d4e5-6f7a-8b9c-0d1e-2f3a4b5c6d7e');

-- Insert ExerciseMuscleGroups
INSERT INTO "ExerciseMuscleGroups" ("ExerciseId", "MuscleGroupId", "MuscleRoleId") VALUES
-- Bench Press - Primary: Chest, Secondary: Triceps, Stabilizer: Anterior Deltoid
('950e8400-e29b-41d4-a716-446655440001', '550e8400-e29b-41d4-a716-446655440001', '5d8e9a7b-3c2d-4f6a-9b8c-1e5d7f3a2c9b'),
('950e8400-e29b-41d4-a716-446655440001', '550e8400-e29b-41d4-a716-446655440014', '8c7d6b5a-4e3f-2a1b-9c8d-7e6f5d4c3b2a'),
('950e8400-e29b-41d4-a716-446655440001', '550e8400-e29b-41d4-a716-446655440010', '1a2b3c4d-5e6f-7a8b-9c0d-1e2f3a4b5c6d'),
-- Pull-ups - Primary: Lats, Secondary: Biceps
('950e8400-e29b-41d4-a716-446655440003', '550e8400-e29b-41d4-a716-446655440003', '5d8e9a7b-3c2d-4f6a-9b8c-1e5d7f3a2c9b'),
('950e8400-e29b-41d4-a716-446655440003', '550e8400-e29b-41d4-a716-446655440013', '8c7d6b5a-4e3f-2a1b-9c8d-7e6f5d4c3b2a'),
-- Barbell Squat - Primary: Quadriceps, Secondary: Glutes
('950e8400-e29b-41d4-a716-446655440005', '550e8400-e29b-41d4-a716-446655440006', '5d8e9a7b-3c2d-4f6a-9b8c-1e5d7f3a2c9b'),
('950e8400-e29b-41d4-a716-446655440005', '550e8400-e29b-41d4-a716-446655440008', '8c7d6b5a-4e3f-2a1b-9c8d-7e6f5d4c3b2a');

-- Insert ExerciseMovementPatterns
INSERT INTO "ExerciseMovementPatterns" ("ExerciseId", "MovementPatternId") VALUES
-- Bench Press - Push
('950e8400-e29b-41d4-a716-446655440001', '750e8400-e29b-41d4-a716-446655440001'),
-- Push-ups - Push
('950e8400-e29b-41d4-a716-446655440002', '750e8400-e29b-41d4-a716-446655440001'),
-- Pull-ups - Pull
('950e8400-e29b-41d4-a716-446655440003', '750e8400-e29b-41d4-a716-446655440002'),
-- Barbell Row - Pull
('950e8400-e29b-41d4-a716-446655440004', '750e8400-e29b-41d4-a716-446655440002'),
-- Barbell Squat - Squat
('950e8400-e29b-41d4-a716-446655440005', '750e8400-e29b-41d4-a716-446655440003'),
-- Romanian Deadlift - Hinge
('950e8400-e29b-41d4-a716-446655440006', '750e8400-e29b-41d4-a716-446655440004'),
-- Overhead Press - Push
('950e8400-e29b-41d4-a716-446655440007', '750e8400-e29b-41d4-a716-446655440001'),
-- Russian Twists - Rotation
('950e8400-e29b-41d4-a716-446655440012', '750e8400-e29b-41d4-a716-446655440006');

-- Insert ExerciseMetricSupport
INSERT INTO "ExerciseMetricSupport" ("ExerciseId", "MetricTypeId") VALUES
-- Most exercises support Reps and Weight
('950e8400-e29b-41d4-a716-446655440001', '850e8400-e29b-41d4-a716-446655440001'),
('950e8400-e29b-41d4-a716-446655440001', '850e8400-e29b-41d4-a716-446655440002'),
('950e8400-e29b-41d4-a716-446655440002', '850e8400-e29b-41d4-a716-446655440001'),
('950e8400-e29b-41d4-a716-446655440003', '850e8400-e29b-41d4-a716-446655440001'),
('950e8400-e29b-41d4-a716-446655440003', '850e8400-e29b-41d4-a716-446655440002'),
('950e8400-e29b-41d4-a716-446655440004', '850e8400-e29b-41d4-a716-446655440001'),
('950e8400-e29b-41d4-a716-446655440004', '850e8400-e29b-41d4-a716-446655440002'),
('950e8400-e29b-41d4-a716-446655440005', '850e8400-e29b-41d4-a716-446655440001'),
('950e8400-e29b-41d4-a716-446655440005', '850e8400-e29b-41d4-a716-446655440002'),
-- Plank supports Time
('950e8400-e29b-41d4-a716-446655440011', '850e8400-e29b-41d4-a716-446655440003');

-- Insert sample Users
INSERT INTO "Users" ("Id", "Email") VALUES
('a50e8400-e29b-41d4-a716-446655440001', 'admin@getfittergetbigger.com'),
('a50e8400-e29b-41d4-a716-446655440002', 'trainer1@getfittergetbigger.com'),
('a50e8400-e29b-41d4-a716-446655440003', 'client1@example.com'),
('a50e8400-e29b-41d4-a716-446655440004', 'client2@example.com');

-- Insert Claims for users
INSERT INTO "Claims" ("Id", "UserId", "ClaimType", "Resource", "ExpirationDate") VALUES
('b50e8400-e29b-41d4-a716-446655440001', 'a50e8400-e29b-41d4-a716-446655440001', 'Admin-Tier', NULL, NULL),
('b50e8400-e29b-41d4-a716-446655440002', 'a50e8400-e29b-41d4-a716-446655440002', 'PT-Tier', NULL, NULL),
('b50e8400-e29b-41d4-a716-446655440003', 'a50e8400-e29b-41d4-a716-446655440003', 'Free-Tier', NULL, NULL),
('b50e8400-e29b-41d4-a716-446655440004', 'a50e8400-e29b-41d4-a716-446655440004', 'Free-Tier', NULL, NULL);

-- Insert sample WorkoutTemplate
INSERT INTO "WorkoutTemplates" ("Id", "Name", "Description", "CategoryId", "DifficultyId", "WorkoutStateId", "EstimatedDurationMinutes", "IsPublic", "Tags", "CreatedAt", "UpdatedAt") VALUES
('c50e8400-e29b-41d4-a716-446655440001', 'Beginner Push Day', 'A simple push workout for beginners focusing on chest, shoulders, and triceps', '20000002-2000-4000-8000-200000000001', '8a8adb1d-24d2-4979-a5a6-0d760e6da24b', '02000001-0000-0000-0000-000000000002', 45, true, '{"beginner", "push", "upper-body"}', NOW(), NOW()),
('c50e8400-e29b-41d4-a716-446655440002', 'Full Body Strength', 'Complete full body workout for overall strength development', '20000002-2000-4000-8000-200000000005', '9c7b59a4-bcd8-48a6-971a-cd67b0a7ab5a', '02000001-0000-0000-0000-000000000002', 60, true, '{"full-body", "strength", "intermediate"}', NOW(), NOW());

-- Insert WorkoutTemplateObjectives
INSERT INTO "WorkoutTemplateObjectives" ("WorkoutTemplateId", "WorkoutObjectiveId") VALUES
('c50e8400-e29b-41d4-a716-446655440001', '10000001-1000-4000-8000-100000000002'),
('c50e8400-e29b-41d4-a716-446655440002', '10000001-1000-4000-8000-100000000001');

-- Insert WorkoutTemplateExercises
INSERT INTO "WorkoutTemplateExercises" ("Id", "WorkoutTemplateId", "ExerciseId", "Zone", "SequenceOrder", "Notes") VALUES
-- Push Day Template
('d50e8400-e29b-41d4-a716-446655440001', 'c50e8400-e29b-41d4-a716-446655440001', '950e8400-e29b-41d4-a716-446655440002', 1, 1, 'Start with bodyweight push-ups as warmup'),
('d50e8400-e29b-41d4-a716-446655440002', 'c50e8400-e29b-41d4-a716-446655440001', '950e8400-e29b-41d4-a716-446655440001', 2, 1, 'Main chest exercise'),
('d50e8400-e29b-41d4-a716-446655440003', 'c50e8400-e29b-41d4-a716-446655440001', '950e8400-e29b-41d4-a716-446655440007', 2, 2, 'Shoulder development'),
('d50e8400-e29b-41d4-a716-446655440004', 'c50e8400-e29b-41d4-a716-446655440001', '950e8400-e29b-41d4-a716-446655440010', 2, 3, 'Tricep finisher'),
-- Full Body Template
('d50e8400-e29b-41d4-a716-446655440005', 'c50e8400-e29b-41d4-a716-446655440002', '950e8400-e29b-41d4-a716-446655440005', 2, 1, 'Start with squats'),
('d50e8400-e29b-41d4-a716-446655440006', 'c50e8400-e29b-41d4-a716-446655440002', '950e8400-e29b-41d4-a716-446655440001', 2, 2, 'Upper body push'),
('d50e8400-e29b-41d4-a716-446655440007', 'c50e8400-e29b-41d4-a716-446655440002', '950e8400-e29b-41d4-a716-446655440004', 2, 3, 'Upper body pull'),
('d50e8400-e29b-41d4-a716-446655440008', 'c50e8400-e29b-41d4-a716-446655440002', '950e8400-e29b-41d4-a716-446655440011', 3, 1, 'Core finisher');

-- Insert SetConfigurations
INSERT INTO "SetConfigurations" ("Id", "WorkoutTemplateExerciseId", "SetNumber", "TargetReps", "TargetWeight", "TargetTimeSeconds", "RestSeconds") VALUES
-- Push-ups warmup
('e50e8400-e29b-41d4-a716-446655440001', 'd50e8400-e29b-41d4-a716-446655440001', 1, '10', NULL, NULL, 60),
('e50e8400-e29b-41d4-a716-446655440002', 'd50e8400-e29b-41d4-a716-446655440001', 2, '10', NULL, NULL, 60),
-- Bench Press
('e50e8400-e29b-41d4-a716-446655440003', 'd50e8400-e29b-41d4-a716-446655440002', 1, '12', 60.0, NULL, 90),
('e50e8400-e29b-41d4-a716-446655440004', 'd50e8400-e29b-41d4-a716-446655440002', 2, '10', 65.0, NULL, 90),
('e50e8400-e29b-41d4-a716-446655440005', 'd50e8400-e29b-41d4-a716-446655440002', 3, '8', 70.0, NULL, 90),
-- Overhead Press
('e50e8400-e29b-41d4-a716-446655440006', 'd50e8400-e29b-41d4-a716-446655440003', 1, '10', 40.0, NULL, 90),
('e50e8400-e29b-41d4-a716-446655440007', 'd50e8400-e29b-41d4-a716-446655440003', 2, '10', 40.0, NULL, 90),
('e50e8400-e29b-41d4-a716-446655440008', 'd50e8400-e29b-41d4-a716-446655440003', 3, '8', 45.0, NULL, 90),
-- Squats
('e50e8400-e29b-41d4-a716-446655440009', 'd50e8400-e29b-41d4-a716-446655440005', 1, '5', 80.0, NULL, 120),
('e50e8400-e29b-41d4-a716-446655440010', 'd50e8400-e29b-41d4-a716-446655440005', 2, '5', 85.0, NULL, 120),
('e50e8400-e29b-41d4-a716-446655440011', 'd50e8400-e29b-41d4-a716-446655440005', 3, '5', 90.0, NULL, 120),
-- Plank
('e50e8400-e29b-41d4-a716-446655440012', 'd50e8400-e29b-41d4-a716-446655440008', 1, NULL, NULL, 30, 60),
('e50e8400-e29b-41d4-a716-446655440013', 'd50e8400-e29b-41d4-a716-446655440008', 2, NULL, NULL, 45, 60),
('e50e8400-e29b-41d4-a716-446655440014', 'd50e8400-e29b-41d4-a716-446655440008', 3, NULL, NULL, 60, 60);

-- Insert sample CoachNotes
INSERT INTO "CoachNotes" ("Id", "ExerciseId", "Text", "Order") VALUES
('f50e8400-e29b-41d4-a716-446655440001', '950e8400-e29b-41d4-a716-446655440001', 'Keep your shoulder blades retracted and maintain a slight arch in your lower back', 1),
('f50e8400-e29b-41d4-a716-446655440002', '950e8400-e29b-41d4-a716-446655440001', 'Lower the bar to your chest with control, then press explosively', 2),
('f50e8400-e29b-41d4-a716-446655440003', '950e8400-e29b-41d4-a716-446655440005', 'Keep your chest up and core engaged throughout the movement', 1),
('f50e8400-e29b-41d4-a716-446655440004', '950e8400-e29b-41d4-a716-446655440005', 'Descend until your thighs are parallel to the ground', 2);

-- ========================================
-- Script completed successfully
-- ========================================