# GetFitterGetBigger Admin Application

This is the admin web application for Personal Trainers to manage the GetFitterGetBigger platform.

## Quick Start

### Running Tests with Coverage Report

We provide convenient scripts that run tests and automatically generate HTML coverage reports:

**Linux/macOS:**
```bash
./test.sh
```

**Windows PowerShell:**
```powershell
./test.ps1
```

These scripts will:
- Run all unit tests
- Generate an HTML coverage report
- Display the coverage summary
- Report location: `TestResults/CoverageReport/index.html`

### Running the Application

```bash
dotnet run --project GetFitterGetBigger.Admin
```

The application will be available at:
- HTTP: http://localhost:5077
- HTTPS: https://localhost:7211

## Project Structure

```
GetFitterGetBigger.Admin/
├── GetFitterGetBigger.Admin/       # Main web application project
│   ├── Components/                 # Blazor components
│   ├── Models/                     # Data models
│   ├── Services/                   # Business logic services
│   └── wwwroot/                    # Static files
├── GetFitterGetBigger.Admin.Tests/ # Unit test project
├── memory-bank/                    # Project documentation
├── docs/                           # Technical documentation
├── test.sh                         # Test runner script (Linux/macOS)
├── test.ps1                        # Test runner script (Windows)
└── generate-coverage-report.sh/ps1 # Detailed coverage report generators
```

## Development

### Prerequisites

- .NET 9.0 SDK
- Node.js (for Tailwind CSS)
- ReportGenerator (for coverage reports): `dotnet tool install -g dotnet-reportgenerator-globaltool`

### Building

```bash
dotnet build
```

### Testing

Run tests with automatic HTML coverage report:
```bash
./test.sh  # Linux/macOS
./test.ps1 # Windows
```

Current test coverage: Check the latest HTML report for detailed metrics.

## Features

- **Authentication**: Google and Facebook OAuth integration
- **Authorization**: Role-based access control (PT-Tier, Admin-Tier)
- **Reference Tables**: Management of system reference data
- **Tailwind CSS**: Modern utility-first CSS framework

## Configuration

- API Base URL is configured in `appsettings.json`
- OAuth providers are configured in `appsettings.Development.json`

## Contributing

1. Create a feature branch from master
2. Follow the Feature Implementation Process (see memory-bank/FEATURE_IMPLEMENTATION_PROCESS.md)
3. Ensure all tests pass and coverage remains high
4. Submit a pull request

## Documentation

- Test guidelines: See GetFitterGetBigger.Admin.Tests/README.md
- Feature documentation: See memory-bank/features/
- API integration: See memory-bank/api-*.md files
- Code quality standards: See memory-bank/CODE_QUALITY_STANDARDS.md and ADMIN-CODE_QUALITY_STANDARDS.md
- Code review process: See memory-bank/CODE_REVIEW_PROCESS.md

## Claude AI Assistant

This project includes custom slash commands for Claude AI to assist with development:

- `/feature-code-review` - Comprehensive feature review with Blazor-specific checks
- `/component-code-review` - Detailed review of Blazor components
- `/service-code-review` - Review of service implementations
- `/new-page` - Create a new Blazor page following patterns
- `/new-component` - Create a reusable component
- `/start-implementing` - Begin feature implementation
- `/catch-up` - Get up to speed with project context

See `.claude/commands/README.md` for detailed command documentation.