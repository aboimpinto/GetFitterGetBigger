Feature: Database Persistence
  As a fitness application
  I want to test data persistence across requests
  So that I can ensure data is properly stored and retrieved

  Background:
    Given I am authenticated as a user
    And the database has reference data

  @database @persistence @integration
  Scenario: Create and retrieve exercise with shared database persists across requests
    When I create a workout exercise named "Persistence Test Exercise"
    And I set the difficulty to "Beginner"
    And I set the kinetic chain type to "Compound"
    And I set the weight type to "Weight Required"
    And I add muscle group "Chest" as "Primary"
    And I add coach note "Test Note" with order 1
    And I submit the exercise
    Then the response status should be 201
    And I store the response property "id" as "createdExerciseId"
    When I send a GET request to "/api/exercises/<createdExerciseId>"
    Then the response status should be 200
    And the response should have property "name" with value "Persistence Test Exercise"
    And the response should have property "description" with value "Test description for Persistence Test Exercise"
    And the response should have property "coachNotes" as array with length 1
    And the response property "coachNotes[0].text" should be "Test Note"
    And the response property "coachNotes[0].order" should be "1"