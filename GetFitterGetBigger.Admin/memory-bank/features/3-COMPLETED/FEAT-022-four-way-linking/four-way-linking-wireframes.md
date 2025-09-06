# Four-Way Exercise Linking Feature Wireframes

Generated: 2025-08-28
Feature: FEAT-030 - Four-Way Exercise Linking System

## 1. Enhanced Exercise Detail Page Overview

```
┌─────────────────────────────────────────────────────────┐
│ div (wrapper)                                           │
│ ┌───────────────────────────────────────────────────┐  │
│ │ Breadcrumb (mb-4)                                 │  │
│ │ [Home] > [Exercises] > [Barbell Squat]           │  │
│ └───────────────────────────────────────────────────┘  │
│ ┌───────────────────────────────────────────────────┐  │
│ │ bg-white rounded-lg shadow-md p-6                 │  │
│ │                                                   │  │
│ │ [EXISTING CONTENT: Header, Description, etc...]  │  │
│ │                                                   │  │
│ │ ┌─────────────────────────────────────────────┐   │  │
│ │ │ NEW: Four-Way Exercise Linking Section      │   │  │
│ │ │ (Replaces current "Linked Exercises")       │   │  │
│ │ │ ┌─────────────────────────────────────────┐ │   │  │
│ │ │ │ Context-Aware Linking Interface         │ │   │  │
│ │ │ │ (Shows different options based on       │ │   │  │
│ │ │ │  exercise types)                        │ │   │  │
│ │ │ └─────────────────────────────────────────┘ │   │  │
│ │ └─────────────────────────────────────────────┘   │  │
│ │                                                   │  │
│ │ [Back to Exercises] (mt-8)                        │  │
│ └───────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────┘

Component Notes:
- Replaces lines 233-240 in ExerciseDetail.razor
- No longer restricted to "Workout" type exercises
- Shows for ANY exercise type with contextual options
```

## 2. Context-Aware Four-Way Linking Variations

### 2.1 Workout-Only Exercise (Current Enhanced)

```
┌─────────────────────────────────────────────────────────┐
│ div class="mt-6" data-testid="four-way-linking-manager" │
│ ┌───────────────────────────────────────────────────┐  │
│ │ bg-white shadow-lg rounded-lg overflow-hidden     │  │
│ │ ┌─────────────────────────────────────────────┐   │  │
│ │ │ px-4 sm:px-6 py-3 sm:py-4 bg-emerald-50    │   │  │
│ │ │ border-b border-gray-200                    │   │  │
│ │ │ ┌─────────────────────────────────────────┐ │   │  │
│ │ │ │ Workout Exercise Context                │ │   │  │
│ │ │ │ h2 text-lg font-semibold + icons        │ │   │  │
│ │ │ │ 🔗 Four-Way Exercise Links             │ │   │  │
│ │ │ └─────────────────────────────────────────┘ │   │  │
│ │ └─────────────────────────────────────────────┘   │  │
│ │ ┌─────────────────────────────────────────────┐   │  │
│ │ │ p-4 sm:p-6 (main content)                  │   │  │
│ │ │ ┌─────────────────────────────────────────┐ │   │  │
│ │ │ │ Warmup Section (bg-orange-50)           │ │   │  │
│ │ │ │ "Warmup Exercises" + count + [Add]      │ │   │  │
│ │ │ │ [Exercise Cards with reorder controls]  │ │   │  │
│ │ │ └─────────────────────────────────────────┘ │   │  │
│ │ │ ┌─────────────────────────────────────────┐ │   │  │
│ │ │ │ space-y-6 gap                           │ │   │  │
│ │ │ └─────────────────────────────────────────┘ │   │  │
│ │ │ ┌─────────────────────────────────────────┐ │   │  │
│ │ │ │ Cooldown Section (bg-blue-50)           │ │   │  │
│ │ │ │ "Cooldown Exercises" + count + [Add]    │ │   │  │
│ │ │ │ [Exercise Cards with reorder controls]  │ │   │  │
│ │ │ └─────────────────────────────────────────┘ │   │  │
│ │ │ ┌─────────────────────────────────────────┐ │   │  │
│ │ │ │ NEW: Alternative Section (bg-purple-50) │ │   │  │
│ │ │ │ "Alternative Workouts" + count + [Add]  │ │   │  │
│ │ │ │ [Exercise Cards with remove only]       │ │   │  │
│ │ │ └─────────────────────────────────────────┘ │   │  │
│ │ └─────────────────────────────────────────────┘   │  │
│ └───────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────┘

New Implementation:
- Component: FourWayExerciseLinkManager.razor
- Colors: emerald-50 header, orange-50, blue-50, purple-50 sections
- Alternatives: No ordering (unlike warmup/cooldown)
- Alternative validation: Must be same type (workout->workout)
```

