# Authentication Setup Guide

This document explains how to set up authentication for the GetFitterGetBigger Admin application.

## External Authentication Providers

The application supports authentication with Google and Facebook. To use these providers, you need to set up OAuth credentials.

### Securing Authentication Credentials

Authentication credentials (Client IDs, Client Secrets, App IDs, etc.) are sensitive information and should **never** be committed to source control. Instead, we use .NET's User Secrets feature to store these credentials securely during development.

## Setting Up User Secrets

### Prerequisites

- .NET SDK 9.0 or later
- Command line access to your development environment

### Steps

1. **Initialize User Secrets for the project**

   This has already been done for the project. The UserSecretsId is stored in the project file.

   ```bash
   dotnet user-secrets init
   ```

2. **Set Google Authentication Credentials**

   ```bash
   dotnet user-secrets set "Authentication:Google:ClientId" "YOUR_GOOGLE_CLIENT_ID"
   dotnet user-secrets set "Authentication:Google:ClientSecret" "YOUR_GOOGLE_CLIENT_SECRET"
   ```

3. **Set Facebook Authentication Credentials**

   ```bash
   dotnet user-secrets set "Authentication:Facebook:AppId" "YOUR_FACEBOOK_APP_ID"
   dotnet user-secrets set "Authentication:Facebook:AppSecret" "YOUR_FACEBOOK_APP_SECRET"
   ```

4. **Verify Your Secrets**

   You can list all the secrets stored for the project:

   ```bash
   dotnet user-secrets list
   ```

## Obtaining OAuth Credentials

### Google OAuth Credentials

1. Go to the [Google Cloud Console](https://console.cloud.google.com/)
2. Create a new project or select an existing one
3. Navigate to "APIs & Services" > "Credentials"
4. Click "Create Credentials" > "OAuth client ID"
5. Set the application type to "Web application"
6. Add authorized redirect URIs:
   - For development: `https://localhost:5001/signin-google`
   - For production: `https://yourdomain.com/signin-google`
7. Create the client ID and note the Client ID and Client Secret

### Facebook OAuth Credentials

1. Go to the [Facebook Developers](https://developers.facebook.com/)
2. Create a new app or select an existing one
3. Navigate to "Settings" > "Basic"
4. Note the App ID and App Secret
5. Under "Products", add "Facebook Login"
6. In the Facebook Login settings, add the following OAuth Redirect URIs:
   - For development: `https://localhost:5001/signin-facebook`
   - For production: `https://yourdomain.com/signin-facebook`

## Production Deployment

For production environments, you should use environment variables or a secure configuration provider like Azure Key Vault to store your authentication credentials. Never store sensitive information in configuration files that might be committed to source control.

### Using Environment Variables

In your production environment, set the following environment variables:

```
Authentication__Google__ClientId=YOUR_GOOGLE_CLIENT_ID
Authentication__Google__ClientSecret=YOUR_GOOGLE_CLIENT_SECRET
Authentication__Facebook__AppId=YOUR_FACEBOOK_APP_ID
Authentication__Facebook__AppSecret=YOUR_FACEBOOK_APP_SECRET
```

Note the double underscore (`__`) which is used to represent the hierarchical structure in the configuration.

### Using Azure Key Vault

For Azure deployments, consider using Azure Key Vault to store your secrets. You can configure your application to retrieve secrets from Key Vault at runtime.

## Troubleshooting

If you encounter authentication issues:

1. Verify that your credentials are correctly set in user secrets
2. Check that the redirect URIs in your OAuth provider settings match your application's callback URLs
3. Ensure that your application is running on the correct URL (e.g., https://localhost:5001)
4. Check the application logs for any authentication-related errors
