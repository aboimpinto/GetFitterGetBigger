# Workout Reference Data API Endpoints

This document defines the REST API endpoints for the Workout Reference Data feature. All endpoints follow read-only patterns as these are reference tables with predefined data.

## Base URL
- **Development**: `http://localhost:5214/api`
- **Production**: TBD

## Authentication
All endpoints require JWT Bearer token authentication with minimum Free-Tier claims.

## Workout Objectives Endpoints

### GET /workout-objectives
Retrieves all active workout objectives.

**Request Headers:**
```
Authorization: Bearer {jwt-token}
Accept: application/json
```

**Query Parameters:**
- `includeInactive` (boolean, optional): Include inactive objectives. Default: false

**Response (200 OK):**
```json
[
  {
    "workoutObjectiveId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "value": "Muscular Strength",
    "description": "Develops maximum force production capabilities. Typical programming includes 1-5 reps per set, 3-5 sets total, 3-5 minute rest periods between sets, and 85-100% intensity of 1RM. Focus on heavy compound movements with excellent form and full recovery between efforts.",
    "displayOrder": 1,
    "isActive": true
  },
  {
    "workoutObjectiveId": "7b8c9d12-3456-789a-bcde-f012345678ab",
    "value": "Hypertrophy",
    "description": "Promotes muscle size increase through controlled volume and moderate intensity. Typical programming includes 6-12 reps per set, 3-4 sets total, 1-3 minute rest periods, and 65-85% intensity of 1RM. Emphasizes time under tension and metabolic stress.",
    "displayOrder": 2,
    "isActive": true
  }
]
```

**Caching Headers:**
```
Cache-Control: public, max-age=3600
ETag: "workout-objectives-v1"
```

### GET /workout-objectives/{id}
Retrieves a specific workout objective by ID.

**Request Headers:**
```
Authorization: Bearer {jwt-token}
Accept: application/json
```

**Path Parameters:**
- `id` (string, required): WorkoutObjective GUID

**Response (200 OK):**
```json
{
  "workoutObjectiveId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "value": "Muscular Strength",
  "description": "Develops maximum force production capabilities. Typical programming includes 1-5 reps per set, 3-5 sets total, 3-5 minute rest periods between sets, and 85-100% intensity of 1RM. Focus on heavy compound movements with excellent form and full recovery between efforts.",
  "displayOrder": 1,
  "isActive": true
}
```

