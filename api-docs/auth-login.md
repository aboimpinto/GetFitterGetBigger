# Login API

This endpoint allows users to authenticate and receive an access token.

---
used_by:
  - admin
  - client
  - shared
---

## Endpoint URL

`/api/auth/login`

## HTTP Method

`POST`

## Request Body Format

```json
{
  "email": "string",
  "password": "string"
}
```

## Response Codes and Formats

### 200 OK

```json
{
  "accessToken": "string",
  "refreshToken": "string",
  "expiresIn": "number",
  "userProfile": {
    "id": "string",
    "email": "string",
    "firstName": "string",
    "lastName": "string",
    "role": "string"
  }
}
```

### 400 Bad Request

```json
{
  "error": "Invalid credentials"
}
```

### 401 Unauthorized

```json
{
  "error": "Authentication failed"
}
```

## Authentication Requirements

None. This endpoint is used to obtain authentication tokens.

## Example Request

```http
POST /api/auth/login HTTP/1.1
Host: api.getfitterbigger.com
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "password123"
}
```

## Example Response

```http
HTTP/1.1 200 OK
Content-Type: application/json

{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresIn": 3600,
  "userProfile": {
    "id": "123456",
    "email": "user@example.com",
    "firstName": "John",
    "lastName": "Doe",
    "role": "client"
  }
}
