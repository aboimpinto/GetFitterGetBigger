# FEAT-026 Workout Template UI - Postponed Features from API Implementation

This document captures all features that were postponed or marked as TODO during the API implementation of FEAT-026 (Workout Template Core). These items have direct implications for the UI implementation and must be considered during UI development.

## Overview

During the API implementation of FEAT-026, several features were intentionally postponed to maintain architectural integrity and avoid technical debt. These postponements fall into three main categories:

1. **Authentication & Authorization** - Waiting for auth infrastructure
2. **Domain Entity Caching** - Requires architectural foundation (FEAT-027)
3. **Advanced Business Logic** - Complex features deferred for initial release

## Postponed Features

### 1. Authentication & Authorization Context

**Status**: TODO - Hardcoded user IDs throughout
**Impact**: High - Affects all UI authorization logic

**Current State in API:**
- All controllers use hardcoded `userId = Guid.Parse("...")` 
- Authorization attributes are placeholders
- No actual user context extraction from tokens/claims

**UI Implications:**
- ❌ Cannot implement proper user-specific template filtering
- ❌ Cannot enforce ownership-based UI permissions
- ❌ Cannot show/hide UI elements based on user roles
- ⚠️ Must use mock user IDs for development
- ⚠️ UI authorization logic will need complete rewrite when auth is implemented

**Workaround for UI Development:**
```typescript
// Temporary mock auth context
const MOCK_USER_ID = "11111111-1111-1111-1111-111111111111";
const MOCK_USER_ROLE = "PT-Tier";
```

### 2. Equipment Aggregation Logic

**Status**: TODO - Returns empty list
**Impact**: Medium - Affects equipment display and filtering

**Current State in API:**
```csharp
// WorkoutTemplateService.cs
private List<string> AggregateRequiredEquipment(WorkoutTemplate template)
{
    // TODO: Implement equipment aggregation logic
    // Should aggregate all equipment from all exercises
    return new List<string>();
}
```

**UI Implications:**
- ❌ Cannot display required equipment for templates
- ❌ Cannot filter templates by available equipment
- ❌ Cannot show equipment warnings
- ⚠️ Equipment section should be hidden or show "Not available"

**UI Design Consideration:**
- Add placeholder UI for equipment section
- Design should accommodate future equipment list display
- Consider "Equipment information coming soon" message

### 3. Exercise Suggestion Algorithm

**Status**: TODO - Returns empty list
**Impact**: High - Core feature missing

**Current State in API:**
```csharp
// WorkoutTemplateService.cs
public async Task<ServiceResult<List<ExerciseSuggestionDto>>> GetExerciseSuggestionsAsync(...)
{
    // TODO: Implement exercise suggestion algorithm
    return ServiceResult<List<ExerciseSuggestionDto>>.Success(new List<ExerciseSuggestionDto>());
}
```

**UI Implications:**
- ❌ Cannot show exercise suggestions when building templates
- ❌ Cannot implement smart exercise recommendations
- ❌ No push/pull balance suggestions
- ❌ No warmup/cooldown association suggestions
- ⚠️ Must implement manual exercise search only

**UI Design Consideration:**
- Focus on manual exercise search/browse functionality
- Add placeholder for "Suggested exercises" section
- Design UI to easily integrate suggestions later

### 4. Domain Entity Caching

**Status**: Architectural decision - Deferred to FEAT-027
**Impact**: Low - Performance optimization only

**Current State in API:**
- No caching implemented for WorkoutTemplate entities
- Waiting for Domain Entity caching architecture (Tier 3)
- All requests hit database directly

**UI Implications:**
- ⚠️ Template lists may load slower
- ⚠️ No offline capability for templates
- ⚠️ Consider implementing client-side caching
- ✅ Functionality works correctly, just not optimized

**UI Optimization Strategy:**
- Implement aggressive client-side caching
- Use React Query or similar for request caching
- Add loading states for all template operations

### 5. Controller Unit Tests

**Status**: TODO - Deferred
**Impact**: None for UI - Testing only

**Current State in API:**
- Service layer has comprehensive tests (1,156 tests)
- Controller unit tests were not implemented
- Integration tests provide coverage

