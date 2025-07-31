Feature: Exercise Basic Operations
  As a fitness application
  I want to perform basic exercise operations
  So that I can manage exercises efficiently

  Background:
    Given I am authenticated as a user
    And the database has reference data

  @exercise @basic
  Scenario: Get exercises without any exercises returns paginated list
    When I send a GET request to "/api/exercises?page=1&pageSize=10"
    Then the response status should be 200
    And the response should have property "currentPage" with value "1"
    And the response should have property "pageSize" with value "10"
    And the response should have property "items"
    And the response should have property "totalCount"
    And the response should have property "totalPages"

  @exercise @basic @validation
  Scenario: Get exercise with invalid format returns not found
    When I send a GET request to "/api/exercises/invalid-format"
    Then the response status should be 404

  @exercise @basic @validation
  Scenario: Create exercise with empty name returns bad request
    When I send a POST request to "/api/exercises" with body:
      """
      {
        "name": "",
        "description": "Test Description",
        "coachNotes": [
          {
            "text": "Test",
            "order": 0
          }
        ],
        "exerciseTypeIds": ["exercisetype-b2c3d4e5-6f7a-8b9c-0d1e-2f3a4b5c6d7e"],
        "isUnilateral": false,
        "difficultyId": "difficultylevel-8a8adb1d-24d2-4979-a5a6-0d760e6da24b",
        "kineticChainId": "kineticchaintype-f5d5a2de-9c4e-4b87-b8c3-5d1e17d0b1f4",
        "exerciseWeightTypeId": "exerciseweighttype-b2e4d3c5-6a7b-5c8d-9e0f-1a2b3c4d5e6f",
        "muscleGroups": [],
        "equipmentIds": [],
        "bodyPartIds": [],
        "movementPatternIds": []
      }
      """
    Then the response status should be 400

  @exercise @basic @validation
  Scenario: Create exercise with missing muscle groups returns bad request
    When I send a POST request to "/api/exercises" with body:
      """
      {
        "name": "Test Exercise Missing Muscle Groups",
        "description": "Test Description",
        "coachNotes": [
          {
            "text": "Test",
            "order": 0
          }
        ],
        "exerciseTypeIds": ["exercisetype-b2c3d4e5-6f7a-8b9c-0d1e-2f3a4b5c6d7e"],
        "isUnilateral": false,
        "difficultyId": "difficultylevel-8a8adb1d-24d2-4979-a5a6-0d760e6da24b",
        "kineticChainId": "kineticchaintype-f5d5a2de-9c4e-4b87-b8c3-5d1e17d0b1f4",
        "exerciseWeightTypeId": "exerciseweighttype-b2e4d3c5-6a7b-5c8d-9e0f-1a2b3c4d5e6f",
        "muscleGroups": [],
        "equipmentIds": [],
        "bodyPartIds": [],
        "movementPatternIds": []
      }
      """
    Then the response status should be 400