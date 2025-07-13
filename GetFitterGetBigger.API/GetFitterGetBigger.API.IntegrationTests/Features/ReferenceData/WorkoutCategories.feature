Feature: Workout Categories Reference Data
  As a fitness application user
  I want to access workout category reference data
  So that I can organize exercises by muscle groups and movement patterns

  Background:
    Given the following workout categories exist in the database:
      | WorkoutCategoryId                            | Value              | Description                                        | Icon | Color   | PrimaryMuscleGroups             | DisplayOrder | IsActive |
      | workoutcategory-11111111-1111-1111-1111-111111111111 | Upper Body - Push  | Push exercises targeting chest, shoulders, and triceps | 💪   | #FF5722 | Chest,Shoulders,Triceps         | 1            | true     |
      | workoutcategory-22222222-2222-2222-2222-222222222222 | Upper Body - Pull  | Pull exercises targeting back and biceps           | 🏋️   | #4CAF50 | Back,Biceps                     | 2            | true     |
      | workoutcategory-33333333-3333-3333-3333-333333333333 | Lower Body         | Lower body exercises for legs and glutes           | 🦵   | #2196F3 | Quadriceps,Hamstrings,Glutes,Calves | 3            | true     |
      | workoutcategory-44444444-4444-4444-4444-444444444444 | Core               | Core stability and strength exercises              | 🎯   | #9C27B0 | Abs,Obliques,Lower Back         | 4            | true     |
      | workoutcategory-55555555-5555-5555-5555-555555555555 | Inactive Category  | This category is no longer used                    | ❌   | #757575 | None                            | 5            | false    |

  Scenario: Get all active workout categories
    When I send a GET request to "/api/workout-categories"
    Then the response status should be 200
    And the response should contain 4 workout categories
    And each workout category should have the following fields:
      | Field               | Type    | Required |
      | workoutCategoryId   | string  | true     |
      | value               | string  | true     |
      | description         | string  | false    |
      | icon                | string  | true     |
      | color               | string  | true     |
      | primaryMuscleGroups | string  | false    |
      | displayOrder        | number  | true     |
      | isActive            | boolean | true     |
    And the workout categories should be ordered by displayOrder ascending
    And no inactive categories should be included

  Scenario: Get all workout categories including inactive
    When I send a GET request to "/api/workout-categories?includeInactive=true"
    Then the response status should be 200
    And the response should contain 5 workout categories
    And the response should include both active and inactive categories

  Scenario: Get workout category by valid ID
    When I send a GET request to "/api/workout-categories/workoutcategory-11111111-1111-1111-1111-111111111111"
    Then the response status should be 200
    And the response should contain a workout category with:
      | Field               | Value                                                  |
      | workoutCategoryId   | workoutcategory-11111111-1111-1111-1111-111111111111   |
      | value               | Upper Body - Push                                      |
      | description         | Push exercises targeting chest, shoulders, and triceps |
      | icon                | 💪                                                     |
      | color               | #FF5722                                                |
      | primaryMuscleGroups | Chest,Shoulders,Triceps                                |
      | displayOrder        | 1                                                      |
      | isActive            | true                                                   |

  Scenario: Get workout category by non-existent ID
    When I send a GET request to "/api/workout-categories/workoutcategory-00000000-0000-0000-0000-000000000000"
    Then the response status should be 404
    And the response should contain an error with:
      | Field   | Value                      |
      | message | Workout category not found |

  Scenario: Get workout category with invalid ID format
    When I send a GET request to "/api/workout-categories/invalid-id-format"
    Then the response status should be 404
    And the response should contain an error with:
      | Field   | Value                      |
      | message | Workout category not found |

  Scenario: Response caching headers are set correctly
    When I send a GET request to "/api/workout-categories"
    Then the response status should be 200
    And the response should have cache control headers
    And the cache duration should be 3600 seconds

  Scenario: Get inactive workout category by ID without includeInactive flag
    When I send a GET request to "/api/workout-categories/workoutcategory-55555555-5555-5555-5555-555555555555"
    Then the response status should be 404
    And the response should contain an error with:
      | Field   | Value                      |
      | message | Workout category not found |

  Scenario: Get inactive workout category by ID with includeInactive flag
    When I send a GET request to "/api/workout-categories/workoutcategory-55555555-5555-5555-5555-555555555555?includeInactive=true"
    Then the response status should be 200
    And the response should contain a workout category with:
      | Field               | Value                                                  |
      | workoutCategoryId   | workoutcategory-55555555-5555-5555-5555-555555555555   |
      | value               | Inactive Category                                      |
      | isActive            | false                                                  |

  Scenario: Verify emoji support in icon field
    When I send a GET request to "/api/workout-categories"
    Then the response status should be 200
    And each category's icon field should contain a valid emoji character