### 2.2 Warmup-Only Exercise

```
┌─────────────────────────────────────────────────────────┐
│ div class="mt-6" data-testid="four-way-linking-manager" │
│ ┌───────────────────────────────────────────────────┐  │
│ │ bg-white shadow-lg rounded-lg overflow-hidden     │  │
│ │ ┌─────────────────────────────────────────────┐   │  │
│ │ │ px-4 sm:px-6 py-3 sm:py-4 bg-orange-50     │   │  │
│ │ │ 🔥 Warmup Exercise Links                    │   │  │
│ │ └─────────────────────────────────────────────┘   │  │
│ │ ┌─────────────────────────────────────────────┐   │  │
│ │ │ p-4 sm:p-6                                  │   │  │
│ │ │ ┌─────────────────────────────────────────┐ │   │  │
│ │ │ │ Workouts Section (bg-emerald-50)        │ │   │  │
│ │ │ │ "Workouts that use this warmup"         │ │   │  │
│ │ │ │ [Exercise Cards - view only]            │ │   │  │
│ │ │ └─────────────────────────────────────────┘ │   │  │
│ │ │ ┌─────────────────────────────────────────┐ │   │  │
│ │ │ │ Alternative Section (bg-orange-100)     │ │   │  │
│ │ │ │ "Alternative Warmups" + [Add]           │ │   │  │
│ │ │ │ [Exercise Cards with remove only]       │ │   │  │
│ │ │ └─────────────────────────────────────────┘ │   │  │
│ │ └─────────────────────────────────────────────┘   │  │
│ └───────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────┘

Key Differences:
- Header: orange-50 theme for warmup context
- Workouts: Read-only view (auto-populated from reverse links)
- Alternatives: warmup->warmup only, no ordering
- Info text: "These workouts use this as a warmup exercise"
```

### 2.3 Cooldown-Only Exercise

```
┌─────────────────────────────────────────────────────────┐
│ div class="mt-6" data-testid="four-way-linking-manager" │
│ ┌───────────────────────────────────────────────────┐  │
│ │ bg-white shadow-lg rounded-lg overflow-hidden     │  │
│ │ ┌─────────────────────────────────────────────┐   │  │
│ │ │ px-4 sm:px-6 py-3 sm:py-4 bg-blue-50       │   │  │
│ │ │ ❄️ Cooldown Exercise Links                   │   │  │
│ │ └─────────────────────────────────────────────┘   │  │
│ │ ┌─────────────────────────────────────────────┐   │  │
│ │ │ p-4 sm:p-6                                  │   │  │
│ │ │ ┌─────────────────────────────────────────┐ │   │  │
│ │ │ │ Workouts Section (bg-emerald-50)        │ │   │  │
│ │ │ │ "Workouts that use this cooldown"       │ │   │  │
│ │ │ │ [Exercise Cards - view only]            │ │   │  │
│ │ │ └─────────────────────────────────────────┘ │   │  │
│ │ │ ┌─────────────────────────────────────────┐ │   │  │
│ │ │ │ Alternative Section (bg-blue-100)       │ │   │  │
│ │ │ │ "Alternative Cooldowns" + [Add]         │ │   │  │
│ │ │ │ [Exercise Cards with remove only]       │ │   │  │
│ │ │ └─────────────────────────────────────────┘ │   │  │
│ │ └─────────────────────────────────────────────┘   │  │
│ └───────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────┘

Key Differences:
- Header: blue-50 theme for cooldown context
- Workouts: Read-only view (auto-populated from reverse links)
- Alternatives: cooldown->cooldown only, no ordering
```

