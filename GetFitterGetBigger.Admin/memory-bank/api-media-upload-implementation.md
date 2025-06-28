# Media Upload Implementation Guide for Admin App

This document provides implementation details for adding media upload functionality to the exercise management feature.

## Overview

The Admin application needs to support uploading images and videos for exercises. This guide covers the UI components and API integration regardless of the backend storage approach chosen.

## File Requirements

### Accepted File Types
- **Images**: JPG, JPEG, PNG, WebP
- **Videos**: MP4, MOV, AVI (recommend MP4 for compatibility)

### File Size Limits
- **Images**: Maximum 10MB
- **Videos**: Maximum 100MB
- **Recommended Image Dimensions**: 1920x1080 or smaller
- **Recommended Video Duration**: Under 2 minutes

## UI Components

### 1. Media Upload Component

```javascript
import React, { useState, useRef } from 'react';
import { Upload, X, FileImage, FileVideo, AlertCircle } from 'lucide-react';

const MediaUpload = ({ 
  onImageChange, 
  onVideoChange, 
  currentImageUrl, 
  currentVideoUrl,
  exerciseId 
}) => {
  const [imagePreview, setImagePreview] = useState(currentImageUrl);
  const [videoPreview, setVideoPreview] = useState(currentVideoUrl);
  const [imageError, setImageError] = useState('');
  const [videoError, setVideoError] = useState('');
  const [uploadProgress, setUploadProgress] = useState({});
  
  const imageInputRef = useRef();
  const videoInputRef = useRef();

  const validateImageFile = (file) => {
    const validTypes = ['image/jpeg', 'image/jpg', 'image/png', 'image/webp'];
    const maxSize = 10 * 1024 * 1024; // 10MB

    if (!validTypes.includes(file.type)) {
      return 'Please upload a valid image file (JPG, PNG, WebP)';
    }
    
    if (file.size > maxSize) {
      return 'Image file size must be less than 10MB';
    }
    
    return null;
  };

  const validateVideoFile = (file) => {
    const validTypes = ['video/mp4', 'video/quicktime', 'video/x-msvideo'];
    const maxSize = 100 * 1024 * 1024; // 100MB

    if (!validTypes.includes(file.type)) {
      return 'Please upload a valid video file (MP4, MOV, AVI)';
    }
    
    if (file.size > maxSize) {
      return 'Video file size must be less than 100MB';
    }
    
    return null;
  };

  const handleImageChange = async (event) => {
    const file = event.target.files[0];
    if (!file) return;

    const error = validateImageFile(file);
    if (error) {
      setImageError(error);
      return;
    }

    setImageError('');
    
    // Create preview
    const reader = new FileReader();
    reader.onloadend = () => {
      setImagePreview(reader.result);
    };
    reader.readAsDataURL(file);

    // Upload file
    try {
      const url = await uploadFile(file, 'image', exerciseId);
      onImageChange(url);
    } catch (err) {
      setImageError('Failed to upload image. Please try again.');
    }
  };

  const handleVideoChange = async (event) => {
    const file = event.target.files[0];
    if (!file) return;

    const error = validateVideoFile(file);
    if (error) {
      setVideoError(error);
      return;
    }

    setVideoError('');
    
    // Create preview (for video thumbnail)
    const videoUrl = URL.createObjectURL(file);
    setVideoPreview(videoUrl);

    // Upload file with progress
    try {
      const url = await uploadFile(file, 'video', exerciseId, (progress) => {
        setUploadProgress(prev => ({ ...prev, video: progress }));
      });
      onVideoChange(url);
      setUploadProgress(prev => ({ ...prev, video: null }));
    } catch (err) {
      setVideoError('Failed to upload video. Please try again.');
      setUploadProgress(prev => ({ ...prev, video: null }));
    }
  };

  const removeImage = () => {
    setImagePreview(null);
    onImageChange(null);
    if (imageInputRef.current) {
      imageInputRef.current.value = '';
    }
  };

  const removeVideo = () => {
    setVideoPreview(null);
    onVideoChange(null);
    if (videoInputRef.current) {
      videoInputRef.current.value = '';
    }
  };

  return (
    <div className="space-y-6">
      {/* Image Upload */}
      <div>
        <label className="block text-sm font-medium text-gray-700 mb-2">
          Exercise Image
        </label>
        
        <div className="relative">
          {imagePreview ? (
            <div className="relative inline-block">
              <img 
                src={imagePreview} 
                alt="Exercise preview" 
                className="max-w-md h-64 object-cover rounded-lg shadow-md"
              />
              <button
                onClick={removeImage}
                className="absolute top-2 right-2 p-1 bg-red-500 text-white rounded-full hover:bg-red-600"
              >
                <X size={20} />
              </button>
            </div>
          ) : (
            <div 
              onClick={() => imageInputRef.current?.click()}
              className="border-2 border-dashed border-gray-300 rounded-lg p-6 hover:border-gray-400 cursor-pointer transition-colors"
            >
              <div className="text-center">
                <FileImage className="mx-auto h-12 w-12 text-gray-400" />
                <p className="mt-2 text-sm text-gray-600">
                  Click to upload image
                </p>
                <p className="text-xs text-gray-500">
                  JPG, PNG, WebP up to 10MB
                </p>
              </div>
            </div>
          )}
          
          <input
            ref={imageInputRef}
            type="file"
            accept="image/jpeg,image/jpg,image/png,image/webp"
            onChange={handleImageChange}
            className="hidden"
          />
        </div>
        
        {imageError && (
          <div className="mt-2 flex items-center text-sm text-red-600">
            <AlertCircle size={16} className="mr-1" />
            {imageError}
          </div>
        )}
      </div>

      {/* Video Upload */}
      <div>
        <label className="block text-sm font-medium text-gray-700 mb-2">
          Exercise Video
        </label>
        
        <div className="relative">
          {videoPreview ? (
            <div className="relative inline-block">
              <video 
                src={videoPreview} 
                className="max-w-md h-64 object-cover rounded-lg shadow-md"
                controls
              />
              <button
                onClick={removeVideo}
                className="absolute top-2 right-2 p-1 bg-red-500 text-white rounded-full hover:bg-red-600"
              >
                <X size={20} />
              </button>
            </div>
          ) : (
            <div 
              onClick={() => videoInputRef.current?.click()}
              className="border-2 border-dashed border-gray-300 rounded-lg p-6 hover:border-gray-400 cursor-pointer transition-colors"
            >
              <div className="text-center">
                <FileVideo className="mx-auto h-12 w-12 text-gray-400" />
                <p className="mt-2 text-sm text-gray-600">
                  Click to upload video
                </p>
                <p className="text-xs text-gray-500">
                  MP4, MOV, AVI up to 100MB
                </p>
              </div>
            </div>
          )}
          
          <input
            ref={videoInputRef}
            type="file"
            accept="video/mp4,video/quicktime,video/x-msvideo"
            onChange={handleVideoChange}
            className="hidden"
          />
        </div>
        
        {uploadProgress.video !== undefined && uploadProgress.video !== null && (
          <div className="mt-2">
            <div className="flex items-center justify-between text-sm text-gray-600 mb-1">
              <span>Uploading video...</span>
              <span>{uploadProgress.video}%</span>
            </div>
            <div className="w-full bg-gray-200 rounded-full h-2">
              <div 
                className="bg-blue-600 h-2 rounded-full transition-all duration-300"
                style={{ width: `${uploadProgress.video}%` }}
              />
            </div>
          </div>
        )}
        
        {videoError && (
          <div className="mt-2 flex items-center text-sm text-red-600">
            <AlertCircle size={16} className="mr-1" />
            {videoError}
          </div>
        )}
      </div>
    </div>
  );
};

export default MediaUpload;
```

