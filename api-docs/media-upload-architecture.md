# Media Upload Architecture for Exercise Management

> **⚠️ IMPORTANT: No architectural decision has been made yet. This feature is currently BLOCKED.**
> 
> This document provides a comprehensive analysis of different approaches, but implementation cannot proceed until a decision is made on:
> - Storage strategy (local vs cloud)
> - Upload approach (direct vs pre-signed URLs)
> - Scale expectations (MVP vs Production)
> - Budget for external services

This document explores different architectural approaches for handling image and video uploads in the GetFitterGetBigger exercise management system.

---
used_by:
  - admin
  - shared
---

## System Context

### Current Architecture
- **Admin App**: Blazor web application used by Personal Trainers
- **API**: ASP.NET Core Minimal API
- **Clients**: All client applications (Mobile, Web, Desktop) use Avalonia
- **File Systems**: Admin and API run on separate systems with no shared filesystem

### Requirements
1. Only Admin users (PT-Tier) can upload media
2. Media files include exercise images (JPG, PNG) and videos (MP4)
3. Clients need read-only access to media
4. Media should be accessible via URLs
5. Consider file size limits (images: 5-10MB, videos: 50-100MB)
6. Media should be backed up and recoverable
7. **Mobile Offline Support**: Mobile clients must cache media locally for offline gym use
8. **Media Optimization**: API must resize/optimize media for mobile bandwidth and storage

## Approach 1: Direct Upload to API Server

### Overview
Upload files directly to the API server using multipart/form-data, store on API's local filesystem.

### Implementation
```http
POST /api/media/upload
Content-Type: multipart/form-data

--boundary
Content-Disposition: form-data; name="file"; filename="squat.jpg"
Content-Type: image/jpeg

[binary data]
--boundary
Content-Disposition: form-data; name="type"

image
--boundary
Content-Disposition: form-data; name="exerciseId"

exercise-123
--boundary--
```

### API Storage Structure
```
/app/media/
├── images/
│   ├── exercise-123/
│   │   ├── squat-{timestamp}.jpg
│   │   └── squat-thumb-{timestamp}.jpg
│   └── exercise-456/
│       └── deadlift-{timestamp}.jpg
└── videos/
    ├── exercise-123/
    │   └── squat-demo-{timestamp}.mp4
    └── exercise-456/
        └── deadlift-demo-{timestamp}.mp4
```

### Pros
- ✅ Simple implementation
- ✅ Direct control over files
- ✅ No external dependencies
- ✅ Fast for small files
- ✅ Easy backup strategies

### Cons
- ❌ API server storage limitations
- ❌ No CDN benefits
- ❌ Scaling issues (multiple API instances)
- ❌ Large video uploads can timeout
- ❌ Bandwidth costs on API server

### Code Example
```csharp
// API Endpoint
app.MapPost("/api/media/upload", async (HttpRequest request) =>
{
    var form = await request.ReadFormAsync();
    var file = form.Files["file"];
    
    if (file == null || file.Length == 0)
        return Results.BadRequest("No file uploaded");
    
    var exerciseId = form["exerciseId"];
    var type = form["type"]; // "image" or "video"
    
    var fileName = $"{Path.GetFileNameWithoutExtension(file.FileName)}-{DateTime.UtcNow.Ticks}{Path.GetExtension(file.FileName)}";
    var filePath = Path.Combine("media", type + "s", exerciseId, fileName);
    
    Directory.CreateDirectory(Path.GetDirectoryName(filePath));
    
    using (var stream = new FileStream(filePath, FileMode.Create))
    {
        await file.CopyToAsync(stream);
    }
    
    return Results.Ok(new { url = $"/media/{type}s/{exerciseId}/{fileName}" });
})
.RequireAuthorization("PTTier");
```

## Approach 2: Cloud Storage (S3/Azure Blob)

### Overview
Upload files to cloud storage services, store URLs in database.

### Implementation Flow
1. Admin requests upload URL from API
2. API generates pre-signed URL
3. Admin uploads directly to cloud storage
4. Admin notifies API of completion
5. API stores the public URL

