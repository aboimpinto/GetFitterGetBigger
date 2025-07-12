Feature: Exercises Controller
  As a fitness application
  I want to test core exercise CRUD operations
  So that I can validate basic exercise management functionality

  Background:
    Given I am authenticated as a user
    And the database has reference data

  @exercise @controller @crud
  Scenario: Get exercise with invalid ID returns not found
    When I send a GET request to "/api/exercises/exercise-99999999-9999-9999-9999-999999999999"
    Then the response status should be 404

  @exercise @controller @crud
  Scenario: Create exercise with valid data returns created exercise
    When I create a workout exercise named "Test Exercise"
    And I set the difficulty to "Beginner"
    And I set the kinetic chain type to "Compound"
    And I set the weight type to "Weight Required"
    And I add muscle group "Chest" as "Primary"
    And I set the video URL to "https://example.com/video.mp4"
    And I set the image URL to "https://example.com/image.jpg"
    And I submit the exercise
    Then the response status should be 201
    And the response should have property "name" with value "Test Exercise"
    And the response should have property "description" with value "Test description for Test Exercise"
    And the response should have property "videoUrl" with value "https://example.com/video.mp4"
    And the response should have property "imageUrl" with value "https://example.com/image.jpg"

  @exercise @controller @crud @validation
  Scenario: Create exercise with invalid data returns bad request
    When I send a POST request to "/api/exercises" with body:
      """
      {
        "name": "",
        "description": "Test Description",
        "coachNotes": []
      }
      """
    Then the response status should be 400

  @exercise @controller @crud
  Scenario: Update exercise with non-existent ID returns not found
    When I send a PUT request to "/api/exercises/exercise-99999999-9999-9999-9999-999999999999" with body:
      """
      {
        "name": "Updated Name",
        "description": "Updated Description",
        "difficultyId": "difficultylevel-8a8adb1d-24d2-4979-a5a6-0d760e6da24b",
        "kineticChainId": "kineticchaintype-f5d5a2de-9c4e-4b87-b8c3-5d1e17d0b1f4",
        "exerciseWeightTypeId": "exerciseweighttype-b2e4d3c5-6a7b-5c8d-9e0f-1a2b3c4d5e6f",
        "exerciseTypeIds": ["exercisetype-87b3e2a1-5d8c-4f9e-a2b3-c4d5e6f7a8b9"],
        "muscleGroups": [
          {
            "muscleGroupId": "musclegroup-23456789-0123-4567-8901-234567890123",
            "muscleRoleId": "musclerole-87654321-0987-6543-2109-876543210987"
          }
        ],
        "equipmentIds": [],
        "bodyPartIds": [],
        "movementPatternIds": [],
        "coachNotes": [],
        "isUnilateral": false
      }
      """
    Then the response status should be 404

  @exercise @controller @crud
  Scenario: Delete exercise with non-existent ID returns not found
    When I send a DELETE request to "/api/exercises/exercise-99999999-9999-9999-9999-999999999999"
    Then the response status should be 404