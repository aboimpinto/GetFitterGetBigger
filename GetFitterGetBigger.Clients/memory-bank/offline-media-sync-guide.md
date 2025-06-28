# Offline Media Sync Guide for Client Applications

This document explains how client applications handle media synchronization for offline usage, particularly important for mobile clients used in gyms without reliable network connectivity.

## Overview

### The Problem
- Gyms often have poor or no network connectivity
- Users need access to exercise images and videos during workouts
- Mobile devices have limited storage
- Different clients have different network availability

### Client Differences

| Client Type | Network Availability | Storage | Media Strategy |
|-------------|---------------------|---------|----------------|
| **Mobile** | Often offline (gym) | Limited | Pre-download optimized media |
| **Desktop** | Always online | Ample | Stream on-demand |
| **Web** | Always online | N/A | Stream on-demand |

## Media Variants from API

The API provides multiple variants of each media file:

```csharp
public class ExerciseMediaInfo
{
    public Dictionary<string, string> Images { get; set; }
    public Dictionary<string, string> Videos { get; set; }
}

// Example response from GET /api/exercises/{id}/media
{
    "images": {
        "thumbnail": "/media/images/exercise-123/thumb/squat.jpg",      // 50KB
        "small": "/media/images/exercise-123/small/squat.jpg",          // 200KB
        "medium": "/media/images/exercise-123/medium/squat.jpg",        // 500KB
        "large": "/media/images/exercise-123/large/squat.jpg",          // 1.5MB
        "original": "/media/images/exercise-123/original/squat.jpg"     // 5MB
    },
    "videos": {
        "720p": "/media/videos/exercise-123/720p/squat-demo.mp4",       // 10MB
        "1080p": "/media/videos/exercise-123/1080p/squat-demo.mp4",     // 25MB
        "original": "/media/videos/exercise-123/original/squat-demo.mp4", // 80MB
        "poster": "/media/videos/exercise-123/poster/squat-demo.jpg"    // 200KB
    }
}
```

## Mobile Implementation (Avalonia)

### 1. Device Profile Detection

```csharp
public class DeviceProfileService
{
    public DeviceProfile GetProfile()
    {
        var screenWidth = DeviceDisplay.MainDisplayInfo.Width;
        var screenHeight = DeviceDisplay.MainDisplayInfo.Height;
        var density = DeviceDisplay.MainDisplayInfo.Density;
        var platform = DeviceInfo.Platform;
        
        // Calculate actual screen size
        var screenInches = Math.Sqrt(
            Math.Pow(screenWidth / density, 2) + 
            Math.Pow(screenHeight / density, 2)
        );
        
        return new DeviceProfile
        {
            IsPhone = screenInches < 7,
            IsTablet = screenInches >= 7 && screenInches < 10,
            Platform = platform,
            AvailableStorageGB = GetAvailableStorage(),
            NetworkType = Connectivity.NetworkAccess
        };
    }
    
    private double GetAvailableStorage()
    {
        // Platform-specific implementation
        #if ANDROID
            var statFs = new Android.OS.StatFs(Android.OS.Environment.DataDirectory.AbsolutePath);
            return (statFs.AvailableBlocksLong * statFs.BlockSizeLong) / (1024.0 * 1024.0 * 1024.0);
        #elif IOS
            var fileManager = NSFileManager.DefaultManager;
            var url = fileManager.GetUrls(NSSearchPathDirectory.DocumentDirectory, NSSearchPathDomain.User)[0];
            var values = url.GetResourceValues(new[] { NSUrl.VolumeAvailableCapacityKey }, out var error);
            return values[NSUrl.VolumeAvailableCapacityKey] as NSNumber / (1024.0 * 1024.0 * 1024.0);
        #endif
    }
}
```

### 2. Offline Media Service

