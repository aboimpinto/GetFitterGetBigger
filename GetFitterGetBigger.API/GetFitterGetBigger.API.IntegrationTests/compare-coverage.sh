#!/bin/bash

# Test Coverage Comparison Script
# Compares coverage between original tests and BDD tests

set -e

echo "🧪 Integration Test Coverage Comparison"
echo "======================================"

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Configuration
ORIGINAL_PROJECT="GetFitterGetBigger.API.Tests"
BDD_PROJECT="GetFitterGetBigger.API.IntegrationTests"
RESULTS_DIR="TestResults"
BASELINE_COVERAGE=89.99

echo -e "${BLUE}📊 Running baseline tests (original)...${NC}"

# Run original tests with coverage
dotnet test $ORIGINAL_PROJECT \
    --configuration Release \
    --collect:"XPlat Code Coverage" \
    --results-directory $RESULTS_DIR/Original \
    --logger "trx;LogFileName=original-tests.trx" \
    --verbosity minimal

echo -e "${BLUE}📊 Running BDD integration tests...${NC}"

# Run BDD tests with coverage
dotnet test $BDD_PROJECT \
    --configuration Release \
    --collect:"XPlat Code Coverage" \
    --results-directory $RESULTS_DIR/BDD \
    --logger "trx;LogFileName=bdd-tests.trx" \
    --verbosity minimal

echo -e "${YELLOW}📈 Analyzing coverage results...${NC}"

# Function to extract coverage from cobertura.xml
extract_coverage() {
    local file=$1
    if [ -f "$file" ]; then
        # Extract line coverage percentage from cobertura.xml
        coverage=$(grep -o 'line-rate="[0-9.]*"' "$file" | head -1 | grep -o '[0-9.]*')
        if [ -n "$coverage" ]; then
            # Convert to percentage
            echo "scale=2; $coverage * 100" | bc
        else
            echo "0.00"
        fi
    else
        echo "N/A"
    fi
}

# Find coverage files
ORIGINAL_COVERAGE=$(find $RESULTS_DIR/Original -name "coverage.cobertura.xml" | head -1)
BDD_COVERAGE=$(find $RESULTS_DIR/BDD -name "coverage.cobertura.xml" | head -1)

# Extract coverage percentages
if [ -f "$ORIGINAL_COVERAGE" ]; then
    ORIGINAL_PCT=$(extract_coverage "$ORIGINAL_COVERAGE")
else
    ORIGINAL_PCT="N/A"
fi

if [ -f "$BDD_COVERAGE" ]; then
    BDD_PCT=$(extract_coverage "$BDD_COVERAGE")
else
    BDD_PCT="N/A"
fi

echo ""
echo "📋 Coverage Comparison Report"
echo "============================"
echo -e "Original Tests Coverage: ${GREEN}${ORIGINAL_PCT}%${NC}"
echo -e "BDD Tests Coverage:      ${GREEN}${BDD_PCT}%${NC}"
echo -e "Baseline Target:         ${BLUE}${BASELINE_COVERAGE}%${NC}"

# Calculate difference if both values are available
if [ "$ORIGINAL_PCT" != "N/A" ] && [ "$BDD_PCT" != "N/A" ]; then
    DIFF=$(echo "scale=2; $BDD_PCT - $ORIGINAL_PCT" | bc)
    if (( $(echo "$DIFF >= 0" | bc -l) )); then
        echo -e "Coverage Difference:     ${GREEN}+${DIFF}%${NC} ✅"
    else
        echo -e "Coverage Difference:     ${RED}${DIFF}%${NC} ⚠️"
    fi
    
    # Check if BDD coverage meets baseline
    if (( $(echo "$BDD_PCT >= $BASELINE_COVERAGE" | bc -l) )); then
        echo -e "Baseline Check:          ${GREEN}PASS${NC} ✅"
    else
        echo -e "Baseline Check:          ${RED}FAIL${NC} ❌"
        echo -e "  ${RED}BDD coverage ($BDD_PCT%) is below baseline ($BASELINE_COVERAGE%)${NC}"
    fi
fi

echo ""
echo "📊 Test Count Comparison"
echo "======================="

# Count tests in TRX files
count_tests_in_trx() {
    local trx_file=$1
    if [ -f "$trx_file" ]; then
        # Count UnitTestResult elements
        grep -c '<UnitTestResult' "$trx_file" 2>/dev/null || echo "0"
    else
        echo "N/A"
    fi
}

