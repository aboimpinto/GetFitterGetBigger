# Four-Way Exercise Linking System Implementation Tasks

## Feature Branch: `feature/exercise-link-four-way-enhancements`
## Estimated Total Time: 32h 45m (Blazor implementation)
## Actual Total Time: [To be calculated at completion]

## Pre-Implementation Checklist

### Required Reading & Analysis
- [ ] Review UX Research findings from `four-way-linking-ux-research.md`
- [ ] Study wireframe specifications from `four-way-linking-wireframes.md`
- [ ] Follow implementation guidance from `four-way-linking-implementation-guide.md`
- [ ] Reference UI standards: `/memory-bank/UI_LIST_PAGE_DESIGN_STANDARDS.md`
- [ ] Check Blazor patterns: `/memory-bank/patterns/state-management-patterns.md`
- [ ] Review testing guide: `/memory-bank/COMPREHENSIVE-TESTING-GUIDE.md`

### Baseline Health Check Report
**Date/Time**: [TO BE COMPLETED WHEN STARTING]
**Branch**: feature/exercise-link-four-way-enhancements

### Build Status
- **Build Result**: [TO BE COMPLETED]
- **Warning Count**: [TO BE COMPLETED]
- **Warning Details**: [TO BE COMPLETED]

### Test Status
- **Total Tests**: [TO BE COMPLETED]
- **Passed**: [TO BE COMPLETED]
- **Failed**: [TO BE COMPLETED - MUST be 0 to proceed]
- **Skipped/Ignored**: [TO BE COMPLETED]
- **Test Execution Time**: [TO BE COMPLETED]

### Decision to Proceed
- [ ] All tests passing
- [ ] Build successful
- [ ] No code analysis errors
- [ ] Warnings documented and approved

**Approval to Proceed**: [TO BE COMPLETED]

---

## Phase 1: Planning & Analysis - Estimated: 4h

### Task 1.1: Study existing Blazor components and patterns
`[Pending]` (Est: 2h)

**Objective:**
- Search for similar Blazor components in the codebase
- Identify UI patterns and state management approaches to follow
- Document findings with specific component references
- Note any lessons learned from completed features

**Implementation Steps:**
- Use Grep/Glob tools to find similar .razor and .razor.cs files
- Analyze existing patterns in Components/, Services/, and State/
- Review `/memory-bank/features/3-COMPLETED/` for similar Blazor features
- Check UI standards in `UI_LIST_PAGE_DESIGN_STANDARDS.md`
- Review state management in `patterns/state-management-patterns.md`

**Deliverables:**
- List of similar Blazor components with file paths
- UI patterns to follow (container layout, card styling, etc.)
- State service patterns that can be adapted
- Component communication patterns identified
- Critical warnings from lessons learned

**Key References:**
- Existing: `/GetFitterGetBigger.Admin/Components/Pages/Exercises/ExerciseLinks/ExerciseLinkManager.razor`
- Existing: `/GetFitterGetBigger.Admin/Components/Pages/Exercises/ExerciseLinks/LinkedExercisesList.razor`
- Existing: `/GetFitterGetBigger.Admin/Components/Pages/Exercises/ExerciseLinks/ExerciseLinkCard.razor`
- Pattern: `/GetFitterGetBigger.Admin/Services/IExerciseLinkStateService.cs`

### Task 1.2: Analyze exercise type contexts and relationships
`[Pending]` (Est: 1h 30m)

**Objective:**
- Map exercise type combinations to UI contexts
- Understand bidirectional relationship rules
- Define context switching requirements for multi-type exercises
- Plan alternative exercise validation logic

**Implementation Steps:**
- Study UX research personas and workflow patterns
- Map wireframe specifications to technical requirements
- Analyze existing ExerciseDto structure for type handling
- Define validation rules for each relationship type
- Plan progressive disclosure strategy

**Deliverables:**
- Context mapping documentation
- Validation rule specifications
- Multi-type exercise handling plan
- UI progressive disclosure strategy

### Task 1.3: Plan component architecture and data flow
`[Pending]` (Est: 30m)

**Objective:**
- Design component hierarchy for four-way linking
- Plan state management extensions
- Define component communication patterns
- Document component responsibilities

**Implementation Steps:**
- Map wireframes to Blazor component structure
- Plan FourWayExerciseLinkManager component design
- Define context-specific view components
- Plan state service extensions for alternative links

**Deliverables:**
- Component hierarchy diagram
- State management extension plan
- Data flow documentation
- Component responsibility matrix

---

## CHECKPOINT: Phase 1 Complete - Planning & Analysis
`[Pending]` - Date: [TO BE COMPLETED]

Build Report:
- Admin Project: [STATUS] [X errors, Y warnings]
- Test Project (bUnit): [STATUS] [X errors, Y warnings]

Code Review: `/memory-bank/features/2-IN_PROGRESS/FEAT-022-four-way-linking/code-reviews/Phase_1_Planning/Code-Review-Phase-1-Planning-YYYY-MM-DD-HH-MM-[STATUS].md` - [[STATUS]]

Git Commit: `[COMMIT_HASH]` - [commit message summary]

Status: [STATUS] Phase 1

Notes: 
- Codebase analysis complete with component references
- UI patterns and state management approaches identified
- Ready for Phase 2: Models & State Management

---

## Phase 2: Models & State Management - Estimated: 6h

### Task 2.1: Enhance ExerciseLinkDto for alternative relationships
`[Pending]` (Est: 45m)