### 2. Upload Service Implementation

```javascript
// services/mediaService.js

class MediaService {
  constructor(apiClient) {
    this.apiClient = apiClient;
  }

  // Direct upload implementation
  async uploadFileDirect(file, type, exerciseId, onProgress) {
    const formData = new FormData();
    formData.append('file', file);
    formData.append('type', type);
    formData.append('exerciseId', exerciseId);

    return this.apiClient.post('/media/upload', formData, {
      headers: {
        'Content-Type': 'multipart/form-data',
      },
      onUploadProgress: (progressEvent) => {
        if (onProgress) {
          const percentCompleted = Math.round(
            (progressEvent.loaded * 100) / progressEvent.total
          );
          onProgress(percentCompleted);
        }
      },
    });
  }

  // Cloud storage implementation
  async uploadFileToCloud(file, type, exerciseId, onProgress) {
    // Step 1: Get upload URL from API
    const { uploadUrl, publicUrl } = await this.apiClient.post('/media/upload-url', {
      fileName: file.name,
      fileType: file.type,
      fileSize: file.size,
      type,
      exerciseId,
    });

    // Step 2: Upload to cloud storage
    await this.uploadToPresignedUrl(uploadUrl, file, onProgress);

    // Step 3: Confirm upload with API
    await this.apiClient.post('/media/confirm-upload', {
      exerciseId,
      type,
      url: publicUrl,
    });

    return publicUrl;
  }

  async uploadToPresignedUrl(url, file, onProgress) {
    return new Promise((resolve, reject) => {
      const xhr = new XMLHttpRequest();

      xhr.upload.addEventListener('progress', (e) => {
        if (e.lengthComputable && onProgress) {
          const percentComplete = Math.round((e.loaded / e.total) * 100);
          onProgress(percentComplete);
        }
      });

      xhr.addEventListener('load', () => {
        if (xhr.status >= 200 && xhr.status < 300) {
          resolve();
        } else {
          reject(new Error(`Upload failed with status ${xhr.status}`));
        }
      });

      xhr.addEventListener('error', () => {
        reject(new Error('Upload failed'));
      });

      xhr.open('PUT', url);
      xhr.setRequestHeader('Content-Type', file.type);
      xhr.send(file);
    });
  }

  // Chunked upload implementation
  async uploadFileInChunks(file, type, exerciseId, onProgress) {
    const chunkSize = 5 * 1024 * 1024; // 5MB chunks
    const totalChunks = Math.ceil(file.size / chunkSize);
    
    // Initialize upload
    const { uploadId } = await this.apiClient.post('/media/upload/init', {
      fileName: file.name,
      fileSize: file.size,
      fileType: file.type,
      totalChunks,
      type,
      exerciseId,
    });

    // Upload chunks
    for (let i = 0; i < totalChunks; i++) {
      const start = i * chunkSize;
      const end = Math.min(start + chunkSize, file.size);
      const chunk = file.slice(start, end);

      await this.uploadChunk(uploadId, i, chunk);
      
      if (onProgress) {
        const progress = Math.round(((i + 1) / totalChunks) * 100);
        onProgress(progress);
      }
    }

    // Complete upload
    const { url } = await this.apiClient.post('/media/upload/complete', {
      uploadId,
    });

    return url;
  }

  async uploadChunk(uploadId, chunkIndex, chunk) {
    const formData = new FormData();
    formData.append('uploadId', uploadId);
    formData.append('chunkIndex', chunkIndex);
    formData.append('chunk', chunk);

    return this.apiClient.post('/media/upload/chunk', formData);
  }

  // Utility function to generate thumbnails client-side
  async generateImageThumbnail(file, maxWidth = 300, maxHeight = 300) {
    return new Promise((resolve) => {
      const reader = new FileReader();
      reader.onload = (e) => {
        const img = new Image();
        img.onload = () => {
          const canvas = document.createElement('canvas');
          let width = img.width;
          let height = img.height;

          if (width > height) {
            if (width > maxWidth) {
              height *= maxWidth / width;
              width = maxWidth;
            }
          } else {
            if (height > maxHeight) {
              width *= maxHeight / height;
              height = maxHeight;
            }
          }

          canvas.width = width;
          canvas.height = height;

          const ctx = canvas.getContext('2d');
          ctx.drawImage(img, 0, 0, width, height);

          canvas.toBlob((blob) => {
            resolve(new File([blob], `thumb_${file.name}`, { type: 'image/jpeg' }));
          }, 'image/jpeg', 0.8);
        };
        img.src = e.target.result;
      };
      reader.readAsDataURL(file);
    });
  }
}

export default MediaService;
```

