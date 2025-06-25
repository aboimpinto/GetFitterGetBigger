# Tasks for Reference Tables Feature

This document outlines the detailed tasks required to implement the Reference Tables feature.

---

## Phase 1: Configuration & Data Layer

### Task 1.1: Configure API Base URL

**File:** `appsettings.Development.json`

Add the `ApiBaseUrl` key to the configuration file.

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "Kestrel": {
    "Certificates": {
      "Development": {
        "Password": null
      }
    }
  },
  "Authentication": {
    "Google": {
      "ClientId": "YOUR_GOOGLE_CLIENT_ID",
      "ClientSecret": "YOUR_GOOGLE_CLIENT_SECRET"
    },
    "Facebook": {
      "AppId": "YOUR_FACEBOOK_APP_ID",
      "AppSecret": "YOUR_FACEBOOK_APP_SECRET"
    }
  },
  "ApiBaseUrl": "http://localhost:5214"
}
```

### Task 1.2: Define Data Models

Create the following two model files.

**File:** `Models/Dtos/ReferenceDataDto.cs`

```csharp
namespace GetFitterGetBigger.Admin.Models.Dtos
{
    public class ReferenceDataDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
```

**File:** `Models/ReferenceTableModel.cs`

```csharp
namespace GetFitterGetBigger.Admin.Models
{
    public class ReferenceTableModel
    {
        public string Name { get; set; }
        public string EndPoint { get; set; }
        public string DisplayName { get; set; }
    }
}
```

### Task 1.3: Create Service Interface

**File:** `Services/IReferenceDataService.cs`

```csharp
using GetFitterGetBigger.Admin.Models.Dtos;

namespace GetFitterGetBigger.Admin.Services
{
    public interface IReferenceDataService
    {
        Task<IEnumerable<ReferenceDataDto>> GetBodyPartsAsync();
        Task<IEnumerable<ReferenceDataDto>> GetDifficultyLevelsAsync();
        Task<IEnumerable<ReferenceDataDto>> GetEquipmentAsync();
        Task<IEnumerable<ReferenceDataDto>> GetKineticChainTypesAsync();
        Task<IEnumerable<ReferenceDataDto>> GetMetricTypesAsync();
        Task<IEnumerable<ReferenceDataDto>> GetMovementPatternsAsync();
        Task<IEnumerable<ReferenceDataDto>> GetMuscleGroupsAsync();
        Task<IEnumerable<ReferenceDataDto>> GetMuscleRolesAsync();
    }
}
```

### Task 1.4: Create Service Implementation

**File:** `Services/ReferenceDataService.cs`

```csharp
using GetFitterGetBigger.Admin.Models.Dtos;
using Microsoft.Extensions.Caching.Memory;
using System.Net.Http.Json;

namespace GetFitterGetBigger.Admin.Services
{
    public class ReferenceDataService : IReferenceDataService
    {
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _cache;
        private readonly IConfiguration _configuration;
        private readonly string _apiBaseUrl;

        public ReferenceDataService(HttpClient httpClient, IMemoryCache cache, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _cache = cache;
            _configuration = configuration;
            _apiBaseUrl = _configuration["ApiBaseUrl"];
        }

        private async Task<IEnumerable<ReferenceDataDto>> GetDataAsync(string endpoint, string cacheKey)
        {
            if (!_cache.TryGetValue(cacheKey, out IEnumerable<ReferenceDataDto> cachedData))
            {
                var requestUrl = $"{_apiBaseUrl}{endpoint}";
                cachedData = await _httpClient.GetFromJsonAsync<IEnumerable<ReferenceDataDto>>(requestUrl);
                _cache.Set(cacheKey, cachedData, TimeSpan.FromHours(24));
            }
            return cachedData;
        }

