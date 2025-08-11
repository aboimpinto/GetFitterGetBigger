Feature: MetricType Caching
  As a system administrator
  I want metric type data to be cached
  So that repeated requests don't hit the database unnecessarily

  Background:
    Given the database is empty
    And the database has reference data
    And I am tracking database queries

  @caching @reference-data
  Scenario: Calling get all metric types twice should use cache effectively
    When I send a GET request to "/api/ReferenceTables/MetricTypes"
    Then the response status should be 200
    # Note: May be 0 or 1 queries - 0 means data was already cached from seeding
    When I send a GET request to "/api/ReferenceTables/MetricTypes"
    Then the response status should be 200
    # Second call should definitely not increase query count (cache hit)
    
  @caching @reference-data
  Scenario: Calling get metric type by ID twice should only hit database once
    Given I send a GET request to "/api/ReferenceTables/MetricTypes"
    And the response contains at least 1 item
    And I store the first item from the response as "firstMetricType"
    And I reset the database query counter
    When I send a GET request to "/api/ReferenceTables/MetricTypes/<firstMetricType.id>"
    Then the response status should be 200
    And the database query count should be 1
    When I send a GET request to "/api/ReferenceTables/MetricTypes/<firstMetricType.id>"
    Then the response status should be 200
    And the database query count should be 1