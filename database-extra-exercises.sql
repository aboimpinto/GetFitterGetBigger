-- GetFitterGetBigger Additional Exercises Script
-- This script adds warmup, cooldown, and alternative exercises
-- It includes new equipment and proper muscle group assignments

-- ========================================
-- PART 1: INSERT NEW EQUIPMENT
-- ========================================

INSERT INTO "Equipment" ("EquipmentId", "Name", "IsActive", "CreatedAt", "UpdatedAt") VALUES
('650e8400-e29b-41d4-a716-446655440011', 'TRX Bands', true, NOW(), NOW()),
('650e8400-e29b-41d4-a716-446655440012', 'Foam Roller', true, NOW(), NOW()),
('650e8400-e29b-41d4-a716-446655440013', 'Yoga Mat', true, NOW(), NOW()),
('650e8400-e29b-41d4-a716-446655440014', 'Box/Platform', true, NOW(), NOW()),
('650e8400-e29b-41d4-a716-446655440015', 'Wall', true, NOW(), NOW()),
('650e8400-e29b-41d4-a716-446655440016', 'Jump Rope', true, NOW(), NOW()),
('650e8400-e29b-41d4-a716-446655440017', 'Exercise Ball', true, NOW(), NOW()),
('650e8400-e29b-41d4-a716-446655440018', 'Elastic Band', true, NOW(), NOW()),
('650e8400-e29b-41d4-a716-446655440019', 'Parallette Bars', true, NOW(), NOW()),
('650e8400-e29b-41d4-a716-446655440020', 'Ab Wheel', true, NOW(), NOW());

-- ========================================
-- PART 2: INSERT WARMUP EXERCISES
-- ========================================

INSERT INTO "Exercises" ("Id", "Name", "Description", "DifficultyId", "KineticChainId", "ExerciseWeightTypeId", "IsUnilateral", "IsActive", "ImageUrl", "VideoUrl") VALUES
-- General Warmup Exercises
('950e8400-e29b-41d4-a716-446655440101', 'Arm Circles', 'Dynamic warmup for shoulders and arms', '8a8adb1d-24d2-4979-a5a6-0d760e6da24b', '2b3e7cb2-9a3e-4c9a-88d8-b7c019c90d1b', 'a1f3e2d4-5b6c-4d7e-8f9a-0b1c2d3e4f5a', false, true, NULL, NULL),
('950e8400-e29b-41d4-a716-446655440102', 'Jumping Jacks', 'Full body dynamic warmup', '8a8adb1d-24d2-4979-a5a6-0d760e6da24b', 'f5d5a2de-9c4e-4b87-b8c3-5d1e17d0b1f4', 'a1f3e2d4-5b6c-4d7e-8f9a-0b1c2d3e4f5a', false, true, NULL, NULL),
('950e8400-e29b-41d4-a716-446655440103', 'Jump Rope', 'Cardiovascular warmup exercise', '8a8adb1d-24d2-4979-a5a6-0d760e6da24b', 'f5d5a2de-9c4e-4b87-b8c3-5d1e17d0b1f4', 'e5b7a698-9d0e-8f1a-2b3c-4d5e6f7a8b9c', false, true, NULL, NULL),

-- Chest/Push Warmup Exercises
('950e8400-e29b-41d4-a716-446655440104', 'Chest Fly with Bands', 'Light resistance chest activation', '8a8adb1d-24d2-4979-a5a6-0d760e6da24b', '2b3e7cb2-9a3e-4c9a-88d8-b7c019c90d1b', 'c3d5c4b6-7b8c-6d9e-0f1a-2b3c4d5e6f7a', false, true, NULL, NULL),
('950e8400-e29b-41d4-a716-446655440105', 'Scapular Push-ups', 'Shoulder blade activation for pushing movements', '8a8adb1d-24d2-4979-a5a6-0d760e6da24b', '2b3e7cb2-9a3e-4c9a-88d8-b7c019c90d1b', 'a1f3e2d4-5b6c-4d7e-8f9a-0b1c2d3e4f5a', false, true, NULL, NULL),

-- Back/Pull Warmup Exercises
('950e8400-e29b-41d4-a716-446655440106', 'Band Pull-aparts', 'Upper back activation with resistance bands', '8a8adb1d-24d2-4979-a5a6-0d760e6da24b', '2b3e7cb2-9a3e-4c9a-88d8-b7c019c90d1b', 'c3d5c4b6-7b8c-6d9e-0f1a-2b3c4d5e6f7a', false, true, NULL, NULL),
('950e8400-e29b-41d4-a716-446655440107', 'Scapular Pulls', 'Hanging scapular activation', '8a8adb1d-24d2-4979-a5a6-0d760e6da24b', '2b3e7cb2-9a3e-4c9a-88d8-b7c019c90d1b', 'a1f3e2d4-5b6c-4d7e-8f9a-0b1c2d3e4f5a', false, true, NULL, NULL),

-- Leg Warmup Exercises
('950e8400-e29b-41d4-a716-446655440108', 'Bodyweight Squats', 'Dynamic warmup for legs', '8a8adb1d-24d2-4979-a5a6-0d760e6da24b', 'f5d5a2de-9c4e-4b87-b8c3-5d1e17d0b1f4', 'a1f3e2d4-5b6c-4d7e-8f9a-0b1c2d3e4f5a', false, true, NULL, NULL),
('950e8400-e29b-41d4-a716-446655440109', 'Leg Swings', 'Dynamic hip mobilization', '8a8adb1d-24d2-4979-a5a6-0d760e6da24b', '2b3e7cb2-9a3e-4c9a-88d8-b7c019c90d1b', 'a1f3e2d4-5b6c-4d7e-8f9a-0b1c2d3e4f5a', true, true, NULL, NULL),
('950e8400-e29b-41d4-a716-446655440110', 'Walking Lunges', 'Dynamic lower body warmup', '8a8adb1d-24d2-4979-a5a6-0d760e6da24b', 'f5d5a2de-9c4e-4b87-b8c3-5d1e17d0b1f4', 'a1f3e2d4-5b6c-4d7e-8f9a-0b1c2d3e4f5a', true, true, NULL, NULL),

-- Shoulder Warmup Exercises
('950e8400-e29b-41d4-a716-446655440111', 'Band Dislocations', 'Shoulder mobility with resistance band', '8a8adb1d-24d2-4979-a5a6-0d760e6da24b', '2b3e7cb2-9a3e-4c9a-88d8-b7c019c90d1b', 'c3d5c4b6-7b8c-6d9e-0f1a-2b3c4d5e6f7a', false, true, NULL, NULL),
('950e8400-e29b-41d4-a716-446655440112', 'YTW Raises', 'Shoulder activation in Y, T, and W positions', '8a8adb1d-24d2-4979-a5a6-0d760e6da24b', '2b3e7cb2-9a3e-4c9a-88d8-b7c019c90d1b', 'a1f3e2d4-5b6c-4d7e-8f9a-0b1c2d3e4f5a', false, true, NULL, NULL),

-- Core Warmup Exercises
('950e8400-e29b-41d4-a716-446655440113', 'Cat-Cow Stretch', 'Spinal mobility exercise', '8a8adb1d-24d2-4979-a5a6-0d760e6da24b', '2b3e7cb2-9a3e-4c9a-88d8-b7c019c90d1b', 'a1f3e2d4-5b6c-4d7e-8f9a-0b1c2d3e4f5a', false, true, NULL, NULL),
('950e8400-e29b-41d4-a716-446655440114', 'Dead Bug', 'Core activation and stability', '8a8adb1d-24d2-4979-a5a6-0d760e6da24b', '2b3e7cb2-9a3e-4c9a-88d8-b7c019c90d1b', 'a1f3e2d4-5b6c-4d7e-8f9a-0b1c2d3e4f5a', false, true, NULL, NULL);