**Objective:**
- Extend existing ExerciseLinkDto to support ALTERNATIVE link type
- Ensure compatibility with existing WARMUP and COOLDOWN types
- Add properties needed for context-aware UI display

**Implementation Steps:**
- Review existing ExerciseLinkDto in `/GetFitterGetBigger.Admin/Models/Dtos/ExerciseLinkDto.cs`
- Ensure LinkType supports "ALTERNATIVE" value
- Verify TargetExercise property includes all needed details for UI
- Add any missing properties for alternative exercise display

**Critical Requirements:**
- Must maintain backward compatibility with existing warmup/cooldown links
- Alternative links do not have DisplayOrder (not sequenced)
- TargetExercise must include ExerciseTypes for validation

**Reference Existing:**
- `Models/Dtos/ExerciseLinkDto.cs`
- `Models/Dtos/ExerciseLinkType.cs`
- `Models/Dtos/CreateExerciseLinkDto.cs`

### Task 2.2: Create context-specific DTOs for multi-type exercises
`[Pending]` (Est: 30m)

**Objective:**
- Create DTOs to support context switching for multi-type exercises
- Enable clean separation between workout, warmup, and cooldown contexts

**Implementation Steps:**
- Create ExerciseContextDto to represent current context
- Add context-related properties to support UI state
- Define ExerciseRelationshipDto for grouped relationships

**Deliverables:**
- ExerciseContextDto with context type and available relationships
- Context enumeration (Workout, Warmup, Cooldown)
- Relationship grouping structure for multi-context display

### Task 2.3: Write bUnit tests for enhanced DTOs
`[Pending]` (Est: 45m)

**Objective:**
- Test DTO serialization/deserialization for new properties
- Verify context-switching logic and validation
- Ensure backward compatibility

**Testing Focus:**
- Alternative link DTO validation
- Multi-context exercise scenarios
- Edge cases with complex exercise types
- Serialization of new properties

**Location:** `Tests/Models/ExerciseLinkDtoTests.cs`

### Task 2.4: Extend IExerciseLinkStateService interface
`[Pending]` (Est: 1h 30m)

**Objective:**
- Add support for alternative links in state service interface
- Add context management for multi-type exercises
- Define methods for bidirectional relationship handling

**Implementation Steps:**
- Extend existing IExerciseLinkStateService
- Add AlternativeLinks and WorkoutLinks properties (reverse relationships)
- Add context management: ActiveContext, SwitchContextAsync
- Add validation methods for alternative link compatibility
- Add bulk operation methods for suggested alternatives

**New Interface Methods:**
```csharp
// Alternative relationship properties
IEnumerable<ExerciseLinkDto> AlternativeLinks { get; }
IEnumerable<ExerciseLinkDto> WorkoutLinks { get; } // Reverse relationships
int AlternativeLinkCount { get; }

// Context management for multi-type exercises
string ActiveContext { get; set; } // "Workout", "Warmup", "Cooldown"
bool HasMultipleContexts { get; }
IEnumerable<string> AvailableContexts { get; }

// New operations
Task CreateBidirectionalLinkAsync(CreateExerciseLinkDto dto);
Task DeleteBidirectionalLinkAsync(string linkId);
Task SwitchContextAsync(string contextType);
Task LoadAlternativeLinksAsync();
Task LoadWorkoutLinksAsync(); // For reverse relationships

// Validation
ValidationResult ValidateLinkCompatibility(Exercise source, Exercise target, LinkType type);
```

**Reference:** Existing `Services/IExerciseLinkStateService.cs`

### Task 2.5: Implement enhanced ExerciseLinkStateService
`[Pending]` (Est: 2h)

**Objective:**
- Implement the extended interface with alternative link support
- Add context switching logic for multi-type exercises
- Implement bidirectional relationship management
- Add proper error handling and state management

**Implementation Focus:**
- Alternative links collection management
- Context switching preserves current state
- Bidirectional link creation/deletion
- Optimistic UI updates with rollback capability
- Cache invalidation for both source and target exercises

**Critical Blazor Patterns:**
- Use `StateHasChanged()` after context switches
- Implement `IDisposable` for event cleanup
- Follow error message persistence pattern from existing service
- Use optimistic updates with rollback on API failures

**Location:** `Services/ExerciseLinkStateService.cs`

### Task 2.6: Write comprehensive bUnit tests for enhanced state service
`[Pending]` (Est: 1h 30m)

**Objective:**
- Test all new alternative link functionality
- Test context switching for multi-type exercises
- Test bidirectional relationship creation/deletion
- Test validation logic and error scenarios

**Testing Focus:**
- Alternative link CRUD operations
- Context switching preserves state
- Bidirectional relationship management
- Error message persistence during rollbacks
- Optimistic updates with proper rollback
- Validation for alternative link compatibility

**Location:** `Tests/Services/ExerciseLinkStateServiceTests.cs`

---

## CHECKPOINT: Phase 2 Complete - Models & State Management
`[Pending]` - Date: [TO BE COMPLETED]

Build Report:
- Admin Project: [STATUS] [X errors, Y warnings]
- Test Project (bUnit): [STATUS] [X errors, Y warnings]

Code Review: `/memory-bank/features/2-IN_PROGRESS/FEAT-022-four-way-linking/code-reviews/Phase_2_State/Code-Review-Phase-2-State-YYYY-MM-DD-HH-MM-[STATUS].md` - [[STATUS]]

