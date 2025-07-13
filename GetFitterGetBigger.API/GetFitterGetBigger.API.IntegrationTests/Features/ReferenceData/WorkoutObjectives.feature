Feature: Workout Objectives Reference Data
  As a fitness application user
  I want to access workout objective reference data
  So that I can understand different training goals

  Background:
    Given the following workout objectives exist in the database:
      | WorkoutObjectiveId                            | Value                  | Description                                                  | DisplayOrder | IsActive |
      | workoutobjective-11111111-1111-1111-1111-111111111111 | Muscular Strength      | Build maximum strength through heavy loads and low repetitions | 1            | true     |
      | workoutobjective-22222222-2222-2222-2222-222222222222 | Muscular Hypertrophy   | Increase muscle size through moderate loads and volume        | 2            | true     |
      | workoutobjective-33333333-3333-3333-3333-333333333333 | Muscular Endurance     | Improve ability to sustain effort over time                  | 3            | true     |
      | workoutobjective-44444444-4444-4444-4444-444444444444 | Power Development      | Develop explosive strength and speed                          | 4            | true     |
      | workoutobjective-55555555-5555-5555-5555-555555555555 | Inactive Objective     | This objective is no longer used                             | 5            | false    |

  Scenario: Get all active workout objectives
    When I send a GET request to "/api/workout-objectives"
    Then the response status should be 200
    And the response should contain 4 workout objectives
    And each workout objective should have the following fields:
      | Field        | Type    | Required |
      | workoutObjectiveId | string  | true     |
      | value        | string  | true     |
      | description  | string  | false    |
      | displayOrder | number  | true     |
      | isActive     | boolean | true     |
    And the workout objectives should be ordered by displayOrder ascending
    And no inactive objectives should be included

  Scenario: Get all workout objectives including inactive
    When I send a GET request to "/api/workout-objectives?includeInactive=true"
    Then the response status should be 200
    And the response should contain 5 workout objectives
    And the response should include both active and inactive objectives

  Scenario: Get workout objective by valid ID
    When I send a GET request to "/api/workout-objectives/workoutobjective-11111111-1111-1111-1111-111111111111"
    Then the response status should be 200
    And the response should contain a workout objective with:
      | Field        | Value                                                          |
      | workoutObjectiveId | workoutobjective-11111111-1111-1111-1111-111111111111         |
      | value        | Muscular Strength                                              |
      | description  | Build maximum strength through heavy loads and low repetitions |
      | displayOrder | 1                                                              |
      | isActive     | true                                                           |

  Scenario: Get workout objective by non-existent ID
    When I send a GET request to "/api/workout-objectives/workoutobjective-00000000-0000-0000-0000-000000000000"
    Then the response status should be 404
    And the response should contain an error with:
      | Field   | Value                                    |
      | message | Workout objective not found              |

  Scenario: Get workout objective with invalid ID format
    When I send a GET request to "/api/workout-objectives/invalid-id-format"
    Then the response status should be 404
    And the response should contain an error with:
      | Field   | Value                                    |
      | message | Workout objective not found              |

  Scenario: Response caching headers are set correctly
    When I send a GET request to "/api/workout-objectives"
    Then the response status should be 200
    And the response should have cache control headers
    And the cache duration should be 3600 seconds

  Scenario: Get inactive workout objective by ID without includeInactive flag
    When I send a GET request to "/api/workout-objectives/workoutobjective-55555555-5555-5555-5555-555555555555"
    Then the response status should be 404
    And the response should contain an error with:
      | Field   | Value                                    |
      | message | Workout objective not found              |

  Scenario: Get inactive workout objective by ID with includeInactive flag
    When I send a GET request to "/api/workout-objectives/workoutobjective-55555555-5555-5555-5555-555555555555?includeInactive=true"
    Then the response status should be 200
    And the response should contain a workout objective with:
      | Field        | Value                                                          |
      | workoutObjectiveId | workoutobjective-55555555-5555-5555-5555-555555555555         |
      | value        | Inactive Objective                                             |
      | isActive     | false                                                          |