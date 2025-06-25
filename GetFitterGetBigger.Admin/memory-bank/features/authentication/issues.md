# Authentication Issues

## 1. Logout Functionality is Broken

-   **Tag:** BUG
-   **Status:** Open
-   **Description:** After implementing the new Blazor-native authentication flow, the logout functionality is no longer working as expected. Clicking the logout button in the `UserProfile` component does not successfully log the user out.
-   **Expected Behavior:** Clicking the logout button should terminate the user's session and redirect them to the login page.
-   **Actual Behavior:** The logout button click is not being handled correctly, and the user remains logged in.
-   **Notes:** This needs to be investigated and fixed. The issue is likely related to the Blazor component lifecycle and how the `NavigationManager` is being used after the `AuthService.LogoutAsync()` call.
