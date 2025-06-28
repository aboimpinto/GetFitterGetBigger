# [BLOCKED] Media Upload API Endpoints

## Status: BLOCKED - Awaiting Architectural Decision

### API Feature Overview
Implementation of media upload endpoints to support exercise images and videos, including processing pipeline for mobile optimization.

### Why is this BLOCKED?

**No architectural decision has been made on how to implement media storage and processing.**

### Endpoints Ready to Implement (Once Unblocked)

All endpoint designs are complete in `/api-docs/media-upload-endpoints.md`:

1. **Upload Endpoints**:
   - `POST /api/media/upload` - Direct file upload
   - `POST /api/media/upload-url` - Pre-signed URL generation
   - `POST /api/media/upload/chunk` - Chunked upload support

2. **Media Processing Pipeline**:
   - Image resizing (thumbnail, small, medium, large)
   - Video compression (720p, 1080p)
   - Poster frame extraction

3. **Media Serving**:
   - `GET /api/exercises/{id}/media` - Get all variants
   - `GET /media/{type}/{exerciseId}/{filename}` - Serve files

### Technical Decisions Needed

1. **Storage Backend**:
   - Local filesystem (simple but not scalable)
   - Azure Blob Storage (scalable but adds cost)
   - Hybrid approach (complexity vs flexibility)

2. **Processing Strategy**:
   - Synchronous processing (blocks upload)
   - Asynchronous with queues (more complex)
   - External service (additional cost)

3. **Mobile Optimization**:
   - How many image variants to generate?
   - Video compression settings?
   - Storage cost vs bandwidth savings?

### Implementation Considerations

When unblocked, the API needs to handle:
- Large file uploads (up to 100MB for videos)
- Concurrent upload requests
- Media processing without blocking
- Storage cleanup for failed uploads
- CDN integration for performance

### Dependencies

- **Libraries Needed** (when unblocked):
  - ImageSharp or SkiaSharp for image processing
  - FFMpegCore for video processing
  - Azure.Storage.Blobs (if using Azure)

### Impact on Other Features

Blocking media upload affects:
- Exercise management (no visual content)
- Mobile offline sync (no media to cache)
- Workout experience (text-only instructions)

### To Unblock This Feature

1. **Make architectural decision** based on analysis
2. **Approve budget** for chosen approach
3. **Set up infrastructure** (storage, CDN, etc.)
4. **Implement endpoints** following documentation
5. **Test with various file sizes** and types

### Current State

- ✅ Endpoint documentation complete
- ✅ Processing pipeline designed
- ✅ Mobile optimization strategy defined
- ❌ Architectural approach not selected
- ❌ Infrastructure not provisioned
- ❌ Implementation blocked

---

**Note**: All technical analysis is complete. Waiting only for business/architectural decision to proceed with implementation.