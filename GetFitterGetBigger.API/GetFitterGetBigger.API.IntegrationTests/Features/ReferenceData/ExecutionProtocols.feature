Feature: Execution Protocols Reference Data
  As a fitness application user
  I want to access execution protocol reference data
  So that I can understand different workout execution methods

  Background:
    Given the following execution protocols exist in the database:
      | ExecutionProtocolId                           | Value      | Description                                    | Code       | TimeBase | RepBase | RestPattern                      | IntensityLevel | DisplayOrder | IsActive |
      | executionprotocol-11111111-1111-1111-1111-111111111111 | Standard   | Standard protocol with balanced rep and time components | STANDARD   | true     | true    | 60-90 seconds between sets       | Moderate to High | 1            | true     |
      | executionprotocol-22222222-2222-2222-2222-222222222222 | Superset   | Perform exercises back-to-back without rest    | SUPERSET   | false    | true    | Rest after completing both exercises | High           | 2            | true     |
      | executionprotocol-33333333-3333-3333-3333-333333333333 | Drop Set   | Reduce weight after reaching failure           | DROP_SET   | false    | true    | Minimal rest between drops       | Very High      | 3            | true     |
      | executionprotocol-44444444-4444-4444-4444-444444444444 | AMRAP      | As Many Reps As Possible in given time        | AMRAP      | true     | false   | Fixed rest periods               | High           | 4            | true     |
      | executionprotocol-55555555-5555-5555-5555-555555555555 | Inactive   | This protocol is no longer used                | INACTIVE   | false    | false   | N/A                              | N/A            | 5            | false    |

  Scenario: Get all active execution protocols
    When I send a GET request to "/api/execution-protocols"
    Then the response status should be 200
    And the response should contain 4 execution protocols
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

  Scenario: Get all execution protocols including inactive
    When I send a GET request to "/api/execution-protocols?includeInactive=true"
    Then the response status should be 200
    And the response should contain 5 execution protocols
    And the response should include both active and inactive protocols

  Scenario: Get execution protocol by valid ID
    When I send a GET request to "/api/execution-protocols/executionprotocol-11111111-1111-1111-1111-111111111111"
    Then the response status should be 200
    And the response should contain an execution protocol with:
      | Field              | Value                                                  |
      | executionProtocolId | executionprotocol-11111111-1111-1111-1111-111111111111 |
      | value              | Standard                                               |
      | description        | Standard protocol with balanced rep and time components |
      | code               | STANDARD                                               |
      | timeBase           | true                                                   |
      | repBase            | true                                                   |
      | restPattern        | 60-90 seconds between sets                             |
      | intensityLevel     | Moderate to High                                       |
      | displayOrder       | 1                                                      |
      | isActive           | true                                                   |

  Scenario: Get execution protocol by non-existent ID
    When I send a GET request to "/api/execution-protocols/executionprotocol-00000000-0000-0000-0000-000000000000"
    Then the response status should be 404
    And the response should contain an error with:
      | Field   | Value                        |
      | message | Execution protocol not found |

  Scenario: Get execution protocol with invalid ID format
    When I send a GET request to "/api/execution-protocols/invalid-id-format"
    Then the response status should be 404
    And the response should contain an error with:
      | Field   | Value                        |
      | message | Execution protocol not found |

  Scenario: Get execution protocol by valid code
    When I send a GET request to "/api/execution-protocols/by-code/STANDARD"
    Then the response status should be 200
    And the response should contain an execution protocol with:
      | Field              | Value                                                  |
      | executionProtocolId | executionprotocol-11111111-1111-1111-1111-111111111111 |
      | value              | Standard                                               |
      | code               | STANDARD                                               |
      | isActive           | true                                                   |

  Scenario: Get execution protocol by code - case insensitive
    When I send a GET request to "/api/execution-protocols/by-code/standard"
    Then the response status should be 200
    And the response should contain an execution protocol with:
      | Field | Value    |
      | code  | STANDARD |

  Scenario: Get execution protocol by non-existent code
    When I send a GET request to "/api/execution-protocols/by-code/NONEXISTENT"
    Then the response status should be 404
    And the response should contain an error with:
      | Field   | Value                        |
      | message | Execution protocol not found |

  Scenario: Get inactive execution protocol by code
    When I send a GET request to "/api/execution-protocols/by-code/INACTIVE"
    Then the response status should be 404
    And the response should contain an error with:
      | Field   | Value                        |
      | message | Execution protocol not found |

  Scenario: Response caching headers are set correctly
    When I send a GET request to "/api/execution-protocols"
    Then the response status should be 200
    And the response should have cache control headers
    And the cache duration should be 3600 seconds

  Scenario: Get inactive execution protocol by ID without includeInactive flag
    When I send a GET request to "/api/execution-protocols/executionprotocol-55555555-5555-5555-5555-555555555555"
    Then the response status should be 404
    And the response should contain an error with:
      | Field   | Value                        |
      | message | Execution protocol not found |

  Scenario: Get inactive execution protocol by ID with includeInactive flag
    When I send a GET request to "/api/execution-protocols/executionprotocol-55555555-5555-5555-5555-555555555555?includeInactive=true"
    Then the response status should be 200
    And the response should contain an execution protocol with:
      | Field              | Value                                                  |
      | executionProtocolId | executionprotocol-55555555-5555-5555-5555-555555555555 |
      | value              | Inactive                                               |
      | isActive           | false                                                  |

  Scenario: Verify time and rep base combinations
    When I send a GET request to "/api/execution-protocols"
    Then the response status should be 200
    And at least one protocol should have both timeBase and repBase as true
    And at least one protocol should have timeBase true and repBase false
    And at least one protocol should have timeBase false and repBase true