-- ========================================
-- PART 3: INSERT COOLDOWN EXERCISES
-- ========================================

INSERT INTO "Exercises" ("Id", "Name", "Description", "DifficultyId", "KineticChainId", "ExerciseWeightTypeId", "IsUnilateral", "IsActive", "ImageUrl", "VideoUrl") VALUES
-- Stretching and Cooldown Exercises
('950e8400-e29b-41d4-a716-446655440201', 'Chest Doorway Stretch', 'Static stretch for chest and shoulders', '8a8adb1d-24d2-4979-a5a6-0d760e6da24b', '2b3e7cb2-9a3e-4c9a-88d8-b7c019c90d1b', 'e5b7a698-9d0e-8f1a-2b3c-4d5e6f7a8b9c', true, true, NULL, NULL),
('950e8400-e29b-41d4-a716-446655440202', 'Lat Stretch', 'Hanging or standing lat stretch', '8a8adb1d-24d2-4979-a5a6-0d760e6da24b', '2b3e7cb2-9a3e-4c9a-88d8-b7c019c90d1b', 'e5b7a698-9d0e-8f1a-2b3c-4d5e6f7a8b9c', true, true, NULL, NULL),
('950e8400-e29b-41d4-a716-446655440203', 'Quad Stretch', 'Standing quadriceps stretch', '8a8adb1d-24d2-4979-a5a6-0d760e6da24b', '2b3e7cb2-9a3e-4c9a-88d8-b7c019c90d1b', 'e5b7a698-9d0e-8f1a-2b3c-4d5e6f7a8b9c', true, true, NULL, NULL),
('950e8400-e29b-41d4-a716-446655440204', 'Hamstring Stretch', 'Seated or standing hamstring stretch', '8a8adb1d-24d2-4979-a5a6-0d760e6da24b', '2b3e7cb2-9a3e-4c9a-88d8-b7c019c90d1b', 'e5b7a698-9d0e-8f1a-2b3c-4d5e6f7a8b9c', true, true, NULL, NULL),
('950e8400-e29b-41d4-a716-446655440205', 'Shoulder Cross-body Stretch', 'Shoulder and upper back stretch', '8a8adb1d-24d2-4979-a5a6-0d760e6da24b', '2b3e7cb2-9a3e-4c9a-88d8-b7c019c90d1b', 'e5b7a698-9d0e-8f1a-2b3c-4d5e6f7a8b9c', true, true, NULL, NULL),
('950e8400-e29b-41d4-a716-446655440206', 'Tricep Overhead Stretch', 'Overhead tricep and shoulder stretch', '8a8adb1d-24d2-4979-a5a6-0d760e6da24b', '2b3e7cb2-9a3e-4c9a-88d8-b7c019c90d1b', 'e5b7a698-9d0e-8f1a-2b3c-4d5e6f7a8b9c', true, true, NULL, NULL),
('950e8400-e29b-41d4-a716-446655440207', 'Bicep Wall Stretch', 'Wall-assisted bicep stretch', '8a8adb1d-24d2-4979-a5a6-0d760e6da24b', '2b3e7cb2-9a3e-4c9a-88d8-b7c019c90d1b', 'e5b7a698-9d0e-8f1a-2b3c-4d5e6f7a8b9c', true, true, NULL, NULL),
('950e8400-e29b-41d4-a716-446655440208', 'Child Pose', 'Relaxation pose for back and shoulders', '8a8adb1d-24d2-4979-a5a6-0d760e6da24b', '2b3e7cb2-9a3e-4c9a-88d8-b7c019c90d1b', 'e5b7a698-9d0e-8f1a-2b3c-4d5e6f7a8b9c', false, true, NULL, NULL),
('950e8400-e29b-41d4-a716-446655440209', 'Foam Roll Back', 'Myofascial release for back', '8a8adb1d-24d2-4979-a5a6-0d760e6da24b', '2b3e7cb2-9a3e-4c9a-88d8-b7c019c90d1b', 'e5b7a698-9d0e-8f1a-2b3c-4d5e6f7a8b9c', false, true, NULL, NULL),
('950e8400-e29b-41d4-a716-446655440210', 'Foam Roll Quads', 'Myofascial release for quadriceps', '8a8adb1d-24d2-4979-a5a6-0d760e6da24b', '2b3e7cb2-9a3e-4c9a-88d8-b7c019c90d1b', 'e5b7a698-9d0e-8f1a-2b3c-4d5e6f7a8b9c', true, true, NULL, NULL),
('950e8400-e29b-41d4-a716-446655440211', 'Pigeon Pose', 'Hip opener stretch', '8a8adb1d-24d2-4979-a5a6-0d760e6da24b', '2b3e7cb2-9a3e-4c9a-88d8-b7c019c90d1b', 'e5b7a698-9d0e-8f1a-2b3c-4d5e6f7a8b9c', true, true, NULL, NULL),
('950e8400-e29b-41d4-a716-446655440212', 'Cobra Stretch', 'Abdominal and hip flexor stretch', '8a8adb1d-24d2-4979-a5a6-0d760e6da24b', '2b3e7cb2-9a3e-4c9a-88d8-b7c019c90d1b', 'e5b7a698-9d0e-8f1a-2b3c-4d5e6f7a8b9c', false, true, NULL, NULL);

-- ========================================
-- PART 4: INSERT ALTERNATIVE EXERCISES
-- ========================================

INSERT INTO "Exercises" ("Id", "Name", "Description", "DifficultyId", "KineticChainId", "ExerciseWeightTypeId", "IsUnilateral", "IsActive", "ImageUrl", "VideoUrl") VALUES
-- Push-up Alternatives
('950e8400-e29b-41d4-a716-446655440301', 'Wall Push-ups', 'Easiest push-up variation against a wall', '8a8adb1d-24d2-4979-a5a6-0d760e6da24b', 'f5d5a2de-9c4e-4b87-b8c3-5d1e17d0b1f4', 'a1f3e2d4-5b6c-4d7e-8f9a-0b1c2d3e4f5a', false, true, NULL, NULL),
('950e8400-e29b-41d4-a716-446655440302', 'Incline Push-ups', 'Push-ups with hands elevated on a box or bench', '8a8adb1d-24d2-4979-a5a6-0d760e6da24b', 'f5d5a2de-9c4e-4b87-b8c3-5d1e17d0b1f4', 'a1f3e2d4-5b6c-4d7e-8f9a-0b1c2d3e4f5a', false, true, NULL, NULL),
('950e8400-e29b-41d4-a716-446655440303', 'Knee Push-ups', 'Modified push-ups on knees', '8a8adb1d-24d2-4979-a5a6-0d760e6da24b', 'f5d5a2de-9c4e-4b87-b8c3-5d1e17d0b1f4', 'a1f3e2d4-5b6c-4d7e-8f9a-0b1c2d3e4f5a', false, true, NULL, NULL),
('950e8400-e29b-41d4-a716-446655440304', 'Decline Push-ups', 'Push-ups with feet elevated', '9c7b59a4-bcd8-48a6-971a-cd67b0a7ab5a', 'f5d5a2de-9c4e-4b87-b8c3-5d1e17d0b1f4', 'a1f3e2d4-5b6c-4d7e-8f9a-0b1c2d3e4f5a', false, true, NULL, NULL),
('950e8400-e29b-41d4-a716-446655440305', 'Diamond Push-ups', 'Close-grip push-ups for tricep emphasis', '9c7b59a4-bcd8-48a6-971a-cd67b0a7ab5a', 'f5d5a2de-9c4e-4b87-b8c3-5d1e17d0b1f4', 'a1f3e2d4-5b6c-4d7e-8f9a-0b1c2d3e4f5a', false, true, NULL, NULL),
('950e8400-e29b-41d4-a716-446655440306', 'TRX Push-ups', 'Push-ups using TRX suspension straps', '9c7b59a4-bcd8-48a6-971a-cd67b0a7ab5a', 'f5d5a2de-9c4e-4b87-b8c3-5d1e17d0b1f4', 'a1f3e2d4-5b6c-4d7e-8f9a-0b1c2d3e4f5a', false, true, NULL, NULL),
('950e8400-e29b-41d4-a716-446655440307', 'Weighted Push-ups', 'Push-ups with added weight on back', '3e27f9a7-d5a5-4f8e-8a76-6de2d23c9a3c', 'f5d5a2de-9c4e-4b87-b8c3-5d1e17d0b1f4', 'b2e4d3c5-6a7b-5c8d-9e0f-1a2b3c4d5e6f', false, true, NULL, NULL),

