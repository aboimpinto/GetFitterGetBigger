Feature: Execution Protocols Reference Data
  As a fitness application user
  I want to access execution protocol reference data
  So that I can understand different workout execution methods

  Background:
    Given the following execution protocols exist in the database:
      | ExecutionProtocolId                           | Value      | Description                                    | Code       | TimeBase | RepBase | RestPattern                      | IntensityLevel | DisplayOrder | IsActive |
      | executionprotocol-30000003-3000-4000-8000-300000000001 | Reps and Sets | Traditional workout with fixed sets and repetitions | REPS_AND_SETS | true     | true    | 60-90 seconds between sets       | Moderate to High | 1            | true     |
      | executionprotocol-30000003-3000-4000-8000-300000000002 | Superset   | Perform exercises back-to-back without rest    | SUPERSET   | false    | true    | Rest after completing both exercises | High           | 2            | true     |
      | executionprotocol-30000003-3000-4000-8000-300000000003 | Drop Set   | Reduce weight after reaching failure           | DROP_SET   | false    | true    | Minimal rest between drops       | Very High      | 3            | true     |
      | executionprotocol-30000003-3000-4000-8000-300000000004 | AMRAP      | As Many Reps As Possible in given time        | AMRAP      | true     | false   | Fixed rest periods               | High           | 4            | true     |
      | executionprotocol-55555555-5555-5555-5555-555555555555 | Inactive   | This protocol is no longer used                | INACTIVE   | false    | false   | N/A                              | N/A            | 5            | false    |

  Scenario: Get all active execution protocols
    When I send a GET request to "/api/ReferenceTables/ExecutionProtocols"
    Then the response status should be 200
    And each execution protocol should have the following fields:
      | Field              | Type    | Required |
      | executionProtocolId | string  | true     |
      | value              | string  | true     |
      | description        | string  | false    |
      | code               | string  | true     |
      | timeBase           | boolean | true     |
      | repBase            | boolean | true     |
      | restPattern        | string  | false    |
      | intensityLevel     | string  | false    |
      | displayOrder       | number  | true     |
      | isActive           | boolean | true     |
    And the execution protocols should be ordered by displayOrder ascending
    And no inactive protocols should be included

  Scenario: Get execution protocol by valid ID
    When I send a GET request to "/api/ReferenceTables/ExecutionProtocols/executionprotocol-30000003-3000-4000-8000-300000000001"
    Then the response status should be 200
    And the response should contain an execution protocol with:
      | Field              | Value                                                  |
      | executionProtocolId | executionprotocol-30000003-3000-4000-8000-300000000001 |
      | value              | Reps and Sets                                          |
      | description        | Traditional workout with fixed sets and repetitions    |
      | code               | REPS_AND_SETS                                          |
      | timeBase           | true                                                   |
      | repBase            | true                                                   |
      | restPattern        | 60-90 seconds between sets                             |
      | intensityLevel     | Moderate to High                                       |
      | displayOrder       | 1                                                      |
      | isActive           | true                                                   |

  Scenario: Get execution protocol by non-existent ID
    When I send a GET request to "/api/ReferenceTables/ExecutionProtocols/executionprotocol-99999999-9999-9999-9999-999999999999"
    Then the response status should be 404

  Scenario: Get execution protocol with invalid ID format
    When I send a GET request to "/api/ReferenceTables/ExecutionProtocols/invalid-id-format"
    Then the response status should be 400

  Scenario: Get execution protocol by valid code
    When I send a GET request to "/api/ReferenceTables/ExecutionProtocols/ByCode/REPS_AND_SETS"
    Then the response status should be 200
    And the response should contain an execution protocol with:
      | Field              | Value                                                  |
      | executionProtocolId | executionprotocol-30000003-3000-4000-8000-300000000001 |
      | value              | Reps and Sets                                          |
      | code               | REPS_AND_SETS                                          |
      | isActive           | true                                                   |

  Scenario: Get execution protocol by code - case insensitive
    When I send a GET request to "/api/ReferenceTables/ExecutionProtocols/ByCode/reps_and_sets"
    Then the response status should be 200
    And the response should contain an execution protocol with:
      | Field | Value    |
      | code  | REPS_AND_SETS |

  Scenario: Get execution protocol by non-existent code
    When I send a GET request to "/api/ReferenceTables/ExecutionProtocols/ByCode/NONEXISTENT"
    Then the response status should be 404

  Scenario: Get inactive execution protocol by code
    When I send a GET request to "/api/ReferenceTables/ExecutionProtocols/ByCode/INACTIVE"
    Then the response status should be 404

  Scenario: Get inactive execution protocol by ID
    When I send a GET request to "/api/ReferenceTables/ExecutionProtocols/executionprotocol-55555555-5555-5555-5555-555555555555"
    Then the response status should be 404

  Scenario: Verify time and rep base combinations
    When I send a GET request to "/api/ReferenceTables/ExecutionProtocols"
    Then the response status should be 200
    And at least one protocol should have both timeBase and repBase as true
    And at least one protocol should have timeBase true and repBase false
    And at least one protocol should have timeBase false and repBase true