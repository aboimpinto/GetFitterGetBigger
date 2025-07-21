Feature: Equipment API Controller Operations
  As an API consumer
  I want to perform CRUD operations on equipment
  So that I can manage equipment reference data

  Background:
    Given I am authenticated as a "PT-Tier"
    And the system has been initialized with seed data

  @equipment @api @read
  Scenario: Get all equipment returns only active equipment
    Given I have created equipment "Test Barbell" via API
    And I have created equipment "Test Dumbbell" via API
    When I get all equipment via API
    Then the response status should be "ok"
    And I should receive a collection of equipment
    And all equipment items should be active

  @equipment @api @read
  Scenario: Get equipment by valid ID returns equipment
    Given I have created equipment "Test Barbell" via API
    When I get the created equipment by ID via API
    Then the response status should be "ok"
    And I should receive the equipment
    And the equipment name should be "Test Barbell"

  @equipment @api @validation
  Scenario: Get equipment by invalid ID format returns bad request
    When I get equipment by ID "invalid-format" via API
    Then the response status should be "bad request"

  @equipment @api @validation
  Scenario: Get equipment by empty GUID returns bad request
    When I get equipment by ID "equipment-00000000-0000-0000-0000-000000000000" via API
    Then the response status should be "bad request"

  @equipment @api @validation
  Scenario: Get equipment by non-existent ID returns not found
    When I get equipment by ID "equipment-11111111-1111-1111-1111-111111111111" via API
    Then the response status should be "not found"

  @equipment @api @read
  Scenario: Get equipment by value returns equipment
    Given I have created equipment "Test Barbell" via API
    When I get equipment by value "Test Barbell" via API
    Then the response status should be "ok"
    And I should receive the equipment reference data
    And the equipment value should be "Test Barbell"

  @equipment @api @read
  Scenario Outline: Get equipment by value is case insensitive
    Given I have created equipment "CaseSensitiveBarbell" via API
    When I get equipment by value "<casing_variant>" via API
    Then the response status should be "ok"
    And I should receive the equipment reference data
    And the equipment value should be "CaseSensitiveBarbell"

    Examples:
      | casing_variant |
      | CaseSensitiveBarbell |
      | casesensitivebarbell |
      | CASESENSITIVEBARBELL |
      | CaSeSenSiTiveBaRbElL |

  @equipment @api @create
  Scenario: Create equipment with valid data returns created equipment
    When I create equipment "NewKettlebell" via API
    Then the response status should be "created"
    And I should receive the created equipment
    And the equipment name should be "NewKettlebell"
    And the equipment should be active
    And the equipment created timestamp should be set
    And the equipment updated timestamp should be null

  @equipment @api @validation
  Scenario: Create equipment with duplicate name returns conflict
    Given I have created equipment "DuplicateBarbell" via API
    When I create equipment "DuplicateBarbell" via API
    Then the response status should be "conflict"
    And the response should contain "already exists"

  @equipment @api @update
  Scenario: Update equipment with valid data returns updated equipment
    Given I have created equipment "OriginalBarbell" via API
    When I update the equipment to name "UpdatedBarbell" via API
    Then the response status should be "ok"
    And I should receive the updated equipment
    And the equipment name should be "UpdatedBarbell"
    And the equipment updated timestamp should be set

  @equipment @api @validation
  Scenario: Update equipment with empty GUID returns bad request
    When I update equipment "equipment-00000000-0000-0000-0000-000000000000" to name "Updated" via API
    Then the response status should be "bad request"

  @equipment @api @validation
  Scenario: Update non-existent equipment returns not found
    When I update equipment "equipment-11111111-1111-1111-1111-111111111111" to name "Updated" via API
    Then the response status should be "not found"

  @equipment @api @validation
  Scenario: Update equipment with duplicate name returns conflict
    Given I have created equipment "ExistingBarbell" via API
    And I have created equipment "OriginalDumbbell" via API
    When I update the second equipment to name "ExistingBarbell" via API
    Then the response status should be "conflict"

  @equipment @api @delete
  Scenario: Delete existing equipment returns no content
    Given I have created equipment "DeleteTestBarbell" via API
    When I delete the equipment via API
    Then the response status should be "no content"
    And the equipment should no longer be retrievable

  @equipment @api @validation
  Scenario: Delete equipment with empty GUID returns bad request
    When I delete equipment "equipment-00000000-0000-0000-0000-000000000000" via API
    Then the response status should be "bad request"

  @equipment @api @validation
  Scenario: Delete non-existent equipment returns not found
    When I delete equipment "equipment-11111111-1111-1111-1111-111111111111" via API
    Then the response status should be "not found"

  @equipment @api @validation
  Scenario: Delete equipment in use returns conflict
    Given I have created equipment "InUseBarbell" via API
    And I have created an exercise "Barbell Squat" that uses the equipment
    When I delete the equipment via API
    Then the response status should be "conflict"
    And the response should contain "in use"