Git Commit: `[COMMIT_HASH]` - [commit message summary]

Status: [STATUS] Phase 2

Notes: 
- State service extended to support alternative relationships
- Context switching logic implemented for multi-type exercises
- Ready for Phase 3: Base Components

---

## Phase 3: Base Components & Services - Estimated: 8h

### Task 3.1: Create AlternativeExerciseCard component
`[Pending]` (Est: 1h 30m)

**Component Implementation (1h):**
- Create `Components/ExerciseLinks/AlternativeExerciseCard.razor`
- Create code-behind `AlternativeExerciseCard.razor.cs`
- Different from ExerciseLinkCard - NO reordering capabilities
- Purple theme styling: `border-purple-200 bg-purple-50`
- Display exercise details without sequence numbers

**Key Differences from ExerciseLinkCard:**
- No move up/down buttons (alternatives are unordered)
- Purple color theme for visual distinction
- "View Exercise" link instead of position info
- Different data-testid: `alternative-exercise-card`

**UI Requirements:**
```razor
<div class="bg-white border border-purple-200 p-4 rounded-lg" data-testid="alternative-exercise-card">
    <div class="flex items-start justify-between">
        <div class="flex-1">
            <h4 class="font-medium text-gray-900">@Link.TargetExercise.Name</h4>
            <!-- Exercise details: difficulty, equipment, muscle groups -->
        </div>
        <div class="flex-shrink-0 space-x-2">
            <button @onclick="() => NavigateToExercise(Link.TargetExerciseId)">View Exercise</button>
            <button @onclick="RemoveLink">Remove</button>
        </div>
    </div>
</div>
```

**bUnit Tests (30m):**
- Test component rendering with different exercise data
- Test remove functionality
- Test navigation to exercise detail
- Mock navigation service
- Verify purple styling classes applied

**Accessibility:**
- ARIA labels for all interactive elements
- Proper keyboard navigation
- Screen reader compatibility

**Reference Pattern:** `Components/Pages/Exercises/ExerciseLinks/ExerciseLinkCard.razor`

### Task 3.2: Create ExerciseContextSelector component
`[Pending]` (Est: 1h)

**Component Implementation (45m):**
- Create `Components/ExerciseLinks/ExerciseContextSelector.razor`
- Tab interface for multi-type exercises (as shown in wireframes)
- Manages active context state
- Follows accessibility standards for tab navigation

**UI Implementation:**
```razor
<div role="tablist" aria-label="Exercise contexts" class="mb-4">
    @foreach (var context in Contexts)
    {
        <button role="tab" 
                aria-selected="@(context == ActiveContext)"
                aria-controls="@GetPanelId(context)"
                class="@GetTabClasses(context)"
                @onclick="() => SwitchContext(context)">
            @GetContextLabel(context)
        </button>
    }
</div>
```

**Context Labels:**
- "As Workout Exercise" - shows warmups, cooldowns, alternatives
- "As Warmup Exercise" - shows workouts using this, alternative warmups
- "As Cooldown Exercise" - shows workouts using this, alternative cooldowns

**bUnit Tests (15m):**
- Test tab rendering for multi-context exercises
- Test context switching and ARIA attributes
- Test active/inactive styling
- Verify OnContextChange callback

**Reference Pattern:** Standard tab component patterns in codebase

### Task 3.3: Create context-specific view components
`[Pending]` (Est: 2h 30m)

**WorkoutContextView.razor (45m):**
- Shows warmups, cooldowns, and alternative workouts sections
- Uses existing warmup/cooldown patterns from LinkedExercisesList
- Adds new alternative workouts section

**WarmupContextView.razor (45m):**
- Shows "Workouts using this warmup" (read-only, reverse links)
- Shows alternative warmups section (editable)
- Orange theme throughout (bg-orange-50)

**CooldownContextView.razor (45m):**
- Shows "Workouts using this cooldown" (read-only, reverse links)
- Shows alternative cooldowns section (editable)
- Blue theme throughout (bg-blue-50)

**Shared Patterns (15m):**
- All views follow same section structure
- Consistent empty states and loading indicators
- Proper ARIA labeling and accessibility

**Key UI Patterns:**
- Section headers: `px-4 sm:px-6 py-3 sm:py-4 border-b border-gray-200`
- Content areas: `p-4 sm:p-6`
- Empty states: Center-aligned with appropriate icons
- Color themes: orange for warmup, blue for cooldown, purple for alternatives

**Reference:** `Components/Pages/Exercises/ExerciseLinks/LinkedExercisesList.razor`

### Task 3.4: Write bUnit tests for context view components
`[Pending]` (Est: 1h 30m)

**Test Coverage:**
- WorkoutContextView: Test all three sections render correctly
- WarmupContextView: Test read-only workouts and editable alternatives
- CooldownContextView: Test read-only workouts and editable alternatives
- Verify proper theming and styling
- Test empty states and loading indicators

**Testing Patterns:**
- Mock IExerciseLinkStateService
- Use data-testid selectors
- Test event callbacks for add/remove operations
- Verify ARIA attributes and accessibility

**Location:** `Tests/Components/ExerciseLinks/ContextViewTests.cs`

### Task 3.5: Create SmartExerciseSelector modal enhancement
`[Pending]` (Est: 1h 30m)

**Enhancement Implementation (1h):**
- Extend existing AddExerciseLinkModal.razor
- Add context-aware filtering for alternative exercises
- Show compatibility indicators for alternative links
- Add type badges and compatibility scores

