# Placeholder Features Documentation - Workout Template UI

## Overview
This document outlines features that are currently implemented as placeholders in the Workout Template UI due to API limitations or postponed functionality. These placeholders ensure a complete user experience while clearly indicating future enhancements.

## 1. Equipment Information Placeholder

### Current State
- **Location**: WorkoutTemplateDetail component and WorkoutTemplateCard component
- **Display**: "Equipment information coming soon" message
- **Technical Implementation**: 
  ```razor
  <div class="text-gray-500 italic">
      <i class="bi bi-info-circle me-1"></i>
      Equipment information coming soon
  </div>
  ```

### Reason for Placeholder
- The API currently does not aggregate equipment information from exercises
- Equipment data model exists but aggregation logic is not implemented
- API returns empty equipment arrays for all templates

### Future Implementation
When the API is updated to provide equipment aggregation:
1. Remove the placeholder message
2. Display equipment list similar to exercise display
3. Show equipment counts on template cards
4. Add equipment filtering capability

### User Communication
- The placeholder clearly indicates this is a planned feature
- No user action is required
- The message uses a neutral gray color to avoid confusion with errors

## 2. Exercise Suggestions (AI-Powered)

### Current State
- **Location**: Not visible in current UI
- **Display**: Feature completely hidden
- **Technical Implementation**: No UI elements created for this feature

### Reason for Placeholder
- AI-powered exercise suggestions require significant backend infrastructure
- Feature postponed to focus on core CRUD functionality
- Would require integration with AI services and complex UI components

### Future Implementation
When this feature is implemented:
1. Add "Get AI Suggestions" button in WorkoutTemplateForm
2. Create modal dialog for suggestion interface
3. Implement exercise selection and addition workflow
4. Add configuration for AI parameters (goals, constraints)

### Design Decisions
- **Option Considered**: Showing disabled "Get suggestions" button
- **Decision**: Hide feature completely to avoid user confusion
- **Rationale**: 
  - Disabled buttons without clear timeline frustrate users
  - Hidden features don't create false expectations
  - Cleaner UI without non-functional elements

## 3. Advanced Exercise Management

### Current State
- **Location**: WorkoutTemplateExerciseView component
- **Display**: Read-only exercise list
- **Technical Implementation**: Displays exercises but no editing capabilities

### Reason for Placeholder
- Exercise management within templates requires complex UI
- API supports the operations but UI implementation was descoped
- Focus placed on template-level CRUD operations

### Future Implementation
When exercise management is added:
1. Add "Manage Exercises" button in template detail/edit views
2. Create exercise selection modal with search
3. Implement drag-and-drop reordering
4. Add set configuration editing
5. Support for supersets and circuit creation

### Current Workaround
- Users must manage exercises through direct API calls
- Template exercise structure can be viewed but not modified via UI
- This maintains data integrity while limiting functionality

## 4. Equipment-Based Filtering

### Current State
- **Location**: WorkoutTemplateFilters component
- **Display**: No equipment filter option
- **Technical Implementation**: Filtering only by category, difficulty, state, and visibility

### Reason for Placeholder
- Depends on equipment aggregation API functionality
- Cannot filter by equipment until API provides this data
- Placeholder prevents broken filter functionality

### Future Implementation
When equipment data is available:
1. Add equipment multi-select filter
2. Update filter DTO to include equipment IDs
3. Implement equipment-based search
4. Show equipment tags in search results

## 5. Template Statistics

### Current State
- **Location**: Not implemented
- **Display**: Basic counts only (exercises, duration)
- **Technical Implementation**: No advanced statistics

### Reason for Placeholder
- Advanced statistics require API aggregation
- Features like popularity, usage tracking not yet available
- Focus on core functionality first

### Future Implementation
Could include:
1. Template usage statistics
2. Average completion time
3. User ratings and feedback
4. Difficulty progression metrics
5. Equipment utilization stats

## Technical Implementation Guidelines

### Adding Placeholder UI
When adding placeholders for future features:

```razor
@* Standard placeholder pattern *@
<div class="placeholder-feature" data-testid="feature-placeholder">
    <i class="bi bi-info-circle text-muted me-1"></i>
    <span class="text-muted fst-italic">
        Feature coming soon
    </span>
</div>
```

### Styling Guidelines
- Use `text-muted` or `text-gray-500` for placeholder text
- Include `fst-italic` for visual distinction
- Add info icon (`bi-info-circle`) for clarity
- Use semantic `data-testid` for testing

### Testing Placeholders
- Verify placeholder displays correctly
- Ensure no broken functionality
- Test that placeholders don't interfere with working features
- Include in visual regression tests

## Communication Strategy

### For Users
1. **In-App Messages**: Clear "coming soon" indicators
2. **Documentation**: This document explains the roadmap
3. **Release Notes**: Mention planned features
4. **Support**: Prepared responses for feature inquiries

### For Developers
1. **Code Comments**: Mark placeholder sections clearly
2. **TODO Comments**: Reference future implementation tickets
3. **Architecture**: Design supports future additions
4. **API Contracts**: Already support future features

## Maintenance

### Regular Reviews
- Review placeholders quarterly
- Update messages if timelines change
- Remove placeholders as features are implemented
- Track user feedback on missing features

### Priority Tracking
Current implementation priority:
1. ✅ Core CRUD operations (Complete)
2. ✅ State management (Complete)
3. ✅ Search and filtering (Complete)
4. ⏳ Equipment aggregation (Next)
5. ⏳ Exercise management UI (Future)
6. ⏳ AI suggestions (Long-term)

## User Feedback

### Common Questions
**Q: When will equipment information be available?**
A: This depends on API development timeline. The UI is ready to display equipment data once the API provides it.

**Q: Can I add exercises to templates?**
A: Currently, exercises must be managed via direct API calls. The UI displays existing exercises but doesn't support editing yet.

**Q: Will AI suggestions be added?**
A: This is a planned future feature but requires significant infrastructure. No timeline is currently set.

### Feedback Channels
- Feature requests can be submitted through normal channels
- Placeholder features are already tracked in the product backlog
- User feedback helps prioritize which placeholders to implement first

## Conclusion

These placeholders represent a pragmatic approach to shipping valuable functionality while being transparent about limitations. They:
- Provide clear communication about future features
- Avoid broken or confusing UI elements
- Maintain a professional, polished appearance
- Allow for incremental feature additions

The architecture supports adding these features without major refactoring, ensuring a smooth upgrade path as functionality becomes available.