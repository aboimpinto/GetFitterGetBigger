# Workout Template Core Admin Workflows

## Overview
This document describes the key user workflows for Personal Trainers using the Workout Template Core feature in the Admin application.

## Primary Workflows

### 1. Create New Workout Template

**Goal**: Personal Trainer creates a new reusable workout template.

**Steps**:
1. **Navigate to Templates**
   - Click "Workout Templates" in main navigation
   - View existing templates list
   - Click "Create New Template" button

2. **Define Template Basics**
   - Enter template name (e.g., "Upper Body Strength Day")
   - Add optional description
   - Select workout category from dropdown
   - Select workout objective from dropdown
   - Choose execution protocol (initially only "Standard" available)
   - Set estimated duration using slider or input
   - Select difficulty level (Beginner/Intermediate/Advanced)
   - Toggle public/private visibility
   - Add relevant tags for discoverability

3. **Add Exercises**
   - System displays three-zone layout (Warmup, Main, Cooldown)
   - Click "Add Exercise" in Main zone
   - **Exercise Selection**:
     - Browse exercises filtered by selected category
     - Use search to find specific exercises
     - View exercise details (muscles, equipment, instructions)
     - Select one or more exercises
     - Click "Add to Main Zone"
   - **Automatic Suggestions**:
     - System checks if added exercises have warmup associations
     - Displays suggestion: "Add warmup for Bench Press?"
     - Accept or dismiss suggestions
   - **Add Rest Periods**:
     - Select "Rest" from exercise list
     - Position between exercises as needed
     - Set rest duration

4. **Configure Sets**
   - Click on each exercise card
   - **For Standard Protocol**:
     - Enter number of sets
     - Enter target reps (can use range like "8-12")
     - Add intensity guidelines (optional)
   - Save configuration
   - Repeat for all exercises

5. **Organize Exercise Order**
   - Drag and drop exercises within zones
   - Ensure logical progression
   - Warmup → Main → Cooldown flow

6. **Review and Save**
   - Review complete template
   - Check equipment summary (auto-generated)
   - Validate exercise flow
   - Add any final notes
   - Click "Save Template"
   - System confirms creation

**Success Criteria**:
- Template saved successfully
- Available in templates list
- Ready for client assignment

### 2. Edit Existing Template

**Goal**: Modify an existing workout template.

**Steps**:
1. **Find Template**
   - Navigate to templates list
   - Use search or filters to locate
   - Click "Edit" action

2. **Modify as Needed**
   - Update basic information
   - Add/remove exercises
   - Adjust set configurations
   - Reorder exercises
   - Update notes

3. **Handle Warnings**
   - System alerts if removing linked exercises
   - Choose to acknowledge and proceed
   - Or cancel and adjust

4. **Save Changes**
   - Review modifications
   - Update version if significant changes
   - Save template
   - System confirms update

### 3. Duplicate and Customize Template

**Goal**: Create a variation of an existing template.

**Steps**:
1. **Select Template**
   - Find template to duplicate
   - Click "Duplicate" action

2. **Customize Copy**
   - System creates copy with "(Copy)" suffix
   - Rename template
   - Modify exercises as needed
   - Adjust configurations
   - Change difficulty or duration

3. **Save New Template**
   - Review customizations
   - Save as new template
   - Original remains unchanged

### 4. Browse and Filter Templates

**Goal**: Find specific templates quickly.

**Steps**:
1. **Access Template Library**
   - Navigate to templates section
   - View default grid/list

2. **Apply Filters**
   - Select workout category
   - Choose objective
   - Filter by difficulty
   - Filter by duration range
   - Show only public/private
   - Filter by creator

3. **Search Templates**
   - Enter search terms
   - Search in names/descriptions
   - Use tag search
   - View filtered results

4. **Sort Results**
   - Sort by name
   - Sort by date created
   - Sort by popularity
   - Sort by duration

### 5. Quick Template Preview

**Goal**: Review template details without editing.

**Steps**:
1. **Select Template**
   - Click template card
   - Or use "Preview" action

2. **Review Details**
   - View exercise flow
   - Check equipment needs
   - Review duration estimate
   - See set configurations
   - Check difficulty level

3. **Take Action**
   - Edit if needed
   - Duplicate for customization
   - Assign to clients
   - Close preview

### 6. Manage Template Exercises

**Goal**: Efficiently organize exercises within zones.

**Steps**:
1. **Zone Management**
   - View three-zone layout
   - See exercise counts per zone

2. **Add Exercises**
   - Click "Add" in target zone
   - Select from filtered list
   - Accept warmup/cooldown suggestions

3. **Reorder Exercises**
   - Drag exercise cards
   - Drop in new position
   - Within or between zones
   - Maintain logical flow

4. **Configure Sets**
   - Click exercise to expand
   - Enter set details
   - Add multiple configurations if needed
   - Save configuration

5. **Remove Exercises**
   - Click remove icon
   - Handle warnings for linked exercises
   - Confirm removal

### 7. Handle Exercise Dependencies

**Goal**: Manage warmup/cooldown associations properly.

**Workflow**:
1. **Adding Main Exercise**
   - Select exercise with associations
   - System suggests related warmups
   - Accept suggestions
   - Warmups added to warmup zone

2. **Removing Linked Exercise**
   - Attempt to remove warmup
   - System shows warning
   - "This warmup is linked to Bench Press"
   - Choose to proceed or cancel

3. **Managing Dependencies**
   - View exercise relationships
   - Maintain safety protocols
   - Override with acknowledgment

### 8. Template Validation

**Goal**: Ensure template meets quality standards.

**Steps**:
1. **Automatic Validation**
   - System validates on save
   - Shows inline errors
   - Prevents invalid saves

2. **Warning Review**
   - View non-blocking warnings
   - Missing warmups highlighted
   - Equipment concerns noted
   - Choose to address or acknowledge

3. **Manual Review**
   - Check exercise progression
   - Verify time estimates
   - Confirm difficulty accuracy
   - Ensure completeness

## Error Handling Workflows

### 1. Validation Errors
**Scenario**: Required fields missing or invalid.

**Flow**:
1. User attempts to save
2. System highlights errors
3. Error messages displayed
4. User corrects issues
5. Retry save operation

### 2. Conflict Resolution
**Scenario**: Template modified by another user.

**Flow**:
1. User attempts to save
2. System detects conflict
3. Options presented:
   - View changes
   - Overwrite
   - Cancel
4. User chooses action
5. System processes choice

### 3. Permission Denied
**Scenario**: User lacks PT-Tier access.

**Flow**:
1. User attempts restricted action
2. System shows permission error
3. Suggests contacting admin
4. User redirected appropriately

## Best Practices

### Template Creation
1. Start with clear objective
2. Add main exercises first
3. Include appropriate warmups
4. Add cooldown for recovery
5. Insert rest periods strategically
6. Review equipment requirements
7. Test with sample client

### Exercise Selection
1. Match category to template goal
2. Progress from simple to complex
3. Balance muscle groups
4. Consider equipment availability
5. Include exercise variations
6. Plan for different fitness levels

### Set Configuration
1. Align with workout objective
2. Use appropriate rep ranges
3. Consider time constraints
4. Add clear intensity guidelines
5. Plan for progression

### Template Management
1. Use descriptive names
2. Keep descriptions concise
3. Tag appropriately
4. Version significant changes
5. Archive outdated templates
6. Share successful templates