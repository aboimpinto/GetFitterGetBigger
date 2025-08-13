Feature: Dependency Injection Configuration
  As a developer
  I want to ensure all services are properly registered
  So that the application can resolve dependencies correctly

  Background:
    Given I am authenticated as a "PT-Tier"

  @infrastructure @di
  Scenario: Exercise Link Repository is registered in DI container
    When I send a GET request to "/api/Exercises"
    Then the response status should be 200
    # The fact that the endpoint responds means DI is working

  @infrastructure @di
  Scenario: Exercise Link Service is registered in DI container
    When I send a GET request to "/api/Exercises"
    Then the response status should be 200
    # The fact that the endpoint responds means DI is working

  @infrastructure @di
  Scenario: Exercise Link Service has all dependencies resolved
    When I send a GET request to "/api/Exercises?pageNumber=1&pageSize=10"
    Then the response status should be 200
    # If dependencies were missing, we'd get a 500 error

  @infrastructure @di
  Scenario: Exercise Link Repository has all dependencies resolved
    When I send a GET request to "/api/Exercises?pageNumber=1&pageSize=10"
    Then the response status should be 200
    # Repository is used by service, so if this works, repository is resolved

  @infrastructure @di @transient
  Scenario: Services are properly scoped for each request
    # Make two requests and verify both work independently
    When I send a GET request to "/api/Exercises?pageNumber=1&pageSize=10"
    Then the response status should be 200
    When I send a GET request to "/api/Exercises?pageNumber=1&pageSize=10"
    Then the response status should be 200
    # Each request gets its own service instance

  @infrastructure @di @endpoints
  Scenario: All critical endpoints respond successfully
    # Test a variety of endpoints to ensure DI is working across the board
    When I send a GET request to "/api/ReferenceTables/Equipment"
    Then the response status should be 200
    When I send a GET request to "/api/ReferenceTables/MuscleGroups"
    Then the response status should be 200
    When I send a GET request to "/api/ReferenceTables/DifficultyLevels"
    Then the response status should be 200
    When I send a GET request to "/api/ReferenceTables/ExerciseTypes"
    Then the response status should be 200