Feature: Body Parts Reference Data
  As a system user
  I want to access body parts reference data
  So that I can use them in exercise management

  Background:
    Given the database is empty
    And the database has reference data

  @reference-data @smoke
  Scenario: Get all body parts
    When I send a GET request to "/api/ReferenceTables/BodyParts"
    Then the response status should be 200
    And the response should be a JSON array

  @reference-data
  Scenario: Get body part by valid ID
    Given I send a GET request to "/api/ReferenceTables/BodyParts"
    And the response contains at least 1 item
    And I store the first item from the response as "firstBodyPart"
    When I send a GET request to "/api/ReferenceTables/BodyParts/<firstBodyPart.id>"
    Then the response status should be 200
    And the response should have property "id" with value "<firstBodyPart.id>"
    And the response should have property "value" with value "<firstBodyPart.value>"

  @reference-data @validation
  Scenario: Get body part by invalid ID format returns bad request
    When I send a GET request to "/api/ReferenceTables/BodyParts/7c5a2d6e-e87e-4c8a-9f1d-9eb734f3df3c"
    Then the response status should be 400

  @reference-data @validation
  Scenario: Get body part by empty GUID returns not found
    When I send a GET request to "/api/ReferenceTables/BodyParts/bodypart-00000000-0000-0000-0000-000000000000"
    Then the response status should be 400

  @reference-data @validation
  Scenario: Get body part by non-existent ID returns not found
    When I send a GET request to "/api/ReferenceTables/BodyParts/bodypart-11111111-1111-1111-1111-111111111111"
    Then the response status should be 404

  @reference-data
  Scenario: Get body part by valid value
    Given I send a GET request to "/api/ReferenceTables/BodyParts"
    And the response contains at least 1 item
    And I store the first item from the response as "firstBodyPart"
    When I send a GET request to "/api/ReferenceTables/BodyParts/ByValue/<firstBodyPart.value>"
    Then the response status should be 200
    And the response should have property "value" with value "<firstBodyPart.value>"

  @reference-data @validation
  Scenario: Get body part by non-existent value returns not found
    When I send a GET request to "/api/ReferenceTables/BodyParts/ByValue/NonExistentBodyPart"
    Then the response status should be 404

  @reference-data @validation
  Scenario Outline: Get body part by value with different casing
    Given I send a GET request to "/api/ReferenceTables/BodyParts"
    And the response contains an item with value "Chest"
    When I send a GET request to "/api/ReferenceTables/BodyParts/ByValue/<casing>"
    Then the response status should be 200
    And the response should have property "value" with value "Chest"

    Examples:
      | casing |
      | Chest  |
      | chest  |
      | CHEST  |
      | ChEsT  |