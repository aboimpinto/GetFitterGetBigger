# PowerShell script to run tests with automatic HTML coverage report generation

Write-Host "Running tests..." -ForegroundColor Yellow

# Run tests with coverage
dotnet test

# Check if tests passed
if ($LASTEXITCODE -ne 0) {
    Write-Host "Tests failed!" -ForegroundColor Red
    exit 1
}

Write-Host "Tests passed successfully!" -ForegroundColor Green

# Check if reportgenerator is installed
$reportGeneratorExists = $null -ne (Get-Command reportgenerator -ErrorAction SilentlyContinue)

if (-not $reportGeneratorExists) {
    Write-Host "ReportGenerator not found. HTML report will not be generated." -ForegroundColor Yellow
    Write-Host "Install it with: dotnet tool install -g dotnet-reportgenerator-globaltool" -ForegroundColor Yellow
    exit 0
}

# Find the coverage file
$coverageFile = Get-ChildItem -Path "." -Filter "coverage.cobertura.xml" -Recurse -ErrorAction SilentlyContinue | 
    Sort-Object LastWriteTime -Descending | 
    Select-Object -First 1

if (-not $coverageFile) {
    Write-Host "No coverage file found. HTML report will not be generated." -ForegroundColor Yellow
    exit 0
}

Write-Host "Generating HTML coverage report..." -ForegroundColor Yellow

# Generate HTML report
$reportArgs = @(
    "-reports:$($coverageFile.FullName)"
    "-targetdir:./TestResults/CoverageReport"
    "-reporttypes:Html"
    "-title:GetFitterGetBigger.Admin Coverage Report"
    "-verbosity:Error"
)

reportgenerator @reportArgs 2>$null

if ($LASTEXITCODE -eq 0) {
    Write-Host "Coverage report generated at: ./TestResults/CoverageReport/index.html" -ForegroundColor Green
    
    # Show coverage summary
    Write-Host "Coverage Summary:" -ForegroundColor Yellow
    dotnet test --no-build --no-restore 2>$null | Select-String -Pattern "^\| (Module|Total|Average|GetFitterGetBigger)" | Select-Object -Last 6
} else {
    Write-Host "Failed to generate HTML coverage report" -ForegroundColor Yellow
}