ORIGINAL_TRX=$(find $RESULTS_DIR/Original -name "*.trx" | head -1)
BDD_TRX=$(find $RESULTS_DIR/BDD -name "*.trx" | head -1)

ORIGINAL_TEST_COUNT=$(count_tests_in_trx "$ORIGINAL_TRX")
BDD_TEST_COUNT=$(count_tests_in_trx "$BDD_TRX")

echo "Original Tests:          $ORIGINAL_TEST_COUNT"
echo "BDD Tests:               $BDD_TEST_COUNT"

# Integration test specific counts
echo ""
echo "🔍 Integration Test Analysis"
echo "============================"

# Count integration tests specifically
INTEGRATION_TESTS=$(find GetFitterGetBigger.API.Tests/IntegrationTests -name "*.cs" | wc -l)
CONTROLLER_TESTS=$(find GetFitterGetBigger.API.Tests/Controllers -name "*Tests.cs" | wc -l)
TOTAL_INTEGRATION=$(( $INTEGRATION_TESTS + $CONTROLLER_TESTS ))

echo "Integration Test Files:  $INTEGRATION_TESTS"
echo "Controller Test Files:   $CONTROLLER_TESTS" 
echo "Total Integration:       $TOTAL_INTEGRATION"

# Count BDD feature files
BDD_FEATURES=$(find GetFitterGetBigger.API.IntegrationTests/Features -name "*.feature" 2>/dev/null | wc -l)
BDD_DISABLED=$(find GetFitterGetBigger.API.IntegrationTests/Features -name "*.feature.disabled" 2>/dev/null | wc -l)

echo "BDD Feature Files:       $BDD_FEATURES"
echo "BDD Disabled Files:      $BDD_DISABLED"

# Migration progress
if [ $TOTAL_INTEGRATION -gt 0 ]; then
    MIGRATION_PCT=$(echo "scale=1; $BDD_FEATURES * 100 / $TOTAL_INTEGRATION" | bc)
    echo -e "Migration Progress:      ${YELLOW}${MIGRATION_PCT}%${NC} ($BDD_FEATURES/$TOTAL_INTEGRATION)"
fi

echo ""
echo "📁 Generated Reports"
echo "==================="
echo "Original Coverage:       $ORIGINAL_COVERAGE"
echo "BDD Coverage:           $BDD_COVERAGE"
echo "Original TRX:           $ORIGINAL_TRX"
echo "BDD TRX:                $BDD_TRX"

# Generate summary report
SUMMARY_FILE="$RESULTS_DIR/coverage-comparison-summary.md"
cat > "$SUMMARY_FILE" << EOF
# Coverage Comparison Summary

**Date**: $(date)
**Baseline Target**: ${BASELINE_COVERAGE}%

## Coverage Results
| Test Suite | Coverage | Test Count | Status |
|------------|----------|------------|---------|
| Original Tests | ${ORIGINAL_PCT}% | $ORIGINAL_TEST_COUNT | ✅ |
| BDD Tests | ${BDD_PCT}% | $BDD_TEST_COUNT | $([ "$BDD_PCT" != "N/A" ] && (( $(echo "$BDD_PCT >= $BASELINE_COVERAGE" | bc -l) )) && echo "✅" || echo "⚠️") |

## Migration Progress
- **Integration Test Files**: $TOTAL_INTEGRATION
- **BDD Feature Files**: $BDD_FEATURES
- **Migration Complete**: ${MIGRATION_PCT:-0}%

## Files Generated
- Original Coverage: \`$ORIGINAL_COVERAGE\`
- BDD Coverage: \`$BDD_COVERAGE\`
- This Report: \`$SUMMARY_FILE\`

EOF

echo -e "${GREEN}📄 Summary report saved: $SUMMARY_FILE${NC}"

echo ""
echo -e "${GREEN}✅ Coverage comparison complete!${NC}"

# Exit with error if BDD coverage is below baseline (when coverage is available)
if [ "$BDD_PCT" != "N/A" ] && (( $(echo "$BDD_PCT < $BASELINE_COVERAGE" | bc -l) )); then
    echo -e "${RED}❌ BDD coverage is below baseline threshold${NC}"
    exit 1
fi

exit 0