Feature: Exercise Link Enhancements - Four-Way Linking System
    As a fitness system
    I want to support enhanced exercise linking with four link types (WARMUP, COOLDOWN, WORKOUT, ALTERNATIVE)
    So that I can create comprehensive workout relationships with bidirectional linking

Background:
    Given the system has been initialized with seed data

@exercise @links @integration @enhancement @bidirectional @warmup
Scenario: Create WARMUP link should create bidirectional WARMUP/WORKOUT links
    Given I have a workout exercise named "Burpees"
    And I have an exercise named "Jumping-Jacks" with exercise types "Warmup"
    When I create an exercise link from "Burpees" to "Jumping-Jacks" with link type "WARMUP" and display order 1
    Then the exercise link should be created successfully
    And the link should have target exercise "Jumping-Jacks"
    And the link should have link type "WARMUP"
    And the link should have display order 1
    And a reverse link should exist from "Jumping-Jacks" to "Burpees" with link type "WORKOUT"
    And both links should be active
    And both links should have server-assigned display orders

@exercise @links @integration @enhancement @bidirectional @cooldown
Scenario: Create COOLDOWN link should create bidirectional COOLDOWN/WORKOUT links
    Given I have a workout exercise named "Deadlifts"
    And I have an exercise named "Hamstring-Stretch" with exercise types "Cooldown"
    When I create an exercise link from "Deadlifts" to "Hamstring-Stretch" with link type "COOLDOWN" and display order 1
    Then the exercise link should be created successfully
    And the link should have link type "COOLDOWN"
    And a reverse link should exist from "Hamstring-Stretch" to "Deadlifts" with link type "WORKOUT"
    And both links should be active

@exercise @links @integration @enhancement @bidirectional @alternative
Scenario: Create ALTERNATIVE link should create bidirectional ALTERNATIVE links
    Given I have a workout exercise named "Push-ups"
    And I have a workout exercise named "Incline-Push-ups"
    When I create an exercise link from "Push-ups" to "Incline-Push-ups" with link type "ALTERNATIVE" and display order 1
    Then the exercise link should be created successfully
    And the link should have link type "ALTERNATIVE"
    And a reverse link should exist from "Incline-Push-ups" to "Push-ups" with link type "ALTERNATIVE"
    And both links should be active

@exercise @links @integration @enhancement @validation @rest
Scenario: Create link from REST exercise should be blocked
    Given I have a rest exercise named "Rest-Period"
    And I have a workout exercise named "Target-Exercise"
    When I create an exercise link from "Rest-Period" to "Target-Exercise" with link type "WARMUP" and display order 1
    Then the request should fail with bad request
    And the error should contain "REST exercises cannot have links"

@exercise @links @integration @enhancement @validation @rest
Scenario: Create link to REST exercise should be blocked
    Given I have a workout exercise named "Source-Exercise"
    And I have a rest exercise named "Rest-Target"
    When I create an exercise link from "Source-Exercise" to "Rest-Target" with link type "WARMUP" and display order 1
    Then the request should fail with bad request
    And the error should contain "cannot be created to REST exercises"

@exercise @links @integration @enhancement @bidirectional @deletion
Scenario: Delete link should remove both bidirectional links
    Given I have a workout exercise named "Squat-Main"
    And I have an exercise named "Air-Squat-Warmup" with exercise types "Warmup"
    And I have created a link from "Squat-Main" to "Air-Squat-Warmup" with link type "WARMUP" and display order 1
    When I delete the exercise link
    Then the link should be deleted successfully
    And the forward link should not exist in the database
    And the reverse link should not exist in the database

@exercise @links @integration @enhancement @migration @backward_compatibility
Scenario: Existing string-based links should work with enhanced API
    Given I have a workout exercise named "Legacy-Exercise"
    And I have an exercise named "Legacy-Warmup" with exercise types "Warmup"
    When I create an exercise link from "Legacy-Exercise" to "Legacy-Warmup" with link type "Warmup" and display order 1
    Then the exercise link should be created successfully
    And the link should have link type "WARMUP"
    When I get all links for exercise "Legacy-Exercise"
    Then I should receive 1 exercise link
    And the link should be accessible via enhanced API
    And the link should have correct enum mapping

