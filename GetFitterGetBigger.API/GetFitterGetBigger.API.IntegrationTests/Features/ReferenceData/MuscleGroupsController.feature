Feature: Muscle Groups API Controller Operations
  As an API consumer  
  I want to perform CRUD operations on muscle groups
  So that I can manage muscle group reference data

  Background:
    Given I am authenticated as a "PT-Tier"
    And the system has been initialized with seed data

  @reference-data @muscle-groups @read
  Scenario: Get all muscle groups returns collection
    When I get all muscle groups via API
    Then the response status should be "ok"
    And I should receive a collection of muscle groups

  @reference-data @muscle-groups @read
  Scenario: Get muscle group by valid ID returns muscle group
    Given I have muscle groups available in the system
    When I get the first muscle group by ID via API
    Then the response status should be "ok"
    And I should receive the muscle group
    And the muscle group ID should match the requested ID
    And the muscle group name should match the expected name

  @reference-data @muscle-groups @validation
  Scenario: Get muscle group by invalid ID format returns bad request
    When I get muscle group by ID "invalid-format" via API
    Then the response status should be "bad request"

  @reference-data @muscle-groups @validation
  Scenario: Get muscle group by empty GUID returns bad request
    When I get muscle group by ID "musclegroup-00000000-0000-0000-0000-000000000000" via API
    Then the response status should be "bad request"

  @reference-data @muscle-groups @validation  
  Scenario: Get muscle group by non-existent ID returns not found
    When I get muscle group by ID "musclegroup-11111111-1111-1111-1111-111111111111" via API
    Then the response status should be "not found"

  @reference-data @muscle-groups @read
  Scenario: Get muscle group by valid value returns muscle group
    Given I have muscle groups available in the system
    When I get the first muscle group by value via API
    Then the response status should be "ok"
    And I should receive the muscle group
    And the muscle group name should match the requested value

  @reference-data @muscle-groups @validation
  Scenario: Get muscle group by non-existent value returns not found
    When I get muscle group by value "NonExistentMuscleGroup" via API
    Then the response status should be "not found"

  @reference-data @muscle-groups @read
  Scenario Outline: Get muscle group by value is case insensitive
    Given I have a muscle group named "Pectoralis" in the system
    When I get muscle group by value "<casing_variant>" via API
    Then the response status should be "ok"
    And I should receive the muscle group
    And the muscle group name should be "Pectoralis"

    Examples:
      | casing_variant |
      | Pectoralis     |
      | pectoralis     |
      | PECTORALIS     |
      | PeCtOrAlIs     |

  @reference-data @muscle-groups @create
  Scenario: Create muscle group with valid data returns created muscle group
    Given I have body parts available in the system
    When I create muscle group "TestMuscleGroup" with first available body part via API
    Then the response status should be "created"
    And I should receive the created muscle group
    And the muscle group name should be "TestMuscleGroup"
    And the muscle group should be active
    And the muscle group created timestamp should be set
    And the muscle group updated timestamp should be null
    And I clean up the created muscle group

  @reference-data @muscle-groups @validation
  Scenario: Create muscle group with duplicate name returns conflict
    Given I have body parts available in the system
    And I have created a muscle group "DuplicateTestMuscleGroup" via API
    When I create muscle group "DuplicateTestMuscleGroup" with first available body part via API
    Then the response status should be "conflict"
    And I clean up the created muscle group

  @reference-data @muscle-groups @validation
  Scenario: Create muscle group with invalid body part ID returns bad request
    When I create muscle group "TestMuscleGroup" with body part ID "invalid-format" via API
    Then the response status should be "bad request"

  @reference-data @muscle-groups @update
  Scenario: Update muscle group with valid data returns updated muscle group
    Given I have body parts available in the system
    And I have created a muscle group "OriginalMuscleGroup" via API
    When I update the muscle group to name "UpdatedMuscleGroup" via API
    Then the response status should be "ok"
    And I should receive the updated muscle group
    And the muscle group name should be "UpdatedMuscleGroup"
    And the muscle group updated timestamp should be set
    And I clean up the created muscle group

  @reference-data @muscle-groups @validation
  Scenario: Update muscle group with empty GUID returns bad request
    Given I have body parts available in the system
    When I update muscle group "musclegroup-00000000-0000-0000-0000-000000000000" to name "UpdatedName" via API
    Then the response status should be "bad request"

  @reference-data @muscle-groups @validation
  Scenario: Update non-existent muscle group returns not found
    Given I have body parts available in the system
    When I update muscle group "musclegroup-11111111-1111-1111-1111-111111111111" to name "UpdatedName" via API
    Then the response status should be "not found"

  @reference-data @muscle-groups @delete
  Scenario: Delete existing muscle group returns no content
    Given I have body parts available in the system
    And I have created a muscle group "DeleteTestMuscleGroup" via API
    When I delete the muscle group via API
    Then the response status should be "no content"
    And the muscle group should no longer be retrievable

  @reference-data @muscle-groups @validation
  Scenario: Delete muscle group with empty GUID returns bad request
    When I delete muscle group "musclegroup-00000000-0000-0000-0000-000000000000" via API
    Then the response status should be "bad request"

  @reference-data @muscle-groups @validation
  Scenario: Delete non-existent muscle group returns not found
    When I delete muscle group "musclegroup-11111111-1111-1111-1111-111111111111" via API
    Then the response status should be "not found"