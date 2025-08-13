Feature: Exercise Integration
  As a fitness application
  I want to manage exercises with various attributes
  So that I can provide comprehensive exercise information

  Background:
    Given I am authenticated as a user
    And the database has reference data
    # Get actual IDs from the database
    When I send a GET request to "/api/ReferenceTables/MuscleGroups"
    Then the response status should be 200
    And I store the first item from the response as "firstMuscleGroup"
    When I send a GET request to "/api/ReferenceTables/MuscleRoles"
    Then the response status should be 200
    And I store the first item from the response as "firstMuscleRole"
    When I send a GET request to "/api/ReferenceTables/ExerciseTypes"
    Then the response status should be 200
    And I store the first item from the response as "firstExerciseType"

  @exercise @integration
  Scenario: Create exercise with coach notes returns created exercise with ordered notes
    When I send a POST request to "/api/exercises" with body:
      """
      {
        "name": "Integration Test Squat",
        "description": "Test squat exercise with coach notes",
        "coachNotes": [
          {
            "text": "Warm up properly first",
            "order": 1
          },
          {
            "text": "Keep your back straight",
            "order": 2
          },
          {
            "text": "Control the descent",
            "order": 3
          }
        ],
        "exerciseTypeIds": ["<firstExerciseType.id>"],
        "isUnilateral": false,
        "difficultyId": "difficultylevel-8a8adb1d-24d2-4979-a5a6-0d760e6da24b",
        "kineticChainId": "kineticchaintype-f5d5a2de-9c4e-4b87-b8c3-5d1e17d0b1f4",
        "exerciseWeightTypeId": "exerciseweighttype-b2e4d3c5-6a7b-5c8d-9e0f-1a2b3c4d5e6f",
        "muscleGroups": [
          {
            "muscleGroupId": "<firstMuscleGroup.id>",
            "muscleRoleId": "<firstMuscleRole.id>"
          }
        ],
        "equipmentIds": [],
        "bodyPartIds": [],
        "movementPatternIds": [],
        "videoUrl": "https://example.com/squat.mp4",
        "imageUrl": "https://example.com/squat.jpg"
      }
      """
    Then the response status should be 201
    And the response should have property "name" with value "Integration Test Squat"
    And the response should have property "description" with value "Test squat exercise with coach notes"
    And the response should have property "videoUrl" with value "https://example.com/squat.mp4"
    And the response should have property "imageUrl" with value "https://example.com/squat.jpg"
    And the response should have property "coachNotes" as array with length 3
    And the response property "coachNotes[0].text" should be "Warm up properly first"
    And the response property "coachNotes[0].order" should be "1"
    And the response property "coachNotes[1].text" should be "Keep your back straight"
    And the response property "coachNotes[1].order" should be "2"
    And the response property "coachNotes[2].text" should be "Control the descent"
    And the response property "coachNotes[2].order" should be "3"

  @exercise @integration
  Scenario: Create exercise with multiple exercise types returns created exercise with all types
    When I send a POST request to "/api/exercises" with body:
      """
      {
        "name": "Integration Test Compound Exercise",
        "description": "Test exercise with multiple types",
        "coachNotes": [],
        "exerciseTypeIds": ["exercisetype-b2c3d4e5-6f7a-8b9c-0d1e-2f3a4b5c6d7e", "exercisetype-c3d4e5f6-7a8b-9c0d-1e2f-3a4b5c6d7e8f"],
        "isUnilateral": false,
        "difficultyId": "difficultylevel-8a8adb1d-24d2-4979-a5a6-0d760e6da24b",
        "kineticChainId": "kineticchaintype-f5d5a2de-9c4e-4b87-b8c3-5d1e17d0b1f4",
        "exerciseWeightTypeId": "exerciseweighttype-b2e4d3c5-6a7b-5c8d-9e0f-1a2b3c4d5e6f",
        "muscleGroups": [
          {
            "muscleGroupId": "<firstMuscleGroup.id>",
            "muscleRoleId": "<firstMuscleRole.id>"
          }
        ],
        "equipmentIds": [],
        "bodyPartIds": [],
        "movementPatternIds": []
      }
      """
    Then the response status should be 201
    And the response should have property "name" with value "Integration Test Compound Exercise"
    And the response should have property "exerciseTypes" as array with length 2

  @exercise @integration
  Scenario: Create exercise with empty coach notes returns created exercise with no notes
    When I send a POST request to "/api/exercises" with body:
      """
      {
        "name": "Exercise Without Notes",
        "description": "Test exercise with no coach notes",
        "coachNotes": [],
        "exerciseTypeIds": ["<firstExerciseType.id>"],
        "isUnilateral": false,
        "difficultyId": "difficultylevel-8a8adb1d-24d2-4979-a5a6-0d760e6da24b",
        "kineticChainId": "kineticchaintype-f5d5a2de-9c4e-4b87-b8c3-5d1e17d0b1f4",
        "exerciseWeightTypeId": "exerciseweighttype-b2e4d3c5-6a7b-5c8d-9e0f-1a2b3c4d5e6f",
        "muscleGroups": [
          {
            "muscleGroupId": "<firstMuscleGroup.id>",
            "muscleRoleId": "<firstMuscleRole.id>"
          }
        ],
        "equipmentIds": [],
        "bodyPartIds": [],
        "movementPatternIds": []
      }
      """
    Then the response status should be 201
    And the response should have property "name" with value "Exercise Without Notes"
    And the response should have property "coachNotes" as array with length 0

  @exercise @integration @rest
  Scenario: Create exercise with rest and other types returns bad request
    When I create an exercise with rest and other types
    And I submit the exercise
    Then the response status should be 400

  @exercise @integration @rest
  Scenario: Create exercise with only rest type returns created exercise
    When I create a rest exercise named "Integration Test Rest Period"
    And I submit the exercise
    Then the response status should be 201
    And the response should have property "name" with value "Integration Test Rest Period"
    And the response should have property "exerciseTypes" as array with length 1

  @exercise @integration @update
  Scenario: Update exercise add coach notes updates exercise with new notes
    When I update the exercise "Update Test Exercise" with new coach notes
    And I send a PUT request to update the exercise with coach notes
    Then the response status should be 200
    And the response should have property "name" with value "Updated Test Exercise"
    And the response should have property "coachNotes" as array with length 3
    And the response property "coachNotes[0].text" should be "First step"
    And the response property "coachNotes[0].order" should be "1"
    And the response property "coachNotes[1].text" should be "Second step"
    And the response property "coachNotes[1].order" should be "2"
    And the response property "coachNotes[2].text" should be "Third step"
    And the response property "coachNotes[2].order" should be "3"

  @exercise @integration @update
  Scenario: Update exercise modify existing coach notes updates notes correctly
    When I update the exercise "Exercise With Notes" with new coach notes
    And I send a PUT request to update the exercise with coach notes
    Then the response status should be 200
    And the response should have property "name" with value "Updated Test Exercise"
    And the response should have property "coachNotes" as array with length 3

  @exercise @integration @update
  Scenario: Update exercise change exercise types updates types correctly
    When I update the exercise "Multi-Type Exercise" with new coach notes
    And I send a PUT request to update exercise types
    Then the response status should be 200
    And the response should have property "name" with value "Multi-Type Exercise"
    And the response should have property "exerciseTypes" as array with length 2

  @exercise @integration @update @rest
  Scenario: Update exercise with rest type and other types returns bad request
    When I update the exercise "Normal Exercise" with new coach notes
    And I send a PUT request to update with rest and other types
    Then the response status should be 400
