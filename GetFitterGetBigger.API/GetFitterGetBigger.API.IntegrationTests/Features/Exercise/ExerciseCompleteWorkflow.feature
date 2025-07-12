Feature: Exercise Complete Workflow
  As a fitness application
  I want to test complete exercise creation workflows
  So that I can validate end-to-end exercise functionality

  Background:
    Given I am authenticated as a user
    And the database has reference data

  @exercise @workflow @comprehensive
  Scenario: Complete exercise workflow create with all features success
    When I create a workout exercise named "Complete Feature Test Exercise"
    And I set the difficulty to "Intermediate"
    And I set the kinetic chain type to "Compound"
    And I set the weight type to "Weight Required"
    And I add muscle group "Quadriceps" as "Primary"
    And I add muscle group "Chest" as "Stabilizer"
    And I add exercise type "Warmup"
    And I add coach note "Setup: Position yourself at the squat rack" with order 1
    And I add coach note "Execution: Lower slowly for 3 seconds" with order 2
    And I add coach note "Hold: Pause at the bottom for 1 second" with order 3
    And I add coach note "Return: Push through heels to stand" with order 4
    And I add coach note "Breathing: Inhale down, exhale up" with order 5
    And I set the video URL to "https://example.com/squat-tutorial.mp4"
    And I set the image URL to "https://example.com/squat-form.jpg"
    And I submit the exercise
    Then the response status should be 201
    And the response should have property "name" with value "Complete Feature Test Exercise"
    And the response should have property "videoUrl" with value "https://example.com/squat-tutorial.mp4"
    And the response should have property "imageUrl" with value "https://example.com/squat-form.jpg"
    And the response should have property "isUnilateral" with value "false"
    And the response should have property "coachNotes" as array with length 5
    And the response property "coachNotes[0].text" should be "Setup: Position yourself at the squat rack"
    And the response property "coachNotes[0].order" should be "1"
    And the response property "coachNotes[4].text" should be "Breathing: Inhale down, exhale up"
    And the response property "coachNotes[4].order" should be "5"
    And the response should have property "exerciseTypes" as array with length 2
    And the response should have property "muscleGroups" as array with length 2

  @exercise @workflow @order
  Scenario: Complete exercise workflow create then retrieve maintains coach notes order
    When I create a workout exercise named "Retrieve Order Test Exercise"
    And I set the difficulty to "Beginner"
    And I set the kinetic chain type to "Compound"
    And I set the weight type to "Weight Required"
    And I add muscle group "Chest" as "Primary"
    And I add coach note "Step E" with order 5
    And I add coach note "Step A" with order 1
    And I add coach note "Step C" with order 3
    And I add coach note "Step B" with order 2
    And I add coach note "Step D" with order 4
    And I submit the exercise
    Then the response status should be 201
    And I store the response property "id" as "createdExerciseId"
    When I send a GET request to "/api/exercises/<createdExerciseId>"
    Then the response status should be 200
    And the response should have property "coachNotes" as array with length 5
    And the response property "coachNotes[0].text" should be "Step A"
    And the response property "coachNotes[1].text" should be "Step B"
    And the response property "coachNotes[2].text" should be "Step C"
    And the response property "coachNotes[3].text" should be "Step D"
    And the response property "coachNotes[4].text" should be "Step E"

  @exercise @workflow @minimal
  Scenario: Complete exercise workflow create minimal exercise success
    When I create a workout exercise named "Minimal Exercise Test"
    And I set the difficulty to "Beginner"
    And I set the kinetic chain type to "Compound"
    And I set the weight type to "Weight Required"
    And I add muscle group "Chest" as "Primary"
    And I submit the exercise
    Then the response status should be 201
    And the response should have property "name" with value "Minimal Exercise Test"
    And the response should have property "coachNotes" as array with length 0
    And the response should have property "exerciseTypes" as array with length 1
    And the response should have property "muscleGroups" as array with length 1
    And the response should have property "kineticChain"
    And the response should have property "exerciseWeightType"