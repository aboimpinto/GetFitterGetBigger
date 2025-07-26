# UI Form Page Design Standards

**🎯 PURPOSE**: This document defines the consistent UI design patterns for all form pages (create/edit/view) in the GetFitterGetBigger Admin application. These standards ensure a cohesive user experience with proper spacing, visual hierarchy, and calm aesthetics that match our list pages.

## 📐 Core Layout Structure

### Form Page Layout (Create/Edit)
```razor
<div>
    <div class="container mx-auto px-4 py-6">
        <div class="mb-4">
            <Breadcrumb Items="@breadcrumbItems" />
        </div>
    </div>
    <div class="bg-white rounded-lg shadow-md p-6 max-w-4xl mx-auto mb-32">
        @* Form content *@
    </div>
</div>
```

**Key Elements for Forms:**
- **Breadcrumb Container**: Uses `container mx-auto` for consistent breadcrumb positioning
- **Form Card**: Centered with `max-w-4xl mx-auto` for focused editing experience
- **Bottom Margin**: `mb-32` to provide space for floating action buttons
- **Padding**: Container provides horizontal padding, card provides internal padding

### View Page Layout
```razor
<div>
    <div class="mb-4">
        <Breadcrumb Items="@breadcrumbItems" />
    </div>
    <div class="bg-white rounded-lg shadow-md p-6">
        @* View content *@
    </div>
</div>
```

**Key Elements for View Pages:**
- **Wrapper**: Simple div without container class for full-width content
- **Breadcrumb**: Direct placement with 4px bottom margin  
- **Content Card**: White background with rounded corners, shadow, and 24px padding
- **No max-width constraints**: Allow the content to flow naturally with MainLayout padding

**Important Distinction**: 
- **Form Pages (Create/Edit)**: Should be centered with max-width constraints for focused data entry
- **View/List Pages**: Should stretch to use available space for better data visibility

### Form Page Header
```razor
<div class="mb-6">
    <h1 class="text-2xl font-semibold text-gray-800">Page Title</h1>
    <p class="text-gray-600">Brief description of page purpose</p>
</div>
```

**Key Elements:**
- **Title**: H1 tag with 2xl size and semibold weight
- **Description**: Gray subtitle explaining the form's purpose
- **Bottom Margin**: 24px (mb-6) separation from form content

### View Page Header
```razor
<div class="mb-6">
    <div class="flex justify-between items-start">
        <div>
            <h1 class="text-3xl font-semibold text-gray-800">@item.Name</h1>
            <div class="mt-2 flex items-center space-x-4">
                @* Status badges and metadata *@
            </div>
        </div>
        <div class="flex space-x-2">
            <button class="px-4 py-2 bg-indigo-600 text-white rounded-md hover:bg-indigo-700">
                Edit
            </button>
            <button class="px-4 py-2 bg-red-600 text-white rounded-md hover:bg-red-700">
                Delete
            </button>
        </div>
    </div>
</div>
```

## 🎨 Form Design Patterns

### Form Section Structure
```razor
<div class="space-y-6">
    <!-- Section -->
    <div class="border-b pb-6">
        <h3 class="text-lg font-medium text-gray-900 mb-4">Section Title</h3>
        @* Section content *@
    </div>
    <!-- Last section without border -->
    <div class="pb-6">
        <h3 class="text-lg font-medium text-gray-900 mb-4">Last Section</h3>
        @* Section content *@
    </div>
</div>
```

**Key Elements:**
- **Section Spacing**: 24px gap between sections (space-y-6)
- **Section Border**: Gray bottom border except last section
- **Section Title**: H3 with lg size and medium weight
- **Section Padding**: 24px bottom padding (pb-6)

### Form Field Layout
```razor
<div class="grid grid-cols-1 gap-6">
    <div>
        <label for="fieldId" class="block text-sm font-medium text-gray-700 mb-1">
            Field Label @if (IsRequired) { <span class="text-red-500">*</span> }
        </label>
        <input type="text" id="fieldId" 
               class="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500">
        @if (hasError)
        {
            <p class="mt-1 text-sm text-red-600">Validation message</p>
        }
    </div>
</div>
```

### Grid Layouts for Multiple Fields
```razor
<!-- Two columns on medium screens -->
<div class="grid grid-cols-1 md:grid-cols-2 gap-6">
    @* Fields *@
</div>

<!-- Three columns on large screens -->
<div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
    @* Fields *@
</div>
```

