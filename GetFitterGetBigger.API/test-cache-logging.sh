#!/bin/bash

echo "Starting API server with detailed logging..."
cd GetFitterGetBigger.API

# Run the API with debug logging level
dotnet run --urls http://localhost:5214 &
API_PID=$!

# Wait for the API to start
sleep 5

echo -e "\n=== Testing Cache Operations ===\n"

echo "1. First GET request (should be a cache MISS):"
curl -s http://localhost:5214/api/ReferenceTables/Equipment | jq length

echo -e "\n2. Second GET request (should be a cache HIT):"
curl -s http://localhost:5214/api/ReferenceTables/Equipment | jq length

echo -e "\n3. GET by ID (should be a cache MISS first time):"
curl -s http://localhost:5214/api/ReferenceTables/Equipment/equipment-33445566-7788-99aa-bbcc-ddeeff001122 | jq .value

echo -e "\n4. GET by ID again (should be a cache HIT):"
curl -s http://localhost:5214/api/ReferenceTables/Equipment/equipment-33445566-7788-99aa-bbcc-ddeeff001122 | jq .value

echo -e "\n5. Creating new equipment (should trigger cache invalidation):"
NEW_ID=$(curl -X POST http://localhost:5214/api/ReferenceTables/Equipment \
  -H "Content-Type: application/json" \
  -d '{"name": "Cache Test Equipment"}' \
  -s | jq -r .id)
echo "Created equipment with ID: $NEW_ID"

echo -e "\n6. GET all after creation (should be a cache MISS due to invalidation):"
curl -s http://localhost:5214/api/ReferenceTables/Equipment | jq length

echo -e "\n7. Updating equipment (should trigger cache invalidation):"
curl -X PUT http://localhost:5214/api/ReferenceTables/Equipment/$NEW_ID \
  -H "Content-Type: application/json" \
  -d '{"name": "Updated Cache Test Equipment"}' \
  -s | jq .name

echo -e "\n8. Deactivating equipment (should trigger cache invalidation):"
curl -X DELETE http://localhost:5214/api/ReferenceTables/Equipment/$NEW_ID \
  -w "\nHTTP Status: %{http_code}\n" \
  -s

echo -e "\n\nStopping API server..."
kill $API_PID

echo -e "\n=== Test completed ==="