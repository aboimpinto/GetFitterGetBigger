Feature: WorkoutTemplate Case Insensitive Search
  As a fitness application
  I want to search for workout templates using case-insensitive patterns
  So that users can find templates regardless of case

  Background:
    Given I am authenticated as a user
    And the database has reference data
    And the following workout templates exist for case insensitive search:
      | Name                 | Description                      | Duration | Tags           |
      | Leg Burner I         | High intensity leg workout       | 45       | leg,intensity  |
      | Leg Burner II        | Advanced leg workout             | 60       | leg,advanced   |
      | Upper Body Burner    | Upper body strength training     | 45       | upper,strength |
      | Core Crusher         | Core strengthening routine       | 30       | core,abs       |
      | Full Body Blast      | Complete body workout            | 60       | full,complete  |

  @workouttemplate @search
  Scenario: Search with lowercase pattern finds mixed case templates
    When I send a GET request to "/api/workout-templates?namePattern=leg"
    Then the response status should be 200
    And the response should have property "totalCount" with value "2"
    And the response should have property "items" as array with 2 items
    And the response items should contain templates with names:
      | Name          |
      | Leg Burner I  |
      | Leg Burner II |

  @workouttemplate @search
  Scenario: Search with uppercase pattern finds mixed case templates
    When I send a GET request to "/api/workout-templates?namePattern=BURNER"
    Then the response status should be 200
    And the response should have property "totalCount" with value "3"
    And the response should have property "items" as array with 3 items
    And the response items should contain templates with names:
      | Name              |
      | Leg Burner I      |
      | Leg Burner II     |
      | Upper Body Burner |

  @workouttemplate @search
  Scenario: Search with mixed case pattern finds all variations
    Given the following additional workout templates exist for case insensitive search:
      | Name             | Description     | Duration | Tags       |
      | LEG CRUSHER      | All uppercase   | 45       | leg,upper  |
      | leg destroyer    | All lowercase   | 45       | leg,lower  |
      | Leg Workout      | Title case      | 30       | leg,basic  |
      | LEGENDARY Legs   | Mixed case      | 50       | leg,legend |
    When I send a GET request to "/api/workout-templates?namePattern=LeG"
    Then the response status should be 200
    And the response should have property "totalCount" with value "6"
    And the response items should contain templates with names:
      | Name            |
      | Leg Burner I    |
      | Leg Burner II   |
      | LEG CRUSHER     |
      | leg destroyer   |
      | Leg Workout     |
      | LEGENDARY Legs  |

  @workouttemplate @search
  Scenario: Partial match works with case insensitive search
    When I send a GET request to "/api/workout-templates?namePattern=burn"
    Then the response status should be 200
    And the response should have property "totalCount" with value "3"
    And all response items should contain "burn" in name (case insensitive)

  @workouttemplate @search @pagination
  Scenario: Case insensitive search works with pagination
    When I send a GET request to "/api/workout-templates?namePattern=burner&page=1&pageSize=2"
    Then the response status should be 200
    And the response should have property "totalCount" with value "3"
    And the response should have property "currentPage" with value "1"
    And the response should have property "pageSize" with value "2"
    And the response should have property "items" as array with 2 items
    
    When I send a GET request to "/api/workout-templates?namePattern=burner&page=2&pageSize=2"
    Then the response status should be 200
    And the response should have property "items" as array with 1 items