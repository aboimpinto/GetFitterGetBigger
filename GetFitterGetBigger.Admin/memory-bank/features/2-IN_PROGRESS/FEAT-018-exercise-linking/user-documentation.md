# Exercise Linking Feature - User Documentation

## Overview

The Exercise Linking feature allows Personal Trainers to create relationships between exercises by linking warmup and cooldown exercises to main workout exercises. This helps create comprehensive workout routines and provides better exercise recommendations.

## Feature Availability

- **Available for:** Workout-type exercises only
- **Not available for:** Warmup-only or Cooldown-only exercises
- **Maximum links:** 10 warmup exercises + 10 cooldown exercises per workout

## How to Use Exercise Linking

### Accessing Exercise Links

1. Navigate to the **Exercise List** page
2. Click on any exercise that has **"Workout"** in its exercise types
3. The exercise details page will show a **"Linked Exercises"** section at the bottom

### Creating Exercise Links

#### Adding Warmup Exercises

1. In the exercise detail page, locate the **"Linked Exercises"** section
2. In the **"Warmup Exercises"** section, click **"Add Warmup"**
3. A modal will open showing available warmup exercises
4. Use the search box to find specific exercises
5. Click on an exercise card to select it
6. The exercise will be added to your warmup list

#### Adding Cooldown Exercises

1. In the **"Cooldown Exercises"** section, click **"Add Cooldown"**
2. A modal will open showing available cooldown exercises
3. Use the search box to find specific exercises
4. Click on an exercise card to select it
5. The exercise will be added to your cooldown list

### Managing Exercise Links

#### Reordering Exercises

1. Each linked exercise has **up (↑)** and **down (↓)** buttons
2. Click the **up button** to move an exercise higher in the order
3. Click the **down button** to move an exercise lower in the order
4. The first exercise cannot be moved up
5. The last exercise cannot be moved down

#### Removing Exercise Links

1. Click the **trash icon** on any linked exercise
2. A confirmation dialog will appear
3. Click **"Remove"** to confirm deletion
4. Click **"Cancel"** to keep the link

### Understanding Exercise Counts

- Each section shows current count vs. maximum (e.g., "3 / 10")
- When you reach the maximum (10), the **"Add"** button will be hidden
- You can have up to 10 warmup exercises and 10 cooldown exercises per workout

## Exercise List Indicators

### Link Indicators

- Exercises with links show a **link icon** (🔗) in the exercise list
- The indicator shows link counts in format: **"W/C"** (Warmup/Cooldown)
- Examples:
  - **"3/2"** = 3 warmup exercises, 2 cooldown exercises
  - **"5 W"** = 5 warmup exercises only
  - **"3 C"** = 3 cooldown exercises only

### Tooltips

- Hover over the link indicator to see detailed information
- Tooltip shows: **"X warmup exercises, Y cooldown exercises"**
- Uses correct singular/plural forms (e.g., "1 warmup exercise" vs "2 warmup exercises")

### Filtering by Links

1. In the exercise list, use the **"Links"** filter dropdown
2. Select **"Has Links"** to show only exercises with links
3. Select **"No Links"** to show only exercises without links
4. Select **"All Exercises"** to show all exercises

## Validation Rules

### Exercise Type Compatibility

- **Only Workout exercises** can have links added to them
- **Only Warmup exercises** can be linked as warmup
- **Only Cooldown exercises** can be linked as cooldown

### Link Limitations

- **Maximum 10 links** per type (warmup/cooldown)
- **No duplicate links** - same exercise cannot be linked twice
- **No self-references** - exercise cannot link to itself
- **No circular references** - if Exercise A links to Exercise B, Exercise B cannot link to Exercise A

### Error Messages

Common validation errors and their meanings:

- **"Only Workout type exercises can have links"** - You're trying to add links to a non-workout exercise
- **"Exercise is already linked as warmup"** - You're trying to add a duplicate warmup link
- **"Maximum 10 warmup links allowed"** - You've reached the limit for warmup exercises
- **"Cannot create circular reference between exercises"** - This would create an invalid circular relationship

## User Interface Features

### Success Notifications

- **Green notifications** appear when actions complete successfully
- Auto-dismiss after 3 seconds
- Can be manually dismissed by clicking the X button