**UI Implications:**
- ✅ No impact on UI development
- ✅ API endpoints are tested via integration tests
- ⚠️ Some edge cases might not be covered

### 6. Workout State Transitions

**Status**: Partially implemented
**Impact**: Medium - Affects state management UI

**Current State in API:**
- Basic state transitions work (DRAFT ↔ PRODUCTION ↔ ARCHIVED)
- Complex validation rules not fully implemented
- Missing detailed error messages for invalid transitions

**UI Implications:**
- ⚠️ State transition errors may be generic
- ⚠️ Cannot show detailed reasons for blocked transitions
- ✅ Basic state changes work correctly
- ⚠️ UI should handle generic error messages gracefully

## UI Development Recommendations

### 1. Authentication Placeholder Strategy
```typescript
// services/auth.service.ts
export class AuthService {
  // TODO: Replace with real implementation when auth is ready
  getCurrentUserId(): string {
    return MOCK_USER_ID;
  }
  
  hasRole(role: string): boolean {
    // TODO: Check real user claims
    return role === MOCK_USER_ROLE;
  }
}
```

### 2. Feature Flags for Postponed Features
```typescript
// config/features.ts
export const FEATURE_FLAGS = {
  SHOW_EQUIPMENT: false,          // Enable when aggregation works
  SHOW_SUGGESTIONS: false,        // Enable when algorithm implemented
  ENABLE_AUTH: false,            // Enable when auth context ready
  SHOW_DETAILED_ERRORS: false    // Enable when validation improved
};
```

### 3. UI Components to Build with Placeholders

1. **Equipment Display Component**
   - Design the UI structure
   - Show "Equipment details coming soon"
   - Hide or disable equipment filters

2. **Exercise Suggestion Panel**
   - Create the UI layout
   - Show "Suggestions not available"
   - Focus on manual search UI

3. **User Context Display**
   - Show mock user info in dev mode
   - Add banner: "Using development user"
   - Prepare for real user data structure

### 4. Error Handling Strategy

Since some API error messages are generic, implement rich client-side error handling:

```typescript
// utils/error-handler.ts
export function enhanceErrorMessage(error: ApiError): string {
  switch (error.code) {
    case 'STATE_TRANSITION_FAILED':
      // Provide client-side context since API is generic
      return 'Cannot change workout state. Templates in production cannot be edited if they have execution logs.';
    default:
      return error.message;
  }
}
```

### 5. Development Priorities

**Phase 1 - Core Functionality** (Build these first)
- Template CRUD operations
- Exercise management within templates
- Set configuration management
- Basic state transitions

**Phase 2 - Enhanced Features** (Add when available)
- Equipment aggregation display
- Exercise suggestions
- Advanced error messages
- Performance optimizations

**Phase 3 - Post-Auth Implementation**
- User-specific template filtering
- Ownership-based permissions
- Role-based UI elements
- Audit trail display

## Testing Considerations

1. **Mock Data Requirements**
   - Create comprehensive mock data that includes empty equipment lists
   - Test UI with empty suggestion responses
   - Simulate different mock user contexts

2. **Integration Test Adjustments**
   - Test UI behavior when equipment list is empty
   - Test UI when suggestions return no results
   - Verify graceful handling of auth placeholders

3. **Future-Proofing Tests**
   - Write tests that can easily adapt when features are implemented
   - Use feature flags in tests
   - Document which tests need updates when features are ready

## Migration Plan

When postponed features are implemented in the API:

1. **Equipment Aggregation**
   - Remove "coming soon" messages
   - Enable equipment display components
   - Add equipment-based filtering

2. **Exercise Suggestions**
   - Enable suggestion panel
   - Remove manual-only messaging
   - Add suggestion-based workflows

3. **Authentication Context**
   - Remove all mock user code
   - Implement real token/claims extraction
   - Update all authorization checks
   - Full regression testing required

## Summary

The UI implementation should proceed with full functionality except for the postponed features. Use feature flags and placeholder UI components to prepare for future integration. Focus on delivering a complete user experience with manual workflows, then enhance with automated features as they become available in the API.

**Key Principle**: Build the UI structure for all features, but disable/hide functionality that depends on postponed API features. This allows for easier integration when those features are implemented.