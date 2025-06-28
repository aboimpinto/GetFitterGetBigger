# API Authentication and Authorization

## 1. Feature Overview

A new federated authentication and claims-based authorization system has been designed and specified for the GetFitterGetBigger ecosystem. This system is being implemented first between the **Admin Client** and the **API**.

This document provides a high-level overview for the Mobile, Desktop, and Web client teams.

## 2. System Architecture

The authentication flow is designed as follows:

1.  **Federated Login**: The client application authenticates the user through a federated identity provider (e.g., Google, Facebook).
2.  **API Token Exchange**: The client sends the user's email to a dedicated API endpoint (`POST /api/auth/login`).
3.  **Session Management**: The API returns a JSON Web Token (JWT) and a list of the user's claims (permissions). The client must store this token and send it in the `Authorization` header for all subsequent API calls.
4.  **Sliding Expiration**: The session token has a 1-hour sliding expiration. A refreshed token is provided by the API in a response header (`X-Refreshed-Token`) on every successful call, and the client is responsible for updating its stored token.

## 3. Implementation Status for Mobile, Desktop, and Web Clients

**This feature is not yet ready for implementation on the Mobile, Desktop, and Web clients.**

The immediate focus is on the integration between the Admin client and the API. The following prerequisites must be met before this feature can be implemented on other platforms:

*   **Federated Identity Provider Integration**: The client applications must first implement the necessary libraries and UI to support user login via Google and/or Facebook.
*   **Further Refinement**: The specific implementation details for native mobile and desktop applications may require further refinement and discussion.

Once the Admin/API integration is complete and stable, and the prerequisites are met, a more detailed specification will be provided for the other client applications. Please consider this feature **planned but blocked** for now.
