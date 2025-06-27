# PowerShell script to generate code coverage report

Write-Host "Running tests with code coverage..." -ForegroundColor Yellow

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage" --results-directory ./TestResults /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura /p:CoverletOutput=./TestResults/

# Check if tests passed
if ($LASTEXITCODE -ne 0) {
    Write-Host "Tests failed. Exiting..." -ForegroundColor Red
    exit 1
}

Write-Host "Tests passed successfully!" -ForegroundColor Green
Write-Host "Generating HTML coverage report..." -ForegroundColor Yellow

# Find the latest coverage file
$coverageFile = Get-ChildItem -Path "./TestResults" -Filter "coverage.cobertura.xml" -Recurse | 
    Sort-Object LastWriteTime -Descending | 
    Select-Object -First 1

if (-not $coverageFile) {
    Write-Host "No coverage file found!" -ForegroundColor Red
    exit 1
}

# Generate HTML report
reportgenerator `
    -reports:"$($coverageFile.FullName)" `
    -targetdir:"./TestResults/CoverageReport" `
    -reporttypes:Html `
    -title:"GetFitterGetBigger.Admin Coverage Report" `
    -verbosity:Info

# Check if report was generated successfully
if ($LASTEXITCODE -eq 0) {
    Write-Host "Coverage report generated successfully!" -ForegroundColor Green
    Write-Host "Report location: ./TestResults/CoverageReport/index.html" -ForegroundColor Green
    
    # Try to open the report in the default browser
    $reportPath = Join-Path (Get-Location) "TestResults/CoverageReport/index.html"
    if (Test-Path $reportPath) {
        Write-Host "Opening report in browser..." -ForegroundColor Yellow
        Start-Process $reportPath
    }
} else {
    Write-Host "Failed to generate coverage report" -ForegroundColor Red
    exit 1
}