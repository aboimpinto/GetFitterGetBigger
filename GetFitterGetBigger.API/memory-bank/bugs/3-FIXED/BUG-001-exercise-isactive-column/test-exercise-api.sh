#!/bin/bash
# Test script for Exercise API endpoint - BUG-001 verification

echo "========================================"
echo "Exercise API Endpoint Test Script"
echo "BUG-001: IsActive Column Missing Fix"
echo "========================================"
echo ""

# Check if API is running
echo "Checking if API is running on port 5214..."
if ! curl -s -o /dev/null -w "%{http_code}" http://localhost:5214/health 2>/dev/null | grep -q "200"; then
    echo "❌ API is not running. Please start the API with: dotnet run"
    echo "   From directory: /home/esqueleto/myWork/GetFitterGetBigger/GetFitterGetBigger.API/GetFitterGetBigger.API"
    exit 1
fi
echo "✅ API is running"
echo ""

# Test 1: Get all active exercises (default)
echo "Test 1: Get all active exercises (default behavior)"
echo "Expected: Should return only active exercises without error"
echo "Command: curl -X GET 'http://localhost:5214/api/Exercises'"
echo "----------------------------------------"
response=$(curl -s -w "\nHTTP_CODE:%{http_code}" "http://localhost:5214/api/Exercises" -H "accept: application/json")
http_code=$(echo "$response" | grep "HTTP_CODE:" | cut -d: -f2)
body=$(echo "$response" | sed '/HTTP_CODE:/d')

if [ "$http_code" = "200" ]; then
    echo "✅ Test 1 PASSED - Status Code: $http_code"
    echo "Response:"
    echo "$body" | jq '.' 2>/dev/null || echo "$body"
else
    echo "❌ Test 1 FAILED - Status Code: $http_code"
    echo "Response: $body"
fi
echo ""

# Test 2: Get all exercises including inactive
echo "Test 2: Get all exercises including inactive"
echo "Expected: Should return both active and inactive exercises"
echo "Command: curl -X GET 'http://localhost:5214/api/Exercises?IncludeInactive=true'"
echo "----------------------------------------"
response=$(curl -s -w "\nHTTP_CODE:%{http_code}" "http://localhost:5214/api/Exercises?IncludeInactive=true" -H "accept: application/json")
http_code=$(echo "$response" | grep "HTTP_CODE:" | cut -d: -f2)
body=$(echo "$response" | sed '/HTTP_CODE:/d')

if [ "$http_code" = "200" ]; then
    echo "✅ Test 2 PASSED - Status Code: $http_code"
    echo "Response:"
    echo "$body" | jq '.' 2>/dev/null || echo "$body"
else
    echo "❌ Test 2 FAILED - Status Code: $http_code"
    echo "Response: $body"
fi
echo ""

# Test 3: Filter exercises by name
echo "Test 3: Filter exercises by name containing 'press'"
echo "Expected: Should return filtered exercises without error"
echo "Command: curl -X GET 'http://localhost:5214/api/Exercises?Name=press'"
echo "----------------------------------------"
response=$(curl -s -w "\nHTTP_CODE:%{http_code}" "http://localhost:5214/api/Exercises?Name=press" -H "accept: application/json")
http_code=$(echo "$response" | grep "HTTP_CODE:" | cut -d: -f2)
body=$(echo "$response" | sed '/HTTP_CODE:/d')

if [ "$http_code" = "200" ]; then
    echo "✅ Test 3 PASSED - Status Code: $http_code"
    echo "Response:"
    echo "$body" | jq '.' 2>/dev/null || echo "$body"
else
    echo "❌ Test 3 FAILED - Status Code: $http_code"
    echo "Response: $body"
fi
echo ""

echo "========================================"
echo "Test Summary"
echo "========================================"
echo "If all tests show ✅ PASSED, the bug has been fixed successfully."
echo "The PostgreSQL error 'column e.IsActive does not exist' should no longer occur."
echo ""
echo "Note: If tests fail, ensure:"
echo "1. The API is running (dotnet run)"
echo "2. The database migrations have been applied (dotnet ef database update)"
echo "3. The database connection is properly configured"