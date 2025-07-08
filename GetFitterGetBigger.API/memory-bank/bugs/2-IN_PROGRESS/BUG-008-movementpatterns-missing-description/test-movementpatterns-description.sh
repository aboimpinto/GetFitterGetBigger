#!/bin/bash
# Test script for MovementPatterns Description field

# Start the API (assuming it's running on localhost:5214)
echo "Testing MovementPatterns endpoints for Description field..."

# Test GetAll endpoint
echo -e "\n1. Testing GET /api/movementpatterns"
curl -s http://localhost:5214/api/movementpatterns | jq '.[0]'

# Test GetById endpoint (using a known ID - adjust as needed)
echo -e "\n2. Testing GET /api/movementpatterns/{id}"
# Get first ID from GetAll response
ID=$(curl -s http://localhost:5214/api/movementpatterns | jq -r '.[0].id')
curl -s http://localhost:5214/api/movementpatterns/$ID | jq '.'

# Test GetByName endpoint
echo -e "\n3. Testing GET /api/movementpatterns/ByName/{name}"
curl -s http://localhost:5214/api/movementpatterns/ByName/Squat | jq '.'

# Test GetByValue endpoint
echo -e "\n4. Testing GET /api/movementpatterns/ByValue/{value}"
curl -s http://localhost:5214/api/movementpatterns/ByValue/Squat | jq '.'

echo -e "\nAll endpoints should return objects with 'id', 'value', and 'description' fields."