### 2.4 Multi-Type Exercise (Complex Scenario)

```
┌─────────────────────────────────────────────────────────┐
│ div class="mt-6" data-testid="four-way-linking-manager" │
│ ┌───────────────────────────────────────────────────┐  │
│ │ bg-white shadow-lg rounded-lg overflow-hidden     │  │
│ │ ┌─────────────────────────────────────────────┐   │  │
│ │ │ px-4 sm:px-6 py-3 sm:py-4 bg-indigo-50     │   │  │
│ │ │ 🎯 Multi-Type Exercise Links                │   │  │
│ │ │ p class="text-sm": Exercise: "Burpees"     │   │  │
│ │ │ p class="text-xs": Types: Workout, Warmup  │   │  │
│ │ └─────────────────────────────────────────────┘   │  │
│ │ ┌─────────────────────────────────────────────┐   │  │
│ │ │ p-4 sm:p-6                                  │   │  │
│ │ │ ┌─────────────────────────────────────────┐ │   │  │
│ │ │ │ Context Selector Tabs (mb-4)            │ │   │  │
│ │ │ │ ┌─────────────┐ ┌─────────────────────┐ │ │   │  │
│ │ │ │ │[As Workout] │ │[As Warmup] (active) │ │ │   │  │
│ │ │ │ │bg-gray-100  │ │bg-indigo-100       │ │ │   │  │
│ │ │ │ └─────────────┘ └─────────────────────┘ │ │   │  │
│ │ │ └─────────────────────────────────────────┘ │   │  │
│ │ │ ┌─────────────────────────────────────────┐ │   │  │
│ │ │ │ Active Context Content                  │ │   │  │
│ │ │ │ (Shows warmup sections in this case)   │ │   │  │
│ │ │ │ [Workouts that use this as warmup]     │ │   │  │
│ │ │ │ [Alternative warmups]                   │ │   │  │
│ │ │ └─────────────────────────────────────────┘ │   │  │
│ │ └─────────────────────────────────────────────┘   │  │
│ └───────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────┘

Implementation Notes:
- Context tabs: role="tablist" with proper ARIA
- Active tab: bg-indigo-100 text-indigo-800
- Inactive tab: bg-gray-100 text-gray-600 hover:bg-gray-200
- Content changes based on selected context
- State preserved during session
```

## 3. Enhanced Add Link Modal

