Feature: Exercise Link Migration Compatibility
    As a system maintaining backward compatibility
    I want existing string-based exercise links to work seamlessly with the enhanced enum system
    So that no functionality is lost during the migration from string to enum LinkTypes

Background:
    Given the system has been initialized with seed data

@exercise @links @integration @migration @backward-compatibility
Scenario: Existing string-based links should work with enhanced API
    Given I have a workout exercise named "Migration Test Workout"
    And I have an exercise named "Migration Test Warmup" with exercise types "Workout,Warmup"
    # Create link using old string format
    When I create an exercise link from "Migration Test Workout" to "Migration Test Warmup" with link type "Warmup" and display order 1
    Then the exercise link should be created successfully
    # Verify it works with enhanced API
    When I get all links for exercise "Migration Test Workout"
    Then I should receive 1 exercise link
    And the link should be accessible via enhanced API
    And the link should have correct enum mapping

@exercise @links @integration @migration @mixed-formats
Scenario: Mixed string and enum requests should work consistently
    Given I have a workout exercise named "Mixed Format Source"
    And I have an exercise named "String Target" with exercise types "Workout,Warmup"
    And I have an exercise named "Enum Target" with exercise types "Workout,Cooldown"
    # Create one link with old string format
    When I create an exercise link from "Mixed Format Source" to "String Target" with link type "Warmup" and display order 1
    Then the exercise link should be created successfully
    # Create another link with new enum format
    When I create an exercise link from "Mixed Format Source" to "Enum Target" with link type "COOLDOWN" and display order 2
    Then the exercise link should be created successfully
    # Verify both work consistently
    When I get all links for exercise "Mixed Format Source"
    Then I should receive 2 exercise links
    And the links should include both "Warmup" and "Cooldown" types

@exercise @links @integration @migration @performance
Scenario: Bidirectional query performance with mixed data formats
    Given I have a workout exercise named "Performance Test Source"
    And I have an exercise named "Performance Target 1" with exercise types "Workout,Warmup"
    And I have an exercise named "Performance Target 2" with exercise types "Workout,Cooldown"
    And I have an exercise named "Performance Target 3" with exercise types "Workout,Warmup"
    And I have an exercise named "Performance Target 4" with exercise types "Workout,Cooldown"
    # Create multiple links with different formats
    When I create an exercise link from "Performance Test Source" to "Performance Target 1" with link type "Warmup" and display order 1
    And I create an exercise link from "Performance Test Source" to "Performance Target 2" with link type "COOLDOWN" and display order 2
    And I create an exercise link from "Performance Test Source" to "Performance Target 3" with link type "WARMUP" and display order 3
    And I create an exercise link from "Performance Test Source" to "Performance Target 4" with link type "Cooldown" and display order 4
    Then all links should be created successfully
    # Verify performance of bidirectional queries
    When I get all links for exercise "Performance Test Source"
    Then I should receive 4 exercise links
    And the response time should be under 200 milliseconds
    # Test bidirectional query performance
    When I get all links for exercise "Performance Target 1"
    Then the response time should be under 200 milliseconds

@exercise @links @integration @migration @data-consistency
Scenario: Data consistency during migration period
    Given I have a workout exercise named "Consistency Test Source"
    And I have an exercise named "Consistency Test Target" with exercise types "Workout,Warmup"
    # Create link with string format
    When I create an exercise link from "Consistency Test Source" to "Consistency Test Target" with link type "Warmup" and display order 1
    Then the exercise link should be created successfully
    # Verify the link using different query approaches
    When I get all links for exercise "Consistency Test Source"
    Then I should receive 1 exercise link
    And the first link should have link type "Warmup"
    When I get links for exercise "Consistency Test Source" filtered by link type "WARMUP"
    Then I should receive 1 exercise link
    And the first link should have link type "Warmup"

@exercise @links @integration @migration @enum-validation
Scenario: All four enum link types should work correctly
    Given I have a workout exercise named "Enum Test Source"
    And I have an exercise named "Warmup Target" with exercise types "Workout,Warmup"
    And I have an exercise named "Cooldown Target" with exercise types "Workout,Cooldown"
    And I have an exercise named "Alternative Target" with exercise types "Workout"
    # Test each enum type
    When I create an exercise link from "Enum Test Source" to "Warmup Target" with link type "WARMUP" and display order 1
    Then the exercise link should be created successfully
    When I create an exercise link from "Enum Test Source" to "Cooldown Target" with link type "COOLDOWN" and display order 2
    Then the exercise link should be created successfully
    When I create an exercise link from "Enum Test Source" to "Alternative Target" with link type "ALTERNATIVE" and display order 3
    Then the exercise link should be created successfully
    # Verify all enum types work
    When I get all links for exercise "Enum Test Source"
    Then I should receive 3 exercise links
    And the links should include "WARMUP", "COOLDOWN", and "ALTERNATIVE" types

@exercise @links @integration @migration @legacy-compatibility
Scenario: Legacy string formats should continue to work without modification
    Given I have a workout exercise named "Legacy Test Source"
    And I have an exercise named "Legacy Target 1" with exercise types "Workout,Warmup"
    And I have an exercise named "Legacy Target 2" with exercise types "Workout,Cooldown"
    # Use exact legacy format (capitalized first letter)
    When I create an exercise link from "Legacy Test Source" to "Legacy Target 1" with link type "Warmup" and display order 1
    And I create an exercise link from "Legacy Test Source" to "Legacy Target 2" with link type "Cooldown" and display order 2
    Then both exercise links should be created successfully
    # Verify legacy format queries still work
    When I get links for exercise "Legacy Test Source" filtered by link type "Warmup"
    Then I should receive 1 exercise link
    When I get links for exercise "Legacy Test Source" filtered by link type "Cooldown"
    Then I should receive 1 exercise link

@exercise @links @integration @migration @error-handling
Scenario: Error handling should work consistently across formats
    Given I have a workout exercise named "Error Test Source"
    And I have a rest exercise named "Invalid Rest Target"
    # Test error with string format
    When I create an exercise link from "Error Test Source" to "Invalid Rest Target" with link type "Warmup" and display order 1
    Then the request should fail with bad request
    # Test error with enum format
    When I create an exercise link from "Error Test Source" to "Invalid Rest Target" with link type "WARMUP" and display order 1
    Then the request should fail with bad request