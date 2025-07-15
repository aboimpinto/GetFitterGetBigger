#!/bin/bash
# Debug script to test empty GUID handling

echo "Testing Movement Pattern Empty GUID..."
curl -X GET http://localhost:5214/api/ReferenceTables/MovementPatterns/movementpattern-00000000-0000-0000-0000-000000000000 -v

echo -e "\n\nTesting Body Part Empty GUID..."
curl -X GET http://localhost:5214/api/ReferenceTables/BodyParts/bodypart-00000000-0000-0000-0000-000000000000 -v