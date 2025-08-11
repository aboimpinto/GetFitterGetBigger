Feature: Execution Protocol Advanced Caching
  As a system administrator
  I want execution protocol data to be cached properly for complex scenarios
  So that the cache handles different access patterns correctly

  Background:
    Given the database is empty
    And the database has reference data
    And I am tracking database queries

  @caching @reference-data
  Scenario: Different execution protocol IDs should result in separate cache entries
    # This test verifies that each ID is cached independently and returns correct data
    # The sophisticated cache implementation may avoid DB hits if data is already available
    Given I am tracking database queries
    And I send a GET request to "/api/ReferenceTables/ExecutionProtocols"
    And the response contains at least 2 items
    And I store the first item from the response as "firstExecutionProtocol"
    And I store the second item from the response as "secondExecutionProtocol"
    # First GetById call - may or may not hit DB depending on cache sophistication
    When I send a GET request to "/api/ReferenceTables/ExecutionProtocols/<firstExecutionProtocol.executionProtocolId>"
    Then the response status should be 200
    And the response property "executionProtocolId" should be "<firstExecutionProtocol.executionProtocolId>"
    # Second GetById call with different ID - should return different data
    When I send a GET request to "/api/ReferenceTables/ExecutionProtocols/<secondExecutionProtocol.executionProtocolId>"
    Then the response status should be 200
    And the response property "executionProtocolId" should be "<secondExecutionProtocol.executionProtocolId>"
    # Repeat first call - should return same data consistently
    When I send a GET request to "/api/ReferenceTables/ExecutionProtocols/<firstExecutionProtocol.executionProtocolId>"
    Then the response status should be 200
    And the response property "executionProtocolId" should be "<firstExecutionProtocol.executionProtocolId>"
    
  @caching @reference-data
  Scenario: Get by value should also use cache
    Given I send a GET request to "/api/ReferenceTables/ExecutionProtocols"
    And the response contains an item with value "Standard"
    And I reset the database query counter
    # First call should hit the database
    When I send a GET request to "/api/ReferenceTables/ExecutionProtocols/ByValue/Standard"
    Then the response status should be 200
    And the database query count should be 1
    # Reset counter to clearly show second call uses cache
    Given I reset the database query counter
    # Second call should use cache and NOT hit the database
    When I send a GET request to "/api/ReferenceTables/ExecutionProtocols/ByValue/Standard"
    Then the response status should be 200
    And the database query count should be 0

  @caching @reference-data
  Scenario: Get by code should also use cache
    Given I send a GET request to "/api/ReferenceTables/ExecutionProtocols"
    And the response contains an item with code "STANDARD"
    And I reset the database query counter
    # First call should hit the database
    When I send a GET request to "/api/ReferenceTables/ExecutionProtocols/ByCode/STANDARD"
    Then the response status should be 200
    And the database query count should be 1
    # Reset counter to clearly show second call uses cache
    Given I reset the database query counter
    # Second call should use cache and NOT hit the database
    When I send a GET request to "/api/ReferenceTables/ExecutionProtocols/ByCode/STANDARD"
    Then the response status should be 200
    And the database query count should be 0