**New Features:**
- Filter exercises by compatible types for alternatives
- Show "Already linked" indicators
- Display compatibility scores (muscle group overlap)
- Context-specific help text and validation

**Enhanced Modal UI:**
```razor
<div class="modal-content max-w-4xl">
    @if (LinkType == "Alternative")
    {
        <div class="mb-4 p-4 bg-purple-50 rounded-md">
            <p class="text-sm text-purple-800">
                💡 Showing exercises compatible with @SourceExercise.Name
            </p>
            <div class="mt-2 flex flex-wrap gap-1">
                @foreach (var type in SourceExercise.ExerciseTypes)
                {
                    <span class="px-2 py-1 bg-purple-100 text-purple-700 rounded text-xs">
                        @type.Value
                    </span>
                }
            </div>
        </div>
    }
    <!-- Enhanced search and filter grid -->
</div>
```

**bUnit Tests (30m):**
- Test context-aware filtering
- Test compatibility scoring display
- Test alternative exercise validation
- Verify modal accessibility

**Reference:** `Components/Pages/Exercises/ExerciseLinks/AddExerciseLinkModal.razor`

---

## CHECKPOINT: Phase 3 Complete - Base Components
`[Pending]` - Date: [TO BE COMPLETED]

Build Report:
- Admin Project: [STATUS] [X errors, Y warnings]
- Test Project (bUnit): [STATUS] [X errors, Y warnings]

Code Review: `/memory-bank/features/2-IN_PROGRESS/FEAT-022-four-way-linking/code-reviews/Phase_3_Components/Code-Review-Phase-3-Components-YYYY-MM-DD-HH-MM-[STATUS].md` - [[STATUS]]

Git Commit: `[COMMIT_HASH]` - [commit message summary]

Status: [STATUS] Phase 3

Notes: 
- Alternative exercise card component with purple styling
- Context selector for multi-type exercises with accessibility
- Context-specific views with proper theming
- Enhanced modal with compatibility indicators
- Ready for Phase 4: Feature Components

---

## Phase 4: Feature Components - Estimated: 8h 30m

### Task 4.1: Create FourWayExerciseLinkManager component
`[Pending]` (Est: 2h 30m)

**Component Implementation (2h):**
- Create `Components/ExerciseLinks/FourWayExerciseLinkManager.razor`
- Replaces existing ExerciseLinkManager (lines 236-240 in ExerciseDetail.razor)
- Context detection based on exercise types
- Manages active context for multi-type exercises
- Coordinates sub-components for each link type

**Core Logic:**
```csharp
private bool ShowComponent => Exercise.ExerciseTypes?.Any() == true; // Show for ANY type now
private bool HasMultipleContexts => Exercise.ExerciseTypes?.Count() > 1;

private IEnumerable<string> GetExerciseContexts()
{
    var contexts = new List<string>();
    var types = Exercise.ExerciseTypes?.Select(t => t.Value) ?? Enumerable.Empty<string>();
    
    if (types.Contains("Workout")) contexts.Add("Workout");
    if (types.Contains("Warmup")) contexts.Add("Warmup");
    if (types.Contains("Cooldown")) contexts.Add("Cooldown");
    
    return contexts;
}
```

**UI Structure:**
- Header with context indicator
- Context selector (if multiple contexts)
- Dynamic content area showing context-specific views
- Follow UI standards: `bg-white shadow-lg rounded-lg overflow-hidden`

**State Management:**
- Initialize state service with exercise context
- Handle context switching
- Coordinate between multiple relationship types
- Manage loading states across all sections

**bUnit Tests (30m):**
- Test context detection logic
- Test multi-type exercise handling
- Test component rendering for different exercise types
- Test state service integration

**Critical Warnings:**
- Ensure proper disposal of event subscriptions!
- Use `@implements IDisposable`
- Call `StateHasChanged()` after context switches

**Reference:** Existing `Components/Pages/Exercises/ExerciseLinks/ExerciseLinkManager.razor`

### Task 4.2: Implement alternative exercise validation service
`[Pending]` (Est: 1h 30m)

**Service Implementation (1h):**
- Create `Services/AlternativeExerciseLinkValidationService.cs`
- Implement type compatibility checking
- Add exercise compatibility scoring
- Validate bidirectional relationship creation

**Validation Rules:**
```csharp
public class AlternativeExerciseLinkValidation
{
    public static ValidationResult ValidateAlternativeLink(ExerciseDto source, ExerciseDto target)
    {
        // Rule: Cannot self-reference
        if (source.Id == target.Id)
            return ValidationResult.Error("An exercise cannot be an alternative to itself");
            
        // Rule: Must share at least one exercise type
        var sourceTypes = source.ExerciseTypes.Select(t => t.Value);
        var targetTypes = target.ExerciseTypes.Select(t => t.Value);
        
        if (!sourceTypes.Intersect(targetTypes).Any())
            return ValidationResult.Error("Alternative exercises must share at least one exercise type");
            
        return ValidationResult.Success();
    }
}
```

**bUnit Tests (30m):**
- Test type compatibility validation
- Test self-reference prevention
- Test duplicate link detection
- Test edge cases with complex exercise types

**Location:** `Services/AlternativeExerciseLinkValidationService.cs`

### Task 4.3: Create bidirectional relationship handling
`[Pending]` (Est: 1h 45m)

