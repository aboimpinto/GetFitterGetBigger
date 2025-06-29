# API Documentation

This folder contains the API documentation for the GetFitterGetBigger ecosystem. Each API endpoint is documented in its own file, with a consistent structure.

## Structure

Each API endpoint documentation file follows this structure:

- **Endpoint URL**
- **HTTP Method**
- **Request Parameters**
- **Request Body Format** (if applicable)
- **Response Codes and Formats**
- **Authentication Requirements**
- **Example Requests and Responses**
- **Projects Using This Endpoint** (Admin, Client, or both)

## Metadata

Each API documentation file includes metadata indicating which projects use the endpoint. This metadata is used by the memory bank update script to propagate changes to the relevant projects.

Example metadata:

```yaml
---
used_by:
  - admin
  - client
---
```

## Workflow Documentation

### Development Process
- `development-workflow-process.md` - Unified workflow for all projects
- `workflow-0-submitted-state.md` - Details on the mandatory 0-SUBMITTED state
- `feature-bug-workflow-changes.md` - Recent changes to Feature/Bug processes
- `testing-guidelines.md` - Comprehensive testing standards
- `documentation-propagation-rules.md` - How to propagate documentation

### Feature Tracking
- `feature-propagation-log.md` - Log of features propagated to Admin/Clients

## Workflow

When changes are needed to an API endpoint:

1. Update the API documentation in this folder
2. Run the memory bank update script to propagate the changes to the relevant projects
3. Implement the changes in the API project
4. Update the shared models if necessary
5. Update the client projects as needed