## 🏷️ View Page Patterns

### Information Sections
```razor
<div class="space-y-6">
    <!-- Basic info section with background -->
    <div class="bg-gray-50 p-4 rounded-lg">
        <h3 class="text-lg font-medium text-gray-900 mb-2">Section Title</h3>
        <p class="text-gray-700">Content</p>
    </div>

    <!-- List section -->
    <div>
        <h3 class="text-lg font-medium text-gray-900 mb-2">List Title</h3>
        <div class="flex flex-wrap gap-2">
            @* Badges or items *@
        </div>
    </div>
</div>
```

### Badge Styles
```razor
<!-- Primary badge -->
<span class="inline-flex items-center px-3 py-1 rounded-full text-sm font-medium bg-blue-100 text-blue-800">
    Primary Label
</span>

<!-- Status badges -->
<span class="inline-flex items-center px-3 py-1 rounded-full text-sm font-medium bg-green-100 text-green-800">
    Active
</span>

<!-- Category badges -->
<span class="inline-flex items-center px-3 py-1 rounded-md text-sm bg-gray-200 text-gray-700">
    Category
</span>
```

## 🔄 Floating Action Buttons (Optional)

For forms that benefit from always-visible save/cancel buttons:

```razor
@* Cancel button *@
<div class="fixed bottom-8 z-50 left-4 xl:left-[calc(50%-41rem)]">
    <button type="button" class="flex flex-col items-center group">
        <div class="w-14 h-14 bg-gray-600 hover:bg-gray-700 rounded-full flex items-center justify-center shadow-lg transition-colors">
            <svg class="w-8 h-8 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12"></path>
            </svg>
        </div>
        <span class="text-xs text-gray-600 mt-1 font-medium">Cancel</span>
    </button>
</div>

@* Save button *@
<div class="fixed bottom-8 right-4 z-50 xl:right-[calc(50%-41rem)]">
    <button type="button" class="flex flex-col items-center group">
        <div class="w-14 h-14 bg-blue-600 hover:bg-blue-700 rounded-full flex items-center justify-center shadow-lg transition-colors">
            <svg class="w-8 h-8 text-white" fill="currentColor" viewBox="0 0 24 24">
                <path d="M17 3H5C3.89 3 3 3.9 3 5V19C3 20.1 3.89 21 5 21H19C20.1 21 21 20.1 21 19V7L17 3M19 19H5V5H16.17L19 7.83V19M12 12C10.34 12 9 13.34 9 15S10.34 18 12 18 15 16.66 15 15 13.66 12 12 12M6 6H15V10H6V6Z"/>
            </svg>
        </div>
        <span class="text-xs text-gray-600 mt-1 font-medium">Save</span>
    </button>
</div>
```

## 🚨 Error & Loading States

### Error Message Display
```razor
@if (ErrorMessage != null)
{
    <div class="bg-red-50 border border-red-200 text-red-700 px-4 py-3 rounded-lg mb-4">
        <p>@ErrorMessage</p>
        <button @onclick="ClearError" class="text-sm underline mt-1">Dismiss</button>
    </div>
}
```

### Loading State
```razor
@if (isLoading)
{
    <div class="flex justify-center py-8">
        <div class="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600"></div>
    </div>
}
```

## 📱 Responsive Behavior

### Consistent Alignment
- Remove all `container mx-auto` classes
- Remove all `max-w-*` constraints on main containers
- Let content flow naturally with MainLayout padding
- Use responsive grid layouts for form fields

### Mobile Considerations
- Stack form fields on mobile (grid-cols-1)
- Ensure buttons are thumb-friendly (min 44px touch target)
- Consider floating action buttons for long forms

## 🎯 Navigation Patterns

### Back Navigation
```razor
<div class="mt-8 flex justify-between">
    <button @onclick="NavigateToList" class="text-gray-600 hover:text-gray-800">
        ← Back to [List Name]
    </button>
</div>
```