**API Integration (1h 15m):**
- Implement bidirectional link creation
- Handle reverse link display for warmup/cooldown contexts
- Add proper error handling for bidirectional operations
- Implement cache invalidation for both source and target

**Key Methods:**
```csharp
public async Task CreateBidirectionalLinkAsync(CreateExerciseLinkDto dto)
{
    try
    {
        IsProcessing = true;
        ClearMessages();
        
        // Optimistic update
        var tempLink = CreateTemporaryLink(dto);
        AddLinkToContext(tempLink);
        NotifyStateChanged();
        
        // API call creates bidirectional automatically
        var result = await _exerciseService.CreateExerciseLinkAsync(dto);
        
        // Replace temp with actual
        ReplaceTempLink(tempLink, result);
    }
    catch (Exception ex)
    {
        ErrorMessage = GetUserFriendlyErrorMessage(ex);
        await LoadLinksAsync(preserveErrorMessage: true);
    }
    finally
    {
        IsProcessing = false;
        NotifyStateChanged();
    }
}
```

**bUnit Tests (30m):**
- Test bidirectional link creation
- Test optimistic updates with rollback
- Test cache invalidation
- Test error scenarios

### Task 4.4: Update ExerciseDetail.razor integration
`[Pending]` (Est: 45m)

**Integration Implementation (30m):**
- Replace lines 234-240 in ExerciseDetail.razor
- Remove restriction to only "Workout" type exercises
- Update component reference to FourWayExerciseLinkManager
- Ensure proper parameter passing

**Before (lines 234-240):**
```razor
@if (exercise.ExerciseTypes.Any(t => t.Value == "Workout"))
{
    <ExerciseLinkManager Exercise="@exercise"
                       StateService="@LinkStateService"
                       ExerciseService="@ExerciseService"
                       ExerciseTypes="@StateService.ExerciseTypes" />
}
```

**After:**
```razor
<FourWayExerciseLinkManager Exercise="@exercise"
                           StateService="@LinkStateService"
                           ExerciseService="@ExerciseService"
                           ExerciseTypes="@StateService.ExerciseTypes" />
```

**bUnit Tests (15m):**
- Test component renders for all exercise types
- Test proper parameter passing
- Verify no restriction based on exercise type

### Task 4.5: Implement REST exercise restriction handling
`[Pending]` (Est: 30m)

**Implementation:**
- Add special case for REST exercises
- Display informational message: "REST exercises cannot have relationships"
- Hide all linking functionality for REST type

**UI Implementation:**
```razor
@if (Exercise.ExerciseTypes.Any(t => t.Value == "REST"))
{
    <div class="bg-gray-50 border border-gray-200 rounded-lg p-6 text-center">
        <svg class="mx-auto h-12 w-12 text-gray-400"><!-- Rest icon --></svg>
        <h3 class="mt-2 text-sm font-medium text-gray-900">REST Exercise</h3>
        <p class="mt-1 text-sm text-gray-500">REST exercises cannot have relationships with other exercises.</p>
    </div>
}
else
{
    <!-- Show normal four-way linking interface -->
}
```

**Testing:** Verify REST exercises show message instead of linking interface

---

## CHECKPOINT: Phase 4 Complete - Feature Components
`[Pending]` - Date: [TO BE COMPLETED]

Build Report:
- Admin Project: [STATUS] [X errors, Y warnings]
- Test Project (bUnit): [STATUS] [X errors, Y warnings]

Code Review: `/memory-bank/features/2-IN_PROGRESS/FEAT-022-four-way-linking/code-reviews/Phase_4_Features/Code-Review-Phase-4-Features-YYYY-MM-DD-HH-MM-[STATUS].md` - [[STATUS]]

Git Commit: `[COMMIT_HASH]` - [commit message summary]

Status: [STATUS] Phase 4

Notes: 
- Four-way linking manager implemented with context awareness
- Alternative exercise components with purple styling
- Bidirectional relationship handling complete
- REST exercise restriction properly handled
- Ready for Phase 5: API Integration

---

## Phase 5: API Integration - Estimated: 4h 15m

### Task 5.1: Integrate FEAT-030 API endpoints
`[Pending]` (Est: 1h 30m)

**API Endpoint Integration:**
- POST `/api/exercises/{exerciseId}/links` - Create bidirectional links
- GET `/api/exercises/{exerciseId}/links?linkType=ALTERNATIVE&includeReverse=true`
- DELETE `/api/exercises/{exerciseId}/links/{linkId}?deleteReverse=true`
- PUT `/api/exercises/{exerciseId}/links/{linkId}/reorder`

**CreateExerciseLinkDto Updates:**
```csharp
public class CreateExerciseLinkDto
{
    public string TargetExerciseId { get; set; }
    public ExerciseLinkType LinkType { get; set; } // Now includes ALTERNATIVE
    // Note: displayOrder calculated server-side for WARMUP/COOLDOWN only
}
```

**Service Integration:**
- Update ExerciseService.cs with new endpoints
- Add error handling for API-specific errors
- Implement proper timeout and retry logic

### Task 5.2: Implement caching strategy for alternative links
`[Pending]` (Est: 1h)

**Caching Implementation:**
- Cache exercise relationships for 15 minutes
- Invalidate both source and target caches on changes
- Prefetch related exercises when opening detail page
- Use optimistic UI updates with rollback capability

**Performance Optimizations:**
- Lazy load reverse relationships (workout links for warmup/cooldown)
- Virtual scrolling for large alternative lists
- Progressive loading: core relationships first, then alternatives

