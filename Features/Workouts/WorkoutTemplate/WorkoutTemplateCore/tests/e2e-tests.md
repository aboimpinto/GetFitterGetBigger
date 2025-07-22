# Workout Template Core - End-to-End Tests

## Overview
This document specifies end-to-end tests for the Workout Template Core feature, covering complete user workflows across all applications.

## Personal Trainer Workflows

### E2E Test: Create and Publish Workout Template

**Scenario**: Personal Trainer creates a complete upper body workout template

**Test Steps**:
1. **Login as Personal Trainer**
   - Navigate to Admin portal
   - Enter credentials
   - Verify dashboard access

2. **Navigate to Template Creation**
   - Click "Workout Templates" in navigation
   - Click "Create New Template" button
   - Verify creation wizard loads

3. **Complete Basic Information**
   - Enter name: "Upper Body Power Day"
   - Add description: "Focus on explosive upper body movements"
   - Select category: "Upper Body"
   - Select objective: "Power"
   - Select protocol: "Standard"
   - Set duration: 60 minutes
   - Select difficulty: "Advanced"
   - Enable public visibility
   - Add tags: "power", "explosive", "upper"
   - Click "Next"

4. **Add Exercises**
   - **Warmup Zone**:
     - Search "arm circles"
     - Add to warmup zone
     - Search "band pull aparts"
     - Add to warmup zone
   - **Main Zone**:
     - Search "explosive push-ups"
     - Add to main zone
     - Verify warmup suggestion appears
     - Accept suggested warmup
     - Search "medicine ball slams"
     - Add to main zone
     - Search "plyo push-ups"
     - Add to main zone
   - **Cooldown Zone**:
     - Search "chest stretch"
     - Add to cooldown zone
   - Verify equipment list shows: "Medicine Ball, Resistance Band"
   - Click "Next"

5. **Configure Sets**
   - For each main exercise:
     - Set 3 sets
     - Set 6-8 reps
     - Add intensity note: "Explosive movement"
   - Click "Next"

6. **Review and Create**
   - Verify summary shows:
     - 2 warmup exercises
     - 3 main exercises
     - 1 cooldown exercise
     - Total duration ~60 minutes
     - Equipment needed
   - Click "Save as Draft"
   - Verify success message

7. **Test in Draft Mode**
   - Switch to client app
   - Login with tester account
   - Find template (marked as DRAFT)
   - Execute partial workout
   - Return to admin

8. **Publish Template**
   - Open template in editor
   - Click "Publish"
   - Confirm deletion of test logs
   - Verify state changes to PRODUCTION
   - Check template appears in public list

**Expected Results**:
- Template created successfully
- All exercises properly organized
- Equipment automatically aggregated
- State transitions work correctly
- Template available for clients

### E2E Test: Duplicate and Modify Template

**Scenario**: Create variation of existing successful template

**Test Steps**:
1. **Find Popular Template**
   - Navigate to templates
   - Sort by "Most Used"
   - Select top template

2. **Duplicate Template**
   - Click "Actions" → "Duplicate"
   - Enter new name: "Modified [Original Name]"
   - Confirm duplication

3. **Modify for Different Level**
   - Change difficulty to "Intermediate"
   - Reduce sets from 4 to 3
   - Modify rep ranges (increase slightly)
   - Update duration estimate
   - Add modification notes

4. **Save and Test**
   - Save as new template
   - Preview changes
   - Publish when ready

**Expected Results**:
- Original template unchanged
- New template with modifications
- Proper attribution maintained
- Both templates functional

## Client User Workflows

### E2E Test: Browse and Start Workout

**Scenario**: Client finds and executes appropriate workout template

**Test Steps**:
1. **Login to Mobile App**
   - Open app
   - Use biometric/PIN login
   - Verify home screen loads

2. **Browse Templates**
   - Tap "Browse Workouts"
   - Apply filters:
     - Category: "Upper Body"
     - Difficulty: "Intermediate"
     - Duration: "30-45 min"
   - Scroll through results

3. **Select Template**
   - Tap interesting template
   - Read description
   - Check equipment requirements
   - View exercise list
   - Tap "Start Workout"

4. **Prepare for Workout**
   - Verify equipment checklist
   - Confirm all equipment available
   - Tap "I'm Ready"

5. **Execute Workout**
   - **Warmup Phase**:
     - Complete first warmup exercise
     - Swipe to next exercise
     - Complete all warmups
   - **Main Phase**:
     - View first exercise
     - Complete first set
     - Tap set counter
     - Rest timer auto-starts
     - Complete all sets
     - Swipe to next exercise
   - **Cooldown Phase**:
     - Complete stretches
     - Hold for duration

6. **Complete Workout**
   - View summary screen
   - Rate workout difficulty
   - Save workout log
   - Return to home

**Expected Results**:
- Smooth workout flow
- Progress tracked accurately
- All exercises completed
- Data saved successfully

### E2E Test: Offline Workout Execution

**Scenario**: Execute workout with intermittent connectivity

**Test Steps**:
1. **Download Template Offline**
   - While online, find template
   - Tap download icon
   - Verify offline indicator