```csharp
public class OfflineMediaService
{
    private readonly IApiService _apiService;
    private readonly ILocalDatabase _database;
    private readonly IFileService _fileService;
    private readonly DeviceProfileService _deviceProfile;
    
    public async Task SyncWorkoutMedia(string workoutId, IProgress<SyncProgress> progress = null)
    {
        var workout = await _apiService.GetWorkoutAsync(workoutId);
        var exercises = workout.GetAllExercises();
        var profile = _deviceProfile.GetProfile();
        
        var totalFiles = exercises.Count * 2; // image + video per exercise
        var completed = 0;
        
        foreach (var exercise in exercises)
        {
            try
            {
                // Get media info from API
                var mediaInfo = await _apiService.GetExerciseMediaAsync(exercise.Id);
                
                // Download appropriate variants
                var imageVariant = SelectImageVariant(profile);
                var videoVariant = SelectVideoVariant(profile);
                
                // Download and cache image
                if (mediaInfo.Images.ContainsKey(imageVariant))
                {
                    await DownloadAndCacheMedia(
                        exercise.Id, 
                        mediaInfo.Images[imageVariant], 
                        MediaType.Image,
                        imageVariant
                    );
                }
                
                progress?.Report(new SyncProgress 
                { 
                    Current = ++completed, 
                    Total = totalFiles,
                    CurrentFile = $"{exercise.Name} - Image"
                });
                
                // Download and cache video
                if (mediaInfo.Videos.ContainsKey(videoVariant))
                {
                    await DownloadAndCacheMedia(
                        exercise.Id, 
                        mediaInfo.Videos[videoVariant], 
                        MediaType.Video,
                        videoVariant
                    );
                }
                
                // Always download video poster
                if (mediaInfo.Videos.ContainsKey("poster"))
                {
                    await DownloadAndCacheMedia(
                        exercise.Id, 
                        mediaInfo.Videos["poster"], 
                        MediaType.Image,
                        "poster"
                    );
                }
                
                progress?.Report(new SyncProgress 
                { 
                    Current = ++completed, 
                    Total = totalFiles,
                    CurrentFile = $"{exercise.Name} - Video"
                });
            }
            catch (Exception ex)
            {
                // Log error but continue with other exercises
                await _logger.LogErrorAsync($"Failed to sync media for exercise {exercise.Id}", ex);
            }
        }
    }
    
    private string SelectImageVariant(DeviceProfile profile)
    {
        if (profile.IsPhone)
        {
            return profile.AvailableStorageGB < 2 ? "small" : "medium";
        }
        else if (profile.IsTablet)
        {
            return "medium";
        }
        else
        {
            return "large";
        }
    }
    
    private string SelectVideoVariant(DeviceProfile profile)
    {
        if (profile.IsPhone || profile.AvailableStorageGB < 5)
        {
            return "720p";
        }
        else
        {
            return "1080p";
        }
    }
    
    private async Task DownloadAndCacheMedia(
        string exerciseId, 
        string url, 
        MediaType type,
        string variant)
    {
        // Check if already cached
        var cachedPath = GetCachedMediaPath(exerciseId, type, variant);
        if (File.Exists(cachedPath))
        {
            // Verify file integrity
            var fileInfo = new FileInfo(cachedPath);
            if (fileInfo.Length > 0)
            {
                return; // Already cached
            }
        }
        
        // Download file
        using var httpClient = new HttpClient();
        httpClient.Timeout = TimeSpan.FromMinutes(5);
        
        var response = await httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();
        
        // Save to cache
        Directory.CreateDirectory(Path.GetDirectoryName(cachedPath));
        
        using var fileStream = File.Create(cachedPath);
        await response.Content.CopyToAsync(fileStream);
        
        // Update database
        await _database.InsertOrUpdateAsync(new CachedMedia
        {
            ExerciseId = exerciseId,
            MediaType = type,
            Variant = variant,
            LocalPath = cachedPath,
            RemoteUrl = url,
            CachedAt = DateTime.UtcNow,
            FileSize = new FileInfo(cachedPath).Length
        });
    }
    
    private string GetCachedMediaPath(string exerciseId, MediaType type, string variant)
    {
        var cacheDir = Path.Combine(
            FileSystem.CacheDirectory,
            "media",
            type.ToString().ToLower(),
            exerciseId
        );
        
        var extension = type == MediaType.Video ? ".mp4" : ".jpg";
        return Path.Combine(cacheDir, $"{variant}{extension}");
    }
}
```