### Task 5.3: Add API error handling and user feedback
`[Pending]` (Est: 1h)

**Error Handling:**
- Map API errors to user-friendly messages
- Handle network timeouts gracefully
- Provide retry mechanisms for transient errors
- Show appropriate loading states

**User-Friendly Error Messages:**
- "Alternative exercises must share at least one exercise type"
- "This relationship already exists"
- "Maximum of 10 warmup exercises reached"
- "Network error - please check your connection and try again"

### Task 5.4: Write API integration tests
`[Pending]` (Est: 45m)

**Integration Testing:**
- Test all new API endpoint calls
- Test bidirectional relationship creation
- Test error handling and rollback scenarios
- Mock API responses for different scenarios

**Location:** `Tests/Services/ExerciseServiceIntegrationTests.cs`

---

## CHECKPOINT: Phase 5 Complete - API Integration
`[Pending]` - Date: [TO BE COMPLETED]

Build Report:
- Admin Project: [STATUS] [X errors, Y warnings]
- Test Project (bUnit): [STATUS] [X errors, Y warnings]

API Integration Report:
- FEAT-030 Endpoints: [STATUS] [All endpoints functional]
- Bidirectional Links: [STATUS] [Creation and deletion working]
- Error Handling: [STATUS] [User-friendly messages implemented]

Code Review: `/memory-bank/features/2-IN_PROGRESS/FEAT-022-four-way-linking/code-reviews/Phase_5_API/Code-Review-Phase-5-API-YYYY-MM-DD-HH-MM-[STATUS].md` - [[STATUS]]

Git Commit: `[COMMIT_HASH]` - [commit message summary]

Status: [STATUS] Phase 5

Notes: 
- All FEAT-030 API endpoints integrated successfully
- Bidirectional relationship creation working
- Caching strategy implemented with proper invalidation
- Ready for Phase 6: Testing & Polish

---

## Phase 6: Testing & Polish - Estimated: 6h

### Task 6.1: Comprehensive component integration testing
`[Pending]` (Est: 2h)

**End-to-End Component Testing:**
- Test complete four-way linking workflow
- Test context switching for multi-type exercises
- Test bidirectional relationship creation/deletion
- Test validation rule enforcement across all scenarios

**Test Scenarios:**
1. Single-type exercise (Workout only): Add warmups, cooldowns, alternatives
2. Multi-type exercise (Workout + Warmup): Context switching works
3. Alternative validation: Only compatible types can be linked
4. Maximum limits: Warmup/cooldown limited to 10, alternatives unlimited
5. Bidirectional deletion: Both directions removed when link deleted

**Location:** `Tests/Components/ExerciseLinks/FourWayLinkingIntegrationTests.cs`

### Task 6.2: Accessibility compliance testing
`[Pending]` (Est: 1h 30m)

**WCAG AA Compliance:**
- Full keyboard navigation support
- Screen reader compatibility
- Focus management in modals and context switches
- Color contrast verification (WCAG AA)
- ARIA labels and descriptions

**Accessibility Features:**
```razor
<!-- Live region for announcements -->
<div aria-live="polite" aria-atomic="true" class="sr-only">
    @StateService.ScreenReaderAnnouncement
</div>

<!-- Context tabs with proper ARIA -->
<div role="tablist" aria-label="Exercise relationship contexts">
    <button role="tab" 
            aria-selected="@IsActive"
            aria-controls="context-panel-@context">
        @contextLabel
    </button>
</div>
```

**Testing:** Automated accessibility testing with focus on keyboard navigation

### Task 6.3: Performance optimization for large datasets
`[Pending]` (Est: 1h)

**Optimizations:**
- Implement virtual scrolling for alternative lists with 100+ items
- Optimize component re-renders with ShouldRender()
- Add pagination for exercise search in modal
- Implement debouncing for search inputs

**Performance Targets:**
- Page loads in < 2 seconds with 100+ exercises
- Context switching in < 200ms
- Alternative list scrolling stays responsive with 500+ items

### Task 6.4: Manual testing preparation and user acceptance
`[Pending]` (Est: 1h 30m)

**Manual Testing Scenarios:**
1. **Context Switching Test:**
   - Create exercise with multiple types (e.g., Push-ups: Workout + Warmup)
   - Switch between contexts and verify UI changes correctly
   - Test relationship management in each context

2. **Alternative Linking Test:**
   - Add alternative exercises ensuring type compatibility
   - Try to add incompatible alternative (should fail validation)
   - Verify bidirectional creation (check reverse relationship)

3. **Maximum Limits Test:**
   - Add 10 warmup exercises to a workout
   - Verify 11th warmup cannot be added
   - Confirm alternatives have no limit

4. **Edge Cases:**
   - Test with REST exercises (should show message, no linking)
   - Test with single-type exercises
   - Test error scenarios (network failures, API errors)

**Deliverables:**
- Detailed manual testing guide
- Sample data setup instructions
- Expected behavior documentation
- Test result collection template

---

## CHECKPOINT: Phase 6 Complete - Testing & Polish
`[Pending]` - Date: [TO BE COMPLETED]

Build Report:
- Admin Project: [STATUS] [X errors, Y warnings]
- Test Project (bUnit): [STATUS] [X errors, Y warnings]

