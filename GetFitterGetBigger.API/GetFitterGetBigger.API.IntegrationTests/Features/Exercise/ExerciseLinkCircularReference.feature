Feature: Exercise Link Circular Reference Prevention
  As a system administrator
  I want to prevent circular references in exercise links
  So that exercise chains maintain logical order and don't create infinite loops

  Background:
    Given I am authenticated as a "PT-Tier"
    And the system has been initialized with seed data

  @exercise @integration @links @circular-reference
  Scenario: Create link with direct circular reference should fail
    Given I have an exercise named "Exercise A" with exercise types "Workout,Warmup"
    And I have an exercise named "Exercise B" with exercise types "Workout,Warmup"
    And I have created a link from "Exercise A" to "Exercise B" with link type "Warmup" and display order 1
    When I create an exercise link from "Exercise B" to "Exercise A" with link type "Warmup" and display order 1
    Then the response status should be "bad request"

  @exercise @integration @links @circular-reference
  Scenario: Create link with indirect circular reference should fail
    Given I have an exercise named "Exercise A Chain" with exercise types "Workout,Warmup"
    And I have an exercise named "Exercise B Chain" with exercise types "Workout,Warmup"
    And I have an exercise named "Exercise C Chain" with exercise types "Workout,Warmup"
    And I have created a link from "Exercise A Chain" to "Exercise B Chain" with link type "Warmup" and display order 1
    And I have created a link from "Exercise B Chain" to "Exercise C Chain" with link type "Warmup" and display order 1
    When I create an exercise link from "Exercise C Chain" to "Exercise A Chain" with link type "Warmup" and display order 1
    Then the response status should be "bad request"

  @exercise @integration @links @circular-reference
  Scenario: Create link with complex circular reference should fail
    Given I have an exercise named "Complex A" with exercise types "Workout,Warmup"
    And I have an exercise named "Complex B" with exercise types "Workout,Warmup,Cooldown"
    And I have an exercise named "Complex C" with exercise types "Workout,Cooldown"
    And I have an exercise named "Complex D" with exercise types "Workout,Warmup,Cooldown"
    And I have created a link from "Complex A" to "Complex B" with link type "Warmup" and display order 1
    And I have created a link from "Complex B" to "Complex C" with link type "Cooldown" and display order 1
    And I have created a link from "Complex C" to "Complex D" with link type "Cooldown" and display order 1
    When I create an exercise link from "Complex D" to "Complex A" with link type "Warmup" and display order 1
    Then the response status should be "bad request"

  @exercise @integration @links @success
  Scenario: Create non-circular complex structure should succeed
    Given I have an exercise named "Root Exercise" with exercise types "Workout"
    And I have an exercise named "Warmup 1" with exercise types "Workout,Warmup"
    And I have an exercise named "Warmup 2" with exercise types "Workout,Warmup"
    And I have an exercise named "Cooldown 1" with exercise types "Workout,Cooldown"
    And I have created a link from "Root Exercise" to "Warmup 1" with link type "Warmup" and display order 1
    And I have created a link from "Root Exercise" to "Warmup 2" with link type "Warmup" and display order 2
    When I create an exercise link from "Root Exercise" to "Cooldown 1" with link type "Cooldown" and display order 1
    Then the exercise link should be created successfully
    And the link should have target exercise "Cooldown 1"
    And the link should have link type "Cooldown"
    And the link should have display order 1

  @exercise @integration @links @success
  Scenario: Valid exercise link creation without circular reference
    Given I have an exercise named "Source Exercise" with exercise types "Workout"
    And I have an exercise named "Target Exercise" with exercise types "Warmup"
    When I create an exercise link from "Source Exercise" to "Target Exercise" with link type "Warmup" and display order 1
    Then the exercise link should be created successfully
    And the link should have target exercise "Target Exercise"
    And the link should have link type "Warmup"