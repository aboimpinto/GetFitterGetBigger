# Claude AI Assistant Guidelines

This document contains project-specific instructions for Claude AI when working on the GetFitterGetBigger project.

## Role in Admin Project

When working in the Admin project folder, the AI assistant can perform **full implementation work** including:
- Creating React components and pages
- Implementing features
- Writing tests
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