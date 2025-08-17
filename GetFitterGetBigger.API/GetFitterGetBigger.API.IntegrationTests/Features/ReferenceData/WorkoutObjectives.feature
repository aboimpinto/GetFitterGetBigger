Feature: Workout Objectives Reference Data
  As a fitness application user
  I want to access workout objective reference data
  So that I can understand different training goals

  Background:
    Given the following workout objectives exist in the database:
      | WorkoutObjectiveId                            | Value                  | Description                                                  | DisplayOrder | IsActive |
      | workoutobjective-10000001-1000-4000-8000-100000000001 | Muscular Strength      | Build maximum strength through heavy loads and low repetitions | 1            | true     |
      | workoutobjective-10000001-1000-4000-8000-100000000002 | Muscular Hypertrophy   | Increase muscle size through moderate loads and volume        | 2            | true     |
      | workoutobjective-10000001-1000-4000-8000-100000000003 | Muscular Endurance     | Improve ability to sustain effort over time                  | 3            | true     |
      | workoutobjective-10000001-1000-4000-8000-100000000004 | Power Development      | Develop explosive strength and speed                          | 4            | true     |
      | workoutobjective-55555555-5555-5555-5555-555555555555 | Inactive Objective     | This objective is no longer used                             | 5            | false    |

  Scenario: Get all active workout objectives
    When I send a GET request to "/api/ReferenceTables/WorkoutObjectives"
    Then the response status should be 200
    And each workout objective should have the following fields:
      | Field        | Type    | Required |
      | id           | string  | true     |
      | value        | string  | true     |
      | description  | string  | false    |
    And no inactive objectives should be included


  Scenario: Get workout objective by valid ID
    When I send a GET request to "/api/ReferenceTables/WorkoutObjectives/workoutobjective-10000001-1000-4000-8000-100000000001"
    Then the response status should be 200
    And the response should contain a workout objective with:
      | Field        | Value                                                          |
      | id           | workoutobjective-10000001-1000-4000-8000-100000000001         |
      | value        | Muscular Strength                                              |
      | description  | Build maximum strength through heavy loads and low repetitions |

  Scenario: Get workout objective by non-existent ID
    When I send a GET request to "/api/ReferenceTables/WorkoutObjectives/workoutobjective-11111111-1111-1111-1111-111111111111"
    Then the response status should be 404

  Scenario: Get workout objective by empty GUID
    When I send a GET request to "/api/ReferenceTables/WorkoutObjectives/workoutobjective-00000000-0000-0000-0000-000000000000"
    Then the response status should be 400

  Scenario: Get workout objective with invalid ID format
    When I send a GET request to "/api/ReferenceTables/WorkoutObjectives/invalid-id-format"
    Then the response status should be 400


  Scenario: Get inactive workout objective by ID
    When I send a GET request to "/api/ReferenceTables/WorkoutObjectives/workoutobjective-55555555-5555-5555-5555-555555555555"
    Then the response status should be 404