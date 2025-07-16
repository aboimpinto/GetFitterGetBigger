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

### 🛑 Important: Checkpoint Behavior
- **DEFAULT**: AI stops after each successful checkpoint for user review
- **To continue after a pause**: `/continue-implementation`
- **For continuous mode**: `/continue-implementation don't stop on checkpoints`

---

## 📋 Feature Development Process

### When Starting a Feature

#### Step 0: Read Code Quality Standards
**🚨 MANDATORY**: Read `CODE_QUALITY_STANDARDS.md` before ANY implementation
- Pattern matching requirements
- Empty/Null Object Pattern rules
- Method quality guidelines
- NO EXCEPTIONS - This is non-negotiable!

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
- **Calculate duration when finishing each task** (Duration vs Est comparison)
- Write tests immediately after implementation
- Run `dotnet clean && dotnet build` and `dotnet test` after each task
  - Always clean first to catch all warnings

#### Quality Checkpoints
**📖 Check**: `UNIFIED_DEVELOPMENT_PROCESS.md` - "Baseline Health Check" section

**MANDATORY Checkpoint Commands**:
```bash
dotnet clean && dotnet build    # Check for errors and warnings
dotnet clean && dotnet test     # Run ALL tests (not just new ones)
```

**Checkpoint Requirements**:
- ✅ Build must succeed (zero errors)
- ✅ ALL tests must pass (100% pass rate)
- ✅ **BOY SCOUT RULE: ZERO warnings** - If you start with 0 warnings, maintain 0 warnings!
- ✅ **Code Review APPROVED** - Must have approved code review for the category

#### 🛑 CHECKPOINT PAUSE BEHAVIOR
**DEFAULT BEHAVIOR**: AI Assistant MUST STOP after each successful checkpoint!
- After completing a category and passing the checkpoint, the AI assistant will pause
- This allows the user to review, refactor, or make any necessary adjustments
- To continue implementation, use the `/continue-implementation` command

**CONTINUOUS MODE**: To proceed without stopping at checkpoints:
- Use: `/continue-implementation don't stop on checkpoints`
- The AI assistant will then continue through all categories without pausing
- This should only be used when you're confident in the implementation flow

#### 📝 Code Review Process
**After completing a category's implementation**:
1. **CREATE** code review using `CODE-REVIEW-TEMPLATE.md`
2. **SAVE** in `/2-IN_PROGRESS/FEAT-XXX/code-reviews/Category_X/`
3. **NAME** as `Code-Review-Category-X-YYYY-MM-DD-HH-MM-{STATUS}.md`
4. **REVIEW** all files created/modified in the category
5. **UPDATE** status based on findings:
   - **APPROVED**: Can proceed to next category
   - **APPROVED_WITH_NOTES**: Can proceed, but document issues for later
   - **REQUIRES_CHANGES**: Must fix and create new review

**Code Review Storage Structure**:
```
2-IN_PROGRESS/FEAT-XXX/code-reviews/
├── Category_1/
│   ├── Code-Review-Category-1-2025-01-16-10-00-REQUIRES_CHANGES.md
│   └── Code-Review-Category-1-2025-01-16-11-00-APPROVED.md
├── Category_2/
│   └── Code-Review-Category-2-2025-01-16-14-00-APPROVED.md
└── Final-Code-Review-2025-01-16-18-00-APPROVED.md
```

#### 🚨 Checkpoint Failure Protocol
**If ANY checkpoint fails**:
1. **STOP** - Do NOT proceed to next category
2. **CREATE** a "CHECKPOINT FIX" task in feature-tasks.md
3. **FIX** all issues before continuing
4. **RE-RUN** checkpoint to verify fixes
5. **UPDATE** checkpoint status to ✅ PASSED
6. **CREATE** new code review if code was changed

**Checkpoint Status Types**:
- 🛑 **PENDING** - Not yet run
- ❌ **FAILED** - Issues found, fix in progress
- ✅ **PASSED** - All checks passed, ready to proceed

**⚠️ CRITICAL**: You CANNOT start the next category if the previous checkpoint is not ✅ PASSED!

#### CHECKPOINT FIX Task Format
When creating a fix task, use this format:
```markdown
**CHECKPOINT FIX - Category X:** Brief description of issue
`[InProgress: Started: YYYY-MM-DD HH:MM]` (Est: Xh)
- **Issue**: Specific failing tests or warnings
- **Root Cause**: Why did this happen?
- **Fix Applied**: What was done to resolve it
- **Lesson Learned**: How to prevent this in future
```

**Task Placement**: Insert immediately after the last task in the current category

**Time Tracking**: Track time spent on checkpoint fixes separately to understand the true cost of quality issues

### When Completing a Feature

#### Step 1: Final Code Review
**📖 Use**: `FINAL-CODE-REVIEW-TEMPLATE.md`
- **MANDATORY**: Complete final overall code review
- **COMBINE**: Summary of all category reviews
- **SCAN**: All files against CODE_QUALITY_STANDARDS.md
- **SAVE**: As `Final-Code-Review-YYYY-MM-DD-HH-MM-{STATUS}.md` in `/code-reviews/`