        public Task<IEnumerable<ReferenceDataDto>> GetBodyPartsAsync() => GetDataAsync("/api/ReferenceTables/BodyParts", "BodyParts");
        public Task<IEnumerable<ReferenceDataDto>> GetDifficultyLevelsAsync() => GetDataAsync("/api/ReferenceTables/DifficultyLevels", "DifficultyLevels");
        public Task<IEnumerable<ReferenceDataDto>> GetEquipmentAsync() => GetDataAsync("/api/ReferenceTables/Equipment", "Equipment");
        public Task<IEnumerable<ReferenceDataDto>> GetKineticChainTypesAsync() => GetDataAsync("/api/ReferenceTables/KineticChainTypes", "KineticChainTypes");
        public Task<IEnumerable<ReferenceDataDto>> GetMetricTypesAsync() => GetDataAsync("/api/ReferenceTables/MetricTypes", "MetricTypes");
        public Task<IEnumerable<ReferenceDataDto>> GetMovementPatternsAsync() => GetDataAsync("/api/ReferenceTables/MovementPatterns", "MovementPatterns");
        public Task<IEnumerable<ReferenceDataDto>> GetMuscleGroupsAsync() => GetDataAsync("/api/ReferenceTables/MuscleGroups", "MuscleGroups");
        public Task<IEnumerable<ReferenceDataDto>> GetMuscleRolesAsync() => GetDataAsync("/api/ReferenceTables/MuscleRoles", "MuscleRoles");
    }
}
```

### Task 1.5: Register Services in `Program.cs`

**File:** `Program.cs`

Add the following lines to register the `IMemoryCache`, `HttpClient`, and the `IReferenceDataService`.

```csharp
// Add after builder.Services.AddScoped<RedirectToLoginHandler>();

builder.Services.AddMemoryCache();
builder.Services.AddHttpClient<GetFitterGetBigger.Admin.Services.IReferenceDataService, GetFitterGetBigger.Admin.Services.ReferenceDataService>();
```

---

## Phase 2: UI Implementation

### Task 2.1: Update Navigation Menu

**File:** `Components/Layout/NavMenu.razor`

Add a new navigation link for "Reference Tables".

```html
<div class="flex-grow">
    <ul class="flex flex-col py-4 space-y-1">
        <!-- ... existing links ... -->
        <li>
            <a href="referencetables" class="relative flex flex-row items-center h-11 focus:outline-none hover:bg-gray-50 text-gray-600 hover:text-gray-800 border-l-4 border-transparent hover:border-indigo-500 pr-6">
                <span class="inline-flex justify-center items-center ml-4">
                    <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24" xmlns="http://www.w3.org/2000/svg"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3 10h18M3 14h18m-9-4v8m-7 0h14a2 2 0 002-2V8a2 2 0 00-2-2H5a2 2 0 00-2 2v8a2 2 0 002 2z"></path></svg>
                </span>
                <span class="ml-2 text-sm tracking-wide truncate">Reference Tables</span>
            </a>
        </li>
    </ul>
</div>
```

### Task 2.2: Create Landing Page

**File:** `Components/Pages/ReferenceTables.razor`

```csharp
@page "/referencetables"
@using GetFitterGetBigger.Admin.Models
@using Microsoft.AspNetCore.Authorization
@attribute [Authorize]

<PageTitle>Reference Tables - GetFitterGetBigger Admin</PageTitle>

<div class="bg-white rounded-lg shadow-md p-6">
    <h2 class="text-2xl font-semibold text-gray-800 mb-4">Reference Tables</h2>
    <p class="text-gray-600 mb-6">
        Manage the core reference data used throughout the application.
    </p>

