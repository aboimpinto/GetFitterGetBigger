# Missing Integration Tests for Authentication System

This document outlines the integration tests that should be implemented for the authentication system.

## AuthController Integration Tests

### Login Endpoint (`POST /api/auth/login`)
1. **New User Registration Flow**
   - Test that sending an email for a non-existent user creates a new user
   - Verify the user is persisted in the database
   - Verify a "Free-Tier" claim is automatically assigned
   - Verify JWT token is returned with correct claims

2. **Existing User Login Flow**
   - Test that existing users can login with their email
   - Verify all active claims are returned
   - Verify expired claims are filtered out
   - Test with users having multiple claims

3. **Edge Cases**
   - Test with invalid email formats
   - Test with empty/null email
   - Test database transaction rollback on failures

## End-to-End Authentication Flow Tests

1. **Complete Authentication Cycle**
   - Login with new user
   - Verify token can be decoded
   - Verify claims match database state
   - Test concurrent login requests for same email

2. **Claims Management**
   - Add claims to existing user and verify they appear in next login
   - Test claim expiration handling
   - Test different claim types (Free-Tier, Admin-Tier, etc.)

## Performance Tests

1. **Load Testing**
   - Multiple concurrent login requests
   - Database connection pooling behavior
   - Token generation performance

## Security Tests

1. **JWT Security**
   - Verify tokens are signed correctly
   - Test token expiration
   - Verify sensitive data is not exposed in tokens

## Database Integration Tests

1. **Transaction Management**
   - Verify atomicity of user + claim creation
   - Test rollback scenarios
   - Verify proper cleanup on failures

2. **Concurrency**
   - Test simultaneous user creation with same email
   - Verify no duplicate users are created