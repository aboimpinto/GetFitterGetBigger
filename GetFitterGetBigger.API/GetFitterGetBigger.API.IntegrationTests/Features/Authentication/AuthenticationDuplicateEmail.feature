Feature: Authentication with Duplicate Email
    As a user of the GetFitterGetBigger platform
    I want to be able to authenticate multiple times with the same email
    So that I can log in repeatedly without encountering errors

Background:
    Given the system is properly configured for authentication

Scenario: User authenticates successfully on first login
    When I authenticate with email "test@example.com"
    Then I should receive a valid authentication token
    And the token should contain claim "email" with value "test@example.com"
    And the response status should be 200

Scenario: User authenticates successfully on second login (duplicate email)
    Given a user with email "duplicate@example.com" has already been created
    When I authenticate with email "duplicate@example.com"  
    Then I should receive a valid authentication token
    And the token should contain claim "email" with value "duplicate@example.com"
    And the response status should be 200
    And the authentication should not cause any database constraint violations

Scenario: Multiple sequential authentications with same email
    When I authenticate with email "sequential@example.com"
    Then I should receive a valid authentication token
    And the response status should be 200
    When I authenticate with email "sequential@example.com" again
    Then I should receive a valid authentication token  
    And the response status should be 200
    When I authenticate with email "sequential@example.com" a third time
    Then I should receive a valid authentication token
    And the response status should be 200
    And no database errors should occur during the process