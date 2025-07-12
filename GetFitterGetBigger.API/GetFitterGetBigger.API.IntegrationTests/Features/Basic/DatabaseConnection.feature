Feature: Database Connection
    As a developer
    I want to verify the database connection works
    So that I know the test infrastructure is set up correctly

Scenario: Database is accessible
    Given the database is empty
    Then the database should be accessible