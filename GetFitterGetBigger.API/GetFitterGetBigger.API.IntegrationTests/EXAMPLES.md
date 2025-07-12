# BDD Test Examples

This file contains example scenarios for common testing patterns in the GetFitterGetBigger API.

## 📚 Table of Contents

- [Basic CRUD Operations](#basic-crud-operations)
- [Authentication & Authorization](#authentication--authorization)
- [Validation Testing](#validation-testing)
- [Complex Workflows](#complex-workflows)
- [Error Handling](#error-handling)
- [Database Relationships](#database-relationships)

## 🔧 Basic CRUD Operations

### Create Entity

```gherkin
Feature: Exercise Management
  As a personal trainer
  I want to manage exercises
  So that I can create workout plans

Scenario: Create a new exercise successfully
  Given I am authenticated as a "PT-Tier"
  And the database has reference data
  When I send a POST request to "/api/exercises" with body:
    """
    {
      "name": "Bench Press",
      "description": "Classic chest exercise",
      "difficultyId": "difficultylevel-00000000-0000-0000-0000-000000000001",
      "kineticChainId": "kineticchaintype-00000000-0000-0000-0000-000000000001",
      "exerciseTypeIds": ["exercisetype-00000000-0000-0000-0000-000000000001"],
      "isUnilateral": false
    }
    """
  Then the response status should be 201
  And the response should have property "id"
  And the response should have property "name" with value "Bench Press"
  And the database should contain 1 exercise records
```

### Read Entity

```gherkin
Scenario: Get exercise by ID
  Given I am authenticated as a "Free-Tier"
  And the following exercise exists:
    | Name        | Description         | DifficultyLevel |
    | Pull-ups    | Upper body compound | Advanced        |
  When I send a GET request to "/api/exercises/<Pullups.Id>"
  Then the response status should be 200
  And the response should have property "name" with value "Pull-ups"
  And the response should have property "description" with value "Upper body compound"
```

### Update Entity

```gherkin
Scenario: Update existing exercise
  Given I am authenticated as a "PT-Tier"
  And the following exercise exists:
    | Name     | Description   | DifficultyLevel |
    | Deadlift | Back exercise | Intermediate    |
  When I send a PUT request to "/api/exercises/<Deadlift.Id>" with body:
    """
    {
      "name": "Romanian Deadlift",
      "description": "Hamstring-focused deadlift variation"
    }
    """
  Then the response status should be 200
  And the response should have property "name" with value "Romanian Deadlift"
```

### Delete Entity

```gherkin
Scenario: Delete an exercise
  Given I am authenticated as a "Admin-Tier"
  And the following exercise exists:
    | Name       | Description    |
    | Bicep Curl | Arm isolation  |
  When I send a DELETE request to "/api/exercises/<BicepCurl.Id>"
  Then the response status should be 204
  And the exercise with id "<BicepCurl.Id>" should not exist
```

### List with Pagination

```gherkin
Scenario: List exercises with pagination
  Given I am authenticated as a "Free-Tier"
  And the database has reference data
  And 15 exercises exist in the database
  When I send a GET request to "/api/exercises?pageNumber=2&pageSize=10"
  Then the response status should be 200
  And the response should have property "items"
  And the response should have property "totalCount" with value "15"
  And the response should have property "pageNumber" with value "2"
  And the response should have property "pageSize" with value "10"
  And the response should be a JSON array with 5 items
```

## 🔐 Authentication & Authorization

### Testing Different Roles

```gherkin
Scenario Outline: Access control for exercise management
  Given I am authenticated as a "<role>"
  When I send a POST request to "/api/exercises" with body:
    """
    {
      "name": "Test Exercise",
      "description": "Test Description"
    }
    """
  Then the response status should be <expectedStatus>
  
  Examples:
    | role       | expectedStatus | notes                    |
    | PT-Tier    | 201           | Can create exercises     |
    | Admin-Tier | 201           | Admin has full access    |
    | Free-Tier  | 403           | Read-only access         |
```

### Unauthenticated Access

```gherkin
Scenario: Unauthenticated access is denied
  Given I am not authenticated
  When I send a GET request to "/api/exercises"
  Then the response status should be 401
  And the response should contain "unauthorized"
```

### Token Validation

```gherkin
Scenario: Access with expired token
  Given I have an expired JWT token
  When I send a GET request to "/api/exercises"
  Then the response status should be 401
  And the response should contain "token expired"
```

## ✅ Validation Testing

### Required Fields

```gherkin
Scenario: Exercise name is required
  Given I am authenticated as a "PT-Tier"
  When I send a POST request to "/api/exercises" with body:
    """
    {
      "description": "Missing name field"
    }
    """
  Then the response status should be 400
  And the response should contain "Name is required"
```

### Field Length Validation

```gherkin
Scenario Outline: Validate exercise name length
  Given I am authenticated as a "PT-Tier"
  When I send a POST request to "/api/exercises" with body:
    """
    {
      "name": "<name>",
      "description": "Testing name length"
    }
    """
  Then the response status should be <status>
  
  Examples:
    | name         | status | reason              |
    | A            | 400    | Too short (min: 2)  |
    | AB           | 201    | Valid minimum       |
    | ValidName    | 201    | Normal length       |
    | #{"A" * 101} | 400    | Too long (max: 100) |
```

### Business Rule Validation

```gherkin
Scenario: Cannot create duplicate exercise names
  Given I am authenticated as a "PT-Tier"
  And the following exercise exists:
    | Name        | Description |
    | Bench Press | Existing    |
  When I send a POST request to "/api/exercises" with body:
    """
    {
      "name": "Bench Press",
      "description": "Duplicate name"
    }
    """
  Then the response status should be 409
  And the response should contain "already exists"
```

## 🔄 Complex Workflows

### Multi-Step Process

```gherkin
Scenario: Complete exercise creation workflow
  Given I am authenticated as a "PT-Tier"
  And the database has reference data
  
  # Step 1: Create equipment
  When I send a POST request to "/api/equipment" with body:
    """
    {
      "name": "Olympic Barbell",
      "description": "20kg standard barbell"
    }
    """
  Then the response status should be 201
  And I store the response property "id" as "BarbellId"
  
  # Step 2: Create exercise using the equipment
  When I send a POST request to "/api/exercises" with body:
    """
    {
      "name": "Barbell Squat",
      "description": "Compound leg exercise",
      "equipmentIds": ["<BarbellId>"]
    }
    """
  Then the response status should be 201
  And I store the response property "id" as "SquatId"
  
  # Step 3: Verify the relationship
  When I send a GET request to "/api/exercises/<SquatId>"
  Then the response status should be 200
  And the response should have property "equipment[0].name" with value "Olympic Barbell"
```

### Conditional Logic

```gherkin
Scenario: Rest exercise excludes muscle groups
  Given I am authenticated as a "PT-Tier"
  And the database has reference data
  When I send a POST request to "/api/exercises" with body:
    """
    {
      "name": "Rest Period",
      "description": "Recovery time",
      "exerciseTypeIds": ["exercisetype-rest-id"],
      "muscleGroups": []
    }
    """
  Then the response status should be 201
  And the response should not have property "muscleGroups"
```

## ❌ Error Handling

### Validation Errors

```gherkin
Scenario: Multiple validation errors
  Given I am authenticated as a "PT-Tier"
  When I send a POST request to "/api/exercises" with body:
    """
    {
      "name": "A",
      "description": "",
      "difficultyId": "invalid-id"
    }
    """
  Then the response status should be 400
  And the response should have property "errors"
  And the response should contain "Name must be at least 2 characters"
  And the response should contain "Description is required"
  And the response should contain "Invalid difficulty level"
```

### Not Found Errors

```gherkin
Scenario: Update non-existent exercise
  Given I am authenticated as a "PT-Tier"
  When I send a PUT request to "/api/exercises/exercise-99999999-9999-9999-9999-999999999999" with body:
    """
    {
      "name": "Updated Exercise"
    }
    """
  Then the response status should be 404
  And the response should contain "Exercise not found"
```

### Conflict Errors

```gherkin
Scenario: Cannot delete equipment in use
  Given I am authenticated as a "Admin-Tier"
  And the following equipment exists:
    | Name    | Description |
    | Barbell | In use      |
  And an exercise exists that uses the "Barbell" equipment
  When I send a DELETE request to "/api/equipment/<Barbell.Id>"
  Then the response status should be 409
  And the response should contain "Cannot delete equipment"
  And the response should contain "currently in use"
```

## 🔗 Database Relationships

### One-to-Many Relationships

```gherkin
Scenario: Exercise with multiple equipment
  Given I am authenticated as a "PT-Tier"
  And the following equipment exists:
    | Name       | Description          |
    | Barbell    | Standard barbell     |
    | Bench      | Adjustable bench     |
    | Power Rack | Safety equipment     |
  When I send a POST request to "/api/exercises" with body:
    """
    {
      "name": "Bench Press",
      "description": "Chest exercise",
      "equipmentIds": [
        "<Barbell.Id>",
        "<Bench.Id>",
        "<PowerRack.Id>"
      ]
    }
    """
  Then the response status should be 201
  And the response should have property "equipment"
  And the response property "equipment" should contain 3 items
```

### Many-to-Many Relationships

```gherkin
Scenario: Exercise with muscle groups and roles
  Given I am authenticated as a "PT-Tier"
  And the database has reference data
  When I send a POST request to "/api/exercises" with body:
    """
    {
      "name": "Bench Press",
      "description": "Compound chest exercise",
      "muscleGroups": [
        {
          "muscleGroupId": "musclegroup-chest-id",
          "muscleRoleId": "musclerole-primary-id"
        },
        {
          "muscleGroupId": "musclegroup-triceps-id",
          "muscleRoleId": "musclerole-secondary-id"
        },
        {
          "muscleGroupId": "musclegroup-shoulders-id",
          "muscleRoleId": "musclerole-stabilizer-id"
        }
      ]
    }
    """
  Then the response status should be 201
  And the response should have property "muscleGroups"
  And the response property "muscleGroups" should contain 3 items
```

### Cascade Operations

```gherkin
Scenario: Delete exercise with coach notes
  Given I am authenticated as a "Admin-Tier"
  And the following exercise exists with coach notes:
    | Name        | CoachNotes                                    |
    | Squat       | Keep back straight; Drive through heels       |
  When I send a DELETE request to "/api/exercises/<Squat.Id>"
  Then the response status should be 204
  And the exercise with id "<Squat.Id>" should not exist
  And the database should contain 0 coach notes for exercise "<Squat.Id>"
```

## 🎯 Best Practices Demonstrated

1. **Use Background** for common setup
2. **Use Scenario Outlines** for data-driven tests
3. **Store intermediate values** for multi-step workflows
4. **Test both success and failure paths**
5. **Verify database state** after operations
6. **Use meaningful test data** that explains the scenario
7. **Include authorization tests** for all endpoints
8. **Test edge cases** and boundary conditions

## 📝 Notes

- Replace ID placeholders with actual IDs from your system
- Adjust endpoints to match your actual API routes
- Verify authorization rules with stakeholders
- Add more examples as new patterns emerge