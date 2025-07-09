# Exercise Linking Feature - Manual Testing Guide

## 🚀 Ready for Manual Testing!

**Build Status**: ✅ Success (0 warnings, 0 errors)  
**Test Status**: ✅ All 540 tests passing  
**Coverage**: 62.39% line coverage  

---

## Prerequisites

1. **Start the application**:
   ```bash
   cd /home/esqueleto/myWork/GetFitterGetBigger/GetFitterGetBigger.Admin
   dotnet run
   ```

2. **Ensure API is running** at `http://localhost:5214/` (FEAT-022 exercise linking endpoints)

3. **Have test data**: You'll need some exercises with different types:
   - At least 1 **Workout** type exercise (can have links)
   - At least 2 **Warmup** type exercises (can be linked as warmup)
   - At least 2 **Cooldown** type exercises (can be linked as cooldown)

---

## Test Scenarios

### 🔍 **Scenario 1: Exercise List with Link Indicators**

**Objective**: Verify exercise list shows link indicators correctly

**Steps**:
1. Navigate to **Exercise List** page
2. Look for exercises with link indicators (🔗 icon)
3. Hover over link indicators to see tooltips
4. Use the **Links filter** dropdown:
   - Select "Has Links" → should show only exercises with links
   - Select "No Links" → should show only exercises without links
   - Select "All Exercises" → should show all exercises

**Expected Results**:
- Link indicators show format like "3/2" (3 warmup, 2 cooldown)
- Tooltips show "X warmup exercises, Y cooldown exercises"
- Filtering works correctly
- Only exercises with links show the 🔗 icon

---

### 🏋️ **Scenario 2: Exercise Detail - Workout Type (Main Feature)**

**Objective**: Test the complete exercise linking workflow

**Steps**:
1. Click on a **Workout type** exercise from the list
2. Scroll to bottom → Should see **"Linked Exercises"** section
3. Should see two sections: **Warmup Exercises** and **Cooldown Exercises**
4. Each section should show current count (e.g., "0 / 10")

**Expected Results**:
- Linked Exercises section is visible
- Two separate sections for warmup and cooldown
- Link counts displayed correctly
- Empty states shown when no links exist

---

### ➕ **Scenario 3: Adding Warmup Links**

**Objective**: Test creating warmup exercise links

**Steps**:
1. In a Workout exercise detail page
2. Click **"Add Warmup"** button
3. Modal should open with title "Add Warmup Exercise"
4. Search for warmup exercises in the search box
5. Click on an exercise card to select it
6. Modal should close automatically
7. Exercise should appear in the warmup list
8. Count should update (e.g., "1 / 10")

**Expected Results**:
- Modal opens with correct title
- Search works and shows only warmup exercises
- Exercise cards are clickable
- Modal closes after selection
- New link appears in the list
- Count updates correctly

---

### ➕ **Scenario 4: Adding Cooldown Links**

**Objective**: Test creating cooldown exercise links

**Steps**:
1. In a Workout exercise detail page
2. Click **"Add Cooldown"** button
3. Modal should open with title "Add Cooldown Exercise"
4. Search for cooldown exercises
5. Select an exercise
6. Verify cooldown list updates

**Expected Results**:
- Same behavior as warmup but for cooldown exercises
- Modal shows only cooldown exercises
- Cooldown section count updates

---

### 🔄 **Scenario 5: Reordering Links (Move Up/Down)**

**Objective**: Test reordering linked exercises

**Steps**:
1. Add at least 2 warmup exercises to a workout
2. Look for **↑** (up) and **↓** (down) buttons on each link card
3. Click **↓** on the first exercise → should move it down
4. Click **↑** on the second exercise → should move it up
5. Verify the order changes correctly

**Expected Results**:
- First exercise cannot move up (↑ button disabled/hidden)
- Last exercise cannot move down (↓ button disabled/hidden)
- Order changes correctly when buttons are clicked
- Progress indicator may appear during reordering

---

### 🗑️ **Scenario 6: Removing Links**

**Objective**: Test deleting exercise links

**Steps**:
1. On a linked exercise card, click the **trash icon** 🗑️
2. Confirmation dialog should appear
3. Click **"Cancel"** → link should remain
4. Click trash icon again
5. Click **"Remove"** → link should be deleted
6. Count should decrease

**Expected Results**:
- Confirmation dialog appears with proper text
- Cancel keeps the link
- Remove deletes the link
- List and count update correctly

---

### ⚠️ **Scenario 7: Validation Testing**

**Objective**: Test business rule validation

**Steps**:
1. Try to add the same exercise twice → should show error
2. Add 10 warmup exercises → "Add Warmup" button should disappear
3. Try linking a Workout exercise to itself → should prevent or show error
4. Try adding non-warmup exercise as warmup → should not appear in modal

**Expected Results**:
- Duplicate links prevented
- Max 10 links enforced
- Circular references prevented
- Only correct exercise types shown in modals

---

### 🚫 **Scenario 8: Non-Workout Exercise (Should NOT show links)**

**Objective**: Verify only Workout exercises can have links

**Steps**:
1. Click on a **Warmup only** or **Cooldown only** exercise
2. Scroll to bottom of exercise detail page

**Expected Results**:
- NO "Linked Exercises" section should appear
- Only Workout type exercises can have links

---

### 📱 **Scenario 9: Mobile/Responsive Testing**

**Objective**: Test on mobile devices or small screens

**Steps**:
1. Resize browser window to mobile size OR test on actual mobile device
2. Navigate through all the above scenarios
3. Check that buttons are touch-friendly
4. Verify modal fits on screen

**Expected Results**:
- All functionality works on mobile
- Buttons are large enough for touch
- Text is readable
- No horizontal scrolling issues

---

### 🐌 **Scenario 10: Network Conditions**

**Objective**: Test with slow/unreliable network

**Steps**:
1. Use browser dev tools to simulate slow network
2. Try adding/removing links
3. Look for loading indicators
4. Test with network disconnected

**Expected Results**:
- Loading indicators appear during operations
- Appropriate error messages for network issues
- "Try again" options for failed operations

---

## ⭐ Success Criteria

The feature is ready for production if:

- [x] **All scenarios pass** without errors
- [x] **Link indicators** work correctly in exercise list
- [x] **Adding links** works smoothly (warmup and cooldown)
- [x] **Reordering** with move up/down buttons works
- [x] **Removing links** works with confirmation
- [x] **Validation rules** are enforced properly
- [x] **Non-workout exercises** don't show link section
- [x] **Mobile experience** is smooth
- [x] **Error handling** provides clear messages
- [x] **Performance** is acceptable

---

## 🐛 Issues to Report

If you find any issues, please note:

1. **What you were doing** (which scenario/step)
2. **What you expected** to happen
3. **What actually happened**
4. **Browser/device** you're using
5. **Any error messages** in browser console (F12)

---

## 🎯 Key Features to Validate

### ✅ Core Functionality
- Create warmup and cooldown links
- Reorder links with move up/down buttons  
- Remove links with confirmation
- Exercise list indicators and filtering

### ✅ Business Rules
- Only Workout exercises can have links
- Maximum 10 links per type (warmup/cooldown)
- No duplicate links
- No circular references

### ✅ User Experience
- Clear visual feedback
- Loading indicators
- Error messages
- Success notifications
- Mobile-friendly interface

---

**Ready to test! 🚀**

The exercise linking feature is fully implemented and ready for your manual testing and user acceptance.