-- Bench Press Alternatives
('950e8400-e29b-41d4-a716-446655440308', 'Dumbbell Bench Press', 'Bench press with dumbbells', '9c7b59a4-bcd8-48a6-971a-cd67b0a7ab5a', 'f5d5a2de-9c4e-4b87-b8c3-5d1e17d0b1f4', 'c3d5c4b6-7b8c-6d9e-0f1a-2b3c4d5e6f7a', false, true, NULL, NULL),
('950e8400-e29b-41d4-a716-446655440309', 'Incline Barbell Press', 'Upper chest focused barbell press', '9c7b59a4-bcd8-48a6-971a-cd67b0a7ab5a', 'f5d5a2de-9c4e-4b87-b8c3-5d1e17d0b1f4', 'c3d5c4b6-7b8c-6d9e-0f1a-2b3c4d5e6f7a', false, true, NULL, NULL),
('950e8400-e29b-41d4-a716-446655440310', 'Decline Barbell Press', 'Lower chest focused barbell press', '9c7b59a4-bcd8-48a6-971a-cd67b0a7ab5a', 'f5d5a2de-9c4e-4b87-b8c3-5d1e17d0b1f4', 'c3d5c4b6-7b8c-6d9e-0f1a-2b3c4d5e6f7a', false, true, NULL, NULL),
('950e8400-e29b-41d4-a716-446655440311', 'Smith Machine Bench Press', 'Guided barbell bench press', '8a8adb1d-24d2-4979-a5a6-0d760e6da24b', 'f5d5a2de-9c4e-4b87-b8c3-5d1e17d0b1f4', 'd4c6b5a7-8c9d-7e0f-1a2b-3c4d5e6f7a8b', false, true, NULL, NULL),

-- Pull-up Alternatives
('950e8400-e29b-41d4-a716-446655440312', 'Assisted Pull-ups', 'Pull-ups with band or machine assistance', '8a8adb1d-24d2-4979-a5a6-0d760e6da24b', 'f5d5a2de-9c4e-4b87-b8c3-5d1e17d0b1f4', 'b2e4d3c5-6a7b-5c8d-9e0f-1a2b3c4d5e6f', false, true, NULL, NULL),
('950e8400-e29b-41d4-a716-446655440313', 'Negative Pull-ups', 'Eccentric-only pull-ups', '8a8adb1d-24d2-4979-a5a6-0d760e6da24b', 'f5d5a2de-9c4e-4b87-b8c3-5d1e17d0b1f4', 'a1f3e2d4-5b6c-4d7e-8f9a-0b1c2d3e4f5a', false, true, NULL, NULL),
('950e8400-e29b-41d4-a716-446655440314', 'Chin-ups', 'Underhand grip pull-ups', '9c7b59a4-bcd8-48a6-971a-cd67b0a7ab5a', 'f5d5a2de-9c4e-4b87-b8c3-5d1e17d0b1f4', 'b2e4d3c5-6a7b-5c8d-9e0f-1a2b3c4d5e6f', false, true, NULL, NULL),
('950e8400-e29b-41d4-a716-446655440315', 'Wide-grip Pull-ups', 'Pull-ups with wider hand placement', '3e27f9a7-d5a5-4f8e-8a76-6de2d23c9a3c', 'f5d5a2de-9c4e-4b87-b8c3-5d1e17d0b1f4', 'b2e4d3c5-6a7b-5c8d-9e0f-1a2b3c4d5e6f', false, true, NULL, NULL),
('950e8400-e29b-41d4-a716-446655440316', 'Weighted Pull-ups', 'Pull-ups with added weight', '3e27f9a7-d5a5-4f8e-8a76-6de2d23c9a3c', 'f5d5a2de-9c4e-4b87-b8c3-5d1e17d0b1f4', 'b2e4d3c5-6a7b-5c8d-9e0f-1a2b3c4d5e6f', false, true, NULL, NULL),
('950e8400-e29b-41d4-a716-446655440317', 'Lat Pulldown', 'Cable machine pull-down exercise', '8a8adb1d-24d2-4979-a5a6-0d760e6da24b', 'f5d5a2de-9c4e-4b87-b8c3-5d1e17d0b1f4', 'd4c6b5a7-8c9d-7e0f-1a2b-3c4d5e6f7a8b', false, true, NULL, NULL),

-- Squat Alternatives
('950e8400-e29b-41d4-a716-446655440318', 'Goblet Squat', 'Front-loaded squat with dumbbell or kettlebell', '8a8adb1d-24d2-4979-a5a6-0d760e6da24b', 'f5d5a2de-9c4e-4b87-b8c3-5d1e17d0b1f4', 'c3d5c4b6-7b8c-6d9e-0f1a-2b3c4d5e6f7a', false, true, NULL, NULL),
('950e8400-e29b-41d4-a716-446655440319', 'Front Squat', 'Barbell squat with front rack position', '9c7b59a4-bcd8-48a6-971a-cd67b0a7ab5a', 'f5d5a2de-9c4e-4b87-b8c3-5d1e17d0b1f4', 'c3d5c4b6-7b8c-6d9e-0f1a-2b3c4d5e6f7a', false, true, NULL, NULL),
('950e8400-e29b-41d4-a716-446655440320', 'Box Squat', 'Squat to a box or bench', '8a8adb1d-24d2-4979-a5a6-0d760e6da24b', 'f5d5a2de-9c4e-4b87-b8c3-5d1e17d0b1f4', 'c3d5c4b6-7b8c-6d9e-0f1a-2b3c4d5e6f7a', false, true, NULL, NULL),
('950e8400-e29b-41d4-a716-446655440321', 'Bulgarian Split Squat', 'Single-leg squat with rear foot elevated', '9c7b59a4-bcd8-48a6-971a-cd67b0a7ab5a', 'f5d5a2de-9c4e-4b87-b8c3-5d1e17d0b1f4', 'b2e4d3c5-6a7b-5c8d-9e0f-1a2b3c4d5e6f', true, true, NULL, NULL),
('950e8400-e29b-41d4-a716-446655440322', 'Pistol Squat', 'Single-leg bodyweight squat', '3e27f9a7-d5a5-4f8e-8a76-6de2d23c9a3c', 'f5d5a2de-9c4e-4b87-b8c3-5d1e17d0b1f4', 'a1f3e2d4-5b6c-4d7e-8f9a-0b1c2d3e4f5a', true, true, NULL, NULL),
('950e8400-e29b-41d4-a716-446655440323', 'Leg Press', 'Machine-based leg exercise', '8a8adb1d-24d2-4979-a5a6-0d760e6da24b', 'f5d5a2de-9c4e-4b87-b8c3-5d1e17d0b1f4', 'd4c6b5a7-8c9d-7e0f-1a2b-3c4d5e6f7a8b', false, true, NULL, NULL),