### Architecture
```
Admin App ─────────┐
                   ├──> Cloud Storage (S3/Azure)
API ───────────────┘         │
                             │
Clients <────────────────────┘
```

### Pros
- ✅ Scalable and reliable
- ✅ CDN integration
- ✅ Reduced API server load
- ✅ Better for large files
- ✅ Built-in backup/replication
- ✅ Cost-effective for bandwidth

### Cons
- ❌ External dependency
- ❌ More complex implementation
- ❌ Additional costs
- ❌ Requires cloud account management
- ❌ Network latency for uploads

### Code Example (Azure Blob Storage)
```csharp
// API: Generate upload URL
app.MapPost("/api/media/upload-url", async (MediaUploadRequest request, BlobServiceClient blobService) =>
{
    var containerName = request.Type == "image" ? "exercise-images" : "exercise-videos";
    var blobName = $"{request.ExerciseId}/{Guid.NewGuid()}{Path.GetExtension(request.FileName)}";
    
    var container = blobService.GetBlobContainerClient(containerName);
    var blob = container.GetBlobClient(blobName);
    
    var sasUri = blob.GenerateSasUri(
        BlobSasPermissions.Write | BlobSasPermissions.Create,
        DateTimeOffset.UtcNow.AddMinutes(30)
    );
    
    return Results.Ok(new 
    { 
        uploadUrl = sasUri.ToString(),
        blobName = blobName,
        publicUrl = blob.Uri.ToString()
    });
})
.RequireAuthorization("PTTier");

// Blazor Admin App: Upload to Azure
@code {
    private async Task<string> UploadToAzureAsync(IBrowserFile file, string exerciseId)
    {
        // Get upload URL from API
        var request = new MediaUploadRequest
        {
            FileName = file.Name,
            Type = file.ContentType.StartsWith("image/") ? "image" : "video",
            ExerciseId = exerciseId
        };
        
        var response = await HttpClient.PostAsJsonAsync("/api/media/upload-url", request);
        var uploadInfo = await response.Content.ReadFromJsonAsync<UploadUrlResponse>();
        
        // Upload directly to Azure using HttpClient
        using var fileContent = new StreamContent(file.OpenReadStream(maxAllowedSize: 100_000_000)); // 100MB max
        fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
        fileContent.Headers.Add("x-ms-blob-type", "BlockBlob");
        
        var uploadResponse = await HttpClient.PutAsync(uploadInfo.UploadUrl, fileContent);
        uploadResponse.EnsureSuccessStatusCode();
        
        // Notify API of completion
        await HttpClient.PostAsJsonAsync("/api/media/confirm-upload", new 
        { 
            exerciseId, 
            url = uploadInfo.PublicUrl 
        });
        
        return uploadInfo.PublicUrl;
    }
}
```

## Approach 3: Base64 Encoding in JSON

### Overview
Encode media files as Base64 strings and send in JSON payload.

### Implementation
```csharp
// Blazor Admin App
@code {
    private async Task UploadExerciseWithMediaAsync(ExerciseDto exerciseData, IBrowserFile imageFile)
    {
        var base64Image = await FileToBase64Async(imageFile);
        
        var payload = new ExerciseCreateRequest
        {
            Name = exerciseData.Name,
            Description = exerciseData.Description,
            // ... other exercise properties
            Image = new Base64FileUpload
            {
                FileName = imageFile.Name,
                ContentType = imageFile.ContentType,
                Data = base64Image
            }
        };
        
        await HttpClient.PostAsJsonAsync("/api/exercises", payload);
    }
    
    private async Task<string> FileToBase64Async(IBrowserFile file)
    {
        using var stream = file.OpenReadStream(maxAllowedSize: 10_000_000); // 10MB max for base64
        using var memoryStream = new MemoryStream();
        await stream.CopyToAsync(memoryStream);
        
        return Convert.ToBase64String(memoryStream.ToArray());
    }
}

// DTO classes
public class Base64FileUpload
{
    public string FileName { get; set; }
    public string ContentType { get; set; }
    public string Data { get; set; } // Base64 encoded content
}
```

