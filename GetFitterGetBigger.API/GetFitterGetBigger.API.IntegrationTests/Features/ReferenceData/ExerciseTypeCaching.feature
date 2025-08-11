Feature: Exercise Types Caching
  As a system administrator
  I want exercise types data to be cached
  So that repeated requests don't hit the database unnecessarily

  Background:
    Given the database is empty
    And the database has reference data
    And I am tracking database queries

  @caching @reference-data
  Scenario: Calling get all exercise types twice should only hit database once
    When I send a GET request to "/api/ReferenceTables/ExerciseTypes"
    Then the response status should be 200
    And the database query count should be 1
    When I send a GET request to "/api/ReferenceTables/ExerciseTypes"
    Then the response status should be 200
    And the database query count should be 1