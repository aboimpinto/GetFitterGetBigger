# CSS Media Queries Cannot Be Used in Inline Styles

## Discovery Date
2025-07-04

## Issue Description
When attempting to implement responsive positioning for floating action buttons in the Exercise form, we discovered that CSS `@media` queries do not work when placed inside inline `style` attributes.

## The Problem
```html
<!-- This DOES NOT work -->
<div style="left: calc(50% - 25rem); @media (min-width: 1280px) { left: calc(50% - 25rem) !important; }">
```

The browser ignores everything after the first semicolon because:
- The `style` attribute is designed to hold only simple CSS property-value pairs
- Media queries, pseudo-classes, and other CSS rules are not valid in this context
- The browser parses `style` as a simple list of declarations, not a full CSS ruleset

## The Solution
Move responsive styles to CSS classes and use a `<style>` block:

```razor
@* In your Razor component *@
<div class="fixed bottom-8 z-50 cancel-button-position">
    <!-- Button content -->
</div>

<style>
    /* Default positioning for small screens */
    .cancel-button-position {
        left: 1rem;
    }

    /* Large screen positioning */
    @@media (min-width: 1280px) {
        .cancel-button-position {
            left: calc(50% - 25rem) !important;
        }
    }
</style>
```

## Key Points
1. **Inline styles are limited**: Only property-value pairs are allowed
2. **Use CSS classes**: For any responsive behavior, use classes instead of inline styles
3. **Blazor syntax**: Remember to escape `@` with `@@` in Blazor style blocks
4. **Browser behavior**: The browser silently ignores invalid CSS in style attributes

## Example Implementation
See `/Components/Pages/Exercises/ExerciseForm.razor` for a working implementation of responsive floating buttons using this pattern.

## Lessons Learned
- Always test responsive CSS thoroughly
- When media queries don't seem to work, check if they're in inline styles
- CSS classes provide more flexibility than inline styles
- Browser developer tools won't show errors for invalid inline styles