```
┌─────────────────────────────────────────────────────────┐
│ Modal Backdrop (fixed inset-0 bg-gray-500 bg-opacity-75)│
│ ┌───────────────────────────────────────────────────┐  │
│ │ Modal Container (max-w-3xl) - Wider for filters   │  │
│ │ ┌─────────────────────────────────────────────┐   │  │
│ │ │ bg-white px-4 pb-4 pt-5 sm:p-6 sm:pb-4     │   │  │
│ │ │ ┌───────────────────────────────────────┐   │   │  │
│ │ │ │ h3: Add Alternative Exercise         │   │   │  │
│ │ │ │ text-lg font-semibold                │   │   │  │
│ │ │ └───────────────────────────────────────┘   │   │  │
│ │ │ ┌───────────────────────────────────────┐   │   │  │
│ │ │ │ Type Compatibility Info (NEW)        │   │   │  │
│ │ │ │ bg-purple-50 p-3 rounded-md          │   │   │  │
│ │ │ │ "Showing workout exercises that can  │   │   │  │
│ │ │ │  be used as alternatives"            │   │   │  │
│ │ │ └───────────────────────────────────────┘   │   │  │
│ │ │ ┌───────────────────────────────────────┐   │   │  │
│ │ │ │ Enhanced Search & Filters            │   │   │  │
│ │ │ │ ┌─────────────────────────────────┐   │   │   │  │
│ │ │ │ │ Search Input (w-full)           │   │   │   │  │
│ │ │ │ └─────────────────────────────────┘   │   │   │  │
│ │ │ │ ┌─────────────────────────────────┐   │   │   │  │
│ │ │ │ │ Quick Filters (grid-cols-3)     │   │   │   │  │
│ │ │ │ │ [Difficulty▼] [Equipment▼]     │   │   │   │  │
│ │ │ │ │ [Body Part▼]                   │   │   │   │  │
│ │ │ │ └─────────────────────────────────┘   │   │   │  │
│ │ │ └───────────────────────────────────────┘   │   │  │
│ │ │ ┌───────────────────────────────────────┐   │   │  │
│ │ │ │ Results Section (max-h-80)           │   │   │  │
│ │ │ │ ┌─────────────────────────────────┐   │   │   │  │
│ │ │ │ │ Exercise Cards (enhanced)       │   │   │   │  │
│ │ │ │ │ ┌─────┐ Barbell Squat          │   │   │   │  │
│ │ │ │ │ │TYPE │ Difficulty: Intermediate│   │   │   │  │
│ │ │ │ │ │ WO  │ Equipment: Barbell     │   │   │   │  │
│ │ │ │ │ └─────┘ ✓ Compatible            │   │   │   │  │
│ │ │ │ └─────────────────────────────────┘   │   │   │  │
│ │ │ │ ┌─────────────────────────────────┐   │   │   │  │
│ │ │ │ │ [Dumbbell Squat] [Select]       │   │   │   │  │
│ │ │ │ └─────────────────────────────────┘   │   │   │  │
│ │ │ └───────────────────────────────────────┘   │   │  │
│ │ └─────────────────────────────────────────────┘   │  │
│ │ ┌─────────────────────────────────────────────┐   │  │
│ │ │ bg-gray-50 px-4 py-3 (action buttons)      │   │  │
│ │ │ [Cancel] [Add Alternative] (disabled)       │   │  │
│ │ └─────────────────────────────────────────────┘   │  │
│ └───────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────┘

New Features:
- Type compatibility badge in each result
- Quick filter dropdowns
- Enhanced exercise cards with type badges
- Already linked indicator (grayed out)
- Better visual hierarchy
```

## 4. Alternative Exercise Cards (No Ordering)

```
┌─────────────────────────────────────────────────────────┐
│ Alternative Exercise Card (different from warmup/cooldown)│
│ ┌───────────────────────────────────────────────────┐  │
│ │ bg-white border border-purple-200 p-4 rounded-lg │  │
│ │ ┌─────────────────────────────────────────────┐   │  │
│ │ │ flex items-start justify-between            │   │  │
│ │ │ ┌─────────────────────────────────────────┐ │   │  │
│ │ │ │ flex-1 (exercise info)                  │ │   │  │
│ │ │ │ h4: Dumbbell Squat                      │ │   │  │
│ │ │ │ └─────────────────────────────────────┘ │   │   │  │
│ │ │ │ flex text-xs text-gray-500 space-x-4   │ │   │  │
│ │ │ │ span: Difficulty: Intermediate          │ │   │  │
│ │ │ │ span: Equipment: Dumbbells              │ │   │  │
│ │ │ │ ┌─────────────────────────────────────┐ │ │   │  │
│ │ │ │ │ Type badges (mt-2)                  │ │ │   │  │
│ │ │ │ │ [Workout] [Compound]               │ │ │   │  │
│ │ │ │ └─────────────────────────────────────┘ │ │   │  │
│ │ │ └─────────────────────────────────────────┘ │   │  │
│ │ │ ┌─────────────────────────────────────────┐ │   │  │
│ │ │ │ Actions (flex-shrink-0)                 │ │   │  │
│ │ │ │ [View Exercise] [Remove Link]           │ │   │  │
│ │ │ │ text-sm links with icons                │ │   │  │
│ │ │ └─────────────────────────────────────────┘ │   │  │
│ │ └─────────────────────────────────────────────┘   │  │
│ └───────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────┘

Key Differences from Warmup/Cooldown Cards:
- No reorder arrows (alternatives have no sequence)
- Purple accent colors (border-purple-200)
- "View Exercise" link to navigate to alternative
- Different data-testid: "alternative-exercise-card"
- No move up/down handlers in component
```

