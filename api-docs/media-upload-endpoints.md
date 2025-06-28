# Media Upload API Endpoints

> **⚠️ IMPORTANT: This feature is currently BLOCKED pending architectural decisions.**
> 
> See `/api-docs/media-upload-architecture.md` for the analysis and decisions needed.

This document defines the API endpoints needed for media upload functionality, applicable to any chosen storage approach.

---
used_by:
  - admin
  - shared
---

## Authentication

All media endpoints require Bearer token authentication and `PT-Tier` or `Admin-Tier` authorization.

## Endpoints Overview

### 1. Upload Media File

Uploads an image or video file for an exercise.

- **Endpoint:** `POST /api/media/upload`
- **Content-Type:** `multipart/form-data`
- **Authorization:** Required `PT-Tier` or `Admin-Tier` claim

#### Request Parameters

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| file | binary | Yes | The media file to upload |
| type | string | Yes | Media type: "image" or "video" |
| exerciseId | string | Yes | ID of the associated exercise |

#### Success Response (200 OK)

```json
{
  "url": "https://api.getfitterbigger.com/media/images/exercise-123/squat-1234567890.jpg",
  "thumbnailUrl": "https://api.getfitterbigger.com/media/images/exercise-123/squat-thumb-1234567890.jpg",
  "size": 2048576,
  "mimeType": "image/jpeg",
  "uploadedAt": "2024-01-15T10:30:00Z"
}
```

#### Error Responses

- **400 Bad Request**: Invalid file type or missing parameters
- **413 Payload Too Large**: File exceeds size limit
- **415 Unsupported Media Type**: File type not allowed
- **507 Insufficient Storage**: Server storage full

---

### 2. Get Upload URL (Cloud Storage)

Generates a pre-signed URL for direct upload to cloud storage.

- **Endpoint:** `POST /api/media/upload-url`
- **Content-Type:** `application/json`
- **Authorization:** Required `PT-Tier` or `Admin-Tier` claim

#### Request Body

```json
{
  "fileName": "squat-demo.mp4",
  "fileType": "video/mp4",
  "fileSize": 52428800,
  "type": "video",
  "exerciseId": "exercise-123"
}
```

#### Success Response (200 OK)

```json
{
  "uploadUrl": "https://storage.provider.com/temp/upload?signature=...",
  "publicUrl": "https://cdn.getfitterbigger.com/videos/exercise-123/squat-demo.mp4",
  "expiresAt": "2024-01-15T11:00:00Z",
  "uploadId": "upload-456"
}
```

---

### 3. Confirm Upload (Cloud Storage)

Confirms successful upload to cloud storage and updates exercise record.

- **Endpoint:** `POST /api/media/upload-confirm`
- **Content-Type:** `application/json`
- **Authorization:** Required `PT-Tier` or `Admin-Tier` claim

#### Request Body

```json
{
  "uploadId": "upload-456",
  "exerciseId": "exercise-123",
  "type": "video",
  "url": "https://cdn.getfitterbigger.com/videos/exercise-123/squat-demo.mp4"
}
```

#### Success Response (204 No Content)

No response body on success.

---

### 4. Delete Media

Removes media file associated with an exercise.

- **Endpoint:** `DELETE /api/media/{exerciseId}/{type}`
- **Authorization:** Required `PT-Tier` or `Admin-Tier` claim

#### Path Parameters

- `exerciseId`: The exercise ID
- `type`: Media type ("image" or "video")

#### Success Response (204 No Content)

No response body on success.

---

### 5. Initialize Chunked Upload

Starts a chunked upload session for large files.

- **Endpoint:** `POST /api/media/upload/init`
- **Content-Type:** `application/json`
- **Authorization:** Required `PT-Tier` or `Admin-Tier` claim

#### Request Body

```json
{
  "fileName": "detailed-exercise-tutorial.mp4",
  "fileSize": 157286400,
  "fileType": "video/mp4",
  "totalChunks": 32,
  "type": "video",
  "exerciseId": "exercise-123"
}
```

#### Success Response (200 OK)

```json
{
  "uploadId": "chunked-upload-789",
  "chunkSize": 5242880,
  "totalChunks": 32
}
```

---

### 6. Upload Chunk

Uploads a single chunk of a large file.

- **Endpoint:** `POST /api/media/upload/chunk`
- **Content-Type:** `multipart/form-data`
- **Authorization:** Required `PT-Tier` or `Admin-Tier` claim

#### Request Parameters

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| uploadId | string | Yes | The upload session ID |
| chunkIndex | number | Yes | Zero-based chunk index |
| chunk | binary | Yes | The chunk data |

#### Success Response (200 OK)

```json
{
  "chunkIndex": 0,
  "received": true,
  "totalReceived": 1
}
```

---

### 7. Complete Chunked Upload

Finalizes a chunked upload and processes the complete file.

- **Endpoint:** `POST /api/media/upload/complete`
- **Content-Type:** `application/json`
- **Authorization:** Required `PT-Tier` or `Admin-Tier` claim

#### Request Body

```json
{
  "uploadId": "chunked-upload-789"
}
```

#### Success Response (200 OK)

```json
{
  "url": "https://api.getfitterbigger.com/media/videos/exercise-123/tutorial.mp4",
  "size": 157286400,
  "processingStatus": "completed"
}
```

---

## Media Serving Endpoints

### Get Media File

Serves media files with appropriate caching headers.

- **Endpoint:** `GET /media/{type}/{exerciseId}/{filename}`
- **Authentication:** Optional (public access for client apps)

#### Path Parameters

- `type`: "images" or "videos"
- `exerciseId`: The exercise ID
- `filename`: The file name

#### Response Headers

```
Content-Type: image/jpeg
Cache-Control: public, max-age=31536000
ETag: "123456789"
Last-Modified: Mon, 15 Jan 2024 10:30:00 GMT
```

---

## Validation Rules

### File Types
- **Images**: `image/jpeg`, `image/png`, `image/webp`
- **Videos**: `video/mp4`, `video/quicktime`, `video/x-msvideo`

### File Size Limits
- **Images**: Maximum 10MB
- **Videos**: Maximum 100MB
- **Chunk Size**: 5MB (for chunked uploads)

### File Naming
- Files are renamed on upload to prevent conflicts
- Format: `{original-name}-{timestamp}.{extension}`
- Thumbnails: `{original-name}-thumb-{timestamp}.jpg`

---

## Error Response Format

All error responses follow the standard API error format:

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Bad Request",
  "status": 400,
  "errors": {
    "file": ["File type not supported"],
    "size": ["File exceeds maximum size limit"]
  }
}
```

---

## Implementation Notes

1. **Progress Tracking**: Use `onUploadProgress` for XMLHttpRequest or fetch with streams
2. **Retry Logic**: Implement exponential backoff for failed uploads
3. **Cleanup**: Remove orphaned files from failed uploads
4. **Thumbnail Generation**: Generate thumbnails server-side for images
5. **CDN Integration**: Serve media through CDN for better performance
6. **Backup**: Implement regular backup of media files
7. **Monitoring**: Track storage usage and upload failures