### 3. Integration with Exercise Form

```javascript
// components/ExerciseForm.jsx
import React, { useState } from 'react';
import MediaUpload from './MediaUpload';
import { mediaService } from '../services/mediaService';

const ExerciseForm = ({ exercise, onSubmit }) => {
  const [formData, setFormData] = useState({
    name: exercise?.name || '',
    description: exercise?.description || '',
    imageUrl: exercise?.imageUrl || '',
    videoUrl: exercise?.videoUrl || '',
    // ... other fields
  });

  const handleImageChange = (url) => {
    setFormData(prev => ({ ...prev, imageUrl: url }));
  };

  const handleVideoChange = (url) => {
    setFormData(prev => ({ ...prev, videoUrl: url }));
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    
    try {
      await onSubmit(formData);
    } catch (error) {
      console.error('Failed to save exercise:', error);
    }
  };

  return (
    <form onSubmit={handleSubmit} className="space-y-6">
      {/* Other form fields */}
      
      <MediaUpload
        onImageChange={handleImageChange}
        onVideoChange={handleVideoChange}
        currentImageUrl={formData.imageUrl}
        currentVideoUrl={formData.videoUrl}
        exerciseId={exercise?.id}
      />
      
      <button type="submit" className="btn btn-primary">
        {exercise ? 'Update Exercise' : 'Create Exercise'}
      </button>
    </form>
  );
};
```