    <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
        @foreach (var table in ReferenceTablesList)
        {
            <div class="bg-gray-50 p-4 rounded-lg border border-gray-200 hover:shadow-lg transition-shadow">
                <h3 class="text-lg font-medium text-gray-800 mb-2">@table.DisplayName</h3>
                <a href="/referencetables/@table.Name" class="text-blue-600 hover:text-blue-800 font-medium">View Items →</a>
            </div>
        }
    </div>
</div>

@code {
    private List<ReferenceTableModel> ReferenceTablesList = new List<ReferenceTableModel>
    {
        new ReferenceTableModel { Name = "BodyParts", DisplayName = "Body Parts", EndPoint = "/api/ReferenceTables/BodyParts" },
        new ReferenceTableModel { Name = "DifficultyLevels", DisplayName = "Difficulty Levels", EndPoint = "/api/ReferenceTables/DifficultyLevels" },
        new ReferenceTableModel { Name = "Equipment", DisplayName = "Equipment", EndPoint = "/api/ReferenceTables/Equipment" },
        new ReferenceTableModel { Name = "KineticChainTypes", DisplayName = "Kinetic Chain Types", EndPoint = "/api/ReferenceTables/KineticChainTypes" },
        new ReferenceTableModel { Name = "MetricTypes", DisplayName = "Metric Types", EndPoint = "/api/ReferenceTables/MetricTypes" },
        new ReferenceTableModel { Name = "MovementPatterns", DisplayName = "Movement Patterns", EndPoint = "/api/ReferenceTables/MovementPatterns" },
        new ReferenceTableModel { Name = "MuscleGroups", DisplayName = "Muscle Groups", EndPoint = "/api/ReferenceTables/MuscleGroups" },
        new ReferenceTableModel { Name = "MuscleRoles", DisplayName = "Muscle Roles", EndPoint = "/api/ReferenceTables/MuscleRoles" }
    };
}
```

### Task 2.3: Create Detail Page

**File:** `Components/Pages/ReferenceTableDetail.razor`

```csharp
@page "/referencetables/{TableName}"
@using GetFitterGetBigger.Admin.Models.Dtos
@using GetFitterGetBigger.Admin.Services
@using Microsoft.AspNetCore.Authorization
@inject IReferenceDataService ReferenceDataService
@attribute [Authorize]

<PageTitle>@TableName - GetFitterGetBigger Admin</PageTitle>

<div class="bg-white rounded-lg shadow-md p-6">
    <div class="flex justify-between items-center mb-4">
        <h2 class="text-2xl font-semibold text-gray-800">@TableName</h2>
        <button class="bg-gray-300 text-gray-500 font-bold py-2 px-4 rounded cursor-not-allowed" disabled>
            Add New
        </button>
    </div>

    @if (items == null)
    {
        <p class="text-gray-600"><em>Loading...</em></p>
    }
    else
    {
        <div class="overflow-x-auto">
            <table class="min-w-full bg-white">
                <thead class="bg-gray-100">
                    <tr>
                        <th class="py-3 px-6 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Id</th>
                        <th class="py-3 px-6 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Name</th>
                        <th class="py-3 px-6 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Actions</th>
                    </tr>
                </thead>
                <tbody class="text-gray-700">
                    @foreach (var item in items)
                    {
                        <tr class="border-b border-gray-200 hover:bg-gray-50">
                            <td class="py-4 px-6">@item.Id</td>
                            <td class="py-4 px-6">@item.Name</td>
                            <td class="py-4 px-6">
                                <!-- Placeholder for future action buttons -->
                            </td>
                        </tr>
                    }
                </tbody>
            </table>
        </div>
    }
</div>

@code {
    [Parameter]
    public string TableName { get; set; }

    private IEnumerable<ReferenceDataDto> items;

    protected override async Task OnInitializedAsync()
    {
        items = TableName switch
        {
            "BodyParts" => await ReferenceDataService.GetBodyPartsAsync(),
            "DifficultyLevels" => await ReferenceDataService.GetDifficultyLevelsAsync(),
            "Equipment" => await ReferenceDataService.GetEquipmentAsync(),
            "KineticChainTypes" => await ReferenceDataService.GetKineticChainTypesAsync(),
            "MetricTypes" => await ReferenceDataService.GetMetricTypesAsync(),
            "MovementPatterns" => await ReferenceDataService.GetMovementPatternsAsync(),
            "MuscleGroups" => await ReferenceDataService.GetMuscleGroupsAsync(),
            "MuscleRoles" => await ReferenceDataService.GetMuscleRolesAsync(),
            _ => new List<ReferenceDataDto>()
        };
    }
}