**Error Response (404 Not Found):**
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
  "title": "Workout objective not found",
  "status": 404,
  "detail": "No workout objective exists with the specified ID"
}
```

## Workout Categories Endpoints

### GET /workout-categories
Retrieves all active workout categories.

**Request Headers:**
```
Authorization: Bearer {jwt-token}
Accept: application/json
```

**Query Parameters:**
- `includeInactive` (boolean, optional): Include inactive categories. Default: false

**Response (200 OK):**
```json
[
  {
    "workoutCategoryId": "8f7e6d5c-4b3a-2918-0605-847362514938",
    "value": "HIIT",
    "description": "Cardiovascular conditioning with high-intensity bursts and short rest periods. Provides full body engagement through time-based exercises designed to improve cardiovascular efficiency and metabolic conditioning.",
    "icon": "timer-icon",
    "color": "#FF6B35",
    "primaryMuscleGroups": "Full Body",
    "displayOrder": 1,
    "isActive": true
  },
  {
    "workoutCategoryId": "9d8c7b6a-5940-3827-1605-948372615049",
    "value": "Arms",
    "description": "Upper arm muscle development focusing on biceps, triceps, and forearms. Common exercises include curls, extensions, and pressing movements designed to build arm strength and definition.",
    "icon": "bicep-icon",
    "color": "#4ECDC4",
    "primaryMuscleGroups": "Biceps, Triceps, Forearms",
    "displayOrder": 2,
    "isActive": true
  }
]
```

**Caching Headers:**
```
Cache-Control: public, max-age=3600
ETag: "workout-categories-v1"
```

### GET /workout-categories/{id}
Retrieves a specific workout category by ID.

**Request Headers:**
```
Authorization: Bearer {jwt-token}
Accept: application/json
```

**Path Parameters:**
- `id` (string, required): WorkoutCategory GUID

**Response (200 OK):**
```json
{
  "workoutCategoryId": "8f7e6d5c-4b3a-2918-0605-847362514938",
  "value": "HIIT",
  "description": "Cardiovascular conditioning with high-intensity bursts and short rest periods. Provides full body engagement through time-based exercises designed to improve cardiovascular efficiency and metabolic conditioning.",
  "icon": "timer-icon",
  "color": "#FF6B35",
  "primaryMuscleGroups": "Full Body",
  "displayOrder": 1,
  "isActive": true
}
```

## Execution Protocols Endpoints

### GET /execution-protocols
Retrieves all active execution protocols.

**Request Headers:**
```
Authorization: Bearer {jwt-token}
Accept: application/json
```

**Query Parameters:**
- `includeInactive` (boolean, optional): Include inactive protocols. Default: false

**Response (200 OK):**
```json
[
  {
    "executionProtocolId": "1a2b3c4d-5e6f-7890-abcd-ef1234567890",
    "code": "STANDARD",
    "value": "Standard",
    "description": "Traditional sets and repetitions with prescribed rest periods. Perform the specified reps, then rest for the prescribed time before the next set. Used for strength training, hypertrophy, and general fitness with fixed rest patterns.",
    "timeBase": false,
    "repBase": true,
    "restPattern": "Fixed",
    "intensityLevel": "Medium",
    "displayOrder": 1,
    "isActive": true
  },
  {
    "executionProtocolId": "2b3c4d5e-6f70-8901-bcde-f12345678901",
    "code": "AMRAP",
    "value": "AMRAP",
    "description": "Maximum repetitions within a specified time window. Perform as many complete reps as possible within the time limit while maintaining proper form. Used for conditioning, muscular endurance, and metabolic training with minimal rest.",
    "timeBase": true,
    "repBase": true,
    "restPattern": "Minimal",
    "intensityLevel": "High",
    "displayOrder": 2,
    "isActive": true
  }
]
```

**Caching Headers:**
```
Cache-Control: public, max-age=3600
ETag: "execution-protocols-v1"
```

### GET /execution-protocols/{id}
Retrieves a specific execution protocol by ID.

**Request Headers:**
```
Authorization: Bearer {jwt-token}
Accept: application/json
```

**Path Parameters:**
- `id` (string, required): ExecutionProtocol GUID

**Response (200 OK):**
```json
{
  "executionProtocolId": "1a2b3c4d-5e6f-7890-abcd-ef1234567890",
  "code": "STANDARD",
  "value": "Standard",
  "description": "Traditional sets and repetitions with prescribed rest periods. Perform the specified reps, then rest for the prescribed time before the next set. Used for strength training, hypertrophy, and general fitness with fixed rest patterns.",
  "timeBase": false,
  "repBase": true,
  "restPattern": "Fixed",
  "intensityLevel": "Medium",
  "displayOrder": 1,
  "isActive": true
}
```

### GET /execution-protocols/by-code/{code}
Retrieves a specific execution protocol by its programmatic code.

**Request Headers:**
```
Authorization: Bearer {jwt-token}
Accept: application/json
```

**Path Parameters:**
- `code` (string, required): ExecutionProtocol code (e.g., "STANDARD", "AMRAP")

**Response (200 OK):**
```json
{
  "executionProtocolId": "1a2b3c4d-5e6f-7890-abcd-ef1234567890",
  "code": "STANDARD",
  "value": "Standard",
  "description": "Traditional sets and repetitions with prescribed rest periods. Perform the specified reps, then rest for the prescribed time before the next set. Used for strength training, hypertrophy, and general fitness with fixed rest patterns.",
  "timeBase": false,
  "repBase": true,
  "restPattern": "Fixed",
  "intensityLevel": "Medium",
  "displayOrder": 1,
  "isActive": true
}
```

## Common Error Responses

### 401 Unauthorized
```json
{
  "type": "https://tools.ietf.org/html/rfc7235#section-3.1",
  "title": "Unauthorized",
  "status": 401,
  "detail": "Authentication required to access reference data"
}
```

### 403 Forbidden
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.3",
  "title": "Forbidden",
  "status": 403,
  "detail": "Insufficient permissions to access reference tables"
}
```

### 404 Not Found
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
  "title": "Resource not found",
  "status": 404,
  "detail": "Requested reference item does not exist"
}
```

### 500 Internal Server Error
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.6.1",
  "title": "Internal server error",
  "status": 500,
  "detail": "Unable to retrieve reference data at this time"
}
```

## Rate Limiting
Reference data endpoints have generous rate limits due to their read-only nature and caching:
- **Limit**: 1000 requests per hour per user
- **Headers**: 
  - `X-RateLimit-Limit: 1000`
  - `X-RateLimit-Remaining: 999`
  - `X-RateLimit-Reset: 1642694400`

## Caching Strategy
All reference data endpoints implement aggressive caching:
- **Server-side**: Redis cache with 1-hour TTL
- **Client-side**: HTTP cache headers allowing 1-hour browser caching
- **CDN**: CloudFlare caching for 24 hours
- **Invalidation**: Manual cache clearing on reference data updates

## Security Considerations
- All endpoints require valid JWT authentication
- Minimum Free-Tier authorization claim required
- No sensitive data exposure (all fitness reference information)
- Request/response logging for analytics (no PII)
- Standard rate limiting applied

## Performance Characteristics
- **Response time**: < 50ms (cached), < 200ms (uncached)
- **Throughput**: 10,000+ requests/second (cached)
- **Data size**: ~2KB per collection endpoint
- **Availability**: 99.9% uptime target