Feature: REST Exercise Muscle Group Validation
  As a fitness application
  I want to ensure that REST exercises handle muscle groups correctly
  So that REST periods are properly configured

  Background:
    Given the database has reference data
    Given I am authenticated as a user

  @exercise @rest-validation
  Scenario: Create REST exercise without muscle groups succeeds
    When I send a POST request to "/api/exercises" with body:
      """
      {
        "name": "Rest Period Without Muscle Groups",
        "description": "Recovery time between sets",
        "coachNotes": [],
        "exerciseTypeIds": ["exercisetype-d4e5f6a7-8b9c-0d1e-2f3a-4b5c6d7e8f9a"],
        "isUnilateral": false,
        "difficultyId": "difficultylevel-8a8adb1d-24d2-4979-a5a6-0d760e6da24b",
        "kineticChainId": null,
        "exerciseWeightTypeId": null,
        "muscleGroups": [],
        "equipmentIds": [],
        "bodyPartIds": [],
        "movementPatternIds": []
      }
      """
    Then the response status should be 201
    And the response should have property "name" with value "Rest Period Without Muscle Groups"
    And the response should have property "muscleGroups" as array with length 0
    And the response should have property "exerciseTypes" as array with length 1
    And the response property "exerciseTypes[0].value" should be "Rest"

  @exercise @rest-validation
  Scenario: Create non-REST exercise without muscle groups fails
    When I send a POST request to "/api/exercises" with body:
      """
      {
        "name": "Invalid Workout Without Muscle Groups",
        "description": "This should fail - workout needs muscle groups",
        "coachNotes": [],
        "exerciseTypeIds": ["exercisetype-b2c3d4e5-6f7a-8b9c-0d1e-2f3a4b5c6d7e"],
        "isUnilateral": false,
        "difficultyId": "difficultylevel-8a8adb1d-24d2-4979-a5a6-0d760e6da24b",
        "kineticChainId": "kineticchaintype-f5d5a2de-9c4e-4b87-b8c3-5d1e17d0b1f4",
        "exerciseWeightTypeId": "exerciseweighttype-b2e4d3c5-6a7b-5c8d-9e0f-1a2b3c4d5e6f",
        "muscleGroups": [],
        "equipmentIds": [],
        "bodyPartIds": ["bodypart-7c5a2d6e-e87e-4c8a-9f1d-9eb734f3df3c"],
        "movementPatternIds": []
      }
      """
    Then the response status should be 400

  @exercise @rest-validation
  Scenario: Create REST exercise with muscle groups returns bad request
    When I send a GET request to "/api/ReferenceTables/MuscleGroups"
    Then the response status should be 200
    And I store the first item from the response as "firstMuscleGroup"
    
    When I send a POST request to "/api/exercises" with body:
      """
      {
        "name": "Invalid Rest Period With Muscle Groups",
        "description": "REST exercise cannot have muscle groups",
        "coachNotes": [],
        "exerciseTypeIds": ["exercisetype-d4e5f6a7-8b9c-0d1e-2f3a-4b5c6d7e8f9a"],
        "isUnilateral": false,
        "difficultyId": "difficultylevel-8a8adb1d-24d2-4979-a5a6-0d760e6da24b",
        "kineticChainId": null,
        "exerciseWeightTypeId": null,
        "muscleGroups": [
          {
            "muscleGroupId": "<firstMuscleGroup.id>",
            "muscleRoleId": "musclerole-5d8e9a7b-3c2d-4f6a-9b8c-1e5d7f3a2c9b"
          }
        ],
        "equipmentIds": [],
        "bodyPartIds": [],
        "movementPatternIds": []
      }
      """
    Then the response status should be 400

