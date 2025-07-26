# UI List Page Design Standards

**🎯 PURPOSE**: This document defines the consistent UI design patterns for all list pages in the GetFitterGetBigger Admin application. These standards ensure a cohesive user experience with proper spacing, visual hierarchy, and calm aesthetics.

## 📐 Core Layout Structure

### Container Layout
```razor
<div>
    <div class="mb-4">
        <Breadcrumb Items="@breadcrumbItems" />
    </div>
    <div class="bg-white rounded-lg shadow-md p-6">
        @* Page content *@
    </div>
</div>
```

**Key Elements:**
- **Wrapper**: Simple div without container class to avoid alignment issues
- **Breadcrumb**: Separate component with 4px bottom margin  
- **Content Card**: White background with rounded corners, shadow, and 24px padding

**Note**: The MainLayout already provides `p-6` padding, so pages should not add additional padding. Avoid using `container` class as it can cause inconsistent horizontal alignment between pages.

### Page Header
```razor
<div class="flex justify-between items-center mb-6">
    <div>
        <h1 class="text-2xl font-semibold text-gray-800">Page Title</h1>
        <p class="text-gray-600">Brief description of page purpose</p>
    </div>
    <button class="bg-blue-600 hover:bg-blue-700 text-white font-medium py-2 px-4 rounded-lg transition-colors">
        Primary Action
    </button>
</div>
```

**Key Elements:**
- **Flex Layout**: Separates title/description from primary action button
- **Title**: H1 tag with 2xl size and semibold weight
- **Description**: Gray subtitle explaining the page's purpose
- **Primary Button**: Blue background with hover state and rounded corners

## 🎨 Filter Section Design

### Filter Grid Layout
```razor
<div class="mb-6 grid grid-cols-1 md:grid-cols-3 lg:grid-cols-5 gap-4">
    <div>
        <label class="block text-sm font-medium text-gray-700 mb-1">Filter Label</label>
        <input/select class="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500">
    </div>
    @* More filter fields *@
    <div class="flex items-end gap-2">
        <button class="bg-gray-600 hover:bg-gray-700 text-white font-medium py-2 px-4 rounded-md transition-colors">
            Apply
        </button>
        <button class="bg-gray-400 hover:bg-gray-500 text-white font-medium py-2 px-4 rounded-md transition-colors">
            Clear
        </button>
    </div>
</div>
```

**Key Elements:**
- **Responsive Grid**: Adjusts columns based on screen size
- **Labels**: Always above inputs with consistent styling
- **Input Styling**: Consistent border, padding, and focus states
- **Button Group**: Aligned to bottom with 8px gap between buttons

## 📋 Data Display Patterns

### Table Design
```razor
<div class="overflow-x-auto">
    <table class="min-w-full divide-y divide-gray-200">
        <thead class="bg-gray-50">
            <tr>
                <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Column Header
                </th>
            </tr>
        </thead>
        <tbody class="bg-white divide-y divide-gray-200">
            <tr class="hover:bg-gray-50">
                <td class="px-6 py-4 whitespace-nowrap text-sm">
                    Cell Content
                </td>
            </tr>
        </tbody>
    </table>
</div>
```

### Card Grid Design
```razor
<div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
    <div class="bg-gray-50 p-6 rounded-lg border border-gray-200 hover:shadow-lg transition-all duration-200 transform hover:-translate-y-1">
        <h3 class="text-lg font-medium text-gray-800 mb-3">Card Title</h3>
        <p class="text-sm text-gray-600 mb-4">Card description</p>
        <a class="inline-flex items-center text-blue-600 hover:text-blue-800 font-medium">
            Action Link
            <svg class="ml-2 h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5l7 7-7 7" />
            </svg>
        </a>
    </div>
</div>
```

## 🏷️ Empty State Design

```razor
<div class="text-center py-12">
    <svg class="mx-auto h-12 w-12 text-gray-400" fill="none" viewBox="0 0 24 24" stroke="currentColor" aria-hidden="true">
        @* Appropriate icon *@
    </svg>
    <h3 class="mt-2 text-sm font-medium text-gray-900">No items</h3>
    <p class="mt-1 text-sm text-gray-500">Get started by creating your first item.</p>
    <div class="mt-6">
        <button class="inline-flex items-center px-4 py-2 border border-transparent shadow-sm text-sm font-medium rounded-md text-white bg-blue-600 hover:bg-blue-700">
            <svg class="mr-2 h-5 w-5" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 20 20" fill="currentColor">
                <path fill-rule="evenodd" d="M10 3a1 1 0 011 1v5h5a1 1 0 110 2h-5v5a1 1 0 11-2 0v-5H4a1 1 0 110-2h5V4a1 1 0 011-1z" clip-rule="evenodd" />
            </svg>
            Create Item
        </button>
    </div>
</div>
```

