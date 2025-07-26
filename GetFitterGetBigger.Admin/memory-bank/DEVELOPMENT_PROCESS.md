# Development Process Guide - GetFitterGetBigger

**🎯 PURPOSE**: This is your ONE-STOP guide for all development activities. Start here!

## 🚀 Quick Start - What Are You Doing?

### I need to implement a NEW FEATURE
👉 Go to: [Feature Development Process](#feature-development-process)

### I need to fix a BUG
👉 Go to: [Bug Fix Process](#bug-fix-process)

### I need to check our standards/policies
👉 Go to: [Development Standards](#development-standards)

### I need to propagate changes to other projects
👉 Go to: [Cross-Project Coordination](#cross-project-coordination)

---

## 📋 Feature Development Process

### When Starting a Feature

#### Step 1: Create Feature Structure
**📖 Use**: `FEATURE_WORKFLOW_PROCESS.md` - Section "0. Feature Submission"
1. Assign Feature ID from `NEXT_FEATURE_ID.txt`
2. Create folder: `0-SUBMITTED/FEAT-XXX-feature-name/`
3. Create `feature-description.md`
4. Update `NEXT_FEATURE_ID.txt`

#### Step 2: Plan the Feature
**📖 Use**: `FEATURE_IMPLEMENTATION_PROCESS.md` - Section "1. Feature Analysis & Planning"
1. Analyze requirements
2. Create `feature-tasks.md` with time estimates
3. Move folder to `1-READY_TO_DEVELOP`

#### Step 3: Start Implementation
**📖 Use**: `FEATURE_IMPLEMENTATION_PROCESS.md` - Section "3. Baseline Health Check"
1. Move folder to `2-IN_PROGRESS`
2. Create feature branch
3. **MANDATORY**: Run baseline health check
4. Document results in feature-tasks.md

### During Implementation

#### Daily Work
**📖 Primary**: `FEATURE_IMPLEMENTATION_PROCESS.md` - Section "4. Implementation Phase"
- Follow task list sequentially
- Update task status with timestamps
- Write tests immediately after implementation
- Run `dotnet build` and `dotnet test` after each task

#### Quality Checkpoints
**📖 Check**: `UNIFIED_DEVELOPMENT_PROCESS.md` - "Baseline Health Check" section
**📖 NEW**: `CODE_REVIEW_PROCESS.md` - For code review requirements
- Build must succeed
- All tests must pass
- Warnings < 10
- **NEW**: Code review APPROVED after each category

### When Completing a Feature

#### Step 1: Final Testing
**📖 Use**: `UNIFIED_DEVELOPMENT_PROCESS.md` - "Manual Testing Policy"
- **MANDATORY**: Manual testing by user
- Provide test scenarios
- Wait for explicit acceptance

#### Step 2: Final Code Review
**📖 Use**: `CODE_REVIEW_PROCESS.md` - "Final Review Template"
- **MANDATORY**: Perform final comprehensive code review
- Must achieve APPROVED status
- Cannot proceed without approval

#### Step 3: Create Completion Reports
**📖 Use**: `FEATURE_WORKFLOW_PROCESS.md` - "Completion Report Templates"
Create these four MANDATORY reports:
1. `COMPLETION-REPORT.md`
2. `TECHNICAL-SUMMARY.md`
3. `LESSONS-LEARNED.md`
4. `QUICK-REFERENCE.md`

#### Step 4: Move to Completed
**📖 Use**: `FEATURE_WORKFLOW_PROCESS.md` - Section "3. Feature Completion"
- Only after user acceptance
- Move folder to `3-COMPLETED`
- Update tracking files

---

## 🐛 Bug Fix Process

### When Starting a Bug Fix

#### Step 1: Create Bug Structure
**📖 Use**: `BUG_WORKFLOW_PROCESS.md` - Section "Creating a New Bug"
1. Assign Bug ID from `NEXT_BUG_ID.txt`
2. Create folder: `1-OPEN/BUG-XXX-brief-description/`
3. Create `bug-report.md`
4. Update `NEXT_BUG_ID.txt`

#### Step 2: Plan the Fix
**📖 Use**: `BUG_IMPLEMENTATION_PROCESS.md` - Section "2. Bug Analysis"
1. Reproduce the bug
2. Create `bug-tasks.md`
3. Move to `2-IN_PROGRESS`

### During Bug Fix

#### Implementation
**📖 Use**: `BUG_IMPLEMENTATION_PROCESS.md` - Section "4. Implementation"
- Create bug fix branch: `bugfix/BUG-XXX-description`
- Follow same quality standards as features
- Write regression tests

### When Completing a Bug Fix

#### Finalization
**📖 Use**: `BUG_WORKFLOW_PROCESS.md` - Section "Bug Completion"
1. Verify fix with user
2. Move to `3-FIXED`
3. Document resolution in bug-report.md

---

## 📊 Development Standards

### File Management Rules
**📖 Source**: `UNIFIED_DEVELOPMENT_PROCESS.md` - "Mandatory File Management Rules"

**NEVER CREATE**:
- ❌ README.md in feature/bug folders
- ❌ notes.txt or personal files
- ❌ Duplicate documentation

**ALWAYS CREATE**:
- ✅ Required process files only
- ✅ Test scripts if needed
- ✅ API specs if relevant

### Reference Tables Pattern
**📖 Source**: `REFERENCE_TABLES_GUIDE.md` - Complete guide for reference tables

**When to use Reference Tables**:
- ✅ Dropdown options (Body Parts, Equipment, etc.)
- ✅ Configuration values that rarely change
- ✅ Categorization data used across features

**When NOT to use Reference Tables**:
- ❌ User-specific data
- ❌ Frequently changing business data
- ❌ Data requiring complex queries

**Quick Add Process**:
1. Add type marker to `ReferenceTableTypes.cs`
2. Create strategy in `/Services/Strategies/ReferenceTableStrategies/`
3. Use `GetReferenceDataAsync<YourType>()` in components
4. Implement API endpoint

### Quality Gates
**📖 Source**: `UNIFIED_DEVELOPMENT_PROCESS.md` - "Baseline Health Check"

**MANDATORY before proceeding**:
- ✅ `dotnet build` - zero errors
- ✅ `dotnet test` - 100% pass rate
- ✅ Warnings < 10

### Manual Testing Policy
**📖 Source**: `UNIFIED_DEVELOPMENT_PROCESS.md` - "Manual Testing Policy"

**🚨 MANDATORY for ALL work - NO EXCEPTIONS 🚨**
- Can only skip if explicitly requested at start
- Must be for purely technical changes
- Requires comprehensive automated tests

---

## 🔄 Cross-Project Coordination

### Feature Propagation Flow
**📖 Source**: `UNIFIED_DEVELOPMENT_PROCESS.md` - "Feature Origin and Flow"

#### API → UI Projects
1. Implement in API
2. Create propagation features in Admin/Clients `0-SUBMITTED`
3. Include API contract changes

#### UI → API
1. Create API feature request in `0-SUBMITTED`
2. Wait for API implementation
3. Then implement UI changes

---

## 🎯 Quick Decision Tree

```
What am I doing?
├── 🆕 New Feature
│   ├── Starting? → FEATURE_WORKFLOW_PROCESS.md
│   ├── Planning? → FEATURE_IMPLEMENTATION_PROCESS.md (Section 1)
│   ├── Coding? → FEATURE_IMPLEMENTATION_PROCESS.md (Section 4)
│   └── Completing? → FEATURE_WORKFLOW_PROCESS.md (Completion)
│
├── 🐛 Bug Fix
│   ├── Starting? → BUG_WORKFLOW_PROCESS.md
│   ├── Analyzing? → BUG_IMPLEMENTATION_PROCESS.md (Section 2)
│   └── Fixing? → BUG_IMPLEMENTATION_PROCESS.md (Section 4)
│
├── 📋 Process Question
│   ├── Quality Standards? → UNIFIED_DEVELOPMENT_PROCESS.md
│   ├── File Rules? → UNIFIED_DEVELOPMENT_PROCESS.md
│   └── Testing Policy? → UNIFIED_DEVELOPMENT_PROCESS.md
│
└── 🔄 Cross-Project
    └── All cases → UNIFIED_DEVELOPMENT_PROCESS.md
```

---

## 📚 Document Reference

### Core Process Documents
1. **UNIFIED_DEVELOPMENT_PROCESS.md** - Standards and policies for all projects
2. **FEATURE_WORKFLOW_PROCESS.md** - Feature states and transitions
3. **FEATURE_IMPLEMENTATION_PROCESS.md** - Step-by-step feature implementation
4. **BUG_WORKFLOW_PROCESS.md** - Bug states and transitions
5. **BUG_IMPLEMENTATION_PROCESS.md** - Step-by-step bug fixing
6. **CODE_REVIEW_PROCESS.md** - Code review standards and templates

### When to Use Multiple Documents
- **Starting work**: Usually need 2 documents (Workflow + Implementation)
- **During work**: Primarily Implementation document
- **Checking standards**: Always UNIFIED_DEVELOPMENT_PROCESS
- **Completing work**: Workflow for templates + Implementation for steps

---

## 💡 Pro Tips

1. **Bookmark this page** - It's your main entry point
2. **Follow the process** - Don't skip steps
3. **When in doubt** - Check UNIFIED_DEVELOPMENT_PROCESS.md
4. **Ask if unsure** - Better to clarify than assume

---

## 🚦 Remember

- **Features**: MUST start in `0-SUBMITTED`
- **Manual Testing**: MANDATORY (no exceptions without explicit approval)
- **File Discipline**: Only create required files
- **Quality Gates**: Build and tests must pass between tasks
- **Completion Reports**: Four reports are MANDATORY

---

**Need help?** Start with the Quick Decision Tree above!