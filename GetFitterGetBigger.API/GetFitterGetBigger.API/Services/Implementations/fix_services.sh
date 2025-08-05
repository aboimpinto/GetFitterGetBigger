#\!/bin/bash

# List of services and their entity types
declare -A services=(
    ["ExecutionProtocolService.cs"]="ExecutionProtocol"
    ["ExerciseTypeService.cs"]="ExerciseType"
    ["ExerciseWeightTypeService.cs"]="ExerciseWeightType"
    ["KineticChainTypeService.cs"]="KineticChainType"
    ["MetricTypeService.cs"]="MetricType"
    ["MovementPatternService.cs"]="MovementPattern"
    ["MuscleRoleService.cs"]="MuscleRole"
    ["WorkoutCategoryService.cs"]="WorkoutCategory"
    ["WorkoutObjectiveService.cs"]="WorkoutObjective"
    ["WorkoutStateService.cs"]="WorkoutState"
)

for file in "${\!services[@]}"; do
    entity="${services[$file]}"
    echo "Fixing $file for entity $entity"
    
    # Create a temporary fixed version
    sed '/LoadEntityByIdAsync/,/^    }$/{
        s/\.MatchAsync(/.Match(/
        /\.Match($/,/^            );$/{
            s/whenValid: async () => await LoadEntityFromRepository([^)]*)/whenValid: async () => await LoadEntityFromRepository(\1),\n                whenInvalid: errors => ServiceResult<'"$entity"'>.Failure(\n                    '"$entity"'.Empty,\n                    ServiceError.ValidationFailed(errors.FirstOrDefault() ?? "Invalid ID format"))/
        }
    }' "$file" > "${file}.tmp"
    
    # Replace original with fixed version
    mv "${file}.tmp" "$file"
done
