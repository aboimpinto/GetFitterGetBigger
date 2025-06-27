#!/bin/bash

# Colors for output
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
NC='\033[0m' # No Color

echo -e "${YELLOW}Running tests...${NC}"

# Run tests with coverage
dotnet test

# Check if tests passed
if [ $? -ne 0 ]; then
    echo -e "${RED}Tests failed!${NC}"
    exit 1
fi

echo -e "${GREEN}Tests passed successfully!${NC}"

# Check if reportgenerator is installed
if ! command -v reportgenerator &> /dev/null; then
    echo -e "${YELLOW}ReportGenerator not found. HTML report will not be generated.${NC}"
    echo -e "${YELLOW}Install it with: dotnet tool install -g dotnet-reportgenerator-globaltool${NC}"
    exit 0
fi

# Find the coverage file
COVERAGE_FILE=$(find . -name "coverage.cobertura.xml" -type f -printf '%T@ %p\n' 2>/dev/null | sort -n | tail -1 | cut -f2- -d" ")

if [ -z "$COVERAGE_FILE" ]; then
    echo -e "${YELLOW}No coverage file found. HTML report will not be generated.${NC}"
    exit 0
fi

echo -e "${YELLOW}Generating HTML coverage report...${NC}"

# Generate HTML report silently
reportgenerator \
    -reports:"$COVERAGE_FILE" \
    -targetdir:./TestResults/CoverageReport \
    -reporttypes:Html \
    -title:"GetFitterGetBigger.Admin Coverage Report" \
    -verbosity:Error 2>/dev/null

if [ $? -eq 0 ]; then
    echo -e "${GREEN}Coverage report generated at: ./TestResults/CoverageReport/index.html${NC}"
    
    # Show coverage summary from the test output
    echo -e "${YELLOW}Coverage Summary:${NC}"
    dotnet test --no-build --no-restore 2>/dev/null | grep -E "^\| (Module|Total|Average|GetFitterGetBigger)" | tail -6
else
    echo -e "${YELLOW}Failed to generate HTML coverage report${NC}"
fi