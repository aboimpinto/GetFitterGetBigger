# Tailwind CSS Integration Guide

## Overview

This document provides information about the Tailwind CSS integration in the GetFitterGetBigger Admin application, including common issues and their solutions.

## Current Setup

The application uses Tailwind CSS for styling with the following configuration:

- **Tailwind Version**: 3.3.5
- **Build Process**: Uses Tailwind CLI via a Node.js script
- **CSS Source**: `wwwroot/css/tailwind.css`
- **CSS Output**: `wwwroot/css/app.css`

## Common Issues and Solutions

### Issue: Tailwind CSS Not Applied / Unstyled Application

If the application appears unstyled or Tailwind classes are not being applied, check the following:

1. **Check Build Process**:
   - Ensure the Tailwind CSS build process is working correctly
   - Run `npm run css:build` to manually build the CSS file
   - Check for any errors in the build output

2. **Check Package Versions**:
   - Ensure the package.json has the correct versions:
   ```json
   "devDependencies": {
     "autoprefixer": "^10.4.14",
     "postcss": "^8.4.24",
     "tailwindcss": "^3.3.5"
   }
   ```
   - If versions are incorrect, update them and run `npm install`

3. **Check CSS References**:
   - Ensure App.razor is referencing the correct CSS file:
   ```html
   <link rel="stylesheet" href="css/app.css" />
   ```
   - Avoid using CDN versions if you're building locally

4. **Check Build Script**:
   - The build-tailwind.js should use the Tailwind CLI directly:
   ```javascript
   const { execSync } = require('child_process');
   const path = require('path');

   // Paths
   const inputFile = path.join('wwwroot', 'css', 'tailwind.css');
   const outputFile = path.join('wwwroot', 'css', 'app.css');

   try {
     // Run the Tailwind CLI to build the CSS
     execSync(`npx tailwindcss -i ${inputFile} -o ${outputFile}`);
     console.log('Tailwind CSS built successfully!');
   } catch (error) {
     console.error('Error building Tailwind CSS:', error.message);
     process.exit(1);
   }
   ```

### Issue: PostCSS Plugin Error

If you see an error like:
```
It looks like you're trying to use `tailwindcss` directly as a PostCSS plugin. The PostCSS plugin has moved to a separate package...
```

This indicates that you're trying to use Tailwind CSS directly as a PostCSS plugin, which is no longer supported. The solution is to:

1. Update the build script to use the Tailwind CLI directly (as shown above)
2. Update package.json to use the correct build command:
   ```json
   "scripts": {
     "css:build": "npx tailwindcss -i ./wwwroot/css/tailwind.css -o ./wwwroot/css/app.css"
   }
   ```

## Maintenance

To keep the Tailwind CSS integration working correctly:

1. **Regular Updates**:
   - Periodically update Tailwind CSS and related packages
   - Test thoroughly after updates

2. **Build Process**:
   - The build process is integrated with the .NET build pipeline
   - Manual rebuilds can be done with `npm run css:build`

3. **Custom Styles**:
   - Custom styles can be added to `wwwroot/css/tailwind.css`
   - The Tailwind configuration can be customized in `tailwind.config.js`