## 5. Empty States for Each Context

### 5.1 Workout Empty State

```
┌─────────────────────────────────────────────────────────┐
│ Warmup Section Empty State                              │
│ ┌───────────────────────────────────────────────────┐  │
│ │ text-center py-12 text-gray-500                   │  │
│ │ ┌─────────────────────────────────────────────┐   │  │
│ │ │ svg mx-auto h-12 w-12 text-orange-400       │  │
│ │ │ (Warmup flame icon)                         │  │
│ │ │ h3 mt-2 text-sm font-medium                 │  │
│ │ │ "No warmup exercises linked"                │  │
│ │ │ p mt-1 text-sm                              │  │
│ │ │ "Add exercises that prepare for this        │  │
│ │ │  workout movement"                          │  │
│ │ │ div mt-6                                    │  │
│ │ │ [+ Add First Warmup] (orange button)       │  │
│ │ │ inline-flex items-center px-4 py-2         │  │
│ │ │ bg-orange-600 text-white rounded-md        │  │
│ │ └─────────────────────────────────────────────┘   │  │
│ └───────────────────────────────────────────────────┘  │
│                                                        │
│ Alternative Section Empty State                         │
│ ┌───────────────────────────────────────────────────┐  │
│ │ text-center py-8 text-gray-500                    │  │
│ │ svg: puzzle piece icon (h-8 w-8 purple-400)       │  │
│ │ p mt-2: "No alternative exercises"                 │  │
│ │ p text-xs: "Link similar workout exercises"        │  │
│ │ [+ Add Alternative] (purple button, smaller)       │  │
│ └───────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────┘
```

### 5.2 Warmup/Cooldown Empty State

```
┌─────────────────────────────────────────────────────────┐
│ Workouts Section Empty State (for warmup/cooldown types)│
│ ┌───────────────────────────────────────────────────┐  │
│ │ text-center py-8 text-gray-400                    │  │
│ │ svg: info icon (h-8 w-8)                          │  │
│ │ p mt-2 text-sm: "Not used in any workouts yet"    │  │
│ │ p text-xs: "This appears automatically when       │  │
│ │              workout exercises link to this"       │  │
│ └───────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────┘

Color Coding:
- Warmup context: orange-400 for icons, orange-600 for buttons
- Cooldown context: blue-400 for icons, blue-600 for buttons  
- Alternative: purple-400 for icons, purple-600 for buttons
```

## 6. Responsive Design Breakpoints

### 6.1 Desktop (lg: 1024px+)

```
┌─────────────────────────────────────────────────────────┐
│ Full Four-Way Layout                                    │
│ ┌───────────────────────────────────────────────────┐  │
│ │ Header with full icon and description             │  │
│ └───────────────────────────────────────────────────┘  │
│ ┌─────────────────────────────────────────────────────┐ │
│ │ grid grid-cols-2 gap-6 (for alternatives)          │ │
│ │ ┌─────────────────────┐ ┌─────────────────────────┐ │ │
│ │ │ Warmup Section      │ │ Cooldown Section        │ │ │
│ │ │ (full width)        │ │ (full width)            │ │ │
│ │ └─────────────────────┘ └─────────────────────────┘ │ │
│ │ ┌─────────────────────────────────────────────────┐ │ │
│ │ │ Alternatives Section (full span)                │ │ │
│ │ └─────────────────────────────────────────────────┘ │ │
│ └─────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────┘
```

### 6.2 Tablet (md: 768px)

```
┌─────────────────────────────────────┐
│ Tablet Layout                           │
│ ┌─────────────────────────────────────┐ │
│ │ Header (smaller text, responsive)   │ │
│ └─────────────────────────────────────┘ │
│ ┌─────────────────────────────────────┐ │
│ │ Single column sections              │ │
│ │ ┌─────────────────────────────────┐ │ │
│ │ │ Warmup Section                  │ │ │
│ │ └─────────────────────────────────┘ │ │
│ │ ┌─────────────────────────────────┐ │ │
│ │ │ Cooldown Section                │ │ │
│ │ └─────────────────────────────────┘ │ │
│ │ ┌─────────────────────────────────┐ │ │
│ │ │ Alternatives Section            │ │ │
│ │ └─────────────────────────────────┘ │ │
│ └─────────────────────────────────────┘ │
└─────────────────────────────────────────┘

Adjustments:
- px-3 instead of px-4 for tighter spacing
- text-base instead of text-lg for headers
- gap-4 instead of gap-6 between sections
```

