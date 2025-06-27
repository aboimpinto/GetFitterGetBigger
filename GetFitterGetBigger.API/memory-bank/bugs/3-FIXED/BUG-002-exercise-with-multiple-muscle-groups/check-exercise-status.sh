#!/bin/bash
# Script to check exercise status and diagnose empty results

echo "=================================================="
echo "Exercise Status Diagnostic Script"
echo "=================================================="
echo ""

API_URL="http://localhost:5214"

echo "Test 1: Get ALL exercises (including inactive)"
echo "=============================================="
echo "Running: GET /api/Exercises?IncludeInactive=true"
echo ""

response=$(curl -s -w "\nHTTP_CODE:%{http_code}" "$API_URL/api/Exercises?IncludeInactive=true&pageSize=50" -H "accept: application/json")
http_code=$(echo "$response" | grep "HTTP_CODE:" | cut -d: -f2)
body=$(echo "$response" | sed '/HTTP_CODE:/d')

if [ "$http_code" = "200" ]; then
    echo "✅ Request successful"
    
    if command -v jq &> /dev/null; then
        total=$(echo "$body" | jq '.totalCount')
        items=$(echo "$body" | jq '.items | length')
        
        echo "Total Count: $total"
        echo "Items in response: $items"
        echo ""
        
        if [ "$items" -gt 0 ]; then
            echo "Exercises found:"
            echo "$body" | jq -r '.items[] | "- Name: \(.name), IsActive: \(.isActive), MuscleGroups: \(.exerciseMuscleGroups | length)"'
        else
            echo "❌ No exercises found even with IncludeInactive=true"
        fi
    else
        echo "$body"
    fi
else
    echo "❌ Request failed - Status Code: $http_code"
    echo "Response: $body"
fi

echo ""
echo "Test 2: Get only ACTIVE exercises (default)"
echo "==========================================="
echo "Running: GET /api/Exercises"
echo ""

response=$(curl -s -w "\nHTTP_CODE:%{http_code}" "$API_URL/api/Exercises?pageSize=50" -H "accept: application/json")
http_code=$(echo "$response" | grep "HTTP_CODE:" | cut -d: -f2)
body=$(echo "$response" | sed '/HTTP_CODE:/d')

if [ "$http_code" = "200" ]; then
    echo "✅ Request successful"
    
    if command -v jq &> /dev/null; then
        total=$(echo "$body" | jq '.totalCount')
        items=$(echo "$body" | jq '.items | length')
        
        echo "Total Count: $total"
        echo "Items in response: $items"
        
        if [ "$total" = "0" ] && [ "$items" = "0" ]; then
            echo ""
            echo "⚠️  No active exercises found!"
            echo "   This suggests all exercises in the database are marked as IsActive=false"
            echo "   Or there are no exercises in the database at all"
        fi
    else
        echo "$body"
    fi
else
    echo "❌ Request failed - Status Code: $http_code"
fi

echo ""
echo "=================================================="
echo "Diagnosis:"
echo "=================================================="
echo "If Test 1 shows exercises but Test 2 doesn't:"
echo "  → All exercises are marked as IsActive=false"
echo ""
echo "If both tests show no exercises:"
echo "  → Database has no exercise records"
echo "  → Or there's a connection/configuration issue"
echo ""
echo "Check the API logs for [ExerciseRepository] messages"
echo "=================================================="