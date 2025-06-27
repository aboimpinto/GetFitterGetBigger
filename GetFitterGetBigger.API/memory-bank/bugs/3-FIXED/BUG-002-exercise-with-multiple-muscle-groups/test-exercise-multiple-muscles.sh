#!/bin/bash
# Test script for BUG-002: Exercise with Multiple Muscle Groups
# This script tests that exercises with multiple muscle groups are correctly returned by the API

echo "=================================================="
echo "BUG-002: Exercise with Multiple Muscle Groups Test"
echo "=================================================="
echo ""

API_URL="http://localhost:5214"

# Check if API is running
echo "Checking if API is running..."
if ! curl -s -o /dev/null -w "%{http_code}" $API_URL/health 2>/dev/null | grep -q "200"; then
    echo "❌ API is not running. Please start the API with: dotnet run"
    echo "   From directory: /home/esqueleto/myWork/GetFitterGetBigger/GetFitterGetBigger.API/GetFitterGetBigger.API"
    exit 1
fi
echo "✅ API is running"
echo ""

# Function to format JSON output
format_json() {
    if command -v jq &> /dev/null; then
        jq '.'
    else
        cat
    fi
}

echo "Step 1: Get all exercises and check for exercises with multiple muscle groups"
echo "============================================================================"
echo "Fetching all exercises..."
response=$(curl -s -w "\nHTTP_CODE:%{http_code}" "$API_URL/api/Exercises?pageSize=50" -H "accept: application/json")
http_code=$(echo "$response" | grep "HTTP_CODE:" | cut -d: -f2)
body=$(echo "$response" | sed '/HTTP_CODE:/d')

if [ "$http_code" = "200" ]; then
    echo "✅ Successfully fetched exercises"
    
    # Count exercises with multiple muscle groups
    if command -v jq &> /dev/null; then
        total_exercises=$(echo "$body" | jq '.items | length')
        exercises_with_multiple_muscles=$(echo "$body" | jq '.items | map(select(.exerciseMuscleGroups | length > 1)) | length')
        
        echo ""
        echo "📊 Statistics:"
        echo "  - Total exercises: $total_exercises"
        echo "  - Exercises with multiple muscle groups: $exercises_with_multiple_muscles"
        
        if [ "$exercises_with_multiple_muscles" -gt 0 ]; then
            echo ""
            echo "✅ Found exercises with multiple muscle groups!"
            echo ""
            echo "Exercises with multiple muscle groups:"
            echo "$body" | jq -r '.items | map(select(.exerciseMuscleGroups | length > 1)) | .[] | "  - \(.name): \(.exerciseMuscleGroups | length) muscle groups"'
        else
            echo ""
            echo "⚠️  No exercises with multiple muscle groups found in the database"
            echo "   This might indicate the bug is still present or no such exercises exist"
        fi
    else
        echo "Response (install jq for better formatting):"
        echo "$body"
    fi
else
    echo "❌ Failed to fetch exercises - Status Code: $http_code"
    echo "Response: $body"
fi

echo ""
echo "Step 2: Get a specific exercise by ID (if you know one with multiple muscles)"
echo "============================================================================"
echo "To test a specific exercise, you can run:"
echo "  curl -X GET '$API_URL/api/Exercises/{exerciseId}' -H 'accept: application/json' | jq"

echo ""
echo "Step 3: Create a test exercise with multiple muscle groups"
echo "============================================================================"
echo "To create a test exercise with multiple muscle groups, use the Admin interface or run:"
echo ""
echo "curl -X POST '$API_URL/api/Exercises' \\"
echo "  -H 'accept: application/json' \\"
echo "  -H 'Content-Type: application/json' \\"
echo "  -d '{"
echo '    "name": "Test Bench Press",'
echo '    "description": "Test exercise with multiple muscle groups",'
echo '    "instructions": "Test instructions",'
echo '    "difficultyId": "[DIFFICULTY_ID]",'
echo '    "muscleGroups": ['
echo '      { "muscleGroupId": "[CHEST_ID]", "muscleRoleId": "[PRIMARY_ID]" },'
echo '      { "muscleGroupId": "[TRICEPS_ID]", "muscleRoleId": "[SECONDARY_ID]" }'
echo '    ]'
echo "  }'"

echo ""
echo "=================================================="
echo "Test Summary"
echo "=================================================="
echo "The bug is FIXED if:"
echo "✓ Exercises with multiple muscle groups appear in the list"
echo "✓ The exerciseMuscleGroups array contains all associated muscles"
echo "✓ No exercises are missing from the results"
echo ""
echo "The bug is NOT FIXED if:"
echo "✗ Exercises with multiple muscle groups are missing from results"
echo "✗ Only exercises with single muscle groups appear"
echo "✗ The API returns errors when fetching exercises"
echo ""
echo "Note: The fix uses AsSplitQuery() to handle multiple includes properly"
echo "=================================================="