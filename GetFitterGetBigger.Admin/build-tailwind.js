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