### 6.3 Mobile (sm: <768px)

```
┌─────────────────────────────────┐
│ Mobile Layout                   │
│ ┌─────────────────────────────┐ │
│ │ Compact Header              │ │
│ │ Icon + title only           │ │
│ │ Description hidden on XS    │ │
│ └─────────────────────────────┘ │
│ ┌─────────────────────────────┐ │
│ │ Sections stack vertically   │ │
│ │ ┌─────────────────────────┐ │ │
│ │ │ Section Headers         │ │ │
│ │ │ [Title] [Count] [Add]   │ │ │
│ │ │ (responsive flex)       │ │ │
│ │ └─────────────────────────┘ │ │
│ │ ┌─────────────────────────┐ │ │
│ │ │ Exercise Cards          │ │ │
│ │ │ (full width)            │ │ │
│ │ └─────────────────────────┘ │ │
│ └─────────────────────────────┘ │
└─────────────────────────────────┘

Mobile Features:
- Touch-friendly buttons (min-h-11)
- Simpler exercise cards
- Fewer visible details
- Swipe gestures for reordering (future)
```

## 7. Interaction State Wireframes

### 7.1 Adding a Link (Optimistic Update)

```
┌─────────────────────────────────────────────────────────┐
│ State 1: User clicks "Add Alternative" button          │
│ ┌───────────────────────────────────────────────────┐  │
│ │ [+ Add Alternative] -> [Adding...] ⟳                │  │
│ │ bg-purple-600 -> bg-purple-400 opacity-75           │  │
│ │ disabled="true"                                      │  │
│ └───────────────────────────────────────────────────┘  │
│                                                        │
│ State 2: Modal opens with search                       │
│ ┌───────────────────────────────────────────────────┐  │
│ │ AddExerciseLinkModal IsOpen="true"                   │  │
│ │ LinkType="Alternative" (new value)                   │  │
│ │ FilterByCompatibleTypes="true" (new prop)            │  │
│ └───────────────────────────────────────────────────┘  │
│                                                        │
│ State 3: User selects exercise -> Optimistic UI        │
│ ┌───────────────────────────────────────────────────┐  │
│ │ New exercise card appears immediately                │  │
│ │ ┌─────────────────────────────────────────────────┐ │  │
│ │ │ Dumbbell Squat (opacity-75)                     │ │  │
│ │ │ "Adding..." text in corner                      │ │  │
│ │ │ Actions disabled                                 │ │  │
│ │ └─────────────────────────────────────────────────┘ │  │
│ └───────────────────────────────────────────────────┘  │
│                                                        │
│ State 4: API success -> Full opacity                   │
│ │ State 5: API error -> Remove card + error toast      │
└─────────────────────────────────────────────────────────┘
```

### 7.2 Removing a Link (Confirmation)

```
┌─────────────────────────────────────────────────────────┐
│ State 1: User clicks "Remove" on exercise card         │
│ ┌───────────────────────────────────────────────────┐  │
│ │ Delete Confirmation Modal                            │  │
│ │ ┌─────────────────────────────────────────────────┐ │  │
│ │ │ ⚠️ Remove Alternative Exercise                   │ │  │
│ │ │                                                 │ │  │
│ │ │ Remove "Dumbbell Squat" from alternative        │ │  │
│ │ │ exercises? This will also remove the reverse    │ │  │
│ │ │ link automatically.                             │ │  │
│ │ │                                                 │ │  │
│ │ │ [Cancel] [Remove Link] (focus here)             │ │  │
│ │ └─────────────────────────────────────────────────┘ │  │
│ └───────────────────────────────────────────────────┘  │
│                                                        │
│ State 2: User confirms -> Remove animation              │
│ ┌───────────────────────────────────────────────────┐  │
│ │ Exercise card fades out (opacity-50)                │  │
│ │ "Removing..." overlay                               │  │
│ └───────────────────────────────────────────────────┘  │
│                                                        │
│ State 3: Success -> Card gone + toast notification     │
│ State 4: Error -> Card restored + error message        │
└─────────────────────────────────────────────────────────┘
```

