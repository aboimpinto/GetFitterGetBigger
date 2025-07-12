Feature: Equipment Reference Data
  As a system user
  I want to access equipment reference data
  So that I can use them in exercise management

  Background:
    Given the database is empty
    And the database has reference data

  @reference-data @smoke
  Scenario: Get all equipment
    When I send a GET request to "/api/ReferenceTables/Equipment"
    Then the response status should be 200
    And the response should be a JSON array

  @reference-data
  Scenario: Get equipment by valid ID
    Given I send a GET request to "/api/ReferenceTables/Equipment"
    And the response contains at least 1 item
    And I store the first item from the response as "firstEquipment"
    When I send a GET request to "/api/ReferenceTables/Equipment/<firstEquipment.id>"
    Then the response status should be 200
    And the response should have property "id" with value "<firstEquipment.id>"
    And the response should have property "value" with value "<firstEquipment.value>"

  @reference-data @validation
  Scenario: Get equipment by invalid ID format returns bad request
    When I send a GET request to "/api/ReferenceTables/Equipment/abcdef12-3456-7890-abcd-ef1234567890"
    Then the response status should be 400
    And the response should contain "Invalid ID format"
    And the response should contain "Expected format: 'equipment-{guid}'"

  @reference-data @validation
  Scenario: Get equipment by non-existent ID returns not found
    When I send a GET request to "/api/ReferenceTables/Equipment/equipment-00000000-0000-0000-0000-000000000000"
    Then the response status should be 404

  @reference-data
  Scenario: Get equipment by valid value
    Given I send a GET request to "/api/ReferenceTables/Equipment"
    And the response contains at least 1 item
    And I store the first item from the response as "firstEquipment"
    When I send a GET request to "/api/ReferenceTables/Equipment/ByValue/<firstEquipment.value>"
    Then the response status should be 200
    And the response should have property "value" with value "<firstEquipment.value>"

  @reference-data @validation
  Scenario: Get equipment by non-existent value returns not found
    When I send a GET request to "/api/ReferenceTables/Equipment/ByValue/NonExistentEquipment"
    Then the response status should be 404

  @reference-data
  Scenario: Get equipment by value with different casing - Barbell
    Given I send a GET request to "/api/ReferenceTables/Equipment"
    And the response contains an item with value "Barbell"
    When I send a GET request to "/api/ReferenceTables/Equipment/ByValue/barbell"
    Then the response status should be 200
    And the response should have property "value" with value "Barbell"

  @reference-data
  Scenario: Get equipment by value with different casing - BARBELL
    Given I send a GET request to "/api/ReferenceTables/Equipment"
    And the response contains an item with value "Barbell"
    When I send a GET request to "/api/ReferenceTables/Equipment/ByValue/BARBELL"
    Then the response status should be 200
    And the response should have property "value" with value "Barbell"

  @reference-data
  Scenario: Get equipment by value with different casing - BaRbElL
    Given I send a GET request to "/api/ReferenceTables/Equipment"
    And the response contains an item with value "Barbell"
    When I send a GET request to "/api/ReferenceTables/Equipment/ByValue/BaRbElL"
    Then the response status should be 200
    And the response should have property "value" with value "Barbell"