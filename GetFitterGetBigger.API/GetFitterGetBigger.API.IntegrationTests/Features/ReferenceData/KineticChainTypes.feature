Feature: Kinetic Chain Types Reference Data
  As a system user
  I want to access kinetic chain types reference data
  So that I can use them in exercise management

  Background:
    Given the database is empty
    And the database has reference data

  @reference-data @smoke
  Scenario: Get all kinetic chain types
    When I send a GET request to "/api/ReferenceTables/KineticChainTypes"
    Then the response status should be 200
    And the response should be a JSON array

  @reference-data
  Scenario: Get kinetic chain type by valid ID
    Given I send a GET request to "/api/ReferenceTables/KineticChainTypes"
    And the response contains at least 1 item
    And I store the first item from the response as "firstKineticChainType"
    When I send a GET request to "/api/ReferenceTables/KineticChainTypes/<firstKineticChainType.id>"
    Then the response status should be 200
    And the response should have property "id" with value "<firstKineticChainType.id>"
    And the response should have property "value" with value "<firstKineticChainType.value>"

  @reference-data @validation
  Scenario: Get kinetic chain type by invalid ID format returns bad request
    When I send a GET request to "/api/ReferenceTables/KineticChainTypes/f5d5a2de-9c4e-4b87-b8c3-5d1e17d0b1f4"
    Then the response status should be 400
    And the response should contain "Invalid ID format"
    And the response should contain "Expected format: 'kineticchaintype-{guid}'"

  @reference-data @validation
  Scenario: Get kinetic chain type by non-existent ID returns not found
    When I send a GET request to "/api/ReferenceTables/KineticChainTypes/kineticchaintype-00000000-0000-0000-0000-000000000000"
    Then the response status should be 404

  @reference-data
  Scenario: Get kinetic chain type by valid value
    Given I send a GET request to "/api/ReferenceTables/KineticChainTypes"
    And the response contains at least 1 item
    And I store the first item from the response as "firstKineticChainType"
    When I send a GET request to "/api/ReferenceTables/KineticChainTypes/ByValue/<firstKineticChainType.value>"
    Then the response status should be 200
    And the response should have property "value" with value "<firstKineticChainType.value>"

  @reference-data @validation
  Scenario: Get kinetic chain type by non-existent value returns not found
    When I send a GET request to "/api/ReferenceTables/KineticChainTypes/ByValue/InvalidType"
    Then the response status should be 404

  @reference-data @validation
  Scenario Outline: Get kinetic chain type by value with different casing
    Given I send a GET request to "/api/ReferenceTables/KineticChainTypes"
    And the response contains an item with value "Compound"
    When I send a GET request to "/api/ReferenceTables/KineticChainTypes/ByValue/<casing>"
    Then the response status should be 200
    And the response should have property "value" with value "Compound"

    Examples:
      | casing   |
      | Compound |
      | compound |
      | COMPOUND |
      | CoMpOuNd |