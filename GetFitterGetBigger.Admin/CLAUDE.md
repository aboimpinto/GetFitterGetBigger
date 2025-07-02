# Claude AI Assistant Guidelines

This document contains project-specific instructions for Claude AI when working on the GetFitterGetBigger project.

## Role in Admin Project

When working in the Admin project folder, the AI assistant can perform **full implementation work** including:
- Creating React components and pages
- Implementing features
- Writing tests
- Following the FEATURE_IMPLEMENTATION_PROCESS.md

Note: When in the main repository folder, only documentation work is performed.

## Testing Guidelines

**IMPORTANT**: When writing tests for any component (React or Blazor), always consult the **memory-bank/COMPREHENSIVE-TESTING-GUIDE.md** file. This consolidated guide contains:
- Blazor/bUnit testing patterns and lessons learned
- React/Jest testing best practices
- API service testing with xUnit
- Common pitfalls and debugging strategies
- Quick reference for both tech stacks

Key requirements for all component tests:
1. Add `data-testid` attributes to interactive elements (Blazor) or use semantic queries (React)
2. Use `internal` visibility for testable methods/fields in Blazor components
3. Implement both UI interaction and unit tests when appropriate
4. Handle async operations properly (InvokeAsync for Blazor, waitFor/findBy for React)
5. Always wrap React components with required providers
6. Mock all external dependencies at the appropriate level

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

## Project Documentation

All project documentation is maintained in the `memory-bank/` folder. This includes:
- Feature documentation and implementation details
- API documentation and integration guides
- Setup and configuration guides
- Testing strategies and guidelines
- Bug tracking and workflow processes

The memory-bank serves as the central repository for all project context that AI assistants need to understand and work with the codebase effectively.

## Temporary Files Pattern

When creating temporary files during work sessions (plans, summaries, analysis):
- Use `memory-bank/temp/` directory for all temporary files
- This directory is gitignored and won't be committed
- See `memory-bank/AI-ASSISTANT-TEMP-FILES-PATTERN.md` for details
- Never create temporary documentation files in test or source directories