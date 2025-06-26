# [BUG] - Authorization is not working as expected in Development with Swagger

## Description

When the `[Authorize]` attribute is applied to any controller, the application returns a 401 Unauthorized error, even when no JWT is provided. In a development environment with Swagger, the expected behavior is to allow access to the endpoints without a JWT, and to only validate the token if one is provided.

## What we've learned

- The core application is working correctly. When the `[Authorize]` attribute is removed, the endpoints are accessible.
- The issue is not with the JWT generation itself, but with how the token is being validated and how the user's identity is being established.
- The `NameClaimType` in the JWT is set to `System.Security.Claims.ClaimTypes.NameIdentifier`, but the authorization middleware is not correctly interpreting this claim.
- The test environment is not correctly configured, which has made it difficult to diagnose the issue.

## Next Steps

- Re-implement the authentication and authorization from scratch, one step at a time, to ensure that each component is working correctly.
- Configure the test environment to correctly handle JWT authentication.
- Add a custom authorization policy that explicitly uses the `NameIdentifier` claim to identify the user.
- Implement a mechanism to bypass authentication in a development environment when no JWT is provided.