### Pros
- ✅ Simple implementation
- ✅ Works with JSON APIs
- ✅ No multipart complexity
- ✅ Single request for data + media

### Cons
- ❌ 33% size increase
- ❌ Memory intensive
- ❌ Not suitable for large files
- ❌ Slower processing
- ❌ Request size limits

### Recommended Use Case
- Small images only (< 1MB)
- Thumbnails
- Profile pictures
- Icons

## Approach 4: Chunked Upload

### Overview
Split large files into chunks for reliable upload with resume capability.

### Implementation
```csharp
// Blazor Admin App: Chunked upload
@code {
    private async Task<string> UploadInChunksAsync(IBrowserFile file, string exerciseId, 
        IProgress<int> progress = null)
    {
        const int chunkSize = 5 * 1024 * 1024; // 5MB chunks
        var fileSize = file.Size;
        var totalChunks = (int)Math.Ceiling((double)fileSize / chunkSize);
        var uploadId = Guid.NewGuid().ToString();
        
        // Initialize upload
        var initResponse = await HttpClient.PostAsJsonAsync("/api/media/upload/init", new
        {
            uploadId,
            fileName = file.Name,
            fileSize = fileSize,
            totalChunks,
            exerciseId
        });
        initResponse.EnsureSuccessStatusCode();
        
        // Upload chunks
        using var fileStream = file.OpenReadStream(maxAllowedSize: 100_000_000);
        
        for (int i = 0; i < totalChunks; i++)
        {
            var buffer = new byte[Math.Min(chunkSize, fileSize - (i * chunkSize))];
            await fileStream.ReadAsync(buffer, 0, buffer.Length);
            
            var formData = new MultipartFormDataContent();
            formData.Add(new StringContent(uploadId), "uploadId");
            formData.Add(new StringContent(i.ToString()), "chunkIndex");
            formData.Add(new ByteArrayContent(buffer), "chunk", file.Name);
            
            var chunkResponse = await HttpClient.PostAsync("/api/media/upload/chunk", formData);
            chunkResponse.EnsureSuccessStatusCode();
            
            progress?.Report((i + 1) * 100 / totalChunks);
        }
        
        // Complete upload
        var completeResponse = await HttpClient.PostAsJsonAsync("/api/media/upload/complete", 
            new { uploadId });
        var result = await completeResponse.Content.ReadFromJsonAsync<UploadCompleteResponse>();
        
        return result.Url;
    }
}

// API: Handle chunks
app.MapPost("/api/media/upload-chunk", async (ChunkUploadRequest request) =>
{
    var tempPath = Path.Combine("temp", request.UploadId);
    Directory.CreateDirectory(tempPath);
    
    var chunkPath = Path.Combine(tempPath, $"chunk-{request.ChunkIndex}");
    await File.WriteAllBytesAsync(chunkPath, request.Data);
    
    if (request.ChunkIndex == request.TotalChunks - 1)
    {
        // All chunks received, combine them
        await CombineChunks(request.UploadId, request.TotalChunks);
    }
    
    return Results.Ok();
});
```

### Pros
- ✅ Reliable for large files
- ✅ Resume capability
- ✅ Progress tracking
- ✅ Better error handling

### Cons
- ❌ Complex implementation
- ❌ Temporary storage needed
- ❌ Chunk management overhead
- ❌ Cleanup requirements

## Approach 5: Hybrid Solution

### Overview
Combine approaches based on file type and size.

### Decision Tree
```
File Upload Request
        │
        ├─> Is it an image?
        │   │
        │   ├─> < 1MB? ──> Base64 in JSON
        │   │
        │   └─> > 1MB? ──> Direct upload to API
        │
        └─> Is it a video?
            │
            ├─> < 50MB? ──> Direct upload with progress
            │
            └─> > 50MB? ──> Cloud storage or Chunked upload
```