**Final Review Outcomes**:
- **APPROVED** ✅: Can move directly to COMPLETED
- **APPROVED_WITH_NOTES** ⚠️: Requires user approval to proceed
- **REQUIRES_CHANGES** ❌: Must fix all issues and create new final review

#### Step 2: Final Testing
**📖 Use**: `UNIFIED_DEVELOPMENT_PROCESS.md` - "Manual Testing Policy"
- **MANDATORY**: Manual testing by user
- Provide test scenarios
- Wait for explicit acceptance

#### Step 3: Create Completion Reports
**📖 Use**: `FEATURE_WORKFLOW_PROCESS.md` - "Completion Report Templates"
Create these four MANDATORY reports:
1. `COMPLETION-REPORT.md`
2. `TECHNICAL-SUMMARY.md`
3. `LESSONS-LEARNED.md`
4. `QUICK-REFERENCE.md`

#### Step 4: Move to Completed
**📖 Use**: `FEATURE_WORKFLOW_PROCESS.md` - Section "3. Feature Completion"
- **REQUIRES**: Final code review APPROVED (or APPROVED_WITH_NOTES + user approval)
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

### 🚨 MANDATORY: Code Quality Standards
**📖 Source**: `CODE_QUALITY_STANDARDS.md` - **MUST READ BEFORE ANY IMPLEMENTATION**

**Core Requirements**:
- ✅ Pattern matching over if-else chains
- ✅ Empty/Null Object Pattern (NO nulls!)
- ✅ Short, focused methods (< 20 lines)
- ✅ No fake async
- ✅ No defensive programming without justification

### Service Layer Error Handling
**📖 Source**: `SERVICE-RESULT-PATTERN.md` - **MANDATORY for all service implementations**

**Key Pattern**:
- ✅ Use ServiceResult<T> for all service methods
- ✅ No exceptions for business logic flow
- ✅ Use structured error codes (ServiceErrorCode enum)
- ✅ Pattern matching in controllers for clean responses

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

### Quality Gates
**📖 Source**: `UNIFIED_DEVELOPMENT_PROCESS.md` - "Baseline Health Check"

**MANDATORY before proceeding**:
- ✅ `dotnet clean && dotnet build` - zero errors
  - Clean build ensures all warnings are visible
- ✅ `dotnet test` - 100% pass rate
- ✅ **BOY SCOUT RULE**: Maintain ZERO (0) warnings
  - If baseline has 0 warnings → maintain 0 warnings
  - If baseline has warnings → reduce them (never increase)
  - Leave the code better than you found it!
- ✅ **Code Review**: APPROVED status for each category
  - Use CODE-REVIEW-TEMPLATE.md after each category
  - Final review before moving to COMPLETED

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
├── 🚨 FIRST TIME or NEED REMINDER?
│   └── READ → CODE_QUALITY_STANDARDS.md (MANDATORY!)
│
├── 🆕 New Feature
│   ├── Starting? → CODE_QUALITY_STANDARDS.md + FEATURE_WORKFLOW_PROCESS.md
│   ├── Planning? → FEATURE_IMPLEMENTATION_PROCESS.md (Section 1)
│   ├── Coding? → FEATURE_IMPLEMENTATION_PROCESS.md (Section 4)
│   │   └── Services? → SERVICE_RESULT_PATTERN.md (Error handling)
│   └── Completing? → FEATURE_WORKFLOW_PROCESS.md (Completion)
│
├── 🐛 Bug Fix
│   ├── Starting? → CODE_QUALITY_STANDARDS.md + BUG_WORKFLOW_PROCESS.md
│   ├── Analyzing? → BUG_IMPLEMENTATION_PROCESS.md (Section 2)
│   └── Fixing? → BUG_IMPLEMENTATION_PROCESS.md (Section 4)
│       └── Services? → SERVICE_RESULT_PATTERN.md (Error handling)
│
├── 📋 Process Question
│   ├── Code Quality? → CODE_QUALITY_STANDARDS.md
│   ├── Service Errors? → SERVICE_RESULT_PATTERN.md
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
1. **CODE_QUALITY_STANDARDS.md** - 🚨 MANDATORY code quality rules (READ FIRST!)
2. **SERVICE_RESULT_PATTERN.md** - 🚨 MANDATORY for service layer implementations
3. **UNIFIED_DEVELOPMENT_PROCESS.md** - Standards and policies for all projects
4. **FEATURE_WORKFLOW_PROCESS.md** - Feature states and transitions
5. **FEATURE_IMPLEMENTATION_PROCESS.md** - Step-by-step feature implementation
6. **BUG_WORKFLOW_PROCESS.md** - Bug states and transitions
7. **BUG_IMPLEMENTATION_PROCESS.md** - Step-by-step bug fixing

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