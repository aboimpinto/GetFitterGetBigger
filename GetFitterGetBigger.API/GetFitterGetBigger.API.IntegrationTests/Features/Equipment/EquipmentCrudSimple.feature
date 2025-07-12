Feature: Equipment CRUD Simple Operations
  As a fitness application
  I want to manage equipment through CRUD operations
  So that I can maintain equipment data

  Background:
    Given I am authenticated as a user
    And the database has reference data

  @equipment @crud
  Scenario: Equipment CRUD full flow success
    # Create equipment
    When I send a POST request to "/api/ReferenceTables/Equipment" with body:
      """
      {
        "name": "Test Equipment CRUD Flow"
      }
      """
    Then the response status should be 201
    And the response should have property "name" with value "Test Equipment CRUD Flow"
    And the response should have property "isActive" with value "true"
    And I store the response property "id" as "createdEquipmentId"
    
    # Update equipment
    When I send a PUT request to "/api/ReferenceTables/Equipment/<createdEquipmentId>" with body:
      """
      {
        "name": "Test Equipment CRUD Flow Updated"
      }
      """
    Then the response status should be 200
    And the response should have property "name" with value "Test Equipment CRUD Flow Updated"
    
    # Delete equipment
    When I send a DELETE request to "/api/ReferenceTables/Equipment/<createdEquipmentId>"
    Then the response status should be 204
    
    # Verify it's gone from GetAll
    When I send a GET request to "/api/ReferenceTables/Equipment"
    Then the response status should be 200
    And the response should be a JSON array
    And the response should not contain item with property "id" equals "<createdEquipmentId>"

  @equipment @validation
  Scenario: Create equipment with empty name returns bad request
    When I send a POST request to "/api/ReferenceTables/Equipment" with body:
      """
      {
        "name": ""
      }
      """
    Then the response status should be 400

  @equipment @validation
  Scenario: Update non-existent equipment returns not found
    When I send a PUT request to "/api/ReferenceTables/Equipment/equipment-00000000-0000-0000-0000-000000000000" with body:
      """
      {
        "name": "Updated"
      }
      """
    Then the response status should be 404

  @equipment @validation
  Scenario: Delete non-existent equipment returns not found
    When I send a DELETE request to "/api/ReferenceTables/Equipment/equipment-00000000-0000-0000-0000-000000000000"
    Then the response status should be 404

  @equipment @validation
  Scenario Outline: Invalid ID format returns bad request
    When I send a GET request to "/api/ReferenceTables/Equipment/<invalidId>"
    Then the response status should be 400
    
    When I send a PUT request to "/api/ReferenceTables/Equipment/<invalidId>" with body:
      """
      {
        "name": "Test"
      }
      """
    Then the response status should be 400
    
    When I send a DELETE request to "/api/ReferenceTables/Equipment/<invalidId>"
    Then the response status should be 400

    Examples:
      | invalidId           |
      | invalid-format      |
      | equipment-not-a-guid |
      | 12345               |

  @equipment @crud
  Scenario: Create equipment then get by ID success
    # Create equipment
    When I send a POST request to "/api/ReferenceTables/Equipment" with body:
      """
      {
        "name": "Test GetById Equipment"
      }
      """
    Then the response status should be 201
    And I store the response property "id" as "equipmentId"
    And the response property "id" should start with "equipment-"
    
    # Get it by ID
    When I send a GET request to "/api/ReferenceTables/Equipment/<equipmentId>"
    Then the response status should be 200
    And the response should have property "id" with value "<equipmentId>"
    And the response should have property "value" with value "Test GetById Equipment"
    
    # Cleanup
    When I send a DELETE request to "/api/ReferenceTables/Equipment/<equipmentId>"
    Then the response status should be 204

  @equipment @crud
  Scenario: Update just created equipment success
    # Create equipment
    When I send a POST request to "/api/ReferenceTables/Equipment" with body:
      """
      {
        "name": "Test Update Equipment"
      }
      """
    Then the response status should be 201
    And I store the response property "id" as "updateEquipmentId"
    
    # Now update it
    When I send a PUT request to "/api/ReferenceTables/Equipment/<updateEquipmentId>" with body:
      """
      {
        "name": "Test Update Equipment Updated"
      }
      """
    Then the response status should be 200
    
    # Cleanup
    When I send a DELETE request to "/api/ReferenceTables/Equipment/<updateEquipmentId>"
    Then the response status should be 204

  @equipment @validation
  Scenario: Create equipment trims whitespace
    When I send a POST request to "/api/ReferenceTables/Equipment" with body:
      """
      {
        "name": "  Test Trim Equipment  "
      }
      """
    Then the response status should be 201
    And the response should have property "name" with value "Test Trim Equipment"
    And I store the response property "id" as "trimEquipmentId"
    
    # Cleanup
    When I send a DELETE request to "/api/ReferenceTables/Equipment/<trimEquipmentId>"
    Then the response status should be 204