### Implementation Strategy
```csharp
public class MediaUploadService
{
    public async Task<string> UploadMedia(IFormFile file, string exerciseId)
    {
        var fileSize = file.Length;
        var isVideo = file.ContentType.StartsWith("video/");
        
        if (!isVideo && fileSize < 1_000_000) // Images < 1MB
        {
            return await UploadSmallFile(file, exerciseId);
        }
        else if (fileSize < 50_000_000) // Files < 50MB
        {
            return await UploadMediumFile(file, exerciseId);
        }
        else // Large files
        {
            return await UploadToCloudStorage(file, exerciseId);
        }
    }
}
```

## Comparison Matrix

| Aspect | Direct Upload | Cloud Storage | Base64 | Chunked | Hybrid |
|--------|--------------|---------------|---------|----------|---------|
| **Implementation Complexity** | Low | Medium | Low | High | High |
| **Scalability** | Poor | Excellent | Poor | Good | Excellent |
| **Performance (Small Files)** | Good | Good | Fair | Poor | Excellent |
| **Performance (Large Files)** | Poor | Excellent | Very Poor | Good | Excellent |
| **Cost** | Low | Medium-High | Low | Low | Variable |
| **Reliability** | Fair | Excellent | Fair | Good | Excellent |
| **Bandwidth Usage** | High | Low | Very High | High | Optimized |
| **Storage Management** | Manual | Automatic | Manual | Manual | Mixed |
| **CDN Support** | Manual | Built-in | Manual | Manual | Mixed |
| **Backup/Recovery** | Manual | Automatic | Manual | Manual | Mixed |

## Recommendations

### For MVP/Small Scale (< 100 PTs, < 10GB storage)
**Recommended: Direct Upload to API**
- Simple to implement
- No external dependencies
- Adequate for small user base
- Easy to migrate later

### For Production/Growth Phase
**Recommended: Hybrid Approach with Cloud Storage**
- Use direct upload for small images
- Use cloud storage for videos and large images
- Implement progress tracking
- Plan for CDN integration

### For Enterprise/Large Scale
**Recommended: Full Cloud Storage Solution**
- All media in cloud storage
- CDN for global distribution
- Automated backup and replication
- Cost optimization strategies

## Security Considerations

1. **File Validation**
   - Validate file types (whitelist approach)
   - Check file signatures, not just extensions
   - Scan for malware (especially for direct upload)
   - Enforce size limits

2. **Access Control**
   - Authenticate upload requests
   - Validate user permissions
   - Generate unique file names to prevent overwrites
   - Implement rate limiting

3. **Storage Security**
   - Encrypt files at rest
   - Use secure URLs with expiration
   - Implement access logging
   - Regular security audits

## Implementation Checklist

- [ ] Define file size limits
- [ ] Choose primary approach based on scale
- [ ] Implement file validation
- [ ] Create upload progress indicators
- [ ] Design error handling and retry logic
- [ ] Plan storage structure and naming
- [ ] Implement cleanup for failed uploads
- [ ] Create thumbnail generation for images
- [ ] Design CDN integration strategy
- [ ] Plan backup and recovery procedures
- [ ] Document API endpoints
- [ ] Create Admin UI components
- [ ] Test with various file sizes
- [ ] Monitor storage usage
- [ ] Plan migration strategy for growth

## Mobile Offline Considerations

### The Gym Network Problem

Gyms typically have poor or no network connectivity:
- Concrete walls and metal equipment interfere with signals
- Basements and interior rooms have weak coverage
- Users won't use mobile data for large media downloads
- Workouts must function completely offline

### Client Platform Differences

#### Mobile Clients (Avalonia Mobile)
- **Network**: Often offline in gyms
- **Storage**: Limited device storage
- **Usage**: During actual workouts
- **Requirements**: 
  - Pre-download all workout media
  - Optimized file sizes
  - Efficient caching strategy

#### Desktop/Web Clients (Avalonia Desktop/Web)
- **Network**: Always connected (home/office use)
- **Storage**: Ample storage available
- **Usage**: Planning and review
- **Requirements**: 
  - Stream media on-demand
  - Full quality media
  - No pre-caching needed

### Media Processing Pipeline

