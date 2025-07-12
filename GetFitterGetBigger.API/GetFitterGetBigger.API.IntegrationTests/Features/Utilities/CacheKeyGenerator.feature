Feature: Cache Key Generator
  As a fitness application
  I want to generate consistent cache keys for reference tables
  So that I can efficiently cache and retrieve reference data

  @utility @cache
  Scenario: Generate get all cache key
    When I generate a get all cache key for table "DifficultyLevels"
    Then the cache key should be "ReferenceTable:DifficultyLevels:GetAll"

  @utility @cache
  Scenario: Generate get by ID cache key
    When I generate a get by ID cache key for table "Equipment" with ID "equipment-12345"
    Then the cache key should be "ReferenceTable:Equipment:GetById:equipment-12345"

  @utility @cache
  Scenario: Generate get by value cache key
    When I generate a get by value cache key for table "MuscleGroups" with value "Biceps"
    Then the cache key should be "ReferenceTable:MuscleGroups:GetByValue:biceps"

  @utility @cache
  Scenario: Get by value key normalizes to lowercase
    When I generate a get by value cache key for table "BodyParts" with value "CHEST"
    Then the cache key should be "ReferenceTable:BodyParts:GetByValue:chest"
    When I generate a get by value cache key for table "BodyParts" with value "Chest"
    Then the cache key should be "ReferenceTable:BodyParts:GetByValue:chest"
    When I generate a get by value cache key for table "BodyParts" with value "chest"
    Then the cache key should be "ReferenceTable:BodyParts:GetByValue:chest"

  @utility @cache
  Scenario: Get by value key handles null value
    When I generate a get by value cache key for table "MetricTypes" with null value
    Then the cache key should be "ReferenceTable:MetricTypes:GetByValue:"

  @utility @cache
  Scenario: Generate table pattern cache key
    When I generate a table pattern cache key for table "MovementPatterns"
    Then the cache key should be "ReferenceTable:MovementPatterns:"

  @utility @cache
  Scenario Outline: All methods work with all table names
    When I generate cache keys for table "<tableName>"
    Then the get all cache key should contain "<tableName>" and start with "ReferenceTable:" and end with ":GetAll"
    And the get by ID cache key should contain "<tableName>" and contain ":GetById:"
    And the get by value cache key should contain "<tableName>" and contain ":GetByValue:"
    And the table pattern cache key should start with "ReferenceTable:" and end with "<tableName>:"

    Examples:
      | tableName         |
      | DifficultyLevels  |
      | KineticChainTypes |
      | BodyParts         |
      | MuscleRoles       |
      | Equipment         |
      | MetricTypes       |
      | MovementPatterns  |
      | MuscleGroups      |