# Federated Authentication and Claims-Based Authorization

## 1. Feature Overview

This document outlines the specification for implementing a federated authentication and claims-based authorization system for the GetFitterGetBigger ecosystem. The system will leverage federated identity providers (initially Google and Facebook) for user authentication on the client-side, and the API will manage user profiles, claims, and session tokens.

The flow is as follows:
1.  A client application (e.g., Admin, Mobile) authenticates a user via a federated provider (Google, Facebook).
2.  The client sends the user's email to the API.
3.  The API creates a user profile if one doesn't exist and assigns a default claim.
4.  The API generates a JSON Web Token (JWT) with a sliding expiration and returns it along with a list of the user's claims.
5.  The client uses this JWT to authenticate subsequent API requests.

---

## 2. Database Schema

Two new entities are required to support this feature. They should be implemented following the existing `databaseModelPattern.md`.

### Entity Relationship

```mermaid
graph TD
    User -- one-to-many --> Claim;
    User[User Table <br> - UserId (PK, guid) <br> - Email (string, unique)];
    Claim[Claim Table <br> - ClaimId (PK, guid) <br> - UserId (FK, guid) <br> - ClaimType (string) <br> - ExpirationDate (datetime, nullable) <br> - Resource (string, nullable)];
```

### `User` Entity

Represents a user within the GetFitterGetBigger ecosystem.

**Location**: `Models/Entities/User.cs`
**Specialized ID**: `Models/SpecializedIds/UserId.cs`

| Field    | Type   | Constraints              | Description                               |
| :------- | :----- | :----------------------- | :---------------------------------------- |
| `Id`     | `UserId` | Primary Key              | The unique identifier for the user.       |
| `Email`  | `string` | Required, Unique Index   | The user's email address, from the federated provider. |

### `Claim` Entity

Represents a specific permission or role assigned to a user.

**Location**: `Models/Entities/Claim.cs`
**Specialized ID**: `Models/SpecializedIds/ClaimId.cs`

| Field            | Type       | Constraints   | Description                                                                                                |
| :--------------- | :--------- | :------------ | :--------------------------------------------------------------------------------------------------------- |
| `Id`             | `ClaimId`  | Primary Key   | The unique identifier for the claim.                                                                       |
| `UserId`         | `UserId`   | Foreign Key   | Links the claim to a user.                                                                                 |
| `ClaimType`      | `string`   | Required      | The type of claim (e.g., `Free-Tier`, `TimedPlan-Tier`, `PT-Tier`, `Admin`).                                 |
| `ExpirationDate` | `DateTime?`| Nullable      | The date and time when the claim expires. Null for permanent claims.                                       |
| `Resource`       | `string?`  | Nullable      | An optional resource identifier associated with the claim (e.g., `plan-<guid>`, `diet-<guid>`).             |

---

## 3. API Endpoint Specification

A new controller will be created to handle authentication.

**Controller**: `AuthController.cs`
**Endpoint**: `POST /api/auth/login`

### Request Body

The client will send a JSON object with the user's email.

**DTO**: `AuthenticationRequest.cs`
```csharp
// Location: Shared/Models/Dtos/AuthenticationRequest.cs
public record AuthenticationRequest(string Email);
```

**Example**:
```json
{
  "email": "user@example.com"
}
```

### Success Response (200 OK)

Upon successful authentication, the API will return a JWT and a list of the user's claims.

**DTO**: `AuthenticationResponse.cs` and `ClaimInfo.cs`
```csharp
// Location: Shared/Models/Dtos/AuthenticationResponse.cs
public record AuthenticationResponse(string Token, List<ClaimInfo> Claims);

// Location: Shared/Models/Dtos/ClaimInfo.cs
public record ClaimInfo(string ClaimId, DateTime? ExpirationDate, string? Resource);
```

**Example**:
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "claims": [
    {
      "claimId": "claim-c4a5b6d7-e8f9-a0b1-c2d3-e4f5a6b7c8d9",
      "expirationDate": null,
      "resource": null
    },
    {
      "claimId": "claim-a1b2c3d4-e5f6-a7b8-c9d0-e1f2a3b4c5d6",
      "expirationDate": "2025-07-25T14:00:00Z",
      "resource": "plan-b1c2d3e4-f5a6-b7c8-d9e0-f1a2b3c4d5e6"
    }
  ]
}
```

### Error Response (401 Unauthorized)

If token validation fails, the API should return a `401` status with a specific error message.

**Example**:
```json
{
  "error": "UserExpired",
  "message": "The session has expired. Please log in again."
}
```

---

## 4. Business Logic

### User and Claim Management

1.  When a request is received at `POST /api/auth/login`:
2.  The system must query the database for a user with the provided `Email`.
3.  **If the user does not exist**:
    *   Create a new `User` record.
    *   Create a new `Claim` record for this user with `ClaimType = "Free-Tier"`, `ExpirationDate = null`, and `Resource = null`.
    *   Save both to the database.
4.  **If the user exists**:
    *   Retrieve all associated `Claim` records where `ExpirationDate` is either `null` or in the future.
5.  Proceed to token generation.

### Claim Types

*   **`Free-Tier`**: Basic claim, permanent (`ExpirationDate` is null).
*   **`TimedPlan-Tier`**: For subscribed plans, has an `ExpirationDate` and a `Resource` (e.g., `plan-<guid>`).
*   **`PT-Tier`**: For Personal Trainers, has an `ExpirationDate`.
*   **`Admin`**: For system administrators, permanent. Can only be assigned directly in the database.

---

## 5. Token and Session Management

### JWT Generation

*   A dedicated `JwtService` should be created to handle token generation.
*   The token must contain essential user identifiers (e.g., `UserId`) as claims.
*   The token must have a **1-hour expiration time**.

### Token Validation Middleware

A custom middleware must be implemented to protect API endpoints.

1.  **Execution**: The middleware should run on all authenticated API endpoints.
2.  **Token Extraction**: It must extract the JWT from the `Authorization: Bearer <token>` header.
3.  **Development vs. Production Mode**:
    *   **In `Development` mode**: If the `Authorization` header is missing, the middleware should allow the request to proceed to facilitate testing with tools like Swagger. However, if a token *is* provided, it must be validated.
    *   **In `Production` mode**: The `Authorization` header with a valid token is mandatory. If missing or invalid, the request must be rejected.
4.  **Sliding Expiration**:
    *   Upon successful validation of a token, the middleware must generate a **new JWT** with a refreshed 1-hour expiration.
    *   This new token must be added to the response in the `X-Refreshed-Token` header. The client will be responsible for updating its stored token.
5.  **Failure**: If token validation fails (e.g., expired, invalid signature), the middleware must terminate the request and return a `401 Unauthorized` response with the `UserExpired` error message.
