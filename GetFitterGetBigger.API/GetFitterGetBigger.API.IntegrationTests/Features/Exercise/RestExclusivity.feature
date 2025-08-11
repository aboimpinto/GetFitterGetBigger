Feature: Exercise Rest Type Exclusivity
  As a fitness application
  I want to ensure that REST type exercises cannot be combined with other types
  So that exercise categorization remains consistent

  Background:
    Given the database has reference data
    Given I am authenticated as a user

  @exercise @rest-exclusivity
  Scenario: Create exercise with only REST type succeeds
    When I send a POST request to "/api/exercises" with body:
      """
      {
        "name": "Valid Rest Period",
        "description": "A rest period between exercises",
        "coachNotes": [
          {
            "text": "Take a 90 second break",
            "order": 1
          },
          {
            "text": "Hydrate during this time",
            "order": 2
          }
        ],
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
    And the response should have property "name" with value "Valid Rest Period"
    And the response should have property "exerciseTypes" as array with length 1
    And the response property "exerciseTypes[0].value" should be "Rest"

  @exercise @rest-exclusivity
  Scenario: Create exercise with REST and Warmup types returns bad request
    When I send a POST request to "/api/exercises" with body:
      """
      {
        "name": "Invalid Rest with Warmup",
        "description": "Trying to combine Rest with Warmup",
        "coachNotes": [],
        "exerciseTypeIds": [
          "exercisetype-d4e5f6a7-8b9c-0d1e-2f3a-4b5c6d7e8f9a",
          "exercisetype-a1b2c3d4-5e6f-7a8b-9c0d-1e2f3a4b5c6d"
        ],
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
    Then the response status should be 400
    And the response body should contain "Invalid exercise type configuration"

  @exercise @rest-exclusivity
  Scenario: Create exercise with REST and Workout types returns bad request
    When I send a POST request to "/api/exercises" with body:
      """
      {
        "name": "Invalid Rest with Workout",
        "description": "Trying to combine Rest with Workout",
        "coachNotes": [],
        "exerciseTypeIds": [
          "exercisetype-b2c3d4e5-6f7a-8b9c-0d1e-2f3a4b5c6d7e",
          "exercisetype-d4e5f6a7-8b9c-0d1e-2f3a-4b5c6d7e8f9a"
        ],
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
    Then the response status should be 400
    And the response body should contain "Invalid exercise type configuration"

  @exercise @rest-exclusivity
  Scenario: Create exercise with REST and all other types returns bad request
    When I send a POST request to "/api/exercises" with body:
      """
      {
        "name": "Invalid Rest with All Types",
        "description": "Trying to combine Rest with all other types",
        "coachNotes": [],
        "exerciseTypeIds": [
          "exercisetype-a1b2c3d4-5e6f-7a8b-9c0d-1e2f3a4b5c6d",
          "exercisetype-b2c3d4e5-6f7a-8b9c-0d1e-2f3a4b5c6d7e",
          "exercisetype-c3d4e5f6-7a8b-9c0d-1e2f-3a4b5c6d7e8f",
          "exercisetype-d4e5f6a7-8b9c-0d1e-2f3a-4b5c6d7e8f9a"
        ],
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
    Then the response status should be 400
    And the response body should contain "Invalid exercise type configuration"

  @exercise @rest-exclusivity
  Scenario: Create exercise without REST type allows multiple types
    When I send a POST request to "/api/exercises" with body:
      """
      {
        "name": "Multiple Types Without Rest",
        "description": "Exercise with multiple non-Rest types",
        "coachNotes": [],
        "exerciseTypeIds": [
          "exercisetype-a1b2c3d4-5e6f-7a8b-9c0d-1e2f3a4b5c6d",
          "exercisetype-b2c3d4e5-6f7a-8b9c-0d1e-2f3a4b5c6d7e",
          "exercisetype-c3d4e5f6-7a8b-9c0d-1e2f-3a4b5c6d7e8f"
        ],
        "isUnilateral": false,
        "difficultyId": "difficultylevel-8a8adb1d-24d2-4979-a5a6-0d760e6da24b",
        "kineticChainId": "kineticchaintype-f5d5a2de-9c4e-4b87-b8c3-5d1e17d0b1f4",
        "exerciseWeightTypeId": "exerciseweighttype-b2e4d3c5-6a7b-5c8d-9e0f-1a2b3c4d5e6f",
        "muscleGroups": [
          {
            "muscleGroupId": "musclegroup-ccddeeff-0011-2233-4455-667788990011",
            "muscleRoleId": "musclerole-5d8e9a7b-3c2d-4f6a-9b8c-1e5d7f3a2c9b"
          }
        ],
        "equipmentIds": [],
        "bodyPartIds": ["bodypart-7c5a2d6e-e87e-4c8a-9f1d-9eb734f3df3c"],
        "movementPatternIds": []
      }
      """
    Then the response status should be 201
    And the response should have property "name" with value "Multiple Types Without Rest"
    And the response should have property "exerciseTypes" as array with length 3