-- Plank Alternatives
('950e8400-e29b-41d4-a716-446655440324', 'Knee Plank', 'Modified plank on knees', '8a8adb1d-24d2-4979-a5a6-0d760e6da24b', '2b3e7cb2-9a3e-4c9a-88d8-b7c019c90d1b', 'a1f3e2d4-5b6c-4d7e-8f9a-0b1c2d3e4f5a', false, true, NULL, NULL),
('950e8400-e29b-41d4-a716-446655440325', 'Side Plank', 'Lateral core stability exercise', '9c7b59a4-bcd8-48a6-971a-cd67b0a7ab5a', '2b3e7cb2-9a3e-4c9a-88d8-b7c019c90d1b', 'a1f3e2d4-5b6c-4d7e-8f9a-0b1c2d3e4f5a', true, true, NULL, NULL),
('950e8400-e29b-41d4-a716-446655440326', 'Plank with Leg Lift', 'Plank with alternating leg raises', '9c7b59a4-bcd8-48a6-971a-cd67b0a7ab5a', '2b3e7cb2-9a3e-4c9a-88d8-b7c019c90d1b', 'a1f3e2d4-5b6c-4d7e-8f9a-0b1c2d3e4f5a', false, true, NULL, NULL),
('950e8400-e29b-41d4-a716-446655440327', 'TRX Plank', 'Plank with feet in TRX straps', '3e27f9a7-d5a5-4f8e-8a76-6de2d23c9a3c', '2b3e7cb2-9a3e-4c9a-88d8-b7c019c90d1b', 'a1f3e2d4-5b6c-4d7e-8f9a-0b1c2d3e4f5a', false, true, NULL, NULL),
('950e8400-e29b-41d4-a716-446655440328', 'Ab Wheel Rollout', 'Advanced core exercise with ab wheel', '3e27f9a7-d5a5-4f8e-8a76-6de2d23c9a3c', 'f5d5a2de-9c4e-4b87-b8c3-5d1e17d0b1f4', 'c3d5c4b6-7b8c-6d9e-0f1a-2b3c4d5e6f7a', false, true, NULL, NULL);

-- ========================================
-- PART 5: INSERT EXERCISE TYPES
-- ========================================

-- Warmup exercises
INSERT INTO "ExerciseExerciseTypes" ("ExerciseId", "ExerciseTypeId") VALUES
-- General warmups
('950e8400-e29b-41d4-a716-446655440101', 'a1b2c3d4-5e6f-7a8b-9c0d-1e2f3a4b5c6d'),
('950e8400-e29b-41d4-a716-446655440102', 'a1b2c3d4-5e6f-7a8b-9c0d-1e2f3a4b5c6d'),
('950e8400-e29b-41d4-a716-446655440103', 'a1b2c3d4-5e6f-7a8b-9c0d-1e2f3a4b5c6d'),
('950e8400-e29b-41d4-a716-446655440104', 'a1b2c3d4-5e6f-7a8b-9c0d-1e2f3a4b5c6d'),
('950e8400-e29b-41d4-a716-446655440105', 'a1b2c3d4-5e6f-7a8b-9c0d-1e2f3a4b5c6d'),
('950e8400-e29b-41d4-a716-446655440106', 'a1b2c3d4-5e6f-7a8b-9c0d-1e2f3a4b5c6d'),
('950e8400-e29b-41d4-a716-446655440107', 'a1b2c3d4-5e6f-7a8b-9c0d-1e2f3a4b5c6d'),
('950e8400-e29b-41d4-a716-446655440108', 'a1b2c3d4-5e6f-7a8b-9c0d-1e2f3a4b5c6d'),
('950e8400-e29b-41d4-a716-446655440109', 'a1b2c3d4-5e6f-7a8b-9c0d-1e2f3a4b5c6d'),
('950e8400-e29b-41d4-a716-446655440110', 'a1b2c3d4-5e6f-7a8b-9c0d-1e2f3a4b5c6d'),
('950e8400-e29b-41d4-a716-446655440111', 'a1b2c3d4-5e6f-7a8b-9c0d-1e2f3a4b5c6d'),
('950e8400-e29b-41d4-a716-446655440112', 'a1b2c3d4-5e6f-7a8b-9c0d-1e2f3a4b5c6d'),
('950e8400-e29b-41d4-a716-446655440113', 'a1b2c3d4-5e6f-7a8b-9c0d-1e2f3a4b5c6d'),
('950e8400-e29b-41d4-a716-446655440114', 'a1b2c3d4-5e6f-7a8b-9c0d-1e2f3a4b5c6d'),

-- Cooldown exercises
('950e8400-e29b-41d4-a716-446655440201', 'c3d4e5f6-7a8b-9c0d-1e2f-3a4b5c6d7e8f'),
('950e8400-e29b-41d4-a716-446655440202', 'c3d4e5f6-7a8b-9c0d-1e2f-3a4b5c6d7e8f'),
('950e8400-e29b-41d4-a716-446655440203', 'c3d4e5f6-7a8b-9c0d-1e2f-3a4b5c6d7e8f'),
('950e8400-e29b-41d4-a716-446655440204', 'c3d4e5f6-7a8b-9c0d-1e2f-3a4b5c6d7e8f'),
('950e8400-e29b-41d4-a716-446655440205', 'c3d4e5f6-7a8b-9c0d-1e2f-3a4b5c6d7e8f'),
('950e8400-e29b-41d4-a716-446655440206', 'c3d4e5f6-7a8b-9c0d-1e2f-3a4b5c6d7e8f'),
('950e8400-e29b-41d4-a716-446655440207', 'c3d4e5f6-7a8b-9c0d-1e2f-3a4b5c6d7e8f'),
('950e8400-e29b-41d4-a716-446655440208', 'c3d4e5f6-7a8b-9c0d-1e2f-3a4b5c6d7e8f'),
('950e8400-e29b-41d4-a716-446655440209', 'c3d4e5f6-7a8b-9c0d-1e2f-3a4b5c6d7e8f'),
('950e8400-e29b-41d4-a716-446655440210', 'c3d4e5f6-7a8b-9c0d-1e2f-3a4b5c6d7e8f'),
('950e8400-e29b-41d4-a716-446655440211', 'c3d4e5f6-7a8b-9c0d-1e2f-3a4b5c6d7e8f'),
('950e8400-e29b-41d4-a716-446655440212', 'c3d4e5f6-7a8b-9c0d-1e2f-3a4b5c6d7e8f'),