**Key Elements:**
- **Vertical Padding**: 48px (py-12) for breathing room
- **Icon**: 48x48px gray icon appropriate to context
- **Text Hierarchy**: Small, medium weight title with gray descriptive text
- **CTA Button**: Matches primary action style with icon

## 🎯 Spacing Guidelines

### Consistent Spacing Values
- **Page Container Padding**: Provided by MainLayout (`p-6`)
- **Card Padding**: `p-6` (24px all sides)
- **Section Margins**: `mb-6` (24px) between major sections
- **Element Spacing**: `mb-4` (16px) for related elements
- **Inline Gaps**: `gap-2` (8px) for button groups and inline elements

### Visual Hierarchy
1. **Page Title**: Largest text (text-2xl), semibold
2. **Section Headers**: Large text (text-lg), medium weight
3. **Labels**: Small text (text-sm), medium weight
4. **Body Text**: Small to base text (text-sm/base), regular weight
5. **Helper Text**: Small text (text-sm), gray color

## 🎨 Color Palette

### Primary Colors
- **Primary Action**: `bg-blue-600 hover:bg-blue-700`
- **Secondary Action**: `bg-gray-600 hover:bg-gray-700`
- **Tertiary Action**: `bg-gray-400 hover:bg-gray-500`

### Status Colors
- **Active/Success**: `bg-green-100 text-green-800`
- **Inactive**: `bg-gray-100 text-gray-800`
- **Warning**: `bg-yellow-100 text-yellow-800`
- **Error**: `bg-red-100 text-red-800`

### Background Colors
- **Page Background**: Default (gray-50)
- **Card Background**: `bg-white`
- **Hover States**: `hover:bg-gray-50`
- **Table Headers**: `bg-gray-50`

## 🔄 Interactive Elements

### Hover Effects
- **Cards**: `hover:shadow-lg transition-all duration-200 transform hover:-translate-y-1`
- **Table Rows**: `hover:bg-gray-50`
- **Links**: Color change with transition
- **Buttons**: Darker shade of base color

### Focus States
- **Inputs**: `focus:outline-none focus:ring-2 focus:ring-blue-500`
- **Buttons**: Default browser focus or custom ring

## 📱 Responsive Behavior

### Breakpoint Strategy
- **Mobile First**: Start with single column layouts
- **Tablet (md)**: 2-3 columns for grids
- **Desktop (lg)**: Full column count
- **Wide (xl)**: Additional columns or expanded layouts

### Grid Responsiveness Examples
- **Cards**: `grid-cols-1 md:grid-cols-2 lg:grid-cols-3`
- **Filters**: `grid-cols-1 md:grid-cols-3 lg:grid-cols-5`
- **Tables**: Horizontal scroll on mobile with `overflow-x-auto`

## ✅ Implementation Checklist

When creating a new list page:

1. [ ] Use simple wrapper div (no container class)
2. [ ] Include breadcrumb navigation
3. [ ] Add white card container with shadow
4. [ ] Create header with title, description, and primary action
5. [ ] Use consistent filter layout with labels
6. [ ] Apply proper spacing between sections (mb-6)
7. [ ] Include empty state with icon, text, and CTA
8. [ ] Add hover effects to interactive elements
9. [ ] Ensure responsive grid layouts
10. [ ] Use consistent color palette for states and actions

## 🚀 Quick Start Template

```razor
@page "/yourpage"
@using GetFitterGetBigger.Admin.Components.Shared
@* Other usings *@

<PageTitle>Your Page - GetFitterGetBigger Admin</PageTitle>

<div>
    <div class="mb-4">
        <Breadcrumb Items="@breadcrumbItems" />
    </div>
    <div class="bg-white rounded-lg shadow-md p-6">
        @* Header *@
        <div class="flex justify-between items-center mb-6">
            <div>
                <h1 class="text-2xl font-semibold text-gray-800">Page Title</h1>
                <p class="text-gray-600">Page description</p>
            </div>
            <button class="bg-blue-600 hover:bg-blue-700 text-white font-medium py-2 px-4 rounded-lg transition-colors">
                Primary Action
            </button>
        </div>

        @* Filters *@
        <div class="mb-6 grid grid-cols-1 md:grid-cols-3 gap-4">
            @* Filter fields *@
        </div>

        @* Content *@
        @if (hasData)
        {
            @* Table or card grid *@
        }
        else
        {
            @* Empty state *@
        }
    </div>
</div>

@code {
    private List<Breadcrumb.BreadcrumbItem> breadcrumbItems = new();
    
    protected override void OnInitialized()
    {
        breadcrumbItems = new List<Breadcrumb.BreadcrumbItem>
        {
            new() { Text = "Home", Href = "/" },
            new() { Text = "Current Page" }
        };
    }
}
```

## 📚 References

- WorkoutTemplates page: Example of ideal implementation
- TailwindCSS documentation for utility classes
- Blazor component guidelines for structure

---

**Last Updated**: 2025-01-26
**Version**: 1.0