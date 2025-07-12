Feature: Metric Types Reference Data
  As a system user
  I want to access metric types reference data
  So that I can use them in exercise management

  Background:
    Given the database is empty
    And the database has reference data

  @reference-data @smoke
  Scenario: Get all metric types
    When I send a GET request to "/api/ReferenceTables/MetricTypes"
    Then the response status should be 200
    And the response should be a JSON array


  @reference-data @validation
  Scenario: Get metric type by invalid ID format returns bad request
    When I send a GET request to "/api/ReferenceTables/MetricTypes/abcdef12-3456-7890-abcd-ef1234567890"
    Then the response status should be 400
    And the response should contain "Invalid ID format"
    And the response should contain "Expected format: 'metrictype-{guid}'"

  @reference-data @validation
  Scenario: Get metric type by non-existent ID returns not found
    When I send a GET request to "/api/ReferenceTables/MetricTypes/metrictype-00000000-0000-0000-0000-000000000000"
    Then the response status should be 404

  @reference-data @validation
  Scenario: Get metric type by non-existent value returns not found
    When I send a GET request to "/api/ReferenceTables/MetricTypes/ByValue/NonExistentMetricType"
    Then the response status should be 404