-- Alternative exercises (workout type)
('950e8400-e29b-41d4-a716-446655440301', 'b2c3d4e5-6f7a-8b9c-0d1e-2f3a4b5c6d7e'),
('950e8400-e29b-41d4-a716-446655440302', 'b2c3d4e5-6f7a-8b9c-0d1e-2f3a4b5c6d7e'),
('950e8400-e29b-41d4-a716-446655440303', 'b2c3d4e5-6f7a-8b9c-0d1e-2f3a4b5c6d7e'),
('950e8400-e29b-41d4-a716-446655440304', 'b2c3d4e5-6f7a-8b9c-0d1e-2f3a4b5c6d7e'),
('950e8400-e29b-41d4-a716-446655440305', 'b2c3d4e5-6f7a-8b9c-0d1e-2f3a4b5c6d7e'),
('950e8400-e29b-41d4-a716-446655440306', 'b2c3d4e5-6f7a-8b9c-0d1e-2f3a4b5c6d7e'),
('950e8400-e29b-41d4-a716-446655440307', 'b2c3d4e5-6f7a-8b9c-0d1e-2f3a4b5c6d7e'),
('950e8400-e29b-41d4-a716-446655440308', 'b2c3d4e5-6f7a-8b9c-0d1e-2f3a4b5c6d7e'),
('950e8400-e29b-41d4-a716-446655440309', 'b2c3d4e5-6f7a-8b9c-0d1e-2f3a4b5c6d7e'),
('950e8400-e29b-41d4-a716-446655440310', 'b2c3d4e5-6f7a-8b9c-0d1e-2f3a4b5c6d7e'),
('950e8400-e29b-41d4-a716-446655440311', 'b2c3d4e5-6f7a-8b9c-0d1e-2f3a4b5c6d7e'),
('950e8400-e29b-41d4-a716-446655440312', 'b2c3d4e5-6f7a-8b9c-0d1e-2f3a4b5c6d7e'),
('950e8400-e29b-41d4-a716-446655440313', 'b2c3d4e5-6f7a-8b9c-0d1e-2f3a4b5c6d7e'),
('950e8400-e29b-41d4-a716-446655440314', 'b2c3d4e5-6f7a-8b9c-0d1e-2f3a4b5c6d7e'),
('950e8400-e29b-41d4-a716-446655440315', 'b2c3d4e5-6f7a-8b9c-0d1e-2f3a4b5c6d7e'),
('950e8400-e29b-41d4-a716-446655440316', 'b2c3d4e5-6f7a-8b9c-0d1e-2f3a4b5c6d7e'),
('950e8400-e29b-41d4-a716-446655440317', 'b2c3d4e5-6f7a-8b9c-0d1e-2f3a4b5c6d7e'),
('950e8400-e29b-41d4-a716-446655440318', 'b2c3d4e5-6f7a-8b9c-0d1e-2f3a4b5c6d7e'),
('950e8400-e29b-41d4-a716-446655440319', 'b2c3d4e5-6f7a-8b9c-0d1e-2f3a4b5c6d7e'),
('950e8400-e29b-41d4-a716-446655440320', 'b2c3d4e5-6f7a-8b9c-0d1e-2f3a4b5c6d7e'),
('950e8400-e29b-41d4-a716-446655440321', 'b2c3d4e5-6f7a-8b9c-0d1e-2f3a4b5c6d7e'),
('950e8400-e29b-41d4-a716-446655440322', 'b2c3d4e5-6f7a-8b9c-0d1e-2f3a4b5c6d7e'),
('950e8400-e29b-41d4-a716-446655440323', 'b2c3d4e5-6f7a-8b9c-0d1e-2f3a4b5c6d7e'),
('950e8400-e29b-41d4-a716-446655440324', 'b2c3d4e5-6f7a-8b9c-0d1e-2f3a4b5c6d7e'),
('950e8400-e29b-41d4-a716-446655440325', 'b2c3d4e5-6f7a-8b9c-0d1e-2f3a4b5c6d7e'),
('950e8400-e29b-41d4-a716-446655440326', 'b2c3d4e5-6f7a-8b9c-0d1e-2f3a4b5c6d7e'),
('950e8400-e29b-41d4-a716-446655440327', 'b2c3d4e5-6f7a-8b9c-0d1e-2f3a4b5c6d7e'),
('950e8400-e29b-41d4-a716-446655440328', 'b2c3d4e5-6f7a-8b9c-0d1e-2f3a4b5c6d7e');

-- ========================================
-- PART 6: INSERT EQUIPMENT RELATIONSHIPS
-- ========================================

INSERT INTO "ExerciseEquipment" ("ExerciseId", "EquipmentId") VALUES
-- Warmup equipment
('950e8400-e29b-41d4-a716-446655440103', '650e8400-e29b-41d4-a716-446655440016'), -- Jump Rope
('950e8400-e29b-41d4-a716-446655440104', '650e8400-e29b-41d4-a716-446655440018'), -- Chest Fly with Bands - Elastic Band
('950e8400-e29b-41d4-a716-446655440106', '650e8400-e29b-41d4-a716-446655440008'), -- Band Pull-aparts - Resistance Bands
('950e8400-e29b-41d4-a716-446655440107', '650e8400-e29b-41d4-a716-446655440004'), -- Scapular Pulls - Pull-up Bar
('950e8400-e29b-41d4-a716-446655440109', '650e8400-e29b-41d4-a716-446655440015'), -- Leg Swings - Wall
('950e8400-e29b-41d4-a716-446655440111', '650e8400-e29b-41d4-a716-446655440008'), -- Band Dislocations - Resistance Bands
('950e8400-e29b-41d4-a716-446655440113', '650e8400-e29b-41d4-a716-446655440013'), -- Cat-Cow - Yoga Mat
('950e8400-e29b-41d4-a716-446655440114', '650e8400-e29b-41d4-a716-446655440013'), -- Dead Bug - Yoga Mat

-- Cooldown equipment
('950e8400-e29b-41d4-a716-446655440201', '650e8400-e29b-41d4-a716-446655440015'), -- Chest Doorway Stretch - Wall
('950e8400-e29b-41d4-a716-446655440202', '650e8400-e29b-41d4-a716-446655440004'), -- Lat Stretch - Pull-up Bar
('950e8400-e29b-41d4-a716-446655440207', '650e8400-e29b-41d4-a716-446655440015'), -- Bicep Wall Stretch - Wall
('950e8400-e29b-41d4-a716-446655440208', '650e8400-e29b-41d4-a716-446655440013'), -- Child Pose - Yoga Mat
('950e8400-e29b-41d4-a716-446655440209', '650e8400-e29b-41d4-a716-446655440012'), -- Foam Roll Back - Foam Roller
('950e8400-e29b-41d4-a716-446655440210', '650e8400-e29b-41d4-a716-446655440012'), -- Foam Roll Quads - Foam Roller
('950e8400-e29b-41d4-a716-446655440211', '650e8400-e29b-41d4-a716-446655440013'), -- Pigeon Pose - Yoga Mat
('950e8400-e29b-41d4-a716-446655440212', '650e8400-e29b-41d4-a716-446655440013'), -- Cobra Stretch - Yoga Mat

-- Alternative exercise equipment
('950e8400-e29b-41d4-a716-446655440301', '650e8400-e29b-41d4-a716-446655440015'), -- Wall Push-ups - Wall
('950e8400-e29b-41d4-a716-446655440302', '650e8400-e29b-41d4-a716-446655440014'), -- Incline Push-ups - Box
('950e8400-e29b-41d4-a716-446655440302', '650e8400-e29b-41d4-a716-446655440005'), -- Incline Push-ups - Bench
('950e8400-e29b-41d4-a716-446655440304', '650e8400-e29b-41d4-a716-446655440014'), -- Decline Push-ups - Box
('950e8400-e29b-41d4-a716-446655440306', '650e8400-e29b-41d4-a716-446655440011'), -- TRX Push-ups - TRX Bands
('950e8400-e29b-41d4-a716-446655440308', '650e8400-e29b-41d4-a716-446655440002'), -- Dumbbell Bench Press - Dumbbell
('950e8400-e29b-41d4-a716-446655440308', '650e8400-e29b-41d4-a716-446655440005'), -- Dumbbell Bench Press - Bench
('950e8400-e29b-41d4-a716-446655440309', '650e8400-e29b-41d4-a716-446655440001'), -- Incline Barbell Press - Barbell
('950e8400-e29b-41d4-a716-446655440309', '650e8400-e29b-41d4-a716-446655440005'), -- Incline Barbell Press - Bench
('950e8400-e29b-41d4-a716-446655440310', '650e8400-e29b-41d4-a716-446655440001'), -- Decline Barbell Press - Barbell
('950e8400-e29b-41d4-a716-446655440310', '650e8400-e29b-41d4-a716-446655440005'), -- Decline Barbell Press - Bench
('950e8400-e29b-41d4-a716-446655440311', '650e8400-e29b-41d4-a716-446655440006'), -- Smith Machine Bench Press
('950e8400-e29b-41d4-a716-446655440312', '650e8400-e29b-41d4-a716-446655440008'), -- Assisted Pull-ups - Resistance Bands
('950e8400-e29b-41d4-a716-446655440312', '650e8400-e29b-41d4-a716-446655440004'), -- Assisted Pull-ups - Pull-up Bar
('950e8400-e29b-41d4-a716-446655440313', '650e8400-e29b-41d4-a716-446655440004'), -- Negative Pull-ups - Pull-up Bar
('950e8400-e29b-41d4-a716-446655440314', '650e8400-e29b-41d4-a716-446655440004'), -- Chin-ups - Pull-up Bar
('950e8400-e29b-41d4-a716-446655440315', '650e8400-e29b-41d4-a716-446655440004'), -- Wide-grip Pull-ups - Pull-up Bar
('950e8400-e29b-41d4-a716-446655440316', '650e8400-e29b-41d4-a716-446655440004'), -- Weighted Pull-ups - Pull-up Bar
('950e8400-e29b-41d4-a716-446655440317', '650e8400-e29b-41d4-a716-446655440003'), -- Lat Pulldown - Cable Machine
('950e8400-e29b-41d4-a716-446655440318', '650e8400-e29b-41d4-a716-446655440002'), -- Goblet Squat - Dumbbell
('950e8400-e29b-41d4-a716-446655440318', '650e8400-e29b-41d4-a716-446655440009'), -- Goblet Squat - Kettlebell
('950e8400-e29b-41d4-a716-446655440319', '650e8400-e29b-41d4-a716-446655440001'), -- Front Squat - Barbell
('950e8400-e29b-41d4-a716-446655440320', '650e8400-e29b-41d4-a716-446655440001'), -- Box Squat - Barbell
('950e8400-e29b-41d4-a716-446655440320', '650e8400-e29b-41d4-a716-446655440014'), -- Box Squat - Box
('950e8400-e29b-41d4-a716-446655440321', '650e8400-e29b-41d4-a716-446655440005'), -- Bulgarian Split Squat - Bench
('950e8400-e29b-41d4-a716-446655440323', '650e8400-e29b-41d4-a716-446655440007'), -- Leg Press - Leg Press Machine
('950e8400-e29b-41d4-a716-446655440327', '650e8400-e29b-41d4-a716-446655440011'), -- TRX Plank - TRX Bands
('950e8400-e29b-41d4-a716-446655440328', '650e8400-e29b-41d4-a716-446655440020'); -- Ab Wheel Rollout - Ab Wheel