Testing Report:
- Total Tests: [X] (New: [Y] tests added)
- Component Coverage: [X]% line coverage
- Integration Tests: [STATUS] [All scenarios passing]
- Accessibility: [STATUS] [WCAG AA compliant]
- Performance: [STATUS] [< 2s load time achieved]

Code Review: `/memory-bank/features/2-IN_PROGRESS/FEAT-022-four-way-linking/code-reviews/Phase_6_Testing/Code-Review-Phase-6-Testing-YYYY-MM-DD-HH-MM-[STATUS].md` - [[STATUS]]

Git Commit: `[COMMIT_HASH]` - [commit message summary]

Status: [STATUS] Phase 6

Notes: 
- Comprehensive testing complete with high coverage
- WCAG AA accessibility compliance verified
- Performance optimizations implemented
- Manual testing guide prepared
- Ready for Phase 7: Documentation & Deployment

---

## Phase 7: Documentation & Deployment - Estimated: 1h 30m

### Task 7.1: Update component documentation
`[Pending]` (Est: 30m)

**Documentation Updates:**
- Update component usage examples
- Document context switching behavior
- Add alternative linking examples
- Update troubleshooting guide

### Task 7.2: Create feature quick reference
`[Pending]` (Est: 30m)

**Quick Reference Content:**
- Context-aware linking rules
- Alternative exercise requirements
- Maximum link limits
- Keyboard shortcuts and accessibility features

### Task 7.3: Deployment readiness check
`[Pending]` (Est: 30m)

**Deployment Checklist:**
- All environment variables configured
- Production build successful
- No development-only code deployed
- Feature flags configured (if applicable)

---

## CHECKPOINT: Phase 7 Complete - Documentation & Deployment
`[Pending]` - Date: [TO BE COMPLETED]

Build Report:
- Admin Project: [STATUS] [X errors, Y warnings]
- Production Build: [STATUS] [Successful deployment build]

Documentation:
- Component Documentation: [STATUS] [Updated with four-way linking examples]
- Quick Reference: [STATUS] [Created for PT users]
- Deployment Guide: [STATUS] [Updated with new component dependencies]

Code Review: `/memory-bank/features/2-IN_PROGRESS/FEAT-022-four-way-linking/code-reviews/Phase_7_Docs/Code-Review-Phase-7-Docs-YYYY-MM-DD-HH-MM-[STATUS].md` - [[STATUS]]

Git Commit: `[COMMIT_HASH]` - [commit message summary]

Status: [STATUS] Phase 7

Notes: 
- Documentation complete and user-friendly
- Deployment readiness verified
- Ready for manual testing and user acceptance

---

## Manual Testing & User Acceptance

### Task 8.1: Manual testing by user
`[ReadyForTesting]` (Est: 45m)

**Complete Testing Workflow:**

#### Pre-Testing Setup:
1. Start application: `dotnet run` from Admin project directory
2. Navigate to any exercise detail page
3. Ensure API FEAT-030 is running and accessible

#### Test Scenario 1: Single-Type Exercise Linking (15m)
**Exercise:** Find a "Workout" only exercise (e.g., "Barbell Squat")
1. **Verify context display**: Should show "Workout Exercise Context" header with emerald theme
2. **Test warmup linking:**
   - Click "Add Warmup" button
   - Modal should show only warmup-compatible exercises
   - Add 2-3 warmup exercises
   - Verify sequence numbers appear (1, 2, 3...)
   - Test reordering with move up/down arrows
3. **Test cooldown linking:**
   - Click "Add Cooldown" button
   - Add 1-2 cooldown exercises
   - Verify blue section theme
4. **Test alternative linking:**
   - Click "Add Alternative" in purple section
   - Modal should filter to only "Workout" type exercises
   - Add 2-3 alternative exercises
   - Verify no sequence numbers (alternatives unordered)
5. **Test deletion:**
   - Remove one link from each section
   - Confirm deletion dialogs appear
   - Verify links are removed from UI

#### Test Scenario 2: Multi-Type Exercise Context Switching (15m)
**Exercise:** Find a multi-type exercise (e.g., "Push-ups": Workout + Warmup)
1. **Verify context selector**: Should see tabs for "As Workout Exercise" and "As Warmup Exercise"
2. **Test "As Workout Exercise" context:**
   - Should see warmups, cooldowns, alternative workouts sections
   - Add relationships in each section
   - Verify emerald theme for context
3. **Test "As Warmup Exercise" context:**
   - Click "As Warmup Exercise" tab
   - Should see orange theme
   - Should see "Workouts using this warmup" (read-only, populated from reverse links)
   - Should see "Alternative Warmups" section (editable)
   - Add 1-2 alternative warmups
4. **Test context switching preservation:**
   - Switch back to "As Workout Exercise"
   - Verify previously added relationships still display
   - Switch contexts multiple times to ensure state preserved

#### Test Scenario 3: Alternative Exercise Validation (10m)
1. **Test type compatibility:**
   - Try to add "Warmup" type exercise as alternative to "Workout" type
   - Should show error: "Alternative exercises must share at least one exercise type"
2. **Test self-reference prevention:**
   - Try to add exercise as alternative to itself
   - Should show error and prevent addition
3. **Test duplicate prevention:**
   - Add an alternative exercise successfully
   - Try to add the same exercise again
   - Should show error about existing relationship

#### Test Scenario 4: REST Exercise Restriction (5m)
**Exercise:** Find a "REST" type exercise
1. **Verify restriction message:**
   - Should see: "REST exercises cannot have relationships"
   - Should NOT see any add buttons or linking sections
   - Should show appropriate icon and explanation