### 3. Smart Sync Strategy

```csharp
public class SmartSyncService
{
    private readonly OfflineMediaService _offlineMedia;
    private readonly IConnectivity _connectivity;
    private readonly IPreferences _preferences;
    
    public async Task PerformSmartSync()
    {
        var syncStrategy = GetSyncStrategy();
        
        switch (syncStrategy)
        {
            case SyncStrategy.WiFiOnly:
                if (_connectivity.NetworkAccess == NetworkAccess.Internet && 
                    _connectivity.ConnectionProfiles.Contains(ConnectionProfile.WiFi))
                {
                    await SyncUpcomingWorkouts();
                }
                break;
                
            case SyncStrategy.WiFiAndCellular:
                if (_connectivity.NetworkAccess == NetworkAccess.Internet)
                {
                    await SyncUpcomingWorkouts();
                }
                break;
                
            case SyncStrategy.Manual:
                // User must manually trigger sync
                break;
        }
    }
    
    private async Task SyncUpcomingWorkouts()
    {
        // Get workouts for next 7 days
        var upcomingWorkouts = await _workoutService.GetUpcomingWorkoutsAsync(days: 7);
        
        foreach (var workout in upcomingWorkouts)
        {
            // Check if already synced
            var lastSync = _preferences.Get($"workout_sync_{workout.Id}", DateTime.MinValue);
            
            if (DateTime.UtcNow - lastSync > TimeSpan.FromDays(1))
            {
                await _offlineMedia.SyncWorkoutMedia(workout.Id);
                _preferences.Set($"workout_sync_{workout.Id}", DateTime.UtcNow);
            }
        }
        
        // Clean old media
        await CleanupOldMedia();
    }
    
    private async Task CleanupOldMedia()
    {
        var cacheSizeLimit = _preferences.Get("cache_size_limit_gb", 2.0);
        var currentCacheSize = await CalculateCacheSize();
        
        if (currentCacheSize > cacheSizeLimit)
        {
            // Remove media for workouts older than 30 days
            var oldMedia = await _database.GetCachedMediaOlderThanAsync(DateTime.UtcNow.AddDays(-30));
            
            foreach (var media in oldMedia)
            {
                if (File.Exists(media.LocalPath))
                {
                    File.Delete(media.LocalPath);
                }
                await _database.DeleteAsync(media);
            }
        }
    }
}
```

### 4. Media Display with Offline Support

```csharp
public class ExerciseMediaView : ContentView
{
    private readonly IOfflineMediaService _offlineMedia;
    private readonly IConnectivity _connectivity;
    
    public async Task LoadMedia(Exercise exercise)
    {
        // Try to load from cache first
        var cachedImage = await _offlineMedia.GetCachedMediaPathAsync(
            exercise.Id, 
            MediaType.Image
        );
        
        if (!string.IsNullOrEmpty(cachedImage) && File.Exists(cachedImage))
        {
            // Load from local cache
            ExerciseImage.Source = ImageSource.FromFile(cachedImage);
        }
        else if (_connectivity.NetworkAccess == NetworkAccess.Internet)
        {
            // Load from network
            ExerciseImage.Source = ImageSource.FromUri(new Uri(exercise.ImageUrl));
        }
        else
        {
            // Show placeholder
            ExerciseImage.Source = ImageSource.FromResource("placeholder.png");
        }
        
        // Similar logic for video
        await LoadVideo(exercise);
    }
    
    private async Task LoadVideo(Exercise exercise)
    {
        var cachedVideo = await _offlineMedia.GetCachedMediaPathAsync(
            exercise.Id, 
            MediaType.Video
        );
        
        if (!string.IsNullOrEmpty(cachedVideo) && File.Exists(cachedVideo))
        {
            VideoPlayer.Source = VideoSource.FromFile(cachedVideo);
            PlayButton.IsVisible = true;
        }
        else if (_connectivity.NetworkAccess == NetworkAccess.Internet)
        {
            VideoPlayer.Source = VideoSource.FromUri(exercise.VideoUrl);
            PlayButton.IsVisible = true;
        }
        else
        {
            // Show "Video not available offline" message
            VideoPlayer.IsVisible = false;
            OfflineMessage.IsVisible = true;
        }
    }
}
```

