# API Authorization Middleware

## Feature ID: FEAT-010
## Status: SUBMITTED
## Target PI: TBD

## Feature Overview

This feature implements the authorization layer for the GetFitterGetBigger API, building upon the existing authentication infrastructure. It adds JWT Bearer authentication middleware, custom token validation with sliding expiration, authorization policies, and [Authorize] attribute enforcement on controller actions.

**Status**: SUBMITTED - Awaiting refinement
**Prerequisites**: Federated Authentication Core (COMPLETED)

---

## Background

The authentication foundation is already implemented:
- Users can login via `/api/auth/login`
- JWT tokens are generated with 1-hour expiration
- Claims are managed and returned to clients

What's missing is the authorization layer that validates these tokens and enforces access control on API endpoints.

---

## Technical Requirements

### 1. JWT Bearer Authentication Configuration

Configure ASP.NET Core authentication in `Program.cs`:
- Add JWT Bearer authentication services
- Configure token validation parameters
- Add authentication and authorization middleware to the pipeline

### 2. Custom Token Validation Middleware

Implement middleware that:
- Extracts JWT from `Authorization: Bearer <token>` header
- Handles Development vs Production modes:
  - **Development**: Allow requests without token (for Swagger), but validate if provided
  - **Production**: Require valid token on all protected endpoints
- Implements sliding expiration:
  - Generate new token with refreshed 1-hour expiration
  - Return new token in `X-Refreshed-Token` response header
- Returns proper 401 responses with `UserExpired` error on validation failure

### 3. Authorization Policies

Define policies for different claim types:
- `ReferenceData-Management`: For PT-Tier and Admin-Tier users
- `Client-Access`: For all authenticated users
- `Admin-Only`: For Admin-Tier users only

### 4. Apply [Authorize] Attributes

Protected endpoints should include:
- MuscleGroupsController: Create, Update, Delete actions (ReferenceData-Management policy)
- Future controllers as they are added

Unprotected endpoints:
- `/api/auth/login` - Must remain public

---

## Implementation Notes

### Current State
- Authorization attributes are commented out in MuscleGroupsController
- No JWT Bearer middleware configured
- No authorization policies defined
- No token validation middleware implemented

### Dependencies
- Existing JWT service for token generation
- Existing User and Claim entities
- Microsoft.AspNetCore.Authentication.JwtBearer package

---

## Success Criteria

1. Unauthorized requests return 401 status
2. Valid tokens grant access to protected endpoints
3. Expired tokens are rejected
4. Sliding expiration provides new tokens in response headers
5. Different policies enforce appropriate access levels
6. Development mode allows testing without tokens
7. Production mode enforces strict token validation

---

## Future Considerations

- Token refresh endpoint for explicit token renewal
- Revocation mechanism for compromised tokens
- Rate limiting on authentication endpoints
- Audit logging for authorization events