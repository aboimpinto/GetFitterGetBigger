# Release Process

This document defines the process for creating releases based on Product Increments (PI) using completed features and fixed bugs.

## Release Structure

```
memory-bank/releases/
├── CURRENT_PI.md           # Indicates current PI
├── PI-2025-Q1/            # Q1 2025 Release
├── PI-2025-Q2/            # Q2 2025 Release
├── PI-2025-Q3/            # Q3 2025 Release
└── PI-2025-Q4/            # Q4 2025 Release
```

## Product Increment (PI) Overview

- **Duration**: 3 months (quarterly)
- **Planning**: First week of each quarter
- **Development**: Weeks 2-11
- **Release Prep**: Week 12
- **Release**: Last week of quarter

## Release Process Steps

### 1. Pre-Release Checklist (Week 11)

```markdown
## Pre-Release Checklist for [PI-YYYY-QX]

- [ ] All planned features in COMPLETED state
- [ ] All critical/high bugs in FIXED state
- [ ] All tests passing in CI/CD
- [ ] Documentation updated
- [ ] Performance benchmarks acceptable
- [ ] Security scan completed
- [ ] Database migrations tested
- [ ] Rollback plan prepared
```

### 2. Generate Release Contents

Run this process to prepare release:

1. **Copy Completed Features**
   ```bash
   cp -r memory-bank/features/3-COMPLETED/* memory-bank/releases/[CURRENT-PI]/features/
   ```

2. **Copy Fixed Bugs**
   ```bash
   cp -r memory-bank/bugs/3-FIXED/* memory-bank/releases/[CURRENT-PI]/bugs/
   ```

3. **Generate Release Notes**
   - Create `RELEASE_NOTES.md` in PI folder
   - Extract summaries from all included items
   - Format for different audiences

### 3. Release Notes Template

```markdown
# Release Notes - [PI-YYYY-QX]

## Version: [X.Y.Z]
## Release Date: [Date]
## Code Name: [Optional]

## 🎯 Release Highlights
[2-3 sentence summary of major achievements]

## ✨ New Features

### [Feature Category 1]
#### FEAT-001: [Feature Name]
- **Description**: [Brief description]
- **Business Value**: [Why it matters]
- **Documentation**: [Link to docs]

### [Feature Category 2]
[Additional features...]

## 🐛 Bug Fixes

### Critical Fixes
- **BUG-001**: [Bug description] - [Impact statement]

### High Priority Fixes
[List of high priority bug fixes]

### Other Fixes
[List of medium/low priority fixes]

## 📊 Technical Improvements
- Performance improvements
- Security enhancements
- Infrastructure updates

## 🔄 Breaking Changes
[List any breaking changes with migration guides]

## 📋 Known Issues
[List any known issues not fixed in this release]

## 🚀 Upgrade Instructions
1. Backup database
2. Apply migrations: `dotnet ef database update`
3. Update configuration files
4. Deploy new version
5. Run verification tests

## 📈 Metrics
- Features Delivered: X
- Bugs Fixed: Y
- Test Coverage: Z%
- Performance Improvement: N%

## 🙏 Acknowledgments
[Team members and contributors]

## 📅 Next Release Preview
[Teaser for next PI]
```

### 4. Version Numbering

Follow Semantic Versioning:
- **Major (X.0.0)**: Breaking changes
- **Minor (0.X.0)**: New features (typical PI release)
- **Patch (0.0.X)**: Bug fixes only

Standard PI releases are usually Minor versions.

### 5. Release Execution

1. **Create Release Branch**
   ```bash
   git checkout -b release/[version]
   ```

2. **Tag Release**
   ```bash
   git tag -a v[X.Y.Z] -m "Release [PI-YYYY-QX]: [Summary]"
   ```

3. **Archive Completed Work**
   - Move released features to archive after 1 PI
   - Keep release folder intact for reference

### 6. Post-Release Activities

1. **Reset for Next PI**
   - Update `CURRENT_PI.md`
   - Clear COMPLETED and FIXED folders
   - Plan next PI features

2. **Retrospective**
   - Create `RETROSPECTIVE.md` in PI folder
   - Document lessons learned
   - Identify process improvements

## Release Folder Structure

```
PI-2025-Q1/
├── RELEASE_NOTES.md
├── RELEASE_CHECKLIST.md
├── RETROSPECTIVE.md
├── features/
│   ├── exercise-management/
│   ├── user-authentication/
│   └── api-documentation/
├── bugs/
│   ├── BUG-001-exercise-isactive/
│   └── BUG-002-auth-token-expiry/
└── scripts/
    ├── deploy.sh
    └── rollback.sh
```

## Automated Release Notes Generation

Create a script to generate release notes:

```bash
#!/bin/bash
# generate-release-notes.sh

PI=$1
OUTPUT="memory-bank/releases/$PI/RELEASE_NOTES.md"

echo "# Release Notes - $PI" > $OUTPUT
echo "" >> $OUTPUT

# Extract features
echo "## New Features" >> $OUTPUT
for feature in memory-bank/releases/$PI/features/*; do
    if [ -d "$feature" ]; then
        # Extract feature info from feature-description.md
        # Format and append to release notes
    fi
done

# Extract bugs
echo "## Bug Fixes" >> $OUTPUT
for bug in memory-bank/releases/$PI/bugs/*; do
    if [ -d "$bug" ]; then
        # Extract bug info from bug-report.md
        # Format and append to release notes
    fi
done
```

## Release Communication

1. **Internal Release Notes**: Technical details for team
2. **Customer Release Notes**: User-facing features and fixes
3. **API Changelog**: For API consumers
4. **Migration Guide**: If breaking changes exist

## Emergency Hotfix Process

For critical bugs between PIs:

1. Create hotfix branch from last release tag
2. Fix bug following bug process
3. Create patch release (0.0.X increment)
4. Document in `HOTFIXES.md` in current PI folder
5. Cherry-pick fix to main branch

## Release Metrics Tracking

Track for each release:
- Planned vs Delivered features
- Bug discovery rate
- Time to fix critical bugs
- Release preparation time
- Post-release issue rate

## Best Practices

1. **Feature Freeze**: 2 weeks before release
2. **Code Freeze**: 1 week before release
3. **No Surprises**: All stakeholders informed early
4. **Rollback Ready**: Always have rollback plan
5. **Celebrate**: Acknowledge team achievements

## Release Approval Chain

1. Development Team Lead - Technical approval
2. QA Lead - Quality approval
3. Product Owner - Feature approval
4. DevOps Lead - Deployment approval
5. Stakeholders - Final sign-off