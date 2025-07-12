Feature: Authentication and Authorization
    As a user of the GetFitterGetBigger platform
    I want to authenticate securely
    So that I can access features based on my subscription tier

# ⚠️ IMPORTANT: These scenarios are EXAMPLES ONLY
# MUST verify actual authorization requirements with stakeholders

Scenario: Successful authentication with valid email
    When I authenticate with email "pt@example.com"
    Then I should receive a valid authentication token
    And the token should contain claim "email" with value "pt@example.com"
    And the token should contain claim "access" with value "PT-Tier"

Scenario: Authentication with different subscription tiers
    When I authenticate with email "<email>"
    Then I should receive a valid authentication token
    And the token should contain claim "access" with value "<tier>"
    
    Examples:
        | email              | tier       |
        | pt@example.com     | PT-Tier    |
        | admin@example.com  | Admin-Tier |
        | user@example.com   | Free-Tier  |

Scenario: Failed authentication with invalid email
    When I send a POST request to "/api/auth/authenticate" with body:
        """
        {
            "email": "nonexistent@example.com"
        }
        """
    Then the response status should be 401
    And the response should contain "Invalid credentials"

@authorization
Scenario Outline: Access control for different endpoints
    Given I am authenticated as a "<role>"
    When I send a GET request to "<endpoint>"
    Then the response status should be <status>
    
    Examples:
        | role       | endpoint                | status | notes                                    |
        | PT-Tier    | /api/exercises          | 200    | # PT can view exercises                 |
        | PT-Tier    | /api/clients            | 200    | # TODO: Confirm PT can view clients     |
        | PT-Tier    | /api/admin/users        | 403    | # TODO: Confirm PT cannot access admin  |
        | Admin-Tier | /api/exercises          | 200    | # Admin can view exercises              |
        | Admin-Tier | /api/admin/users        | 200    | # TODO: Confirm Admin can manage users  |
        | Free-Tier  | /api/exercises          | 200    | # Free tier can view exercises          |
        | Free-Tier  | /api/clients            | 403    | # TODO: Confirm Free cannot view clients|

Scenario: Unauthenticated access to protected endpoints
    Given I am not authenticated
    When I send a GET request to "<endpoint>"
    Then the response status should be 401
    
    Examples:
        | endpoint         |
        | /api/exercises   |
        | /api/equipment   |
        | /api/clients     |
        | /api/admin/users |

Scenario: Access with expired token
    Given I have an expired JWT token
    When I send a GET request to "/api/exercises"
    Then the response status should be 401
    And the response should contain "token expired"

Scenario: Access with invalid token
    Given I have an invalid JWT token
    When I send a GET request to "/api/exercises"
    Then the response status should be 401
    And the response should contain "invalid token"

@authorization
Scenario: Verify token claims for PT-Tier
    Given I am authenticated as a "PT-Tier"
    When I send a GET request to "/api/auth/claims"
    Then the response status should be 200
    And the response should have property "claims"
    # TODO: Verify exact claims structure with stakeholders
    
@authorization
Scenario: Verify token claims for Admin-Tier
    Given I am authenticated as a "Admin-Tier"
    When I send a GET request to "/api/auth/claims"
    Then the response status should be 200
    And the response should have property "claims"
    # TODO: Verify exact claims structure with stakeholders

@authorization
Scenario: Verify token claims for Free-Tier
    Given I am authenticated as a "Free-Tier"
    When I send a GET request to "/api/auth/claims"
    Then the response status should be 200
    And the response should have property "claims"
    # TODO: Verify exact claims structure with stakeholders

# ⚠️ AUTHORIZATION MATRIX TO BE CONFIRMED
# This is a placeholder for the actual authorization requirements
# Each endpoint needs explicit verification with stakeholders
#
# Proposed Matrix:
# | Feature          | Free-Tier | PT-Tier | Admin-Tier |
# |-----------------|-----------|---------|------------|
# | View Exercises  | ✓         | ✓       | ✓          |
# | Create Exercise | ✗         | ✓       | ✓          |
# | Update Exercise | ✗         | ✓       | ✓          |
# | Delete Exercise | ✗         | ✓       | ✓          |
# | View Equipment  | ✓         | ✓       | ✓          |
# | Manage Equipment| ✗         | ✓       | ✓          |
# | View Clients    | ✗         | ✓       | ✓          |
# | Manage Clients  | ✗         | ✓       | ✓          |
# | Admin Features  | ✗         | ✗       | ✓          |