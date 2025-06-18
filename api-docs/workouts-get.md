# Get Workouts API

This endpoint retrieves a list of workouts for the authenticated user.

---
used_by:
  - admin
  - client
  - shared
---

## Endpoint URL

`/api/workouts`

## HTTP Method

`GET`

## Request Parameters

| Parameter | Type   | Required | Description                                      |
|-----------|--------|----------|--------------------------------------------------|
| page      | number | No       | Page number for pagination (default: 1)          |
| limit     | number | No       | Number of items per page (default: 10, max: 50)  |
| search    | string | No       | Search term to filter workouts by name           |
| category  | string | No       | Filter workouts by category                      |

## Response Codes and Formats

### 200 OK

```json
{
  "workouts": [
    {
      "id": "string",
      "name": "string",
      "description": "string",
      "category": "string",
      "difficulty": "string",
      "duration": "number",
      "exercises": [
        {
          "id": "string",
          "name": "string",
          "type": "string",
          "sets": "number",
          "reps": "number",
          "duration": "number",
          "restTime": "number",
          "imageUrl": "string"
        }
      ],
      "createdAt": "string",
      "updatedAt": "string"
    }
  ],
  "pagination": {
    "total": "number",
    "pages": "number",
    "currentPage": "number",
    "limit": "number"
  }
}
```

### 401 Unauthorized

```json
{
  "error": "Authentication required"
}
```

### 403 Forbidden

```json
{
  "error": "Access denied"
}
```

## Authentication Requirements

Bearer token required in the Authorization header.

## Example Request

```http
GET /api/workouts?page=1&limit=10&category=strength HTTP/1.1
Host: api.getfitterbigger.com
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

## Example Response

```http
HTTP/1.1 200 OK
Content-Type: application/json

{
  "workouts": [
    {
      "id": "w123456",
      "name": "Full Body Strength",
      "description": "A comprehensive full body workout focusing on strength training.",
      "category": "strength",
      "difficulty": "intermediate",
      "duration": 45,
      "exercises": [
        {
          "id": "e123",
          "name": "Push-ups",
          "type": "bodyweight",
          "sets": 3,
          "reps": 12,
          "duration": 0,
          "restTime": 60,
          "imageUrl": "https://api.getfitterbigger.com/images/pushup.jpeg"
        },
        {
          "id": "e124",
          "name": "Squats",
          "type": "bodyweight",
          "sets": 3,
          "reps": 15,
          "duration": 0,
          "restTime": 60,
          "imageUrl": "https://api.getfitterbigger.com/images/squat.jpeg"
        }
      ],
      "createdAt": "2025-05-15T10:30:00Z",
      "updatedAt": "2025-06-01T14:20:00Z"
    }
  ],
  "pagination": {
    "total": 24,
    "pages": 3,
    "currentPage": 1,
    "limit": 10
  }
}