-- ========================================
-- PART 7: INSERT BODY PARTS
-- ========================================

INSERT INTO "ExerciseBodyParts" ("ExerciseId", "BodyPartId") VALUES
-- Warmup exercises body parts
('950e8400-e29b-41d4-a716-446655440101', 'd7e0e24c-f8d4-4b8a-b1e0-cf9c2e6b5d0a'), -- Arm Circles - Shoulders
('950e8400-e29b-41d4-a716-446655440101', '9c5f1b4e-2b8a-4c9d-8e7f-c5a9e2d7b8c1'), -- Arm Circles - Arms
('950e8400-e29b-41d4-a716-446655440102', '4a6f1b42-5c9b-4c4e-878a-b3d9f2c1f1f5'), -- Jumping Jacks - Legs
('950e8400-e29b-41d4-a716-446655440102', '9c5f1b4e-2b8a-4c9d-8e7f-c5a9e2d7b8c1'), -- Jumping Jacks - Arms
('950e8400-e29b-41d4-a716-446655440103', '4a6f1b42-5c9b-4c4e-878a-b3d9f2c1f1f5'), -- Jump Rope - Legs
('950e8400-e29b-41d4-a716-446655440104', '7c5a2d6e-e87e-4c8a-9f1d-9eb734f3df3c'), -- Chest Fly with Bands - Chest
('950e8400-e29b-41d4-a716-446655440105', 'd7e0e24c-f8d4-4b8a-b1e0-cf9c2e6b5d0a'), -- Scapular Push-ups - Shoulders
('950e8400-e29b-41d4-a716-446655440106', 'b2d89d5c-cb8a-4f5d-8a9e-2c3b76612c5a'), -- Band Pull-aparts - Back
('950e8400-e29b-41d4-a716-446655440107', 'b2d89d5c-cb8a-4f5d-8a9e-2c3b76612c5a'), -- Scapular Pulls - Back
('950e8400-e29b-41d4-a716-446655440108', '4a6f1b42-5c9b-4c4e-878a-b3d9f2c1f1f5'), -- Bodyweight Squats - Legs
('950e8400-e29b-41d4-a716-446655440109', '4a6f1b42-5c9b-4c4e-878a-b3d9f2c1f1f5'), -- Leg Swings - Legs
('950e8400-e29b-41d4-a716-446655440110', '4a6f1b42-5c9b-4c4e-878a-b3d9f2c1f1f5'), -- Walking Lunges - Legs
('950e8400-e29b-41d4-a716-446655440111', 'd7e0e24c-f8d4-4b8a-b1e0-cf9c2e6b5d0a'), -- Band Dislocations - Shoulders
('950e8400-e29b-41d4-a716-446655440112', 'd7e0e24c-f8d4-4b8a-b1e0-cf9c2e6b5d0a'), -- YTW Raises - Shoulders
('950e8400-e29b-41d4-a716-446655440113', '3e9f8a7d-6c5b-4a3e-8d2f-1b7c9a6d5e4c'), -- Cat-Cow - Core
('950e8400-e29b-41d4-a716-446655440114', '3e9f8a7d-6c5b-4a3e-8d2f-1b7c9a6d5e4c'), -- Dead Bug - Core

