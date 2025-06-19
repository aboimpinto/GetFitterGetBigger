# Authentication

## Status: IMPLEMENTED

## Description
The Authentication feature provides user login/logout functionality using external providers (Google and Facebook). It enables secure access to the admin application and displays user profile information.

## Implementation Details
- Uses Google and Facebook OAuth for authentication
- Cookie-based session management
- Automatic redirection to login for unauthenticated users
- User profile display with email and profile picture
- Secure credential management using .NET User Secrets

## Related Components
- AuthService
- CustomAuthStateProvider
- RedirectToLoginHandler
- Login.razor
- UserProfile.razor
- AuthController.cs
- AuthUser.cs

## Documentation
- [Authentication Setup Guide](/docs/authentication-setup.md)

## Implementation History
| Date | Description | Commit | PR |
|------|-------------|--------|-----|
| 2025-06-15 | Initial implementation | - | - |
