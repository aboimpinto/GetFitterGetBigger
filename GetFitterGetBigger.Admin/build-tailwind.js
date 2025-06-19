const fs = require('fs');
const path = require('path');
const postcss = require('postcss');
const tailwindcss = require('tailwindcss');
const autoprefixer = require('autoprefixer');

// Paths
const inputFile = path.join(__dirname, 'wwwroot', 'css', 'tailwind.css');
const outputFile = path.join(__dirname, 'wwwroot', 'css', 'app.css');

// Read the input file
const css = fs.readFileSync(inputFile, 'utf8');

// Process the CSS with PostCSS, Tailwind, and Autoprefixer
postcss([
  tailwindcss,
  autoprefixer
])
  .process(css, { from: inputFile, to: outputFile })
  .then(result => {
    // Write the processed CSS to the output file
    fs.writeFileSync(outputFile, result.css);
    console.log('Tailwind CSS built successfully!');
  })
  .catch(error => {
    console.error('Error building Tailwind CSS:', error);
  });