2. **Enable Airplane Mode**
   - Go to device settings
   - Enable airplane mode
   - Return to app

3. **Start Offline Workout**
   - Navigate to downloaded templates
   - Select template
   - Start workout
   - Execute normally

4. **Mid-Workout Connectivity**
   - After 2 exercises, disable airplane mode
   - Continue workout
   - Complete remaining exercises

5. **Verify Sync**
   - Check sync indicator
   - Verify progress uploaded
   - Check server has complete data

**Expected Results**:
- Workout executes offline
- Progress saved locally
- Automatic sync when online
- No data loss

## Cross-Platform Workflows

### E2E Test: Multi-Device Template Management

**Scenario**: Trainer creates on desktop, modifies on tablet, clients use on mobile

**Test Steps**:
1. **Create on Desktop Admin**
   - Login on desktop browser
   - Create basic template
   - Save as draft

2. **Edit on Tablet**
   - Login on tablet browser
   - Find draft template
   - Add more exercises
   - Adjust configurations
   - Save changes

3. **Review on Desktop**
   - Return to desktop
   - Verify changes synced
   - Make final adjustments
   - Publish template

4. **Execute on Mobile**
   - Client opens mobile app
   - Finds new template
   - Executes workout
   - Completes successfully

**Expected Results**:
- Seamless sync across devices
- All changes preserved
- Consistent experience
- No conflicts

## Data Validation Workflows

### E2E Test: Invalid Data Handling

**Scenario**: User attempts various invalid operations

**Test Steps**:
1. **Invalid Template Creation**
   - Try empty name
   - Try 500+ character name
   - Try invalid duration (500 min)
   - Select mismatched category/objective

2. **Invalid Exercise Operations**
   - Add same exercise twice to zone
   - Set invalid sequence orders
   - Add 50+ exercises
   - Delete required exercises

3. **Invalid State Transitions**
   - Try to edit ARCHIVED template
   - Try to execute DRAFT as regular user
   - Try to delete template with logs

**Expected Results**:
- Clear error messages
- No data corruption
- Graceful handling
- Recovery options provided

## Performance Workflows

### E2E Test: Large Scale Operations

**Scenario**: Handle templates with many exercises efficiently

**Test Steps**:
1. **Create Large Template**
   - Add 30+ exercises
   - Multiple configurations each
   - Complex note system

2. **Performance Checks**
   - Load time < 2 seconds
   - Smooth scrolling
   - Quick exercise reordering
   - Fast save operations

3. **Execute Large Workout**
   - Start workout
   - Navigate between exercises quickly
   - No lag or freezing
   - Progress saves properly

**Expected Results**:
- Acceptable performance
- No timeouts
- Smooth user experience
- All data handled correctly

## Error Recovery Workflows

### E2E Test: Failure Recovery

**Scenario**: System handles various failure scenarios

**Test Steps**:
1. **Network Failure During Save**
   - Create template
   - Disconnect network
   - Attempt save
   - Verify local save
   - Reconnect and sync

2. **App Crash During Workout**
   - Start workout
   - Complete 3 exercises
   - Force close app
   - Reopen app
   - Verify recovery prompt
   - Continue from last position

3. **Server Error Handling**
   - Attempt operations during maintenance
   - Verify graceful degradation
   - Check error messages
   - Verify retry mechanisms

**Expected Results**:
- No data loss
- Clear error communication
- Recovery options available
- Consistent state maintained

## Accessibility Workflows

### E2E Test: Screen Reader Navigation

**Scenario**: Complete workflow using screen reader

**Test Steps**:
1. **Enable Screen Reader**
   - Activate device screen reader
   - Open application

2. **Navigate Templates**
   - Browse using swipe gestures
   - Hear template descriptions
   - Filter using voice commands

3. **Execute Workout**
   - Start workout with voice
   - Navigate exercises
   - Complete sets with gestures
   - Finish workout

**Expected Results**:
- All content readable
- Logical navigation order
- Clear action descriptions
- Full functionality available

## Integration Workflows

### E2E Test: Third-Party Fitness App Sync

**Scenario**: Workout data syncs with fitness platforms

**Test Steps**:
1. **Connect Fitness App**
   - Go to settings
   - Connect Apple Health/Google Fit
   - Grant permissions

2. **Execute Workout**
   - Complete full workout
   - Include various exercise types

3. **Verify Sync**
   - Check fitness app
   - Verify workout appears
   - Check calorie estimates
   - Verify duration matches

**Expected Results**:
- Automatic sync
- Accurate data transfer
- Proper categorization
- No duplicate entries

## Compliance Workflows

### E2E Test: Data Privacy Compliance

**Scenario**: Ensure GDPR compliance for EU users

**Test Steps**:
1. **New EU User Registration**
   - Set location to EU country
   - Verify consent prompts
   - Review data collection notice

2. **Data Export Request**
   - Request personal data export
   - Receive export within 48 hours
   - Verify completeness

3. **Data Deletion Request**
   - Request account deletion
   - Verify workout templates anonymized
   - Check data actually removed

**Expected Results**:
- Full compliance demonstrated
- Clear user controls
- Timely responses
- Audit trail maintained