## Desktop/Web Implementation (Avalonia)

For desktop and web clients, media is streamed on-demand:

```csharp
public class DesktopMediaService
{
    public Task<string> GetMediaUrl(Exercise exercise, MediaType type, string variant = "original")
    {
        // Desktop/Web always use highest quality available
        if (type == MediaType.Image)
        {
            return Task.FromResult(exercise.ImageUrl); // Points to large/original
        }
        else
        {
            return Task.FromResult(exercise.VideoUrl); // Points to 1080p/original
        }
    }
}
```

## User Settings

Allow users to control offline sync behavior:

```csharp
public class MediaSyncSettings
{
    public SyncStrategy Strategy { get; set; } = SyncStrategy.WiFiOnly;
    public double CacheSizeLimitGB { get; set; } = 2.0;
    public int DaysToKeepMedia { get; set; } = 30;
    public bool DownloadVideos { get; set; } = true;
    public VideoQuality PreferredVideoQuality { get; set; } = VideoQuality.Auto;
    public bool ShowDataUsageWarnings { get; set; } = true;
}

public enum SyncStrategy
{
    WiFiOnly,
    WiFiAndCellular,
    Manual
}

public enum VideoQuality
{
    Auto,    // Based on device and storage
    Low,     // Always 720p
    High     // Always 1080p
}
```

## Storage Management UI

```xml
<!-- Settings Page -->
<StackLayout>
    <Label Text="Offline Storage" />
    
    <StackLayout Orientation="Horizontal">
        <Label Text="Cache Size:" />
        <Label Text="{Binding CurrentCacheSize, StringFormat='{0:F1} GB'}" />
        <Label Text="/" />
        <Label Text="{Binding CacheSizeLimit, StringFormat='{0:F1} GB'}" />
    </StackLayout>
    
    <Slider Value="{Binding CacheSizeLimit}" 
            Minimum="0.5" 
            Maximum="10" />
    
    <Label Text="Sync Strategy" />
    <Picker ItemsSource="{Binding SyncStrategies}"
            SelectedItem="{Binding SelectedStrategy}" />
    
    <Label Text="Video Quality" />
    <Picker ItemsSource="{Binding VideoQualities}"
            SelectedItem="{Binding SelectedVideoQuality}" />
    
    <Button Text="Clear Cache"
            Command="{Binding ClearCacheCommand}" />
    
    <Button Text="Sync Now"
            Command="{Binding SyncNowCommand}" />
</StackLayout>
```

## Best Practices

1. **Pre-sync Before Gym**
   - Remind users to sync while on WiFi
   - Show sync status in workout list
   - Indicate which workouts are available offline

2. **Storage Warnings**
   - Alert when device storage is low
   - Offer to clean old workout media
   - Show estimated storage needed before sync

3. **Network Awareness**
   - Respect user's data settings
   - Pause sync on cellular if requested
   - Resume when WiFi available

4. **Error Handling**
   - Gracefully handle missing media
   - Provide clear offline indicators
   - Allow manual retry of failed downloads

5. **Performance**
   - Download media in background
   - Use parallel downloads wisely
   - Implement progressive loading