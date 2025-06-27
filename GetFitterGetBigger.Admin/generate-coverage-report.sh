#!/bin/bash

# Colors for output
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

echo -e "${YELLOW}Running tests with code coverage...${NC}"

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage" --results-directory ./TestResults /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura /p:CoverletOutput=./TestResults/

# Check if tests passed
if [ $? -ne 0 ]; then
    echo -e "${YELLOW}Tests failed. Exiting...${NC}"
    exit 1
fi

echo -e "${GREEN}Tests passed successfully!${NC}"
echo -e "${YELLOW}Generating HTML coverage report...${NC}"

# Find the latest coverage file
COVERAGE_FILE=$(find ./TestResults -name "coverage.cobertura.xml" -type f -printf '%T@ %p\n' | sort -n | tail -1 | cut -f2- -d" ")

if [ -z "$COVERAGE_FILE" ]; then
    echo "No coverage file found!"
    exit 1
fi

# Generate HTML report
reportgenerator \
    -reports:"$COVERAGE_FILE" \
    -targetdir:./TestResults/CoverageReport \
    -reporttypes:Html \
    -title:"GetFitterGetBigger.Admin Coverage Report" \
    -verbosity:Info

# Check if report was generated successfully
if [ $? -eq 0 ]; then
    echo -e "${GREEN}Coverage report generated successfully!${NC}"
    echo -e "${GREEN}Report location: ./TestResults/CoverageReport/index.html${NC}"
    
    # Try to open the report in the default browser
    if command -v xdg-open &> /dev/null; then
        echo -e "${YELLOW}Opening report in browser...${NC}"
        xdg-open ./TestResults/CoverageReport/index.html
    elif command -v open &> /dev/null; then
        echo -e "${YELLOW}Opening report in browser...${NC}"
        open ./TestResults/CoverageReport/index.html
    else
        echo -e "${YELLOW}Please open ./TestResults/CoverageReport/index.html in your browser${NC}"
    fi
else
    echo -e "${YELLOW}Failed to generate coverage report${NC}"
    exit 1
fi