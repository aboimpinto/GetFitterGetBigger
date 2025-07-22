# Workout Template Core Admin Workflows

## Overview
This document describes the user workflows for Personal Trainers managing workout templates in the Admin application.

## Primary Workflows

### 1. Create New Workout Template

**Goal**: Personal Trainer creates a reusable workout blueprint for clients.

**Preconditions**:
- User is authenticated as Personal Trainer
- Has access to workout template management

**Steps**:
1. Navigate to Workout Templates section
2. Click "Create New Template" button
3. **Step 1 - Basic Information**:
   - Enter template name (e.g., "Upper Body Strength Day")
   - Add description explaining workout focus
   - Select workout category from dropdown
   - Select workout objective from dropdown
   - Choose execution protocol (initially "Standard")
   - Set estimated duration using slider
   - Select difficulty level
   - Toggle public visibility
   - Add relevant tags
   - Click "Next"
4. **Step 2 - Exercise Selection**:
   - View intelligent suggestions based on category
   - Search for specific exercises
   - Drag exercises to appropriate zones:
     - Warmup zone for preparatory exercises
     - Main zone for primary exercises
     - Cooldown zone for recovery exercises
   - Add exercise-specific notes if needed
   - Review auto-added warmup/cooldown associations
   - Reorder exercises within zones
   - Click "Next"
5. **Step 3 - Set Configuration**:
   - For each exercise, configure:
     - Number of sets
     - Target repetitions or duration
     - Intensity guidelines
   - Review total workout duration
   - Adjust configurations as needed
   - Click "Next"
6. **Step 4 - Review**:
   - Review complete workout structure
   - Check equipment requirements
   - Verify exercise flow
   - Choose to:
     - "Save as Draft" for further editing
     - "Save and Publish" to make immediately available
7. Receive confirmation with template ID

**Success Criteria**:
- Template created successfully
- Appears in template list
- Can be assigned to clients (if published)

**Error Scenarios**:
- Missing required fields
- Invalid exercise combinations
- Network failure (draft saved locally)

### 2. Edit Existing Workout Template

**Goal**: Modify a workout template while preserving or updating client assignments.

**Preconditions**:
- Template exists and user is the owner
- Template is in DRAFT or PRODUCTION state

**Steps**:
1. Navigate to Workout Templates list
2. Find template using search/filters
3. Click template card to view details
4. Click "Edit" button
5. **If template is in PRODUCTION**:
   - System checks for existing execution logs
   - If logs exist, warns that template cannot return to DRAFT
   - Allows minor edits without state change
6. **Edit Mode**:
   - Modify basic information
   - Add/remove/reorder exercises
   - Update set configurations
   - Change notes and descriptions
7. Save changes:
   - Auto-saves every 30 seconds
   - Manual save button available
   - Version number increments
8. **If significant changes made**:
   - Option to notify assigned clients
   - Create new version or update existing

**Success Criteria**:
- Changes saved successfully
- Version history updated
- Appropriate notifications sent

### 3. Manage Workout Template States

**Goal**: Control the lifecycle of workout templates through state transitions.

**Preconditions**:
- User owns the template
- Has appropriate permissions

**Workflow A - Publish Template (DRAFT → PRODUCTION)**:
1. Open template in DRAFT state
2. Click "Publish Template" button
3. Review publish checklist:
   - All exercises configured
   - Duration is reasonable
   - Equipment list is complete
4. Confirm publication
5. System deletes any test execution logs
6. Template now available to all authorized users

**Workflow B - Rollback Template (PRODUCTION → DRAFT)**:
1. Open template in PRODUCTION state
2. Click "Rollback to Draft"
3. System checks for execution logs
4. If no logs exist:
   - Confirm rollback
   - Template returns to DRAFT
5. If logs exist:
   - Display error message
   - Suggest creating new version instead

**Workflow C - Archive Template**:
1. Open any template
2. Click "Archive Template"
3. Confirm archival with warning:
   - Template becomes read-only
   - No new executions allowed
   - Historical data preserved
4. Template moved to ARCHIVED state
5. Removed from active template lists

**Success Criteria**:
- State transitions complete successfully
- Appropriate data cleanup occurs
- User notifications sent

### 4. Duplicate and Modify Template

**Goal**: Create variations of existing successful templates.

**Preconditions**:
- Source template exists
- User has template creation permissions

**Steps**:
1. Find template to duplicate
2. Click "Duplicate" from action menu
3. Enter new template name
4. Choose duplication options:
   - Copy all exercises and configurations
   - Copy structure only (no configurations)
   - Copy as template for different category
