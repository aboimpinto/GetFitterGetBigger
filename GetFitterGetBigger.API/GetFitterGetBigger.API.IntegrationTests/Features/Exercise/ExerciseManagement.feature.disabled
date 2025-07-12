Feature: Exercise Management
    As a personal trainer
    I want to manage exercises
    So that I can create workout plans for my clients

Background:
    # ⚠️ TODO: Verify with stakeholders - which roles can manage exercises?
    # Currently assuming PT-Tier based on existing tests, but MUST CONFIRM
    Given I am authenticated as a "PT-Tier"
    And the database has reference data

Scenario: Create a new workout exercise successfully
    When I send a POST request to "/api/exercises" with body:
        """
        {
            "name": "Bench Press",
            "description": "Classic chest exercise",
            "difficultyId": "difficultylevel-00000000-0000-0000-0000-000000000001",
            "kineticChainId": "kineticchaintype-00000000-0000-0000-0000-000000000001",
            "exerciseWeightTypeId": "exerciseweighttype-00000000-0000-0000-0000-000000000001",
            "exerciseTypeIds": ["exercisetype-00000000-0000-0000-0000-000000000001"],
            "muscleGroups": [
                {
                    "muscleGroupId": "musclegroup-00000000-0000-0000-0000-000000000001",
                    "muscleRoleId": "musclerole-00000000-0000-0000-0000-000000000001"
                }
            ],
            "equipmentIds": ["equipment-00000000-0000-0000-0000-000000000001"],
            "bodyPartIds": ["bodypart-00000000-0000-0000-0000-000000000001"],
            "movementPatternIds": [],
            "coachNotes": [
                {
                    "text": "Keep your back flat on the bench",
                    "order": 1
                }
            ],
            "isUnilateral": false
        }
        """
    Then the response status should be 201
    And the response should have property "name" with value "Bench Press"
    And the response should have property "description" with value "Classic chest exercise"
    And the database should contain 1 exercise records

Scenario: Fail to create duplicate exercise
    Given the following exercise exists:
        | Name        | Description      | DifficultyLevel |
        | Squat       | Leg exercise     | Beginner        |
    When I send a POST request to "/api/exercises" with body:
        """
        {
            "name": "Squat",
            "description": "Another leg exercise",
            "difficultyId": "difficultylevel-00000000-0000-0000-0000-000000000001",
            "exerciseTypeIds": ["exercisetype-00000000-0000-0000-0000-000000000001"]
        }
        """
    Then the response status should be 409
    And the response should contain "already exists"

Scenario: Update existing exercise
    Given the following exercise exists:
        | Name        | Description    | DifficultyLevel |
        | Deadlift    | Back exercise  | Intermediate    |
    When I send a PUT request to "/api/exercises/<Deadlift.Id>" with body:
        """
        {
            "name": "Romanian Deadlift",
            "description": "Updated back exercise focusing on hamstrings",
            "difficultyId": "difficultylevel-00000000-0000-0000-0000-000000000003"
        }
        """
    Then the response status should be 200
    And the response should have property "name" with value "Romanian Deadlift"
    And the response should have property "description" with value "Updated back exercise focusing on hamstrings"

Scenario: Delete an exercise
    Given the following exercise exists:
        | Name           | Description         | DifficultyLevel |
        | Bicep Curl     | Arm isolation       | Beginner        |
    When I send a DELETE request to "/api/exercises/<BicepCurl.Id>"
    Then the response status should be 204
    And the exercise with id "<BicepCurl.Id>" should not exist

Scenario: Get exercise by ID
    Given the following exercise exists:
        | Name           | Description         | DifficultyLevel |
        | Pull-ups       | Upper body compound | Advanced        |
    When I send a GET request to "/api/exercises/<Pullups.Id>"
    Then the response status should be 200
    And the response should have property "name" with value "Pull-ups"
    And the response should have property "description" with value "Upper body compound"

Scenario: List all exercises with pagination
    Given the database has reference data
    When I send a GET request to "/api/exercises?pageNumber=1&pageSize=10"
    Then the response status should be 200
    And the response should have property "items"
    And the response should have property "totalCount"
    And the response should have property "pageNumber" with value "1"
    And the response should have property "pageSize" with value "10"

@authorization
Scenario Outline: Verify authorization for exercise management
    Given I am authenticated as a "<role>"
    When I send a POST request to "/api/exercises" with body:
        """
        {
            "name": "Test Exercise",
            "description": "Test",
            "difficultyId": "difficultylevel-00000000-0000-0000-0000-000000000001",
            "exerciseTypeIds": ["exercisetype-00000000-0000-0000-0000-000000000001"]
        }
        """
    Then the response status should be <expectedStatus>
    
    Examples:
        | role       | expectedStatus | notes                               |
        | PT-Tier    | 201           | # TODO: Confirm PT can create      |
        | Admin-Tier | 201           | # TODO: Confirm Admin can create   |
        | Free-Tier  | 403           | # TODO: Confirm Free cannot create |

@authorization
Scenario: Unauthenticated access is denied
    Given I am not authenticated
    When I send a GET request to "/api/exercises"
    Then the response status should be 401