When media is uploaded to the API, it must generate multiple versions:

```
Original Upload
      │
      ├─> Full Resolution (Desktop/Web)
      │   ├─> Original Image (5-10MB)
      │   └─> Original Video (50-100MB)
      │
      └─> Mobile Optimized
          ├─> Thumbnail (50-100KB)
          ├─> Small Image (200-500KB)
          ├─> Medium Image (500KB-1MB)
          └─> Compressed Video (5-20MB)
```

### API Processing Requirements

```csharp
public class MediaProcessingService
{
    public async Task ProcessUploadedMedia(string filePath, MediaType type)
    {
        if (type == MediaType.Image)
        {
            await GenerateImageVariants(filePath);
        }
        else if (type == MediaType.Video)
        {
            await GenerateVideoVariants(filePath);
        }
    }

    private async Task GenerateImageVariants(string originalPath)
    {
        // Thumbnail: 150x150, JPEG 70% quality
        await ResizeImage(originalPath, 150, 150, "thumb");
        
        // Small: 480px wide, JPEG 80% quality (mobile list views)
        await ResizeImage(originalPath, 480, 0, "small");
        
        // Medium: 1080px wide, JPEG 85% quality (mobile detail views)
        await ResizeImage(originalPath, 1080, 0, "medium");
        
        // Large: 1920px wide, JPEG 90% quality (tablet/desktop)
        await ResizeImage(originalPath, 1920, 0, "large");
    }

    private async Task GenerateVideoVariants(string originalPath)
    {
        // Mobile: 720p, H.264, lower bitrate
        await CompressVideo(originalPath, "720p", 1500); // 1.5 Mbps
        
        // Tablet: 1080p, H.264, medium bitrate
        await CompressVideo(originalPath, "1080p", 3000); // 3 Mbps
        
        // Generate poster frame
        await ExtractVideoPoster(originalPath);
    }
}
```

### Storage Structure with Variants

```
/app/media/
├── images/
│   └── exercise-123/
│       ├── original/
│       │   └── squat.jpg (5MB)
│       ├── large/
│       │   └── squat.jpg (1.5MB)
│       ├── medium/
│       │   └── squat.jpg (500KB)
│       ├── small/
│       │   └── squat.jpg (200KB)
│       └── thumb/
│           └── squat.jpg (50KB)
└── videos/
    └── exercise-123/
        ├── original/
        │   └── squat-demo.mp4 (80MB)
        ├── 1080p/
        │   └── squat-demo.mp4 (25MB)
        ├── 720p/
        │   └── squat-demo.mp4 (10MB)
        └── poster/
            └── squat-demo.jpg (200KB)
```

### Mobile Sync Strategy

```csharp
// Avalonia Mobile: Offline sync service
public class OfflineMediaService
{
    private readonly IApiService _apiService;
    private readonly IFileService _fileService;
    private readonly IDeviceInfo _deviceInfo;
    
    public async Task SyncWorkoutMediaAsync(string workoutId)
    {
        var exercises = await GetWorkoutExercisesAsync(workoutId);
        
        foreach (var exercise in exercises)
        {
            // Download appropriate variants based on device
            var deviceProfile = GetDeviceProfile();
            
            string imageVariant, videoVariant;
            
            if (deviceProfile.IsPhone)
            {
                imageVariant = "small";
                videoVariant = "720p";
            }
            else if (deviceProfile.IsTablet)
            {
                imageVariant = "medium";
                videoVariant = "1080p";
            }
            else
            {
                imageVariant = "large";
                videoVariant = "1080p";
            }
            
            // Download and cache media
            await DownloadMediaVariantAsync(exercise.Id, exercise.ImageUrl, imageVariant);
            await DownloadMediaVariantAsync(exercise.Id, exercise.VideoUrl, videoVariant);
            
            // Store metadata in local cache
            await CacheMediaMetadataAsync(exercise.Id, imageVariant, videoVariant);
        }
    }
    
    private DeviceProfile GetDeviceProfile()
    {
        var screenWidth = _deviceInfo.ScreenWidth;
        var availableStorage = _deviceInfo.AvailableStorageMB;
        
        return new DeviceProfile
        {
            IsPhone = screenWidth < 768,
            IsTablet = screenWidth >= 768 && screenWidth < 1024,
            HasLimitedStorage = availableStorage < 1000 // MB
        };
    }
}

public class DeviceProfile
{
    public bool IsPhone { get; set; }
    public bool IsTablet { get; set; }
    public bool HasLimitedStorage { get; set; }
}
```

