Feature: Exercise Links Integration
    As a fitness system
    I want to manage exercise links between exercises
    So that I can create proper workout sequences with warmups and cooldowns

Background:
    Given the system has been initialized with seed data

@exercise @links @integration @create
Scenario: Create link with valid data should succeed
    Given I have a workout exercise named "Barbell Squat"
    And I have an exercise named "Air Squat" with exercise types "Workout,Warmup"
    When I create an exercise link from "Barbell Squat" to "Air Squat" with link type "Warmup" and display order 1
    Then the exercise link should be created successfully
    And the link should have target exercise "Air Squat"
    And the link should have link type "Warmup"
    And the link should have display order 1
    And the link should be active

@exercise @links @integration @validation
Scenario: Create link with non-workout source should fail
    Given I have an exercise named "Warmup Only Exercise" with exercise types "Warmup"
    And I have an exercise named "Target Warmup" with exercise types "Warmup"
    When I create an exercise link from "Warmup Only Exercise" to "Target Warmup" with link type "Warmup" and display order 1
    Then the request should fail with bad request
    And the response should contain "Source exercise must be of type 'Workout'"

@exercise @links @integration @validation
Scenario: Create link with mismatched target type should fail
    Given I have a workout exercise named "Source Workout"
    And I have an exercise named "Cooldown Exercise" with exercise types "Cooldown"
    When I create an exercise link from "Source Workout" to "Cooldown Exercise" with link type "Warmup" and display order 1
    Then the request should fail with bad request
    And the response should contain "Target exercise must be of type 'Warmup'"

@exercise @links @integration @validation
Scenario: Create link with rest exercise should fail
    Given I have a workout exercise named "Rest Link Source"
    And I have a rest exercise named "Rest Period"
    When I create an exercise link from "Rest Link Source" to "Rest Period" with link type "Warmup" and display order 1
    Then the request should fail with bad request
    And the response should contain "Target exercise must be of type 'Warmup'"

@exercise @links @integration @query
Scenario: Get links with filters should return correct results
    Given I have a workout exercise named "Get Links Source"
    And I have an exercise named "Warmup Target" with exercise types "Workout,Warmup"
    And I have an exercise named "Cooldown Target" with exercise types "Workout,Cooldown"
    And I have created a link from "Get Links Source" to "Warmup Target" with link type "Warmup" and display order 1
    And I have created a link from "Get Links Source" to "Cooldown Target" with link type "Cooldown" and display order 1
    When I get all links for exercise "Get Links Source"
    Then I should receive 2 exercise links
    When I get links for exercise "Get Links Source" filtered by link type "Warmup"
    Then I should receive 1 exercise link
    And the first link should have link type "Warmup"

@exercise @links @integration @update
Scenario: Update link with valid data should succeed
    Given I have a workout exercise named "Update Link Source"
    And I have an exercise named "Update Link Target" with exercise types "Workout,Warmup"
    And I have created a link from "Update Link Source" to "Update Link Target" with link type "Warmup" and display order 1
    When I update the exercise link to have display order 5 and active status false
    Then the link should be updated successfully
    And the link should have display order 5
    And the link should not be active

@exercise @links @integration @delete
Scenario: Delete link with valid data should succeed
    Given I have a workout exercise named "Delete Link Source"
    And I have an exercise named "Delete Link Target" with exercise types "Workout,Warmup"
    And I have created a link from "Delete Link Source" to "Delete Link Target" with link type "Warmup" and display order 1
    When I delete the exercise link
    Then the link should be deleted successfully
    And the link should not exist in the database

@exercise @links @integration @comprehensive
Scenario: Comprehensive exercise link workflow
    Given I have a workout exercise named "Main Exercise"
    And I have an exercise named "Dynamic Warmup" with exercise types "Workout,Warmup"
    And I have an exercise named "Static Stretch" with exercise types "Workout,Cooldown"
    When I create an exercise link from "Main Exercise" to "Dynamic Warmup" with link type "Warmup" and display order 1
    And I create an exercise link from "Main Exercise" to "Static Stretch" with link type "Cooldown" and display order 1
    Then both exercise links should be created successfully
    When I get all links for exercise "Main Exercise"
    Then I should receive 2 exercise links
    And the links should include both "Warmup" and "Cooldown" types