-- Alternative exercises body parts
('950e8400-e29b-41d4-a716-446655440301', '7c5a2d6e-e87e-4c8a-9f1d-9eb734f3df3c'), -- Wall Push-ups - Chest
('950e8400-e29b-41d4-a716-446655440301', '9c5f1b4e-2b8a-4c9d-8e7f-c5a9e2d7b8c1'), -- Wall Push-ups - Arms
('950e8400-e29b-41d4-a716-446655440302', '7c5a2d6e-e87e-4c8a-9f1d-9eb734f3df3c'), -- Incline Push-ups - Chest
('950e8400-e29b-41d4-a716-446655440302', '9c5f1b4e-2b8a-4c9d-8e7f-c5a9e2d7b8c1'), -- Incline Push-ups - Arms
('950e8400-e29b-41d4-a716-446655440303', '7c5a2d6e-e87e-4c8a-9f1d-9eb734f3df3c'), -- Knee Push-ups - Chest
('950e8400-e29b-41d4-a716-446655440303', '9c5f1b4e-2b8a-4c9d-8e7f-c5a9e2d7b8c1'), -- Knee Push-ups - Arms
('950e8400-e29b-41d4-a716-446655440304', '7c5a2d6e-e87e-4c8a-9f1d-9eb734f3df3c'), -- Decline Push-ups - Chest
('950e8400-e29b-41d4-a716-446655440304', '9c5f1b4e-2b8a-4c9d-8e7f-c5a9e2d7b8c1'), -- Decline Push-ups - Arms
('950e8400-e29b-41d4-a716-446655440305', '7c5a2d6e-e87e-4c8a-9f1d-9eb734f3df3c'), -- Diamond Push-ups - Chest
('950e8400-e29b-41d4-a716-446655440305', '9c5f1b4e-2b8a-4c9d-8e7f-c5a9e2d7b8c1'), -- Diamond Push-ups - Arms
('950e8400-e29b-41d4-a716-446655440306', '7c5a2d6e-e87e-4c8a-9f1d-9eb734f3df3c'), -- TRX Push-ups - Chest
('950e8400-e29b-41d4-a716-446655440306', '9c5f1b4e-2b8a-4c9d-8e7f-c5a9e2d7b8c1'), -- TRX Push-ups - Arms
('950e8400-e29b-41d4-a716-446655440306', '3e9f8a7d-6c5b-4a3e-8d2f-1b7c9a6d5e4c'), -- TRX Push-ups - Core
('950e8400-e29b-41d4-a716-446655440307', '7c5a2d6e-e87e-4c8a-9f1d-9eb734f3df3c'), -- Weighted Push-ups - Chest
('950e8400-e29b-41d4-a716-446655440307', '9c5f1b4e-2b8a-4c9d-8e7f-c5a9e2d7b8c1'), -- Weighted Push-ups - Arms
('950e8400-e29b-41d4-a716-446655440308', '7c5a2d6e-e87e-4c8a-9f1d-9eb734f3df3c'), -- Dumbbell Bench Press - Chest
('950e8400-e29b-41d4-a716-446655440309', '7c5a2d6e-e87e-4c8a-9f1d-9eb734f3df3c'), -- Incline Barbell Press - Chest
('950e8400-e29b-41d4-a716-446655440310', '7c5a2d6e-e87e-4c8a-9f1d-9eb734f3df3c'), -- Decline Barbell Press - Chest
('950e8400-e29b-41d4-a716-446655440311', '7c5a2d6e-e87e-4c8a-9f1d-9eb734f3df3c'), -- Smith Machine Bench Press - Chest
('950e8400-e29b-41d4-a716-446655440312', 'b2d89d5c-cb8a-4f5d-8a9e-2c3b76612c5a'), -- Assisted Pull-ups - Back
('950e8400-e29b-41d4-a716-446655440312', '9c5f1b4e-2b8a-4c9d-8e7f-c5a9e2d7b8c1'), -- Assisted Pull-ups - Arms
('950e8400-e29b-41d4-a716-446655440313', 'b2d89d5c-cb8a-4f5d-8a9e-2c3b76612c5a'), -- Negative Pull-ups - Back
('950e8400-e29b-41d4-a716-446655440313', '9c5f1b4e-2b8a-4c9d-8e7f-c5a9e2d7b8c1'), -- Negative Pull-ups - Arms
('950e8400-e29b-41d4-a716-446655440314', 'b2d89d5c-cb8a-4f5d-8a9e-2c3b76612c5a'), -- Chin-ups - Back
('950e8400-e29b-41d4-a716-446655440314', '9c5f1b4e-2b8a-4c9d-8e7f-c5a9e2d7b8c1'), -- Chin-ups - Arms
('950e8400-e29b-41d4-a716-446655440315', 'b2d89d5c-cb8a-4f5d-8a9e-2c3b76612c5a'), -- Wide-grip Pull-ups - Back
('950e8400-e29b-41d4-a716-446655440316', 'b2d89d5c-cb8a-4f5d-8a9e-2c3b76612c5a'), -- Weighted Pull-ups - Back
('950e8400-e29b-41d4-a716-446655440316', '9c5f1b4e-2b8a-4c9d-8e7f-c5a9e2d7b8c1'), -- Weighted Pull-ups - Arms
('950e8400-e29b-41d4-a716-446655440317', 'b2d89d5c-cb8a-4f5d-8a9e-2c3b76612c5a'), -- Lat Pulldown - Back
('950e8400-e29b-41d4-a716-446655440318', '4a6f1b42-5c9b-4c4e-878a-b3d9f2c1f1f5'), -- Goblet Squat - Legs
('950e8400-e29b-41d4-a716-446655440319', '4a6f1b42-5c9b-4c4e-878a-b3d9f2c1f1f5'), -- Front Squat - Legs
('950e8400-e29b-41d4-a716-446655440320', '4a6f1b42-5c9b-4c4e-878a-b3d9f2c1f1f5'), -- Box Squat - Legs
('950e8400-e29b-41d4-a716-446655440321', '4a6f1b42-5c9b-4c4e-878a-b3d9f2c1f1f5'), -- Bulgarian Split Squat - Legs
('950e8400-e29b-41d4-a716-446655440322', '4a6f1b42-5c9b-4c4e-878a-b3d9f2c1f1f5'), -- Pistol Squat - Legs
('950e8400-e29b-41d4-a716-446655440323', '4a6f1b42-5c9b-4c4e-878a-b3d9f2c1f1f5'), -- Leg Press - Legs
('950e8400-e29b-41d4-a716-446655440324', '3e9f8a7d-6c5b-4a3e-8d2f-1b7c9a6d5e4c'), -- Knee Plank - Core
('950e8400-e29b-41d4-a716-446655440325', '3e9f8a7d-6c5b-4a3e-8d2f-1b7c9a6d5e4c'), -- Side Plank - Core
('950e8400-e29b-41d4-a716-446655440326', '3e9f8a7d-6c5b-4a3e-8d2f-1b7c9a6d5e4c'), -- Plank with Leg Lift - Core
('950e8400-e29b-41d4-a716-446655440327', '3e9f8a7d-6c5b-4a3e-8d2f-1b7c9a6d5e4c'), -- TRX Plank - Core
('950e8400-e29b-41d4-a716-446655440328', '3e9f8a7d-6c5b-4a3e-8d2f-1b7c9a6d5e4c'); -- Ab Wheel Rollout - Core

-- ========================================
-- PART 8: INSERT PRIMARY MUSCLE GROUPS
-- ========================================

INSERT INTO "ExerciseMuscleGroups" ("ExerciseId", "MuscleGroupId", "MuscleRoleId") VALUES
-- Warmup exercises - Primary muscles
('950e8400-e29b-41d4-a716-446655440101', '550e8400-e29b-41d4-a716-446655440011', '5d8e9a7b-3c2d-4f6a-9b8c-1e5d7f3a2c9b'), -- Arm Circles - Lateral Deltoid
('950e8400-e29b-41d4-a716-446655440102', '550e8400-e29b-41d4-a716-446655440006', '5d8e9a7b-3c2d-4f6a-9b8c-1e5d7f3a2c9b'), -- Jumping Jacks - Quadriceps
('950e8400-e29b-41d4-a716-446655440103', '550e8400-e29b-41d4-a716-446655440009', '5d8e9a7b-3c2d-4f6a-9b8c-1e5d7f3a2c9b'), -- Jump Rope - Calves
('950e8400-e29b-41d4-a716-446655440104', '550e8400-e29b-41d4-a716-446655440001', '5d8e9a7b-3c2d-4f6a-9b8c-1e5d7f3a2c9b'), -- Chest Fly - Pectoralis Major
('950e8400-e29b-41d4-a716-446655440105', '550e8400-e29b-41d4-a716-446655440010', '5d8e9a7b-3c2d-4f6a-9b8c-1e5d7f3a2c9b'), -- Scapular Push-ups - Anterior Deltoid
('950e8400-e29b-41d4-a716-446655440106', '550e8400-e29b-41d4-a716-446655440005', '5d8e9a7b-3c2d-4f6a-9b8c-1e5d7f3a2c9b'), -- Band Pull-aparts - Rhomboids
('950e8400-e29b-41d4-a716-446655440107', '550e8400-e29b-41d4-a716-446655440004', '5d8e9a7b-3c2d-4f6a-9b8c-1e5d7f3a2c9b'), -- Scapular Pulls - Trapezius
('950e8400-e29b-41d4-a716-446655440108', '550e8400-e29b-41d4-a716-446655440006', '5d8e9a7b-3c2d-4f6a-9b8c-1e5d7f3a2c9b'), -- Bodyweight Squats - Quadriceps
('950e8400-e29b-41d4-a716-446655440109', '550e8400-e29b-41d4-a716-446655440008', '5d8e9a7b-3c2d-4f6a-9b8c-1e5d7f3a2c9b'), -- Leg Swings - Glutes
('950e8400-e29b-41d4-a716-446655440110', '550e8400-e29b-41d4-a716-446655440006', '5d8e9a7b-3c2d-4f6a-9b8c-1e5d7f3a2c9b'), -- Walking Lunges - Quadriceps
('950e8400-e29b-41d4-a716-446655440111', '550e8400-e29b-41d4-a716-446655440011', '5d8e9a7b-3c2d-4f6a-9b8c-1e5d7f3a2c9b'), -- Band Dislocations - Lateral Deltoid
('950e8400-e29b-41d4-a716-446655440112', '550e8400-e29b-41d4-a716-446655440012', '5d8e9a7b-3c2d-4f6a-9b8c-1e5d7f3a2c9b'), -- YTW Raises - Posterior Deltoid
('950e8400-e29b-41d4-a716-446655440113', '550e8400-e29b-41d4-a716-446655440018', '5d8e9a7b-3c2d-4f6a-9b8c-1e5d7f3a2c9b'), -- Cat-Cow - Lower Back
('950e8400-e29b-41d4-a716-446655440114', '550e8400-e29b-41d4-a716-446655440016', '5d8e9a7b-3c2d-4f6a-9b8c-1e5d7f3a2c9b'), -- Dead Bug - Rectus Abdominis

