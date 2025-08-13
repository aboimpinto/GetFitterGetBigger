Feature: Exercise Link Sequential Operations
  As an Admin UI user
  I want exercise link operations to behave correctly in sequence
  So that real-world usage patterns work reliably

  Background:
    Given I am authenticated as a "PT-Tier"
    And the system has been initialized with seed data

  @exercise @integration @links @validation
  Scenario: Create duplicate link should fail with bad request
    Given I have an exercise named "Source Exercise" with exercise types "Workout"
    And I have an exercise named "Target Exercise" with exercise types "Workout,Warmup"
    When I create an exercise link from "Source Exercise" to "Target Exercise" with link type "Warmup" and display order 1
    Then the exercise link should be created successfully
    When I create an exercise link from "Source Exercise" to "Target Exercise" with link type "Warmup" and display order 1
    Then the response status should be "bad request"

  @exercise @integration @links @limits
  Scenario: Create links beyond maximum limit should fail
    Given I have an exercise named "MaxLinksSource" with exercise types "Workout"
    And I have an exercise named "TargetEx0" with exercise types "Workout,Warmup"
    And I have an exercise named "TargetEx1" with exercise types "Workout,Warmup"
    And I have an exercise named "TargetEx2" with exercise types "Workout,Warmup"
    And I have an exercise named "TargetEx3" with exercise types "Workout,Warmup"
    And I have an exercise named "TargetEx4" with exercise types "Workout,Warmup"
    And I have an exercise named "TargetEx5" with exercise types "Workout,Warmup"
    And I have an exercise named "TargetEx6" with exercise types "Workout,Warmup"
    And I have an exercise named "TargetEx7" with exercise types "Workout,Warmup"
    And I have an exercise named "TargetEx8" with exercise types "Workout,Warmup"
    And I have an exercise named "TargetEx9" with exercise types "Workout,Warmup"
    And I have an exercise named "TargetEx10" with exercise types "Workout,Warmup"
    When I create 10 exercise links manually from "MaxLinksSource"
    Then all 10 manual links should be created successfully
    When I create an exercise link from "MaxLinksSource" to "TargetEx10" with link type "Warmup" and display order 11
    Then the response status should be "bad request"

  @exercise @integration @links @updates
  Scenario: Sequential updates to same link should all succeed
    Given I have an exercise named "Update Source" with exercise types "Workout"
    And I have an exercise named "Update Target" with exercise types "Workout,Cooldown"
    And I have created a link from "Update Source" to "Update Target" with link type "Cooldown" and display order 1
    When I update the exercise link to have display order 5 and active status true
    Then the link should be updated successfully
    And the link should have display order 5
    When I update the exercise link to have display order 10 and active status true
    Then the link should be updated successfully
    And the link should have display order 10
    When I update the exercise link to have display order 3 and active status true
    Then the link should be updated successfully
    And the link should have display order 3
    When I update the exercise link to have display order 7 and active status true
    Then the link should be updated successfully
    And the link should have display order 7

  @exercise @integration @links @deletion
  Scenario: Delete same link twice should fail on second attempt
    Given I have an exercise named "Delete Source" with exercise types "Workout"
    And I have an exercise named "Delete Target" with exercise types "Workout,Cooldown"
    And I have created a link from "Delete Source" to "Delete Target" with link type "Cooldown" and display order 1
    When I delete the exercise link
    Then the link should be deleted successfully
    When I delete the exercise link
    Then the response status should be "not found"

  @exercise @integration @links @workflow
  Scenario: Complete create-update-delete workflow should work correctly
    Given I have an exercise named "Workflow Source" with exercise types "Workout"
    And I have an exercise named "Workflow Target" with exercise types "Workout,Warmup"
    When I create an exercise link from "Workflow Source" to "Workflow Target" with link type "Warmup" and display order 1
    Then the exercise link should be created successfully
    And the link should have display order 1
    And the link should be active
    When I update the exercise link to have display order 5 and active status true
    Then the link should be updated successfully
    And the link should have display order 5
    When I delete the exercise link
    Then the link should be deleted successfully
    When I get all links for exercise "Workflow Source"
    Then I should receive 0 exercise links