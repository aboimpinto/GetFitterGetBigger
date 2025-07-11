// Exercise Request Interfaces for Admin Project

interface CoachNoteRequest {
  id?: string; // Optional for new notes, required for updates
  text: string;
  order: number;
}

interface MuscleGroupAssignment {
  muscleGroupId: string;
  muscleRoleId: string;
}

interface CreateExerciseRequest {
  name: string;
  description: string;
  coachNotes: CoachNoteRequest[];
  exerciseTypeIds: string[];
  videoUrl?: string;
  imageUrl?: string;
  isUnilateral: boolean;
  difficultyId: string;
  kineticChainId: string; // Empty string for REST exercises
  exerciseWeightTypeId: string; // Empty string for REST exercises
  muscleGroups: MuscleGroupAssignment[];
  equipmentIds: string[];
  bodyPartIds: string[];
  movementPatternIds: string[];
}

interface UpdateExerciseRequest extends CreateExerciseRequest {
  isActive?: boolean;
}

// Example usage with proper exerciseWeightTypeId
const createBarbellSquat: CreateExerciseRequest = {
  name: "Barbell Back Squat",
  description: "A compound lower body exercise targeting quads, glutes, and hamstrings",
  coachNotes: [
    { text: "Keep your chest up and core engaged", order: 0 },
    { text: "Drive through your heels", order: 1 }
  ],
  exerciseTypeIds: ["exercisetype-11223344-5566-7788-99aa-bbccddeeff00"],
  videoUrl: "https://example.com/barbell-squat-video.mp4",
  imageUrl: "https://example.com/barbell-squat-image.jpg",
  isUnilateral: false,
  difficultyId: "difficultylevel-8a8adb1d-24d2-4979-a5a6-0d760e6da24b",
  kineticChainId: "kineticchaintype-12345678-9abc-def0-1234-567890abcdef",
  exerciseWeightTypeId: "exerciseweighttype-1b3d5f7a-5b7c-4d8e-9f0a-1b2c3d4e5f6a", // Barbell type
  muscleGroups: [
    {
      muscleGroupId: "musclegroup-ccddeeff-0011-2233-4455-667788990011",
      muscleRoleId: "musclerole-abcdef12-3456-7890-abcd-ef1234567890"
    }
  ],
  equipmentIds: ["equipment-33445566-7788-99aa-bbcc-ddeeff001122"],
  bodyPartIds: ["bodypart-7c5a2d6e-e87e-4c8a-9f1d-9eb734f3df3c"],
  movementPatternIds: ["movementpattern-99aabbcc-ddee-ff00-1122-334455667788"]
};

// Example for REST exercise (empty exerciseWeightTypeId)
const createRestPeriod: CreateExerciseRequest = {
  name: "Rest Period",
  description: "Active recovery period between sets",
  coachNotes: [
    { text: "Focus on breathing and recovery", order: 0 }
  ],
  exerciseTypeIds: ["exercisetype-44556677-8899-aabb-ccdd-eeff00112233"], // REST type
  videoUrl: "",
  imageUrl: "",
  isUnilateral: false,
  difficultyId: "difficultylevel-5d9ceg4g-57g5-7cac-d8d9-3g093h9gd57e",
  kineticChainId: "", // Must be empty for REST
  exerciseWeightTypeId: "", // Must be empty for REST
  muscleGroups: [], // Must be empty for REST
  equipmentIds: [],
  bodyPartIds: [],
  movementPatternIds: []
};

// Update example with exerciseWeightTypeId
const updateExercise: UpdateExerciseRequest = {
  name: "Updated Barbell Back Squat",
  description: "A compound lower body exercise - updated description",
  coachNotes: [
    {
      id: "coachnote-87654321-4321-4321-4321-210987654321", // Existing note to update
      text: "Keep your chest up and core engaged throughout",
      order: 0
    },
    {
      text: "New note: Focus on hip drive", // New note without ID
      order: 1
    }
  ],
  exerciseTypeIds: ["exercisetype-11223344-5566-7788-99aa-bbccddeeff00"],
  videoUrl: "https://example.com/updated-squat-video.mp4",
  imageUrl: "https://example.com/updated-squat-image.jpg",
  isUnilateral: false,
  isActive: true,
  difficultyId: "difficultylevel-8a8adb1d-24d2-4979-a5a6-0d760e6da24b",
  kineticChainId: "kineticchaintype-12345678-9abc-def0-1234-567890abcdef",
  exerciseWeightTypeId: "exerciseweighttype-1b3d5f7a-5b7c-4d8e-9f0a-1b2c3d4e5f6a",
  muscleGroups: [
    {
      muscleGroupId: "musclegroup-ccddeeff-0011-2233-4455-667788990011",
      muscleRoleId: "musclerole-abcdef12-3456-7890-abcd-ef1234567890"
    }
  ],
  equipmentIds: ["equipment-33445566-7788-99aa-bbcc-ddeeff001122"],
  bodyPartIds: ["bodypart-7c5a2d6e-e87e-4c8a-9f1d-9eb734f3df3c"],
  movementPatternIds: ["movementpattern-99aabbcc-ddee-ff00-1122-334455667788"]
};

// Helper function to validate exercise request
function validateExerciseRequest(request: CreateExerciseRequest | UpdateExerciseRequest): string[] {
  const errors: string[] = [];
  
  const isRestExercise = request.exerciseTypeIds.some(id => 
    id.includes("44556677-8899-aabb-ccdd-eeff00112233") // REST exercise type ID
  );
  
  if (isRestExercise) {
    if (request.kineticChainId !== "") {
      errors.push("REST exercises must have empty kineticChainId");
    }
    if (request.exerciseWeightTypeId !== "") {
      errors.push("REST exercises must have empty exerciseWeightTypeId");
    }
    if (request.muscleGroups.length > 0) {
      errors.push("REST exercises must have empty muscleGroups");
    }
    if (request.exerciseTypeIds.length > 1) {
      errors.push("REST exercise type cannot be combined with other types");
    }
  } else {
    if (!request.kineticChainId || request.kineticChainId === "") {
      errors.push("Non-REST exercises must have a valid kineticChainId");
    }
    if (!request.exerciseWeightTypeId || request.exerciseWeightTypeId === "") {
      errors.push("Non-REST exercises must have a valid exerciseWeightTypeId");
    }
    if (!request.muscleGroups || request.muscleGroups.length === 0) {
      errors.push("Non-REST exercises must have at least one muscle group");
    }
  }
  
  return errors;
}

export type {
  CoachNoteRequest,
  MuscleGroupAssignment,
  CreateExerciseRequest,
  UpdateExerciseRequest
};

export {
  validateExerciseRequest
};