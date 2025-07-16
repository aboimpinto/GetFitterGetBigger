Feature: Workout Categories Reference Data
  As a fitness application user
  I want to access workout category reference data
  So that I can organize exercises by muscle groups and movement patterns

  Background:
    Given the following workout categories exist in the database:
      | WorkoutCategoryId                            | Value              | Description                                        | Icon | Color   | PrimaryMuscleGroups             | DisplayOrder | IsActive |
      | workoutcategory-20000002-2000-4000-8000-200000000001 | Upper Body - Push  | Push exercises targeting chest, shoulders, and triceps | 💪   | #FF5722 | Chest,Shoulders,Triceps         | 1            | true     |
      | workoutcategory-20000002-2000-4000-8000-200000000002 | Upper Body - Pull  | Pull exercises targeting back and biceps           | 🏋️   | #4CAF50 | Back,Biceps                     | 2            | true     |
      | workoutcategory-20000002-2000-4000-8000-200000000003 | Lower Body         | Lower body exercises for legs and glutes           | 🦵   | #2196F3 | Quadriceps,Hamstrings,Glutes,Calves | 3            | true     |
      | workoutcategory-20000002-2000-4000-8000-200000000004 | Core               | Core stability and strength exercises              | 🎯   | #9C27B0 | Abs,Obliques,Lower Back         | 4            | true     |
      | workoutcategory-20000002-2000-4000-8000-200000000005 | Full Body          | Compound exercises engaging multiple muscle groups | 🏃   | #FF9800 | Multiple                        | 5            | true     |
      | workoutcategory-55555555-5555-5555-5555-555555555555 | Inactive Category  | This category is no longer used                    | ❌   | #757575 | None                            | 6            | false    |

  Scenario: Get all active workout categories
    When I send a GET request to "/api/ReferenceTables/WorkoutCategories"
    Then the response status should be 200
    And the response should contain 5 workout categories
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


  Scenario: Get workout category by valid ID
    When I send a GET request to "/api/ReferenceTables/WorkoutCategories/workoutcategory-20000002-2000-4000-8000-200000000001"
    Then the response status should be 200
    And the response should contain a workout category with:
      | Field               | Value                                                  |
      | workoutCategoryId   | workoutcategory-20000002-2000-4000-8000-200000000001   |
      | value               | Upper Body - Push                                      |
      | description         | Push exercises targeting chest, shoulders, and triceps |
      | icon                | 💪                                                     |
      | color               | #FF5722                                                |
      | primaryMuscleGroups | Chest,Shoulders,Triceps                                |
      | displayOrder        | 1                                                      |
      | isActive            | true                                                   |

  Scenario: Get workout category by non-existent ID
    When I send a GET request to "/api/ReferenceTables/WorkoutCategories/workoutcategory-00000000-0000-0000-0000-000000000000"
    Then the response status should be 404

  Scenario: Get workout category with invalid ID format
    When I send a GET request to "/api/ReferenceTables/WorkoutCategories/invalid-id-format"
    Then the response status should be 400

  Scenario: Get workout category by value
    When I send a GET request to "/api/ReferenceTables/WorkoutCategories/ByValue/Upper Body - Push"
    Then the response status should be 200
    And the response should contain a workout category with:
      | Field               | Value                                                  |
      | workoutCategoryId   | workoutcategory-20000002-2000-4000-8000-200000000001   |
      | value               | Upper Body - Push                                      |
      | description         | Push exercises targeting chest, shoulders, and triceps |
      | icon                | 💪                                                     |
      | color               | #FF5722                                                |
      | primaryMuscleGroups | Chest,Shoulders,Triceps                                |
      | displayOrder        | 1                                                      |
      | isActive            | true                                                   |

  Scenario: Get workout category by non-existent value
    When I send a GET request to "/api/ReferenceTables/WorkoutCategories/ByValue/Non-Existent Category"
    Then the response status should be 404

  Scenario: Get workout category by empty value
    When I send a GET request to "/api/ReferenceTables/WorkoutCategories/ByValue/"
    Then the response status should be 400

  Scenario: Verify emoji support in icon field
    When I send a GET request to "/api/ReferenceTables/WorkoutCategories"
    Then the response status should be 200
    And each category's icon field should contain a valid emoji character