### 7.3 Loading States

```
┌─────────────────────────────────────────────────────────┐
│ Loading State: Initial Load                             │
│ ┌───────────────────────────────────────────────────┐  │
│ │ bg-white shadow-lg rounded-lg overflow-hidden     │  │
│ │ ┌─────────────────────────────────────────────┐   │  │
│ │ │ Header loads immediately                    │   │  │
│ │ │ 🔗 Four-Way Exercise Links                 │   │  │
│ │ └─────────────────────────────────────────────┘   │  │
│ │ ┌─────────────────────────────────────────────┐   │  │
│ │ │ Loading content area                        │   │  │
│ │ │ ┌─────────────────────────────────────────┐ │   │  │
│ │ │ │ text-center py-8                       │ │   │  │
│ │ │ │ animate-spin h-8 w-8 border-b-2        │ │   │  │
│ │ │ │ border-gray-900                        │ │   │  │
│ │ │ │ "Loading exercise links..."            │ │   │  │
│ │ │ └─────────────────────────────────────────┘ │   │  │
│ │ └─────────────────────────────────────────────┘   │  │
│ └───────────────────────────────────────────────────┘  │
│                                                        │
│ Error State: Loading Failed                            │
│ ┌───────────────────────────────────────────────────┐  │
│ │ bg-red-50 border border-red-200 p-4               │  │
│ │ ⚠️ Error loading exercise links                     │  │
│ │ [Try Again] button                                   │  │
│ └───────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────┘
```

## 8. Implementation Specifications

### 8.1 Component Structure

```
Components/Pages/Exercises/ExerciseLinks/
├── FourWayExerciseLinkManager.razor (NEW - replaces ExerciseLinkManager)
├── ContextualLinkSections.razor (NEW - handles different exercise type contexts)
├── AlternativeExerciseCard.razor (NEW - different from warmup/cooldown cards)
├── ExerciseContextSelector.razor (NEW - tabs for multi-type exercises)
├── EnhancedAddExerciseLinkModal.razor (ENHANCED - better filtering)
└── ExerciseLinkCard.razor (EXISTING - keep for warmup/cooldown)
```

### 8.2 Key Tailwind Classes by Context

```css
/* Context Theme Colors */
.workout-context { @apply bg-emerald-50 text-emerald-800; }
.warmup-context { @apply bg-orange-50 text-orange-800; }
.cooldown-context { @apply bg-blue-50 text-blue-800; }
.alternative-context { @apply bg-purple-50 text-purple-800; }
.multi-context { @apply bg-indigo-50 text-indigo-800; }

/* Section Headers */
.section-header { @apply px-4 sm:px-6 py-3 sm:py-4 border-b border-gray-200; }

/* Exercise Cards */
.alternative-card { @apply bg-white border border-purple-200 p-4 rounded-lg; }
.alternative-card:hover { @apply shadow-md border-purple-300; }

/* Buttons by Context */
.btn-warmup { @apply bg-orange-600 hover:bg-orange-700 text-white; }
.btn-cooldown { @apply bg-blue-600 hover:bg-blue-700 text-white; }
.btn-alternative { @apply bg-purple-600 hover:bg-purple-700 text-white; }

/* Context Tabs */
.context-tab-active { @apply bg-indigo-100 text-indigo-800 border-indigo-300; }
.context-tab-inactive { @apply bg-gray-100 text-gray-600 hover:bg-gray-200; }
```

### 8.3 State Management Updates