5. New template created in DRAFT state
6. Automatically opens in editor
7. Make necessary modifications:
   - Adjust exercises for variation
   - Modify difficulty level
   - Update duration estimates
8. Save and publish as needed

**Success Criteria**:
- New template created with unique ID
- Original template unchanged
- Proper attribution maintained

### 5. Bulk Template Management

**Goal**: Efficiently manage multiple templates simultaneously.

**Preconditions**:
- Multiple templates exist
- User has bulk operation permissions

**Steps**:
1. Navigate to template list
2. Switch to "Selection Mode"
3. Select multiple templates:
   - Click checkboxes
   - Or use Shift+Click for range
4. Choose bulk action:
   - Archive selected
   - Change visibility
   - Add/remove tags
   - Export templates
5. Confirm bulk operation
6. View operation results:
   - Success count
   - Failed items with reasons
   - Option to retry failures

**Success Criteria**:
- Operations completed for all valid selections
- Clear feedback on results
- Audit trail created

### 6. Search and Filter Templates

**Goal**: Quickly find specific templates from large collections.

**Preconditions**:
- Templates exist in system
- User has view permissions

**Steps**:
1. Access template management page
2. Use quick search bar for name/description
3. Open advanced filters:
   - Select workout categories
   - Choose objectives
   - Filter by difficulty
   - Set duration range
   - Filter by state
   - Select date ranges
4. Apply filters
5. Sort results:
   - By name
   - By creation date
   - By last modified
   - By popularity
6. Save filter preset for reuse
7. Export filtered results

**Success Criteria**:
- Relevant templates displayed
- Filters can be combined
- Results update dynamically

### 7. Template Analytics and Insights

**Goal**: Understand template usage and effectiveness.

**Preconditions**:
- Templates have execution history
- User has analytics permissions

**Steps**:
1. Select template from list
2. Click "View Analytics"
3. Review metrics dashboard:
   - Total executions
   - Unique users
   - Completion rates
   - Average duration vs. estimated
   - User feedback scores
4. View trends over time:
   - Usage frequency
   - Seasonal patterns
   - Performance improvements
5. Compare templates:
   - Select multiple for comparison
   - View side-by-side metrics
   - Identify best performers
6. Export analytics data
7. Make data-driven improvements

**Success Criteria**:
- Accurate metrics displayed
- Actionable insights available
- Data exports successfully

## Supporting Workflows

### Exercise Warning Management

**Scenario**: Removing warmup/cooldown exercises linked to main exercises.

**Steps**:
1. When removing a warmup/cooldown exercise
2. System displays warning:
   - "This warmup is associated with [Exercise Name]"
   - "Removing may impact workout safety"
3. Options:
   - Keep exercise
   - Remove anyway
   - Replace with alternative
4. If removed, warning logged in template notes

### Equipment Conflict Resolution

**Scenario**: Selected exercises require conflicting equipment.

**Steps**:
1. System detects equipment conflicts
2. Displays conflict warning
3. Suggests alternatives:
   - Similar exercises with available equipment
   - Modified versions
   - Equipment substitutions
4. User selects resolution
5. Template updates automatically

### Template Version Comparison

**Scenario**: Reviewing changes between template versions.

**Steps**:
1. Open template with version history
2. Click "Version History"
3. Select two versions to compare
4. View differences:
   - Added exercises (green)
   - Removed exercises (red)
   - Modified configurations (yellow)
5. Option to restore previous version
6. Document reason for restoration

## Workflow Best Practices

### Template Creation
1. Start with clear objectives
2. Follow warmup → main → cooldown structure
3. Include variety in exercise selection
4. Test template before publishing
5. Gather feedback for improvements

### State Management
1. Keep templates in DRAFT during development
2. Only publish thoroughly tested templates
3. Archive outdated templates promptly
4. Document reasons for state changes

### Organization
1. Use consistent naming conventions
2. Apply relevant tags for discoverability
3. Maintain accurate difficulty ratings
4. Keep descriptions informative and concise

## Error Recovery

### Network Failures
- Auto-save to local storage
- Retry failed operations
- Queue changes for sync
- Clear status indicators

### Validation Failures
- Highlight problem fields
- Provide clear error messages
- Suggest corrections
- Prevent data loss

### Permission Errors
- Explain permission requirements
- Provide request access option
- Redirect to appropriate page
- Log security events