## API Endpoints

### Option 1: Direct Upload
```
POST /api/media/upload
Content-Type: multipart/form-data

Parameters:
- file: The file to upload (binary)
- type: "image" or "video"
- exerciseId: ID of the exercise

Response:
{
  "url": "https://api.example.com/media/images/exercise-123/squat.jpg",
  "thumbnailUrl": "https://api.example.com/media/images/exercise-123/squat-thumb.jpg"
}
```

### Option 2: Cloud Storage Pre-signed URLs
```
POST /api/media/upload-url
Content-Type: application/json

Request:
{
  "fileName": "squat-demo.mp4",
  "fileType": "video/mp4",
  "fileSize": 52428800,
  "type": "video",
  "exerciseId": "exercise-123"
}

Response:
{
  "uploadUrl": "https://storage.azure.com/...",
  "publicUrl": "https://cdn.example.com/videos/exercise-123/squat-demo.mp4"
}
```

### Option 3: Chunked Upload
```
POST /api/media/upload/init
POST /api/media/upload/chunk
POST /api/media/upload/complete
```

## Error Handling

```javascript
// utils/uploadErrorHandler.js
export const handleUploadError = (error) => {
  if (error.response) {
    switch (error.response.status) {
      case 413:
        return 'File is too large. Please choose a smaller file.';
      case 415:
        return 'File type not supported. Please use JPG, PNG, or MP4.';
      case 507:
        return 'Server storage full. Please contact support.';
      default:
        return 'Upload failed. Please try again.';
    }
  }
  
  if (error.message === 'Network Error') {
    return 'Network error. Please check your connection.';
  }
  
  return 'An unexpected error occurred. Please try again.';
};
```

## Best Practices

1. **File Validation**
   - Always validate file type and size client-side
   - Show clear error messages
   - Prevent upload of invalid files

2. **Progress Indication**
   - Show upload progress for files > 1MB
   - Allow cancellation of uploads
   - Handle upload failures gracefully

3. **Preview Generation**
   - Generate image previews client-side
   - Show video thumbnail after upload
   - Optimize images before upload if possible

4. **User Experience**
   - Drag-and-drop support
   - Clear file requirements
   - Easy removal of uploaded files
   - Preserve uploads during form errors

5. **Performance**
   - Compress images client-side when possible
   - Use appropriate chunk sizes for large files
   - Implement retry logic for failed uploads

## Testing Checklist

- [ ] Test with various file types (valid and invalid)
- [ ] Test file size limits
- [ ] Test upload progress display
- [ ] Test network interruption handling
- [ ] Test concurrent uploads
- [ ] Test form validation with media
- [ ] Test media preview display
- [ ] Test media removal
- [ ] Test edit mode with existing media
- [ ] Test mobile device uploads