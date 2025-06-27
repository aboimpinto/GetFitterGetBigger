# Memory Bank - Development Workflow System

This folder contains the complete development workflow management system for the GetFitterGetBigger API project.

## 📁 Folder Structure Overview

```
memory-bank/
├── README.md                           # This file
├── FEATURE_IMPLEMENTATION_PROCESS.md   # How to implement features
├── FEATURE_WORKFLOW_PROCESS.md         # Feature state management
├── BUG_IMPLEMENTATION_PROCESS.md       # How to fix bugs
├── BUG_WORKFLOW_PROCESS.md             # Bug state management
├── RELEASE_PROCESS.md                  # How to create releases
│
├── features/                           # Feature tracking by state
│   ├── 1-READY_TO_DEVELOP/            # Planned features
│   ├── 2-IN_PROGRESS/                 # Active development
│   ├── 3-COMPLETED/                   # Ready for release
│   ├── 4-BLOCKED/                     # Blocked features
│   └── 5-SKIPPED/                     # Deferred features
│
├── bugs/                              # Bug tracking by state
│   ├── 1-OPEN/                        # New bugs
│   ├── 2-IN_PROGRESS/                 # Being fixed
│   ├── 3-FIXED/                       # Resolved bugs
│   ├── 4-BLOCKED/                     # Blocked bugs
│   └── 5-WONT_FIX/                    # Won't be fixed
│
└── releases/                          # Release management
    ├── CURRENT_PI.md                  # Current PI indicator
    ├── PI-2025-Q1/                    # Q1 2025 release
    ├── PI-2025-Q2/                    # Q2 2025 release
    ├── PI-2025-Q3/                    # Q3 2025 release
    └── PI-2025-Q4/                    # Q4 2025 release
```

## 🔄 Workflow Overview

### Features Workflow
1. **Plan** → Create in `1-READY_TO_DEVELOP`
2. **Develop** → Move to `2-IN_PROGRESS`
3. **Complete** → Move to `3-COMPLETED`
4. **Release** → Copy to `releases/[PI]/features`

### Bugs Workflow
1. **Report** → Create in `1-OPEN`
2. **Fix** → Move to `2-IN_PROGRESS`
3. **Resolve** → Move to `3-FIXED`
4. **Release** → Copy to `releases/[PI]/bugs`

### Release Workflow
1. **Quarterly** → Based on Product Increments (PI)
2. **Collect** → Gather COMPLETED features and FIXED bugs
3. **Document** → Generate release notes
4. **Deploy** → Follow release checklist

## 📋 Quick Reference

### Creating a New Feature
```bash
# 1. Create feature folder
mkdir -p memory-bank/features/1-READY_TO_DEVELOP/my-feature

# 2. Create required files
cd memory-bank/features/1-READY_TO_DEVELOP/my-feature
touch feature-description.md feature-tasks.md

# 3. Fill out templates (see FEATURE_WORKFLOW_PROCESS.md)
```

### Reporting a Bug
```bash
# 1. Get next bug ID
cat memory-bank/bugs/NEXT_BUG_ID.txt

# 2. Create bug folder
mkdir -p memory-bank/bugs/1-OPEN/BUG-XXX-short-description

# 3. Create required files
cd memory-bank/bugs/1-OPEN/BUG-XXX-short-description
touch bug-report.md bug-tasks.md

# 4. Fill out templates (see BUG_WORKFLOW_PROCESS.md)
```

### State Transitions
```bash
# Move feature to in-progress
mv memory-bank/features/1-READY_TO_DEVELOP/my-feature \
   memory-bank/features/2-IN_PROGRESS/

# Move bug to fixed
mv memory-bank/bugs/2-IN_PROGRESS/BUG-001-description \
   memory-bank/bugs/3-FIXED/
```

## 📊 Status Dashboard

To check current status:

```bash
# Count features by state
echo "Features:"
echo "  Ready: $(ls -1 memory-bank/features/1-READY_TO_DEVELOP | wc -l)"
echo "  In Progress: $(ls -1 memory-bank/features/2-IN_PROGRESS | wc -l)"
echo "  Completed: $(ls -1 memory-bank/features/3-COMPLETED | wc -l)"

# Count bugs by state
echo "Bugs:"
echo "  Open: $(ls -1 memory-bank/bugs/1-OPEN | wc -l)"
echo "  In Progress: $(ls -1 memory-bank/bugs/2-IN_PROGRESS | wc -l)"
echo "  Fixed: $(ls -1 memory-bank/bugs/3-FIXED | wc -l)"
```

## 🚀 Getting Started

1. **For Features**: Read `FEATURE_WORKFLOW_PROCESS.md`
2. **For Bugs**: Read `BUG_WORKFLOW_PROCESS.md`
3. **For Releases**: Read `RELEASE_PROCESS.md`

## 🔑 Key Principles

1. **State-Based Organization**: Everything is organized by its current state
2. **Complete History**: All documents travel with their folder
3. **Test-First**: Bugs require failing tests before fixes
4. **Traceability**: Commit hashes link tasks to code
5. **Release Ready**: Completed items ready for release notes

## 📈 Benefits

- **Clear Status**: Instantly see what's in progress
- **No Lost Work**: Everything is tracked and organized
- **Easy Releases**: Automated release note generation
- **Audit Trail**: Complete history of all changes
- **Team Clarity**: Everyone knows the current state

## 🛠️ Maintenance

- Archive completed items after 2 PIs
- Update `NEXT_BUG_ID.txt` when creating bugs
- Review blocked items weekly
- Clean up skipped/won't fix quarterly

## 📞 Questions?

- For implementation details: See process documents
- For state management: See workflow documents
- For releases: See release process document