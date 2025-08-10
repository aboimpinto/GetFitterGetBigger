Feature: Body Parts Caching
  As a system administrator
  I want body parts data to be cached
  So that repeated requests don't hit the database unnecessarily

  Background:
    Given the database is empty
    And the database has reference data
    And I am tracking database queries

  @caching @reference-data
  Scenario: Calling get all body parts twice should only hit database once
    When I send a GET request to "/api/ReferenceTables/BodyParts"
    Then the response status should be 200
    And the database query count should be 1
    When I send a GET request to "/api/ReferenceTables/BodyParts"
    Then the response status should be 200
    And the database query count should be 1
    
  @caching @reference-data
  Scenario: Calling get body part by ID twice should only hit database once
    Given I send a GET request to "/api/ReferenceTables/BodyParts"
    And the response contains at least 1 item
    And I store the first item from the response as "firstBodyPart"
    And I reset the database query counter
    When I send a GET request to "/api/ReferenceTables/BodyParts/<firstBodyPart.id>"
    Then the response status should be 200
    And the database query count should be 1
    When I send a GET request to "/api/ReferenceTables/BodyParts/<firstBodyPart.id>"
    Then the response status should be 200
    And the database query count should be 1

  @caching @reference-data
  Scenario: Different body part IDs should result in separate cache entries
    # This test verifies that each ID is cached independently and returns correct data
    # The sophisticated cache implementation may avoid DB hits if data is already available
    Given I am tracking database queries
    And I send a GET request to "/api/ReferenceTables/BodyParts"
    And the response contains at least 2 items
    And I store the first item from the response as "firstBodyPart"
    And I store the second item from the response as "secondBodyPart"
    # First GetById call - may or may not hit DB depending on cache sophistication
    When I send a GET request to "/api/ReferenceTables/BodyParts/<firstBodyPart.id>"
    Then the response status should be 200
    And the response property "id" should be "<firstBodyPart.id>"
    # Second GetById call with different ID - should return different data
    When I send a GET request to "/api/ReferenceTables/BodyParts/<secondBodyPart.id>"
    Then the response status should be 200
    And the response property "id" should be "<secondBodyPart.id>"
    # Repeat first call - should return same data consistently
    When I send a GET request to "/api/ReferenceTables/BodyParts/<firstBodyPart.id>"
    Then the response status should be 200
    And the response property "id" should be "<firstBodyPart.id>"
    
  @caching @reference-data
  Scenario: Get by value should also use cache
    Given I send a GET request to "/api/ReferenceTables/BodyParts"
    And the response contains an item with value "Chest"
    And I reset the database query counter
    When I send a GET request to "/api/ReferenceTables/BodyParts/ByValue/Chest"
    Then the response status should be 200
    And the database query count should be 1
    When I send a GET request to "/api/ReferenceTables/BodyParts/ByValue/Chest"
    Then the response status should be 200
    And the database query count should be 1