-- Alternative exercises - Primary muscles
('950e8400-e29b-41d4-a716-446655440301', '550e8400-e29b-41d4-a716-446655440001', '5d8e9a7b-3c2d-4f6a-9b8c-1e5d7f3a2c9b'), -- Wall Push-ups - Pectoralis Major
('950e8400-e29b-41d4-a716-446655440302', '550e8400-e29b-41d4-a716-446655440001', '5d8e9a7b-3c2d-4f6a-9b8c-1e5d7f3a2c9b'), -- Incline Push-ups - Pectoralis Major
('950e8400-e29b-41d4-a716-446655440303', '550e8400-e29b-41d4-a716-446655440001', '5d8e9a7b-3c2d-4f6a-9b8c-1e5d7f3a2c9b'), -- Knee Push-ups - Pectoralis Major
('950e8400-e29b-41d4-a716-446655440304', '550e8400-e29b-41d4-a716-446655440001', '5d8e9a7b-3c2d-4f6a-9b8c-1e5d7f3a2c9b'), -- Decline Push-ups - Pectoralis Major
('950e8400-e29b-41d4-a716-446655440305', '550e8400-e29b-41d4-a716-446655440014', '5d8e9a7b-3c2d-4f6a-9b8c-1e5d7f3a2c9b'), -- Diamond Push-ups - Triceps
('950e8400-e29b-41d4-a716-446655440306', '550e8400-e29b-41d4-a716-446655440001', '5d8e9a7b-3c2d-4f6a-9b8c-1e5d7f3a2c9b'), -- TRX Push-ups - Pectoralis Major
('950e8400-e29b-41d4-a716-446655440307', '550e8400-e29b-41d4-a716-446655440001', '5d8e9a7b-3c2d-4f6a-9b8c-1e5d7f3a2c9b'), -- Weighted Push-ups - Pectoralis Major
('950e8400-e29b-41d4-a716-446655440308', '550e8400-e29b-41d4-a716-446655440001', '5d8e9a7b-3c2d-4f6a-9b8c-1e5d7f3a2c9b'), -- Dumbbell Bench Press - Pectoralis Major
('950e8400-e29b-41d4-a716-446655440309', '550e8400-e29b-41d4-a716-446655440001', '5d8e9a7b-3c2d-4f6a-9b8c-1e5d7f3a2c9b'), -- Incline Barbell Press - Pectoralis Major (upper)
('950e8400-e29b-41d4-a716-446655440310', '550e8400-e29b-41d4-a716-446655440001', '5d8e9a7b-3c2d-4f6a-9b8c-1e5d7f3a2c9b'), -- Decline Barbell Press - Pectoralis Major (lower)
('950e8400-e29b-41d4-a716-446655440311', '550e8400-e29b-41d4-a716-446655440001', '5d8e9a7b-3c2d-4f6a-9b8c-1e5d7f3a2c9b'), -- Smith Machine Bench Press - Pectoralis Major
('950e8400-e29b-41d4-a716-446655440312', '550e8400-e29b-41d4-a716-446655440003', '5d8e9a7b-3c2d-4f6a-9b8c-1e5d7f3a2c9b'), -- Assisted Pull-ups - Latissimus Dorsi
('950e8400-e29b-41d4-a716-446655440313', '550e8400-e29b-41d4-a716-446655440003', '5d8e9a7b-3c2d-4f6a-9b8c-1e5d7f3a2c9b'), -- Negative Pull-ups - Latissimus Dorsi
('950e8400-e29b-41d4-a716-446655440314', '550e8400-e29b-41d4-a716-446655440013', '5d8e9a7b-3c2d-4f6a-9b8c-1e5d7f3a2c9b'), -- Chin-ups - Biceps
('950e8400-e29b-41d4-a716-446655440315', '550e8400-e29b-41d4-a716-446655440003', '5d8e9a7b-3c2d-4f6a-9b8c-1e5d7f3a2c9b'), -- Wide-grip Pull-ups - Latissimus Dorsi
('950e8400-e29b-41d4-a716-446655440316', '550e8400-e29b-41d4-a716-446655440003', '5d8e9a7b-3c2d-4f6a-9b8c-1e5d7f3a2c9b'), -- Weighted Pull-ups - Latissimus Dorsi
('950e8400-e29b-41d4-a716-446655440317', '550e8400-e29b-41d4-a716-446655440003', '5d8e9a7b-3c2d-4f6a-9b8c-1e5d7f3a2c9b'), -- Lat Pulldown - Latissimus Dorsi
('950e8400-e29b-41d4-a716-446655440318', '550e8400-e29b-41d4-a716-446655440006', '5d8e9a7b-3c2d-4f6a-9b8c-1e5d7f3a2c9b'), -- Goblet Squat - Quadriceps
('950e8400-e29b-41d4-a716-446655440319', '550e8400-e29b-41d4-a716-446655440006', '5d8e9a7b-3c2d-4f6a-9b8c-1e5d7f3a2c9b'), -- Front Squat - Quadriceps
('950e8400-e29b-41d4-a716-446655440320', '550e8400-e29b-41d4-a716-446655440006', '5d8e9a7b-3c2d-4f6a-9b8c-1e5d7f3a2c9b'), -- Box Squat - Quadriceps
('950e8400-e29b-41d4-a716-446655440321', '550e8400-e29b-41d4-a716-446655440006', '5d8e9a7b-3c2d-4f6a-9b8c-1e5d7f3a2c9b'), -- Bulgarian Split Squat - Quadriceps
('950e8400-e29b-41d4-a716-446655440322', '550e8400-e29b-41d4-a716-446655440006', '5d8e9a7b-3c2d-4f6a-9b8c-1e5d7f3a2c9b'), -- Pistol Squat - Quadriceps
('950e8400-e29b-41d4-a716-446655440323', '550e8400-e29b-41d4-a716-446655440006', '5d8e9a7b-3c2d-4f6a-9b8c-1e5d7f3a2c9b'), -- Leg Press - Quadriceps
('950e8400-e29b-41d4-a716-446655440324', '550e8400-e29b-41d4-a716-446655440016', '5d8e9a7b-3c2d-4f6a-9b8c-1e5d7f3a2c9b'), -- Knee Plank - Rectus Abdominis
('950e8400-e29b-41d4-a716-446655440325', '550e8400-e29b-41d4-a716-446655440017', '5d8e9a7b-3c2d-4f6a-9b8c-1e5d7f3a2c9b'), -- Side Plank - Obliques
('950e8400-e29b-41d4-a716-446655440326', '550e8400-e29b-41d4-a716-446655440016', '5d8e9a7b-3c2d-4f6a-9b8c-1e5d7f3a2c9b'), -- Plank with Leg Lift - Rectus Abdominis
('950e8400-e29b-41d4-a716-446655440327', '550e8400-e29b-41d4-a716-446655440016', '5d8e9a7b-3c2d-4f6a-9b8c-1e5d7f3a2c9b'), -- TRX Plank - Rectus Abdominis
('950e8400-e29b-41d4-a716-446655440328', '550e8400-e29b-41d4-a716-446655440016', '5d8e9a7b-3c2d-4f6a-9b8c-1e5d7f3a2c9b'); -- Ab Wheel Rollout - Rectus Abdominis

-- ========================================
-- Script completed successfully
-- ========================================