### Breadcrumb Examples
```csharp
// For create pages
breadcrumbItems = new List<Breadcrumb.BreadcrumbItem>
{
    new() { Text = "Home", Href = "/" },
    new() { Text = "Exercises", Href = "/exercises" },
    new() { Text = "New Exercise" }
};

// For edit pages
breadcrumbItems = new List<Breadcrumb.BreadcrumbItem>
{
    new() { Text = "Home", Href = "/" },
    new() { Text = "Exercises", Href = "/exercises" },
    new() { Text = exercise.Name, Href = $"/exercises/{exercise.Id}" },
    new() { Text = "Edit" }
};

// For view pages
breadcrumbItems = new List<Breadcrumb.BreadcrumbItem>
{
    new() { Text = "Home", Href = "/" },
    new() { Text = "Exercises", Href = "/exercises" },
    new() { Text = exercise.Name }
};
```

## ✅ Implementation Checklist

When creating a new form/view page:

1. [ ] Use simple wrapper div (no container or max-width classes)
2. [ ] Include breadcrumb navigation with proper hierarchy
3. [ ] Add white card container with shadow
4. [ ] Use H1 for page title with consistent sizing
5. [ ] Group form fields into logical sections with borders
6. [ ] Apply consistent field labels and spacing
7. [ ] Include proper validation error displays
8. [ ] Add loading states where needed
9. [ ] Ensure responsive grid layouts for forms
10. [ ] Include back navigation at bottom of view pages

## 🚀 Quick Start Templates

### Form Page Template
```razor
@page "/items/new"
@page "/items/{Id}/edit"
@using GetFitterGetBigger.Admin.Components.Shared
@* Other usings *@

<PageTitle>@(IsEditMode ? "Edit Item" : "New Item") - GetFitterGetBigger Admin</PageTitle>

<div>
    <div class="container mx-auto px-4 py-6">
        <div class="mb-4">
            <Breadcrumb Items="@breadcrumbItems" />
        </div>
    </div>
    <div class="bg-white rounded-lg shadow-md p-6 max-w-4xl mx-auto mb-32">
        <div class="mb-6">
            <h1 class="text-2xl font-semibold text-gray-800">@(IsEditMode ? "Edit Item" : "Create New Item")</h1>
            <p class="text-gray-600">@(IsEditMode ? "Update item details" : "Add a new item")</p>
        </div>

        @if (ErrorMessage != null)
        {
            <div class="bg-red-50 border border-red-200 text-red-700 px-4 py-3 rounded-lg mb-4">
                <p>@ErrorMessage</p>
                <button @onclick="ClearError" class="text-sm underline mt-1">Dismiss</button>
            </div>
        }

        @if (isLoading)
        {
            <div class="flex justify-center py-8">
                <div class="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600"></div>
            </div>
        }
        else
        {
            <EditForm Model="@model" OnValidSubmit="@HandleSubmit">
                <div class="space-y-6">
                    @* Form sections *@
                </div>
            </EditForm>
        }
    </div>
</div>
```

### View Page Template
```razor
@page "/items/{Id}"
@using GetFitterGetBigger.Admin.Components.Shared
@* Other usings *@

<PageTitle>@(item?.Name ?? "Item") - GetFitterGetBigger Admin</PageTitle>

<div>
    <div class="mb-4">
        <Breadcrumb Items="@breadcrumbItems" />
    </div>
    <div class="bg-white rounded-lg shadow-md p-6">
        @if (isLoading)
        {
            <div class="flex justify-center py-8">
                <div class="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600"></div>
            </div>
        }
        else if (item != null)
        {
            <div class="mb-6">
                <div class="flex justify-between items-start">
                    <div>
                        <h1 class="text-3xl font-semibold text-gray-800">@item.Name</h1>
                        <div class="mt-2 flex items-center space-x-4">
                            @* Status badges *@
                        </div>
                    </div>
                    <div class="flex space-x-2">
                        <button class="px-4 py-2 bg-indigo-600 text-white rounded-md hover:bg-indigo-700">
                            Edit
                        </button>
                        <button class="px-4 py-2 bg-red-600 text-white rounded-md hover:bg-red-700">
                            Delete
                        </button>
                    </div>
                </div>
            </div>

            <div class="space-y-6">
                @* Content sections *@
            </div>

            <div class="mt-8">
                <button @onclick="NavigateToList" class="text-gray-600 hover:text-gray-800">
                    ← Back to Items
                </button>
            </div>
        }
    </div>
</div>
```

## 📚 References

- UI List Page Design Standards: For list page consistency
- TailwindCSS documentation for utility classes
- Blazor component guidelines for structure

---

**Last Updated**: 2025-01-26
**Version**: 1.0