Feature: Exercise Link End-to-End Workflows
  As an Admin UI user
  I want to perform complete exercise link workflows
  So that I can manage exercise relationships effectively

  Background:
    Given I am authenticated as a "PT-Tier"
    And the system has been initialized with seed data

  @exercise @integration @links @workflow @e2e
  Scenario: Complete create-update-delete workflow for workout plan
    Given I have an exercise named "Barbell Back Squat" with exercise types "Workout"
    And I have an exercise named "Air Squat" with exercise types "Workout,Warmup"
    And I have an exercise named "Leg Swings" with exercise types "Warmup"
    And I have an exercise named "Goblet Squat" with exercise types "Workout,Warmup"
    And I have an exercise named "Quad Stretch" with exercise types "Cooldown"
    And I have an exercise named "Pigeon Pose" with exercise types "Cooldown"
    
    # Step 1: Create warmup links
    When I create an exercise link from "Barbell Back Squat" to "Air Squat" with link type "Warmup" and display order 1
    Then the exercise link should be created successfully
    When I create an exercise link from "Barbell Back Squat" to "Goblet Squat" with link type "Warmup" and display order 2
    Then the exercise link should be created successfully
    
    # Step 2: Try invalid link (warmup-only exercise as source)
    When I create an exercise link from "Leg Swings" to "Air Squat" with link type "Warmup" and display order 1
    Then the response status should be "bad request"
    
    # Step 3: Create cooldown links
    When I create an exercise link from "Barbell Back Squat" to "Quad Stretch" with link type "Cooldown" and display order 1
    Then the exercise link should be created successfully
    When I create an exercise link from "Barbell Back Squat" to "Pigeon Pose" with link type "Cooldown" and display order 2
    Then the exercise link should be created successfully
    
    # Step 4: Verify all links with details
    When I get all links for exercise "Barbell Back Squat" with exercise details
    Then I should receive 4 exercise links
    And the links should include 2 "Warmup" type links
    And the links should include 2 "Cooldown" type links
    And all links should have target exercise details
    
    # Step 5: Update display order
    When I update the first created link to have display order 3 and active status true
    Then the link should be updated successfully
    And the link should have display order 3
    
    # Step 6: Get suggested links (if available)
    When I get suggested links for exercise "Barbell Back Squat" with count 3
    Then the response status should be "ok"
    
    # Step 7: Delete a link
    When I delete the last created link
    Then the link should be deleted successfully
    
    # Step 8: Verify final state has 3 active links
    When I get all links for exercise "Barbell Back Squat"
    Then I should receive 3 active exercise links

  @exercise @integration @links @workflow @complex
  Scenario: Complex linking scenarios with multiple exercise types
    Given I have an exercise named "Deadlift" with exercise types "Workout"
    And I have an exercise named "Romanian Deadlift" with exercise types "Workout,Warmup"
    And I have an exercise named "Good Morning" with exercise types "Workout,Warmup"
    And I have an exercise named "Hamstring Stretch" with exercise types "Cooldown"
    
    # Create multi-purpose exercise links
    When I create an exercise link from "Deadlift" to "Romanian Deadlift" with link type "Warmup" and display order 1
    Then the exercise link should be created successfully
    When I create an exercise link from "Deadlift" to "Good Morning" with link type "Warmup" and display order 2
    Then the exercise link should be created successfully
    When I create an exercise link from "Romanian Deadlift" to "Good Morning" with link type "Warmup" and display order 1
    Then the exercise link should be created successfully
    
    # Verify complex relationships
    When I get all links for exercise "Deadlift"
    Then I should receive 2 exercise links
    When I get all links for exercise "Romanian Deadlift"
    Then I should receive 1 exercise link
    
    # Add cooldown and verify final state
    When I create an exercise link from "Deadlift" to "Hamstring Stretch" with link type "Cooldown" and display order 1
    Then the exercise link should be created successfully
    When I get all links for exercise "Deadlift"
    Then I should receive 3 exercise links
    And the links should include 2 "Warmup" type links
    And the links should include 1 "Cooldown" type links

  @exercise @integration @links @workflow @limits
  Scenario: Exercise link limits and edge cases workflow
    Given I have an exercise named "Bench Press" with exercise types "Workout"
    And I have an exercise named "Push Ups" with exercise types "Workout,Warmup"
    And I have an exercise named "Chest Stretch" with exercise types "Cooldown"
    
    # Test complete workflow with edge cases
    When I create an exercise link from "Bench Press" to "Push Ups" with link type "Warmup" and display order 1
    Then the exercise link should be created successfully
    
    # Try duplicate creation
    When I create an exercise link from "Bench Press" to "Push Ups" with link type "Warmup" and display order 1
    Then the response status should be "bad request"
    
    # Update and verify
    When I update the exercise link to have display order 5 and active status true
    Then the link should be updated successfully
    And the link should have display order 5
    
    # Add cooldown and verify final workflow
    When I create an exercise link from "Bench Press" to "Chest Stretch" with link type "Cooldown" and display order 1
    Then the exercise link should be created successfully
    When I get all links for exercise "Bench Press"
    Then I should receive 2 exercise links