@exercise @links @integration @enhancement @validation @workout_direct
Scenario: Direct WORKOUT link creation should be blocked
    Given I have a workout exercise named "Direct-Source"
    And I have a workout exercise named "Direct-Target"
    When I create an exercise link from "Direct-Source" to "Direct-Target" with link type "WORKOUT" and display order 1
    Then the request should fail with bad request
    And the error should contain "WORKOUT links are automatically created as reverse links"

@exercise @links @integration @enhancement @display_order @server_side
Scenario: Display order should be calculated server-side for sequential links
    Given I have a workout exercise named "Multi-Link-Source"
    And I have an exercise named "Warmup-1" with exercise types "Warmup"
    And I have an exercise named "Warmup-2" with exercise types "Warmup"
    And I have an exercise named "Warmup-3" with exercise types "Warmup"
    When I create an exercise link from "Multi-Link-Source" to "Warmup-1" with link type "WARMUP" and display order 1
    And I create an exercise link from "Multi-Link-Source" to "Warmup-2" with link type "WARMUP" and display order 1
    And I create an exercise link from "Multi-Link-Source" to "Warmup-3" with link type "WARMUP" and display order 1
    Then all links should be created successfully
    And the links should have sequential display orders regardless of input
    And each reverse link should have independent display order calculation

@exercise @links @integration @enhancement @validation @type_compatibility
Scenario: WARMUP link type compatibility validation
    Given I have a workout exercise named "WARMUP-Source"
    And I have an exercise named "Cooldown-Target" with exercise types "Cooldown"
    When I create an exercise link from "WARMUP-Source" to "Cooldown-Target" with link type "WARMUP" and display order 1
    Then the request should fail with bad request
    And the error should contain "WARMUP links can only be created to exercises with Warmup exercise type"

@exercise @links @integration @enhancement @validation @type_compatibility
Scenario: COOLDOWN link type compatibility validation
    Given I have a workout exercise named "COOLDOWN-Source"
    And I have an exercise named "Warmup-Target" with exercise types "Warmup"
    When I create an exercise link from "COOLDOWN-Source" to "Warmup-Target" with link type "COOLDOWN" and display order 1
    Then the request should fail with bad request
    And the error should contain "COOLDOWN links can only be created to exercises with Cooldown exercise type"

@exercise @links @integration @enhancement @comprehensive @workflow
Scenario: Comprehensive four-way linking workflow
    Given I have a workout exercise named "Comprehensive-Main"
    And I have an exercise named "Dynamic-Warmup" with exercise types "Warmup"
    And I have an exercise named "Static-Cooldown" with exercise types "Cooldown"
    And I have a workout exercise named "Alternative-Exercise"
    When I create an exercise link from "Comprehensive-Main" to "Dynamic-Warmup" with link type "WARMUP" and display order 1
    And I create an exercise link from "Comprehensive-Main" to "Static-Cooldown" with link type "COOLDOWN" and display order 1
    And I create an exercise link from "Comprehensive-Main" to "Alternative-Exercise" with link type "ALTERNATIVE" and display order 1
    Then all exercise links should be created successfully
    When I get all links for exercise "Comprehensive-Main"
    Then I should receive 3 exercise links
    And the links should include "WARMUP", "COOLDOWN", and "ALTERNATIVE" types
    And each link should have a corresponding reverse link
    And the reverse links should have types "WORKOUT", "WORKOUT", and "ALTERNATIVE" respectively

@exercise @links @integration @enhancement @duplicate @prevention
Scenario: Duplicate bidirectional links should be prevented
    Given I have a workout exercise named "Duplicate-Test-A"
    And I have a workout exercise named "Duplicate-Test-B"
    And I have created a link from "Duplicate-Test-A" to "Duplicate-Test-B" with link type "ALTERNATIVE" and display order 1
    When I create an exercise link from "Duplicate-Test-B" to "Duplicate-Test-A" with link type "ALTERNATIVE" and display order 1
    Then the request should fail with bad request
    And the error should contain "bidirectional link already exists"

@exercise @links @integration @enhancement @enum @validation
Scenario: Invalid link type enum should be rejected
    Given I have a workout exercise named "Enum-Test-Source"
    And I have a workout exercise named "Enum-Test-Target"
    When I create an exercise link from "Enum-Test-Source" to "Enum-Test-Target" with link type "INVALID_TYPE" and display order 1
    Then the request should fail with bad request
    And the error should contain "Link type must be 'Warmup', 'Cooldown', 'WARMUP', 'COOLDOWN', 'WORKOUT', or 'ALTERNATIVE'"