Feature: Equipment Management
    As a personal trainer
    I want to manage gym equipment
    So that I can assign appropriate equipment to exercises

Background:
    # ⚠️ TODO: Verify with stakeholders - which roles can manage equipment?
    # Currently assuming PT-Tier based on exercise management pattern
    Given I am authenticated as a "PT-Tier"
    And the database has reference data

Scenario: Create new equipment successfully
    When I send a POST request to "/api/equipment" with body:
        """
        {
            "name": "Olympic Barbell",
            "description": "Standard 20kg Olympic barbell for heavy lifting"
        }
        """
    Then the response status should be 201
    And the response should have property "name" with value "Olympic Barbell"
    And the response should have property "description" with value "Standard 20kg Olympic barbell for heavy lifting"
    And the response should have property "id"
    And the database should contain 1 equipment record

Scenario: Fail to create duplicate equipment
    Given the following equipment exists:
        | Name     | Description                |
        | Dumbbell | Adjustable weight dumbbell |
    When I send a POST request to "/api/equipment" with body:
        """
        {
            "name": "Dumbbell",
            "description": "Another type of dumbbell"
        }
        """
    Then the response status should be 409
    And the response should contain "already exists"

Scenario: Update existing equipment
    Given the following equipment exists:
        | Name           | Description             |
        | Cable Machine  | Basic cable machine     |
    When I send a PUT request to "/api/equipment/<CableMachine.Id>" with body:
        """
        {
            "name": "Dual Cable Machine",
            "description": "Advanced dual-stack cable machine with multiple attachments"
        }
        """
    Then the response status should be 200
    And the response should have property "name" with value "Dual Cable Machine"
    And the response should have property "description" with value "Advanced dual-stack cable machine with multiple attachments"

Scenario: Delete equipment not used by any exercise
    Given the following equipment exists:
        | Name        | Description          |
        | Jump Rope   | Speed rope for cardio |
    And no exercises are using the equipment "Jump Rope"
    When I send a DELETE request to "/api/equipment/<JumpRope.Id>"
    Then the response status should be 204
    And the equipment with id "<JumpRope.Id>" should not exist

Scenario: Cannot delete equipment used by exercises
    Given the following equipment exists:
        | Name     | Description         |
        | Barbell  | Standard barbell    |
    And the following exercise exists:
        | Name        | Equipment |
        | Bench Press | Barbell   |
    When I send a DELETE request to "/api/equipment/<Barbell.Id>"
    Then the response status should be 409
    And the response should contain "cannot be deleted"
    And the response should contain "used by exercises"

Scenario: Get equipment by ID
    Given the following equipment exists:
        | Name          | Description                    |
        | Kettlebell    | 16kg cast iron kettlebell      |
    When I send a GET request to "/api/equipment/<Kettlebell.Id>"
    Then the response status should be 200
    And the response should have property "name" with value "Kettlebell"
    And the response should have property "description" with value "16kg cast iron kettlebell"

Scenario: List all equipment with pagination
    Given the database has reference data
    When I send a GET request to "/api/equipment?pageNumber=1&pageSize=5"
    Then the response status should be 200
    And the response should have property "items"
    And the response should have property "totalCount"
    And the response should have property "pageNumber" with value "1"
    And the response should have property "pageSize" with value "5"

Scenario: Search equipment by name
    Given the following equipment exists:
        | Name              | Description                  |
        | Resistance Bands  | Set of resistance bands      |
        | Power Bands       | Heavy duty power bands       |
        | Mini Bands        | Small loop resistance bands  |
    When I send a GET request to "/api/equipment?search=bands&pageNumber=1&pageSize=10"
    Then the response status should be 200
    And the response should have property "items"
    And the response should have property "totalCount" with value "3"

@authorization
Scenario Outline: Verify authorization for equipment management
    Given I am authenticated as a "<role>"
    When I send a POST request to "/api/equipment" with body:
        """
        {
            "name": "Test Equipment",
            "description": "Test Description"
        }
        """
    Then the response status should be <expectedStatus>
    
    Examples:
        | role       | expectedStatus | notes                                  |
        | PT-Tier    | 201           | # TODO: Confirm PT can create         |
        | Admin-Tier | 201           | # TODO: Confirm Admin can create      |
        | Free-Tier  | 403           | # TODO: Confirm Free cannot create    |

@authorization
Scenario Outline: Verify read access for different roles
    Given I am authenticated as a "<role>"
    And the database has reference data
    When I send a GET request to "/api/equipment"
    Then the response status should be <expectedStatus>
    
    Examples:
        | role       | expectedStatus | notes                           |
        | PT-Tier    | 200           | # PT can view equipment        |
        | Admin-Tier | 200           | # Admin can view equipment     |
        | Free-Tier  | 200           | # Free can view equipment      |

Scenario: Equipment validation - name required
    Given I am authenticated as a "PT-Tier"
    When I send a POST request to "/api/equipment" with body:
        """
        {
            "description": "Equipment without name"
        }
        """
    Then the response status should be 400
    And the response should contain "Name is required"

Scenario: Equipment validation - name length
    Given I am authenticated as a "PT-Tier"
    When I send a POST request to "/api/equipment" with body:
        """
        {
            "name": "A",
            "description": "Name too short"
        }
        """
    Then the response status should be 400
    And the response should contain "Name must be at least"