**Expected Results for All Scenarios:**
- ✅ All contexts display correctly with proper themes
- ✅ Context switching works smoothly for multi-type exercises
- ✅ Alternative linking respects type compatibility rules
- ✅ Maximum limits enforced (10 for warmups/cooldowns)
- ✅ Bidirectional relationships created and displayed
- ✅ REST exercises properly restricted
- ✅ Validation prevents invalid operations
- ✅ UI responsive and follows design standards

**If any test fails:**
1. Note the specific issue and steps to reproduce
2. Take screenshots if helpful
3. Report back with details for bug fixing

**Sign-off Required:** User must explicitly confirm all tests passed before feature completion.

---

## BOY SCOUT RULE: Code Quality Improvements

*During implementation, if you encounter existing code that could be improved (following the Boy Scout Rule), add tasks here:*

### Potential Improvements Found:
- [To be populated during implementation]

### Boy Scout Tasks Completed:
- [To be populated as improvements are made]

---

## Time Tracking Summary
- **Total Estimated Time:** 32h 45m
- **Total Actual Time:** [To be calculated from task durations]
- **AI Assistance Impact:** [% reduction in time]
- **Implementation Started:** [First task start time]
- **Implementation Completed:** [Last task finish time]

## Critical Success Factors

### Blazor-Specific Requirements:
1. **Component Lifecycle:** Proper OnInitializedAsync and disposal patterns
2. **State Management:** Use StateHasChanged() correctly and sparingly
3. **Parameter Binding:** Use @bind for two-way data binding where appropriate
4. **Event Handling:** EventCallback<T> for parent-child communication
5. **Memory Management:** Implement IDisposable for event subscriptions

### UI/UX Standards Compliance:
1. **Container Layout:** `max-w-7xl mx-auto px-4 sm:px-6 lg:px-8`
2. **Card Styling:** `bg-white rounded-lg shadow-md`
3. **Color Themes:** Orange (warmup), Blue (cooldown), Purple (alternative), Emerald (workout)
4. **Responsive Design:** Mobile-first with progressive enhancement
5. **Accessibility:** Full WCAG AA compliance

### Performance Targets:
1. **Page Load:** < 2 seconds with 100+ exercises
2. **Context Switch:** < 200ms response time
3. **Search Results:** < 500ms for exercise filtering
4. **Memory Usage:** No memory leaks from event subscriptions

### Testing Requirements:
1. **Component Tests:** >90% line coverage for new components
2. **Integration Tests:** All four-way linking workflows covered
3. **Accessibility Tests:** Keyboard navigation and screen reader compatibility
4. **Performance Tests:** Large dataset handling (500+ exercises)

## Dependencies & Prerequisites

### Required Features:
- ✅ **FEAT-030 (API)**: Four-Way Linking System API endpoints
- ✅ **FEAT-018**: Basic exercise linking foundation
- ✅ **Authentication**: PT-Tier access control

### Existing Components to Leverage:
- `ExerciseLinkCard.razor` - Pattern for warmup/cooldown cards
- `LinkedExercisesList.razor` - Section layout pattern
- `AddExerciseLinkModal.razor` - Base modal functionality
- `ExerciseLinkManager.razor` - State management integration

### UI Standards to Follow:
- Container layouts from `UI_LIST_PAGE_DESIGN_STANDARDS.md`
- State patterns from `patterns/state-management-patterns.md`
- Testing approaches from `COMPREHENSIVE-TESTING-GUIDE.md`

## Risk Mitigation

| Risk | Impact | Mitigation Strategy |
|------|--------|-------------------|
| API FEAT-030 changes during development | High | Regular sync with API team, use mocks initially |
| Complex multi-type exercise UI confusion | Medium | Clear visual themes and progressive disclosure |
| Performance with large alternative lists | Medium | Implement virtual scrolling and lazy loading |
| State management complexity | Medium | Clear separation of concerns, comprehensive testing |
| Accessibility compliance challenges | Low | Follow WCAG guidelines, use automated testing tools |

## Success Criteria

### Functional Requirements:
- ✅ All four relationship types work (WARMUP, COOLDOWN, WORKOUT, ALTERNATIVE)
- ✅ Context-aware UI displays correctly for each exercise type
- ✅ Multi-type exercises support context switching
- ✅ Bidirectional relationships created and displayed
- ✅ Alternative exercise validation prevents incompatible links
- ✅ Maximum limits enforced (10 for warmup/cooldown)
- ✅ REST exercises properly restricted

### Technical Requirements:
- ✅ All bUnit tests passing (target >90% coverage)
- ✅ Build successful with 0 errors, minimal warnings
- ✅ WCAG AA accessibility compliance
- ✅ Performance targets met (< 2s load time)
- ✅ Proper Blazor component patterns followed

### User Experience Requirements:
- ✅ PTs can complete workout with links in < 3 minutes
- ✅ Context switching is intuitive and responsive
- ✅ Validation errors provide clear, actionable guidance
- ✅ UI maintains design consistency with existing components
- ✅ Mobile/tablet experience is touch-friendly

---

**Feature ID**: FEAT-022
**Created**: 2025-01-28
**Status**: READY_TO_DEVELOP
**Priority**: High
**Business Impact**: High - Enhanced PT workflow with four-way relationships
**API Dependency**: FEAT-030 (Four-Way Linking System API)