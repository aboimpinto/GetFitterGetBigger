# Available Step Definitions

This document provides a comprehensive reference of all available step definitions in the BDD test suite.

## 📋 Table of Contents

- [Authentication Steps](#authentication-steps)
- [API Request Steps](#api-request-steps)
- [Response Validation Steps](#response-validation-steps)
- [Database Steps](#database-steps)
- [Common Steps](#common-steps)

## 🔐 Authentication Steps

### Given Steps

#### `Given I am authenticated as a "{role}"`
Sets up authentication with the specified role.
- **Parameters**: 
  - `role`: User role (e.g., "PT-Tier", "Admin-Tier", "Free-Tier")
- **Example**: `Given I am authenticated as a "PT-Tier"`

#### `Given I am not authenticated`
Ensures no authentication token is present.
- **Example**: `Given I am not authenticated`

#### `Given I have a valid JWT token`
Sets up a valid JWT token with minimal claims.
- **Example**: `Given I have a valid JWT token`

#### `Given I have an expired JWT token`
Sets up an expired JWT token for testing token expiration.
- **Example**: `Given I have an expired JWT token`

#### `Given I have an invalid JWT token`
Sets up a malformed JWT token for testing invalid token scenarios.
- **Example**: `Given I have an invalid JWT token`

### When Steps

#### `When I authenticate with email "{email}"`
Performs authentication with the specified email.
- **Parameters**: 
  - `email`: User email address
- **Example**: `When I authenticate with email "pt@example.com"`

### Then Steps

#### `Then I should receive a valid authentication token`
Verifies that authentication was successful and a token was received.
- **Example**: `Then I should receive a valid authentication token`

#### `Then the token should contain claim "{claimType}" with value "{claimValue}"`
Verifies specific claims in the authentication token.
- **Parameters**:
  - `claimType`: The type of claim to check
  - `claimValue`: The expected claim value
- **Example**: `Then the token should contain claim "role" with value "PT-Tier"`

## 📡 API Request Steps

### When Steps

#### `When I send a {method} request to "{endpoint}"`
Sends an HTTP request without a body.
- **Parameters**:
  - `method`: HTTP method (GET, POST, PUT, DELETE, PATCH)
  - `endpoint`: API endpoint path
- **Example**: `When I send a GET request to "/api/exercises"`

#### `When I send a {method} request to "{endpoint}" with body:`
Sends an HTTP request with a JSON body.
- **Parameters**:
  - `method`: HTTP method
  - `endpoint`: API endpoint path
  - `body`: JSON body (multi-line string)
- **Example**:
  ```gherkin
  When I send a POST request to "/api/exercises" with body:
  """
  {
    "name": "Bench Press",
    "description": "Chest exercise"
  }
  """
  ```

#### `When I add header "{name}" with value "{value}"`
Adds a custom header to the next request.
- **Parameters**:
  - `name`: Header name
  - `value`: Header value
- **Example**: `When I add header "X-Custom-Header" with value "test-value"`

## ✅ Response Validation Steps

### Then Steps

#### `Then the response status should be {statusCode}`
Verifies the HTTP response status code.
- **Parameters**:
  - `statusCode`: Expected HTTP status code
- **Example**: `Then the response status should be 200`

#### `Then the response status should be "{statusName}"`
Verifies the HTTP response status by name.
- **Parameters**:
  - `statusName`: Status name (e.g., "ok", "created", "bad request")
- **Example**: `Then the response status should be "created"`

#### `Then the response should contain "{text}"`
Verifies that the response body contains specific text.
- **Parameters**:
  - `text`: Text to search for in the response
- **Example**: `Then the response should contain "successfully created"`

#### `Then the response should not contain "{text}"`
Verifies that the response body does not contain specific text.
- **Parameters**:
  - `text`: Text that should not be in the response
- **Example**: `Then the response should not contain "error"`

#### `Then the response should have property "{jsonPath}" with value "{expectedValue}"`
Verifies a specific property value in the JSON response.
- **Parameters**:
  - `jsonPath`: Path to the property (e.g., "data.id", "items[0].name")
  - `expectedValue`: Expected value
- **Example**: `Then the response should have property "name" with value "Bench Press"`

#### `Then the response should have property "{jsonPath}"`
Verifies that a property exists in the JSON response.
- **Parameters**:
  - `jsonPath`: Path to the property
- **Example**: `Then the response should have property "id"`

#### `Then the response should not have property "{jsonPath}"`
Verifies that a property does not exist in the JSON response.
- **Parameters**:
  - `jsonPath`: Path to the property
- **Example**: `Then the response should not have property "errors"`

#### `Then the response should be empty`
Verifies that the response body is empty.
- **Example**: `Then the response should be empty`

#### `Then the response should be a valid JSON`
Verifies that the response contains valid JSON.
- **Example**: `Then the response should be a valid JSON`

#### `Then the response should be a JSON array with {count} items`
Verifies the response is an array with specific item count.
- **Parameters**:
  - `count`: Expected number of items
- **Example**: `Then the response should be a JSON array with 5 items`

## 🗄️ Database Steps

### Given Steps

#### `Given the database is empty`
Cleans the database, removing all data.
- **Example**: `Given the database is empty`

#### `Given the database has reference data`
Seeds the database with reference data (difficulty levels, equipment types, etc.).
- **Example**: `Given the database has reference data`

#### `Given the following {entityType} exists:`
Creates entities in the database from a table.
- **Parameters**:
  - `entityType`: Type of entity to create
  - Table data defining the entity properties
- **Example**:
  ```gherkin
  Given the following exercise exists:
    | Name        | Description    | DifficultyLevel |
    | Bench Press | Chest exercise | Beginner        |
  ```

### When Steps

#### `When the database has reference data`
Alternative to Given step for seeding reference data.
- **Example**: `When the database has reference data`

### Then Steps

#### `Then the database should contain {expectedCount} {entityType} records?`
Verifies the count of entities in the database.
- **Parameters**:
  - `expectedCount`: Expected number of records
  - `entityType`: Type of entity to count
- **Example**: `Then the database should contain 5 exercise records`

#### `Then the {entityType} with id "{id}" should exist`
Verifies that an entity with specific ID exists.
- **Parameters**:
  - `entityType`: Type of entity
  - `id`: Entity ID (supports placeholders)
- **Example**: `Then the exercise with id "<BenchPress.Id>" should exist`

#### `Then the {entityType} with id "{id}" should not exist`
Verifies that an entity with specific ID does not exist.
- **Parameters**:
  - `entityType`: Type of entity
  - `id`: Entity ID
- **Example**: `Then the exercise with id "exercise-123" should not exist`

#### `Then the exercise "{exerciseName}" should have the following properties:`
Verifies properties of a specific exercise.
- **Parameters**:
  - `exerciseName`: Name of the exercise
  - Table with expected properties
- **Example**:
  ```gherkin
  Then the exercise "Bench Press" should have the following properties:
    | Property    | Value          |
    | Description | Chest exercise |
    | Difficulty  | Beginner       |
  ```

#### `Then the database should be accessible`
Verifies basic database connectivity.
- **Example**: `Then the database should be accessible`

#### `Then the database should contain reference data`
Verifies that reference data has been seeded correctly.
- **Example**: `Then the database should contain reference data`

## 🔧 Common Steps

### Given Steps

#### `Given I wait for {seconds} seconds`
Pauses test execution for debugging or timing issues.
- **Parameters**:
  - `seconds`: Number of seconds to wait
- **Example**: `Given I wait for 2 seconds`

#### `Given the current date is "{date}"`
Sets the current date for date-dependent tests.
- **Parameters**:
  - `date`: Date in YYYY-MM-DD format
- **Example**: `Given the current date is "2025-01-15"`

## 📝 Placeholder Resolution

Many steps support placeholder resolution using the format `<EntityType.Property>`:

### Examples:
```gherkin
Given the following exercise exists:
  | Name        |
  | Bench Press |
When I send a GET request to "/api/exercises/<BenchPress.Id>"
Then the response should have property "id" with value "<BenchPress.Id>"
```

### Supported Placeholders:
- `<EntityName.Id>` - Entity's ID
- `<EntityName.Property>` - Any property of a created entity
- `<LastCreatedEntity.Id>` - ID of the last created entity
- `<LastResponse.Property>` - Property from the last API response

## 🎯 Best Practices

1. **Use descriptive scenario names** that explain the business value
2. **Keep steps focused** on behavior, not implementation
3. **Use Background sections** for common setup steps
4. **Use Scenario Outlines** for testing multiple similar cases
5. **Add comments** to explain complex test data or authorization requirements
6. **Use meaningful entity names** in test data for better readability
7. **Verify both success and failure cases** for comprehensive coverage

## 🚫 Common Pitfalls

1. **Don't hardcode IDs** - Use placeholders instead
2. **Don't mix Given/When/Then** - Follow the natural flow
3. **Don't test implementation details** - Focus on behavior
4. **Don't create overly complex scenarios** - Keep them simple and focused
5. **Don't forget cleanup** - Ensure test isolation