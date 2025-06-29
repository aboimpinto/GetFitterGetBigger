# Claude AI Assistant Guidelines

This document contains project-specific instructions for Claude AI when working on the GetFitterGetBigger project.

## Role in API Project

When working in the API project folder, the AI assistant can perform **full implementation work** including:
- Creating and modifying code
- Writing tests
- Implementing features
- Following the FEATURE_IMPLEMENTATION_PROCESS.md

Note: When in the main repository folder, only documentation work is performed.

## Git Commit Messages

When creating git commits, use the following signature format at the end of the commit message:

```
🤖 Generated with [Claude Code](https://claude.ai/code)

Authored-By: Paulo Aboim Pinto <aboimpinto@gmail.com>
```

This signature properly attributes the work as authored by Paulo Aboim Pinto while acknowledging that Claude was used as the tool to generate the content.

## Project Structure

The GetFitterGetBigger ecosystem consists of:
- **API Project**: Backend service providing all endpoints
- **Admin Project**: Web application for Personal Trainers
- **Clients Project**: Contains mobile (Android, iOS), web, and desktop applications

## Documentation Propagation

When propagating API documentation:
1. Update only these two memory-banks:
   - `/GetFitterGetBigger.Admin/memory-bank/`
   - `/GetFitterGetBigger.Clients/memory-bank/`
2. Do NOT update individual sub-project memory-banks
3. Follow the rules defined in `/api-docs/documentation-propagation-rules.md`

## API Configuration

- Development URL: `http://localhost:5214/`
- Swagger Documentation: `http://localhost:5214/swagger`
- Production URL: Not yet assigned

## Authorization Claims

- Admin access: `PT-Tier` or `Admin-Tier`
- Client access: `Free-Tier`, `WorkoutPlan-Tier` (future), `DietPlan-Tier` (future)

## Testing Guidelines

**IMPORTANT**: When debugging test failures, ALWAYS check `/memory-bank/TESTING-QUICK-REFERENCE.md` first! It contains critical patterns learned from fixing 87 test failures, including:
- Common ID format errors
- Missing mock setups
- Navigation property loading issues
- Quick debugging checklist

## Memory Bank Structure

The `/memory-bank/` directory contains all project knowledge and processes:

### Process Documentation
- **`BUG_IMPLEMENTATION_PROCESS.md`** - How to fix bugs systematically
- **`BUG_WORKFLOW_PROCESS.md`** - Bug lifecycle and folder structure
- **`FEATURE_IMPLEMENTATION_PROCESS.md`** - How to implement new features
- **`FEATURE_WORKFLOW_PROCESS.md`** - Feature lifecycle and states
- **`RELEASE_PROCESS.md`** - Release management and PI planning

### Quick References
- **`TESTING-QUICK-REFERENCE.md`** ⚡ - Common test failures and solutions (CHECK FIRST!)
- **`common-testing-errors-and-solutions.md`** - Detailed testing patterns
- **`TestingGuidelines.md`** - Overall testing strategy

### Bug Management (`/bugs/`)
- **`1-OPEN/`** - New bugs awaiting work
- **`2-IN_PROGRESS/`** - Bugs being actively fixed
- **`3-FIXED/`** - Completed bug fixes
- **`4-BLOCKED/`** - Bugs waiting on dependencies
- **`5-WONT_FIX/`** - Bugs that won't be addressed

**IMPORTANT**: Always use the `/memory-bank/bugs/` folder structure for bug tracking. Do NOT use or create a `BUGS.md` file in the memory-bank root. Each bug should have its own folder with proper documentation following the BUG_WORKFLOW_PROCESS.md.

### Feature Management (`/features/`)
- **`1-READY_TO_DEVELOP/`** - Features ready to implement
- **`2-IN_PROGRESS/`** - Features being developed
- **`3-COMPLETED/`** - Finished features
- **`4-BLOCKED/`** - Features waiting on dependencies
- **`5-SKIPPED/`** - Features postponed

### Technical Documentation
- **`systemPatterns.md`** - Architecture patterns used
- **`databaseModelPattern.md`** - Database design patterns
- **`unitOfWorkPattern.md`** - Repository/UoW implementation
- **`cache-configuration.md`** & **`cache-invalidation-strategy.md`** - Caching details

### Context Files
- **`productContext.md`** - Product vision and goals
- **`techContext.md`** - Technical stack and decisions
- **`projectbrief.md`** - Project overview
- **`activeContext.md`** - Current work context

**💡 Pro Tip**: When debugging, start with TESTING-QUICK-REFERENCE.md, then check relevant process docs!