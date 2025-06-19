# HTTPS in Development Environment

This document outlines how HTTPS is configured for the GetFitterGetBigger Admin application in the development environment.

## Configuration Overview

HTTPS has been enabled in the development environment to:
- Ensure a secure connection during development
- Match the production environment more closely
- Test security-related features that require HTTPS
- Avoid mixed content warnings when integrating with secure APIs

## Implementation Details

### 1. HTTPS Profile Configuration

The application is configured to use HTTPS by default in development. This is set up in the `Properties/launchSettings.json` file:

```json
{
  "profiles": {
    "https": {
      "commandName": "Project",
      "dotnetRunMessages": true,
      "launchBrowser": true,
      "applicationUrl": "https://localhost:7263;http://localhost:5127",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    },
    "http": {
      "commandName": "Project",
      "dotnetRunMessages": true,
      "launchBrowser": true,
      "applicationUrl": "http://localhost:5127",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    }
  }
}
```

The HTTPS profile is listed first, making it the default when running the application.

### 2. HTTPS Middleware Configuration

The application is configured to use HTTPS-related middleware in all environments, including development. This is set up in the `Program.cs` file:

```csharp
// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
app.UseHsts();
app.UseHttpsRedirection();
```

- `UseHsts()`: Adds HTTP Strict Transport Security headers to responses
- `UseHttpsRedirection()`: Redirects HTTP requests to HTTPS

### 3. Development Certificate Configuration

The development certificate settings are configured in `appsettings.Development.json`:

```json
"Kestrel": {
  "Certificates": {
    "Development": {
      "Password": null
    }
  }
}
```

This configuration uses the ASP.NET Core development certificate, which is automatically created and trusted by the .NET SDK.

## Using HTTPS in Development

### Running the Application

When you run the application using `dotnet run` or through Visual Studio / VS Code, it will automatically use the HTTPS profile and start on https://localhost:7263.

### Trusting the Development Certificate

If you encounter certificate warnings in your browser:

1. **Windows**: The certificate should be automatically trusted. If not, run:
   ```
   dotnet dev-certs https --trust
   ```

2. **macOS**: Run the following command to trust the certificate:
   ```
   dotnet dev-certs https --trust
   ```

3. **Linux**: The certificate will be created but not automatically trusted. You may need to:
   - Add the certificate to your browser's trusted certificates
   - Run:
     ```
     dotnet dev-certs https --trust
     ```
   - Note that on some Linux distributions, you may need additional steps to trust the certificate

### Switching Between HTTP and HTTPS

If you need to run the application using HTTP only:

- From the command line: `dotnet run --launch-profile http`
- In Visual Studio: Select the "http" profile from the run profile dropdown
- In VS Code: Modify the launch configuration to use the "http" profile

## Troubleshooting

### Certificate Issues

If you encounter certificate-related issues:

1. Reset the development certificate:
   ```
   dotnet dev-certs https --clean
   dotnet dev-certs https --trust
   ```

2. Verify the certificate is trusted:
   ```
   dotnet dev-certs https --check
   ```

### Port Conflicts

If port 7263 is already in use:

1. Change the port in `Properties/launchSettings.json`
2. Restart the application

## Security Considerations

- The development certificate should not be used in production
- Production environments should use properly issued SSL certificates
- HSTS settings may need adjustment for production scenarios
