Feature: Exercise CRUD Operations
  As a fitness application
  I want to manage exercises through CRUD operations
  So that I can maintain a comprehensive exercise database

  Background:
    Given I am authenticated as a user
    And the database has reference data

  @exercise @crud
  Scenario: Get paginated list of exercises
    When I send a GET request to "/api/exercises"
    Then the response status should be 200
    And the response should have property "items"
    And the response should have property "currentPage" with value "1"
    And the response should have property "pageSize" with value "10"
    And the response should have property "totalCount"
    And the response should have property "totalPages"

  @exercise @crud
  Scenario: Get exercise by ID after creating one
    # First create an exercise
    When I send a GET request to "/api/ReferenceTables/MuscleGroups"
    Then the response status should be 200
    And I store the first item from the response as "firstMuscleGroup"
    
    When I send a POST request to "/api/exercises" with body:
      """
      {
        "name": "Test Exercise for Get By ID",
        "description": "Test Description",
        "coachNotes": [
          {
            "text": "Step 1: Starting position",
            "order": 0
          }
        ],
        "exerciseTypeIds": ["exercisetype-b2c3d4e5-6f7a-8b9c-0d1e-2f3a4b5c6d7e"],
        "isUnilateral": false,
        "difficultyId": "difficultylevel-8a8adb1d-24d2-4979-a5a6-0d760e6da24b",
        "kineticChainId": "kineticchaintype-f5d5a2de-9c4e-4b87-b8c3-5d1e17d0b1f4",
        "exerciseWeightTypeId": "exerciseweighttype-b2e4d3c5-6a7b-5c8d-9e0f-1a2b3c4d5e6f",
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
    Then the response status should be 201
    And the response headers should contain "Location"
    And the response should have property "id"
    And the response should have property "name" with value "Test Exercise for Get By ID"
    And the response should have property "description" with value "Test Description"

  @exercise @crud
  Scenario: Get non-existent exercise returns 404
    When I send a GET request to "/api/exercises/exercise-00000000-0000-0000-0000-000000000000"
    Then the response status should be 404

  @exercise @crud @search
  Scenario: Search exercises by name
    When I send a GET request to "/api/exercises?search=squat"
    Then the response status should be 200
    And the response should have property "items"
    And the response property "items" should be a JSON array

  @exercise @crud @filter
  Scenario: Filter exercises by difficulty level
    # First get a difficulty level ID
    When I send a GET request to "/api/ReferenceTables/DifficultyLevels"
    Then the response status should be 200
    And I store the first item from the response as "firstDifficulty"
    
    When I send a GET request to "/api/exercises?difficultyId=<firstDifficulty.id>"
    Then the response status should be 200
    And the response should have property "items"

  @exercise @crud @filter
  Scenario: Filter exercises by muscle group
    # First get a muscle group ID
    When I send a GET request to "/api/ReferenceTables/MuscleGroups"
    Then the response status should be 200
    And I store the first item from the response as "firstMuscleGroup"
    
    When I send a GET request to "/api/exercises?muscleGroupId=<firstMuscleGroup.id>"
    Then the response status should be 200
    And the response should have property "items"

  @exercise @crud @pagination
  Scenario: Get exercises with custom page size
    When I send a GET request to "/api/exercises?pageSize=5&page=1"
    Then the response status should be 200
    And the response should have property "pageSize" with value "5"
    And the response should have property "currentPage" with value "1"

  @exercise @crud @create
  Scenario: Create minimal valid exercise
    When I send a GET request to "/api/ReferenceTables/MuscleGroups"
    Then the response status should be 200
    And I store the first item from the response as "firstMuscleGroup"
    
    When I send a POST request to "/api/exercises" with body:
      """
      {
        "name": "Test Exercise CRUD",
        "description": "Test Description for CRUD",
        "coachNotes": [
          {
            "text": "Step 1: Starting position",
            "order": 0
          },
          {
            "text": "Step 2: Execute movement",
            "order": 1
          }
        ],
        "exerciseTypeIds": ["exercisetype-b2c3d4e5-6f7a-8b9c-0d1e-2f3a4b5c6d7e"],
        "isUnilateral": false,
        "difficultyId": "difficultylevel-8a8adb1d-24d2-4979-a5a6-0d760e6da24b",
        "kineticChainId": "kineticchaintype-f5d5a2de-9c4e-4b87-b8c3-5d1e17d0b1f4",
        "exerciseWeightTypeId": "exerciseweighttype-b2e4d3c5-6a7b-5c8d-9e0f-1a2b3c4d5e6f",
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
    Then the response status should be 201
    And the response should have property "name" with value "Test Exercise CRUD"
    And the response should have property "description" with value "Test Description for CRUD"
    And the response should have property "coachNotes" as array with length 2
    And the response headers should contain "Location"