### API Endpoints for Mobile

```http
# Get media variants for an exercise
GET /api/exercises/{id}/media
Response:
{
    "images": {
        "thumbnail": "/media/images/exercise-123/thumb/squat.jpg",
        "small": "/media/images/exercise-123/small/squat.jpg",
        "medium": "/media/images/exercise-123/medium/squat.jpg",
        "large": "/media/images/exercise-123/large/squat.jpg",
        "original": "/media/images/exercise-123/original/squat.jpg"
    },
    "videos": {
        "720p": "/media/videos/exercise-123/720p/squat-demo.mp4",
        "1080p": "/media/videos/exercise-123/1080p/squat-demo.mp4",
        "original": "/media/videos/exercise-123/original/squat-demo.mp4",
        "poster": "/media/videos/exercise-123/poster/squat-demo.jpg"
    }
}

# Batch download preparation for offline sync
POST /api/workouts/{id}/prepare-offline
Response:
{
    "mediaFiles": [
        {
            "exerciseId": "exercise-123",
            "files": [
                { "url": "...", "size": 204800, "type": "image-small" },
                { "url": "...", "size": 10485760, "type": "video-720p" }
            ]
        }
    ],
    "totalSize": 52428800,
    "fileCount": 12
}
```

### Bandwidth and Storage Optimization

| Media Type | Desktop/Web | Mobile Phone | Mobile Tablet |
|------------|-------------|--------------|---------------|
| **Image List View** | Medium (1MB) | Small (200KB) | Medium (500KB) |
| **Image Detail View** | Large (1.5MB) | Medium (500KB) | Large (1MB) |
| **Video** | Original/1080p | 720p (10MB) | 1080p (25MB) |
| **Storage per Exercise** | Not cached | ~10-15MB | ~25-30MB |
| **50 Exercises Total** | Streamed | ~500-750MB | ~1.25-1.5GB |

### Implementation Priorities

1. **Phase 1**: Basic mobile support
   - Single compressed version for mobile
   - Manual sync before gym
   - Basic offline detection

2. **Phase 2**: Smart optimization
   - Multiple variants
   - Device-aware selection
   - Progressive download

3. **Phase 3**: Advanced features
   - Predictive pre-caching
   - Bandwidth-aware sync
   - Storage management

## 🚨 Decision Required

**This feature is currently BLOCKED pending architectural decisions.**

### Immediate Decisions Needed:

1. **Scale & Timeline**
   - [ ] MVP scope: How many PTs initially?
   - [ ] Expected storage needs (GB/TB)?
   - [ ] Timeline for implementation?

2. **Architecture Choice**
   - [ ] Direct upload to API (simple, limited scale)
   - [ ] Cloud storage (complex, unlimited scale)
   - [ ] Hybrid approach (balance complexity/scale)

3. **Budget Approval**
   - [ ] Cloud storage costs (if applicable)
   - [ ] CDN costs (if applicable)
   - [ ] Development time estimate

4. **Technical Constraints**
   - [ ] Maximum file sizes to support?
   - [ ] Video quality requirements?
   - [ ] Mobile data usage limits?

### To Unblock:
1. Review the comparison matrix above
2. Consider your current scale and growth projections
3. Make a decision based on technical and business needs
4. Document the decision in this file
5. Remove BLOCKED status from Admin and API features

### Blocked Features:
- `/GetFitterGetBigger.Admin/memory-bank/features/BLOCKED-media-upload.md`
- `/GetFitterGetBigger.API/memory-bank/BLOCKED-media-upload-endpoints.md`