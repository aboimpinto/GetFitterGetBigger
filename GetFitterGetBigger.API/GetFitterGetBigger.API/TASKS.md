# Tasks

## Bugs

### [BUG-001] JWT Implementation Issues

**Description:**
The JWT implementation has several issues that need to be addressed:

1. Token refresh mechanism doesn't work properly in the test environment
   - The `OnTokenValidated` event in the JWT middleware is not being triggered in the test environment
   - The `X-Refreshed-Token` header is not being added to the response

2. JWT validation in different environments
   - The current implementation doesn't properly handle JWT validation in different environments (Development, Testing, Production)
   - The `DevelopmentAllowAnonymousFilter` is not working as expected in the test environment

**Impact:**
- Two tests are currently skipped due to these issues:
  - `Login_WithNewUser_ReturnsTokenAndCreatesUser`
  - `ProtectedEndpoint_WithValidToken_ReturnsOkAndRefreshedToken`

**Possible Solutions:**
1. Refactor the JWT middleware configuration to properly handle different environments
2. Ensure the `OnTokenValidated` event is triggered in the test environment
3. Consider using a different approach for token refresh in tests, such as mocking the JWT service

**Priority:** Medium

**Assigned To:** TBD

**Status:** Open