### Error Handling

- **Red error messages** appear when something goes wrong
- Include specific details about what went wrong
- Provide actionable suggestions when possible
- **"Try again"** button appears for network errors

### Loading States

- **Spinning indicators** show when operations are in progress
- Buttons become disabled during operations
- **"Adding..."** text appears during exercise addition
- **"Reordering exercises..."** overlay during reordering

### Progress Indicators

- **Progress overlay** appears during bulk operations
- Shows **spinning animation** and operation description
- Prevents interaction during critical operations

## Accessibility Features

### Keyboard Navigation

- All interactive elements are keyboard accessible
- Use **Tab** to navigate between elements
- Use **Enter** or **Space** to activate buttons
- Use **Escape** to close modals

### Screen Reader Support

- All buttons have descriptive **aria-labels**
- Status changes are announced to screen readers
- Modal dialogs have proper **aria-modal** attributes
- Link counts have descriptive **aria-labels**

### Visual Accessibility

- **High contrast** colors for all interactive elements
- **Clear focus indicators** for keyboard navigation
- **Descriptive tooltips** for all icon buttons
- **Consistent visual hierarchy** throughout the interface

## Mobile Responsiveness

### Touch-Friendly Design

- **Large touch targets** for all interactive elements
- **Swipe gestures** work naturally on touch devices
- **Responsive layout** adapts to different screen sizes

### Mobile-Specific Features

- **Optimized button sizes** for touch interaction
- **Scrollable lists** for long exercise lists
- **Touch-friendly drag and drop** alternative (move up/down buttons)

## Performance Considerations

### Caching

- Exercise links are **cached for 1 hour**
- Cache is automatically invalidated when links are modified
- Improves performance for frequently accessed exercises

### Optimistic Updates

- Changes appear immediately in the UI
- If server operation fails, changes are reverted
- Provides responsive user experience

## Troubleshooting

### Common Issues

#### "No exercises found" in modal

- **Check search terms** - try broader search terms
- **Verify exercise types** - ensure you're searching for the correct type
- **Check network connection** - poor connection may prevent loading

#### Changes not saving

- **Check network connection** - operations require stable internet
- **Try refreshing the page** - may resolve temporary issues
- **Check for error messages** - look for red error notifications

#### Links not appearing

- **Ensure exercise is "Workout" type** - only workout exercises can have links
- **Check for JavaScript errors** - refresh the page if needed
- **Verify user permissions** - ensure you have PT-Tier or Admin-Tier access

### Getting Help

If you encounter issues not covered in this documentation:

1. **Check the browser console** for error messages
2. **Try refreshing the page** to resolve temporary issues
3. **Clear browser cache** if problems persist
4. **Contact support** with specific error messages and steps to reproduce

## Best Practices

### Exercise Selection

- **Choose relevant exercises** - select warmups/cooldowns that complement the main exercise
- **Consider progression** - order exercises from general to specific
- **Think about equipment** - group exercises that use similar equipment

### Link Organization

- **Order by intensity** - start with lower intensity, build up
- **Group by body part** - keep related exercises together
- **Consider timing** - order exercises by when they should be performed

### Maintenance

- **Regular review** - periodically review and update exercise links
- **Remove outdated links** - clean up exercises that are no longer relevant
- **Update based on feedback** - adjust links based on client feedback

## Feature Limitations

### Current Limitations

- **Maximum 10 links** per type (technical limitation)
- **No bulk operations** - links must be added/removed individually
- **No link categories** - all warmup/cooldown exercises are treated equally
- **No link descriptions** - cannot add notes or descriptions to links

### Future Enhancements

Potential improvements being considered:

- **Bulk link operations** - add/remove multiple links at once
- **Link categories** - organize links by intensity, focus area, etc.
- **Link descriptions** - add notes explaining why exercises are linked
- **Export functionality** - export exercise routines with links
- **Link templates** - save and reuse common link patterns

## API Dependencies

This feature depends on:

- **Exercise API** - for fetching exercise data
- **Exercise Link API** - for managing exercise relationships
- **Validation API** - for checking link validity
- **Authentication API** - for user permissions

Ensure all APIs are accessible and functioning for the feature to work properly.