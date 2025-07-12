Feature: Authentication
  As a user of the fitness application
  I want to authenticate using my email
  So that I can access protected resources
  
  Background:
    Given the database is empty
    And the database has reference data

  @authentication @smoke
  Scenario: Successful login with valid email
    When I send a POST request to "/api/Auth/login" with body:
      """
      {
        "email": "admin@example.com"
      }
      """
    Then the response status should be 200
    And the response should have property "token"
    And the property "token" should not be empty
    And the response should have property "claims"
    And the response should be a valid JSON

  @authentication
  Scenario: Login response contains free tier claims
    When I send a POST request to "/api/Auth/login" with body:
      """
      {
        "email": "admin@example.com"
      }
      """
    Then the response status should be 200
    And the response should have property "claims"
    And the response should contain "Free-Tier"
    # Note: Current implementation always returns Free-Tier regardless of email

  @authentication
  Scenario: Login with PT tier email returns free tier
    When I send a POST request to "/api/Auth/login" with body:
      """
      {
        "email": "pt@example.com"
      }
      """
    Then the response status should be 200
    And the response should have property "claims"
    And the response should contain "Free-Tier"
    # Note: Current implementation always returns Free-Tier regardless of email

  @authentication
  Scenario: Login with free tier email
    When I send a POST request to "/api/Auth/login" with body:
      """
      {
        "email": "user@example.com"
      }
      """
    Then the response status should be 200
    And the response should have property "claims"
    And the response should contain "Free-Tier"

  @authentication @validation
  Scenario: Login with empty email returns success
    When I send a POST request to "/api/Auth/login" with body:
      """
      {
        "email": ""
      }
      """
    Then the response status should be 200
    And the response should have property "token"
    And the response should have property "claims"
    # Note: Current implementation accepts empty email

  @authentication @validation
  Scenario: Login with null email returns bad request
    When I send a POST request to "/api/Auth/login" with body:
      """
      {
        "email": null
      }
      """
    Then the response status should be 400
    # Note: JSON deserializer rejects null for non-nullable string

  @authentication @validation
  Scenario: Login with missing email field returns bad request
    When I send a POST request to "/api/Auth/login" with body:
      """
      {}
      """
    Then the response status should be 400
    # Note: Record constructor requires email parameter

  @authentication
  Scenario: Token can be used for authenticated requests
    Given I send a POST request to "/api/Auth/login" with body:
      """
      {
        "email": "admin@example.com"
      }
      """
    And I store the response property "token" as "authToken"
    When I send an authenticated GET request to "/api/ReferenceTables/DifficultyLevels" with token "<authToken>"
    Then the response status should be 200
    And the response should be a JSON array

  @authentication
  Scenario: Protected endpoint without authentication
    When I send a GET request to "/api/ReferenceTables/DifficultyLevels"
    Then the response status should be 200
    # Note: Currently all endpoints are accessible without authentication during development

  @authentication @structure
  Scenario: Login response structure validation
    When I send a POST request to "/api/Auth/login" with body:
      """
      {
        "email": "test@example.com"
      }
      """
    Then the response status should be 200
    And the response should match the schema:
      """
      {
        "type": "object",
        "required": ["token", "claims"],
        "properties": {
          "token": { "type": "string" },
          "claims": { 
            "type": "array",
            "items": {
              "type": "object",
              "properties": {
                "claimId": { "type": "string" },
                "claimType": { "type": "string" },
                "expiresAt": { "type": ["string", "null"] },
                "resourceId": { "type": ["string", "null"] }
              }
            }
          }
        }
      }
      """