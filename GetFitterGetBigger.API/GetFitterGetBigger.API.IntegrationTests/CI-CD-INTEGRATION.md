# CI/CD Integration Guide for BDD Tests

This guide explains how to integrate the BDD integration tests into your CI/CD pipeline.

## 📋 Prerequisites

### Required on CI/CD Agent
- .NET 9.0 SDK
- Docker (for TestContainers)
- Sufficient resources (minimum 2GB RAM for PostgreSQL container)

### Recommended Tools
- SpecFlow.Plus.LivingDoc.CLI (for generating living documentation)
- ReportGenerator (for coverage reports)

## 🐳 Docker Configuration

TestContainers requires Docker to be available. Ensure your CI/CD environment:

1. **Has Docker installed and running**
2. **User has permissions to run Docker**
3. **Can pull images from Docker Hub**

### Common CI/CD Platforms

| Platform | Docker Availability |
|----------|-------------------|
| GitHub Actions | ✅ Pre-installed |
| Azure DevOps | ✅ Pre-installed on Microsoft-hosted agents |
| GitLab CI | ✅ Available with Docker executor |
| Jenkins | ⚠️ Requires Docker plugin or agent setup |
| CircleCI | ✅ Available with machine executor |

## 🚀 Basic Pipeline Steps

### 1. Environment Setup
```bash
# Install .NET SDK
dotnet --version

# Verify Docker
docker --version
docker info
```

### 2. Build
```bash
dotnet restore
dotnet build --configuration Release
```

### 3. Run BDD Tests
```bash
dotnet test GetFitterGetBigger.API.IntegrationTests/GetFitterGetBigger.API.IntegrationTests.csproj \
  --configuration Release \
  --no-build \
  --logger "trx;LogFileName=bdd-tests.trx" \
  --logger "html;LogFileName=bdd-tests.html" \
  --collect:"XPlat Code Coverage"
```

### 4. Generate Reports

#### Living Documentation
```bash
# Install tool
dotnet tool install --global SpecFlow.Plus.LivingDoc.CLI

# Generate report
livingdoc test-assembly \
  GetFitterGetBigger.API.IntegrationTests/bin/Release/net9.0/GetFitterGetBigger.API.IntegrationTests.dll \
  -t TestResults/*.trx \
  --output TestResults/LivingDoc.html
```

#### Coverage Report
```bash
# Install tool
dotnet tool install --global dotnet-reportgenerator-globaltool

# Generate report
reportgenerator \
  -reports:"TestResults/**/coverage.cobertura.xml" \
  -targetdir:"TestResults/CoverageReport" \
  -reporttypes:"Html;Badges"
```

## 📊 Test Result Formats

### TRX (Visual Studio Test Results)
- Standard format for .NET tests
- Supported by most CI/CD platforms
- Use: `--logger trx`

### HTML Reports
- Human-readable test results
- Use: `--logger html`

### JUnit XML (for compatibility)
```bash
# Install JUnit logger
dotnet add package JunitXml.TestLogger

# Use in test command
--logger "junit;LogFilePath=TestResults/junit-results.xml"
```

## 🏷️ Test Filtering

### Run Specific Categories
```bash
# Run only smoke tests
dotnet test --filter "Category=smoke"

# Run non-slow tests
dotnet test --filter "Category!=slow"

# Run specific feature
dotnet test --filter "FullyQualifiedName~ExerciseManagement"
```

### Exclude Work-in-Progress
```bash
dotnet test --filter "Category!=wip"
```

## 📈 Performance Considerations

### Container Reuse
To speed up tests, consider:
1. **Caching Docker images**
2. **Running tests in parallel** (if supported)
3. **Using container pooling** (advanced)

### Resource Limits
```yaml
# Example: Limit container resources
environment:
  TESTCONTAINERS_DOCKER_SOCKET_OVERRIDE: /var/run/docker.sock
  TESTCONTAINERS_HOST_OVERRIDE: localhost
  TESTCONTAINERS_RYUK_DISABLED: true  # Disable cleanup for speed
```

## 🔧 Platform-Specific Examples

### GitHub Actions
See: `.github/workflows/bdd-tests.yml`

Key features:
- Automatic Docker support
- Artifact upload
- Test reporting with dorny/test-reporter

### Azure DevOps
See: `azure-pipelines-bdd.yml`

Key features:
- Living documentation generation
- Built-in test/coverage publishing
- Pipeline artifacts

### GitLab CI
```yaml
bdd-tests:
  image: mcr.microsoft.com/dotnet/sdk:9.0
  services:
    - docker:dind
  script:
    - dotnet test GetFitterGetBigger.API.IntegrationTests/*.csproj
  artifacts:
    reports:
      junit: TestResults/junit-results.xml
    paths:
      - TestResults/
```

### Jenkins
```groovy
pipeline {
    agent {
        docker {
            image 'mcr.microsoft.com/dotnet/sdk:9.0'
            args '-v /var/run/docker.sock:/var/run/docker.sock'
        }
    }
    stages {
        stage('Test') {
            steps {
                sh 'dotnet test --logger trx'
            }
            post {
                always {
                    mstest testResultsFile: '**/TestResults/*.trx'
                }
            }
        }
    }
}
```

## 🚨 Troubleshooting

### Docker Permission Denied
```bash
# Add user to docker group (Linux)
sudo usermod -aG docker $USER

# Or run with sudo (not recommended)
sudo dotnet test
```

### Container Startup Timeout
```bash
# Increase timeout
export TESTCONTAINERS_CONNECTION_TIMEOUT=180
export TESTCONTAINERS_COMMAND_TIMEOUT=180
```

### Cannot Pull Images
```bash
# Use specific registry
export TESTCONTAINERS_HUB_IMAGE_NAME_PREFIX=myregistry.io/
```

### Out of Disk Space
```bash
# Clean up old containers/images
docker system prune -a
```

## 📝 Best Practices

1. **Run BDD tests in a separate job/stage** - They're slower than unit tests
2. **Set appropriate timeouts** - Container startup can take time
3. **Archive test results** - Even if tests fail
4. **Generate living documentation** - Great for stakeholders
5. **Monitor test duration** - Track performance over time
6. **Use test filtering** - Run smoke tests on every commit, full suite on PR
7. **Cache Docker images** - Speeds up subsequent runs

## 🔗 Additional Resources

- [TestContainers CI/CD Guide](https://www.testcontainers.org/supported_docker_environments/)
- [SpecFlow LivingDoc](https://docs.specflow.org/projects/specflow-livingdoc/en/latest/)
- [Azure DevOps Test Reporting](https://docs.microsoft.com/en-us/azure/devops/pipelines/test/review-continuous-test-results-after-build)
- [GitHub Actions Test Reporting](https://github.com/marketplace/actions/test-reporter)