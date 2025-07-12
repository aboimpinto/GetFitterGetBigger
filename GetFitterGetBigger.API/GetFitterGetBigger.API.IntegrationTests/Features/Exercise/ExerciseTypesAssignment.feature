Feature: Exercise Types Assignment
  As a fitness application
  I want to manage exercise types for exercises
  So that I can categorize exercises appropriately

  Background:
    Given I am authenticated as a user
    And the database has reference data

  @exercise @types
  Scenario: Create exercise with single exercise type
    When I create a workout exercise named "Single Type Exercise"
    And I set the difficulty to "Beginner"
    And I set the kinetic chain type to "Compound"
    And I set the weight type to "Weight Required"
    And I add muscle group "Chest" as "Primary"
    And I submit the exercise
    Then the response status should be 201
    And the response should have property "exerciseTypes"
    And the response property "exerciseTypes" should be a JSON array
    And the response should have property "exerciseTypes" as array with length 1
    And the response property "exerciseTypes[0].value" should be "Workout"

  @exercise @types
  Scenario: Create exercise with multiple exercise types
    When I create a workout exercise named "Multi Type Exercise"
    And I set the difficulty to "Beginner"
    And I set the kinetic chain type to "Compound"
    And I set the weight type to "Weight Required"
    And I add muscle group "Chest" as "Primary"
    And I add exercise type "Warmup"
    And I add exercise type "Cooldown"
    And I submit the exercise
    Then the response status should be 201
    And the response should have property "exerciseTypes"
    And the response property "exerciseTypes" should be a JSON array
    And the response should have property "exerciseTypes" as array with length 3

  @exercise @types
  Scenario: Create exercise with no exercise types
    When I create an exercise with no types
    And I set the difficulty to "Beginner"
    And I set the kinetic chain type to "Compound"
    And I set the weight type to "Weight Required"
    And I submit the exercise
    Then the response status should be 201
    And the response should have property "exerciseTypes"
    And the response property "exerciseTypes" should be a JSON array
    And the response should have property "exerciseTypes" as array with length 0

  @exercise @types @validation
  Scenario: Create exercise with mixed valid and invalid exercise type IDs filters invalid ones
    When I create an exercise with mixed valid and invalid types
    And I set the difficulty to "Beginner"
    And I set the kinetic chain type to "Compound"
    And I set the weight type to "Weight Required"
    And I add muscle group "Chest" as "Primary"
    And I submit the exercise
    Then the response status should be 201
    And the response should have property "exerciseTypes"
    And the response property "exerciseTypes" should be a JSON array
    # Should only have the valid exercise type (Workout)
    And the response should have property "exerciseTypes" as array with length 1
    And the response property "exerciseTypes[0].value" should be "Workout"

  @exercise @types @validation
  Scenario: Create exercise with duplicate exercise type IDs deduplicates types
    When I create an exercise with duplicate types: "Workout, Workout, Warmup"
    And I set the difficulty to "Beginner"
    And I set the kinetic chain type to "Compound"
    And I set the weight type to "Weight Required"
    And I submit the exercise
    Then the response status should be 201
    And the response should have property "exerciseTypes"
    And the response property "exerciseTypes" should be a JSON array
    # Should only have 2 unique types
    And the response should have property "exerciseTypes" as array with length 2