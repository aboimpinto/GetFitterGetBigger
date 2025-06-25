# Exercises API

This document outlines the API endpoints for managing exercises. Personal Trainers use these endpoints to create, update, and organize the exercises that will be used in workouts.

---
used_by:
  - admin
  - client
  - shared
---

## 1. Get All Exercises

Retrieves a paginated and filterable list of all exercises.

- **Endpoint:** `GET /api/exercises`
- **Authentication:** Bearer token required.
- **Authorization:** Required `PT-Tier` or `Admin` claim.

### Query Parameters

| Parameter      | Type   | Required | Description                                               |
| :------------- | :----- | :------- | :-------------------------------------------------------- |
| `page`         | number | No       | Page number for pagination (default: 1).                  |
| `limit`        | number | No       | Number of items per page (default: 10, max: 50).          |
| `name`         | string | No       | Search term to filter exercises by name (case-insensitive). |
| `difficultyId` | string | No       | Filter exercises by a specific difficulty level ID.       |

### Success Response (200 OK)

```json
{
  "pagination": {
    "total": 150,
    "pages": 15,
    "currentPage": 1,
    "limit": 10
  },
  "exercises": [
    {
      "id": "exercise-a1b2c3d4-e5f6-7890-1234-567890abcdef",
      "name": "Barbell Back Squat",
      "description": "A fundamental compound exercise for lower body strength.",
      "difficulty": "Intermediate",
      "is_unilateral": false,
      "image_url": "https://api.getfitterbigger.com/images/squat.jpeg",
      "video_url": "https://api.getfitterbigger.com/videos/squat.mp4"
    }
  ]
}
```

---

## 2. Get Exercise by ID

Retrieves a single exercise by its unique identifier.

- **Endpoint:** `GET /api/exercises/{id}`
- **Authentication:** Bearer token required.

### Success Response (200 OK)

```json
{
  "id": "exercise-a1b2c3d4-e5f6-7890-1234-567890abcdef",
  "name": "Barbell Back Squat",
  "description": "A fundamental compound exercise for lower body strength.",
  "instructions": "1. Stand with your feet shoulder-width apart...
2. Place the barbell on your upper back...
3. Lower your body until your thighs are parallel to the floor...",
  "difficulty": "Intermediate",
  "is_unilateral": false,
  "image_url": "https://api.getfitterbigger.com/images/squat.jpeg",
  "video_url": "https://api.getfitterbigger.com/videos/squat.mp4"
}
```

---

## 3. Create Exercise

Creates a new exercise.

- **Endpoint:** `POST /api/exercises`
- **Authentication:** Bearer token required.
- **Authorization:** Required `PT-Tier` or `Admin` claim.

### Request Body

```json
{
  "name": "Push-up",
  "description": "A classic bodyweight exercise for upper body strength.",
  "instructions": "1. Get into a high plank position...
2. Lower your body until your chest nearly touches the floor...",
  "difficultyId": "difficultylevel-b1c2d3e4-f5a6-b7c8-d9e0-f1a2b3c4d5e6",
  "is_unilateral": false,
  "image_url": "https://api.getfitterbigger.com/images/pushup.jpeg",
  "video_url": "https://api.getfitterbigger.com/videos/pushup.mp4"
}
```

### Success Response (201 Created)

The response will include a `Location` header with the URL of the newly created resource (e.g., `/api/exercises/exercise-c4a5b6d7-e8f9-a0b1-c2d3-e4f5a6b7c8d9`) and return the created exercise object.

---

## 4. Update Exercise

Updates an existing exercise.

- **Endpoint:** `PUT /api/exercises/{id}`
- **Authentication:** Bearer token required.
- **Authorization:** Required `PT-Tier` or `Admin` claim.

### Request Body

The request body is the same as the `POST /api/exercises` endpoint. All fields are required.

### Success Response (204 No Content)

A successful update returns a `204 No Content` status code with an empty body.

---

## 5. Deactivate/Delete Exercise

Marks an exercise as inactive. If the exercise is not associated with any workouts, it may be permanently deleted.

- **Endpoint:** `DELETE /api/exercises/{id}`
- **Authentication:** Bearer token required.
- **Authorization:** Required `PT-Tier` or `Admin` claim.

### Success Response (204 No Content)

A successful deactivation/deletion returns a `204 No Content` status code with an empty body.