```csharp
// IExerciseLinkStateService enhancements needed:
public interface IExerciseLinkStateService 
{
    // Existing properties...
    
    // NEW: Alternative links
    IEnumerable<ExerciseLinkDto> AlternativeLinks { get; }
    int AlternativeLinkCount { get; }
    
    // NEW: Context switching for multi-type exercises
    string ActiveContext { get; set; } // "Workout", "Warmup", "Cooldown"
    bool HasMultipleContexts { get; }
    IEnumerable<string> AvailableContexts { get; }
    
    // NEW: Type-specific loading
    Task InitializeForExerciseAsync(string exerciseId, string exerciseName, IEnumerable<string> exerciseTypes);
    Task LoadAlternativeLinksAsync();
    
    // NEW: Alternative CRUD
    Task CreateAlternativeLinkAsync(CreateExerciseLinkDto dto);
    Task DeleteAlternativeLinkAsync(string linkId);
    
    // NEW: Context switching
    Task SwitchContextAsync(string contextType);
}
```

### 8.4 API Endpoints Required

```
GET /api/exercises/{id}/links?linkType=ALTERNATIVE
- Returns alternative exercise links

POST /api/exercises/{id}/links
- Body: { targetExerciseId, linkType: "ALTERNATIVE" }
- Creates bidirectional alternative link

DELETE /api/exercises/links/{linkId}
- Removes link and its reverse

GET /api/exercises/{id}/links?linkType=WORKOUT
- Returns workout exercises for warmup/cooldown context
```

### 8.5 Validation Rules Implementation

```csharp
public class AlternativeExerciseLinkValidation 
{
    public static ValidationResult ValidateAlternativeLink(
        ExerciseDto sourceExercise, 
        ExerciseDto targetExercise)
    {
        // Rule: Alternatives must share at least one exercise type
        var sourceTypes = sourceExercise.ExerciseTypes.Select(t => t.Value);
        var targetTypes = targetExercise.ExerciseTypes.Select(t => t.Value);
        
        if (!sourceTypes.Intersect(targetTypes).Any())
        {
            return ValidationResult.Failure(
                "Alternative exercises must share at least one exercise type.");
        }
        
        // Rule: Cannot self-reference
        if (sourceExercise.Id == targetExercise.Id)
        {
            return ValidationResult.Failure(
                "An exercise cannot be an alternative to itself.");
        }
        
        return ValidationResult.Success();
    }
}
```

## 9. Accessibility Features

```
<!-- ARIA Labels for Four-Way Linking -->
<div role="region" 
     aria-labelledby="four-way-linking-heading"
     aria-describedby="four-way-linking-description">
     
<h2 id="four-way-linking-heading">Exercise Relationships</h2>
<p id="four-way-linking-description" class="sr-only">
  Manage warmup, cooldown, and alternative exercises
</p>

<!-- Context Tabs -->
<div role="tablist" aria-label="Exercise contexts">
  <button role="tab" 
          aria-selected="true"
          aria-controls="workout-context-panel"
          id="workout-context-tab">
    As Workout Exercise
  </button>
</div>

<!-- Live Region for Updates -->
<div aria-live="polite" aria-atomic="true" class="sr-only">
  @StateService.ScreenReaderAnnouncement
</div>

<!-- Keyboard Navigation -->
@onkeydown="HandleKeyNavigation"
<!-- Support: Tab, Enter, Space, Arrow keys for cards and tabs -->
```

## 10. Key Differences from Current Implementation

1. **Universal Availability**: Shows for ALL exercise types, not just "Workout"
2. **Context-Aware Interface**: UI adapts based on exercise types
3. **Alternative Linking**: New bidirectional relationship without ordering
4. **Enhanced Modal**: Better filtering and type compatibility indicators  
5. **Multi-Type Support**: Tabbed interface for exercises with multiple types
6. **Immediate Persistence**: Click to add/remove with optimistic UI updates
7. **Type Validation**: Prevents incompatible alternative links
8. **Reverse Link Display**: Shows auto-generated reverse relationships
9. **Progressive Disclosure**: Starts simple, reveals complexity as needed
10. **Mobile-First Design**: Touch-friendly responsive interface

This comprehensive wireframe set provides developers with detailed visual specifications for implementing the Four-Way Exercise Linking feature that meets the PT workflow requirements while maintaining GetFitterGetBigger's design standards.