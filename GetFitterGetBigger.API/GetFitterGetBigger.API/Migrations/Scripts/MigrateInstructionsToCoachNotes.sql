-- Data migration script to convert existing Instructions to CoachNotes
-- This script should be run BEFORE applying the AddCoachNotesAndExerciseTypes migration
-- if there is existing data in the Instructions column that needs to be preserved

-- Create temporary table to store exercise instructions
CREATE TEMP TABLE temp_exercise_instructions AS
SELECT Id, Instructions
FROM "Exercises"
WHERE Instructions IS NOT NULL AND Instructions != '';

-- After the migration is applied, run this to create CoachNotes from Instructions:
/*
INSERT INTO "CoachNotes" (Id, ExerciseId, Text, Order)
SELECT 
    gen_random_uuid() as Id,
    Id as ExerciseId,
    Instructions as Text,
    1 as Order
FROM temp_exercise_instructions;

DROP TABLE temp_exercise_instructions;
*/

-- Note: This assumes PostgreSQL with gen_random_uuid() function
-- For other databases, adjust the UUID generation method accordingly