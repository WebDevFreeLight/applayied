# Environment Configuration Guide

## Overview

This guide explains how to configure and use different environment settings for the application. Each environment has its own `appsettings.{Environment}.json` file with specific configurations.

## Available Environments

### 1. **Development** (`appsettings.Development.json`)
- Local development with detailed logging
- Database initialization enabled
- CORS allows localhost origins
- Extended JWT token expiration
- Verbose error messages

### 2. **Docker** (`appsettings.Docker.json`)
- Containerized environment
- Supabase pooler connection
- Wildcard CORS for testing
- Port 8887 configuration

### 3. **Staging** (`appsettings.Staging.json`)
- Pre-production testing
- Production-like settings
- Limited CORS origins
- Moderate logging

### 4. **Production** (`appsettings.Production.json`)
- Live environment
- Minimal logging
- Strict security settings
- Environment variables for secrets

## Setting the Environment

### 1. Via Environment Variable

#### Windows (Command Prompt)
```cmd
set ASPNETCORE_ENVIRONMENT=Development
dotnet run
```

#### Windows (PowerShell)
```powershell
$env:ASPNETCORE_ENVIRONMENT="Development"
dotnet run
```

#### Linux/Mac
```bash
export ASPNETCORE_ENVIRONMENT=Development
dotnet run
```

#### Docker
```dockerfile
ENV ASPNETCORE_ENVIRONMENT=Docker
```

Or via docker-compose:
```yaml
environment:
  - ASPNETCORE_ENVIRONMENT=Docker
```

### 2. Via Launch Settings

Edit `Properties/launchSettings.json`:
```json
{
  "profiles": {
    "Development": {
      "commandName": "Project",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      },
      "applicationUrl": "https://localhost:8888;http://localhost:8887"
    },
    "Docker": {
      "commandName": "Project",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Docker"
      },
      "applicationUrl": "http://+:8887"
    },
    "Staging": {
      "commandName": "Project",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Staging"
      },
      "applicationUrl": "http://+:8887"
    },
    "Production": {
      "commandName": "Project",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Production"
      },
      "applicationUrl": "https://+:443;http://+:80"
    }
  }
}
```

## Environment-Specific Features

### Development Environment
```json
{
  "ShowDbLog": true,        // SQL queries logged
  "InitDb": true,           // Auto-create tables
  "DemoMode": false,        // Full functionality
  "SingleLogin": false,     // Multiple sessions allowed
  "Logging": "Debug"        // Verbose logging
}
```

### Docker Environment
```json
{
  "ShowDbLog": true,        // Monitor container SQL
  "InitDb": true,           // Initialize on first run
  "corsUrls": ["*"],        // Allow all origins
  "urls": "http://+:8887"   // Docker port binding
}
```

### Staging Environment
```json
{
  "ShowDbLog": true,        // Debug production issues
  "InitDb": false,          // Manual DB management
  "SingleLogin": false,     // Testing multiple users
  "Logging": "Information"  // Moderate logging
}
```

### Production Environment
```json
{
  "ShowDbLog": false,       // Performance optimization
  "InitDb": false,          // Prevent accidental changes
  "SingleLogin": true,      // Security enforcement
  "Logging": "Warning",     // Minimal logging
  "Use Environment Vars": true  // Secrets management
}
```

## Database Configuration by Environment

### Development
- Local PostgreSQL or Docker PostgreSQL
- Connection pooling: Basic
- SSL: Optional

### Docker
- Supabase pooler connection
- IPv4 enforcement
- SSL: Required

### Staging
- Supabase pooler
- Moderate pooling
- SSL: Required

### Production
- Supabase pooler (primary)
- Maximum pooling
- SSL: Required
- Connection lifetime limits

## Port Configuration

| Environment | HTTP Port | HTTPS Port | Notes |
|------------|-----------|------------|-------|
| Development | 8887 | 8888 | Both protocols |
| Docker | 8887 | - | HTTP only in container |
| Staging | 8887 | - | Behind reverse proxy |
| Production | 80 | 443 | Standard ports |

## Environment Variables

### Production Secrets
Set these environment variables in production:

```bash
# JWT Configuration
export JWT_SECRET_KEY="your-production-secret-key"

# Worker ID for distributed systems
export WORKER_ID="1"

# Redis Configuration
export REDIS_CONNECTION="your-redis-connection-string"
export REDIS_SESSION="your-redis-session-string"

# Aliyun OSS
export ALIYUN_REGION="cn-hangzhou"
export ALIYUN_KEY="your-key"
export ALIYUN_SECRET="your-secret"
export ALIYUN_BUCKET="your-bucket"
export ALIYUN_DOMAIN="https://your-domain.oss-cn-hangzhou.aliyuncs.com"

# Paymob Payment
export PAYMOB_API_KEY="your-api-key"
export PAYMOB_SECRET_KEY="your-secret-key"
export PAYMOB_PUBLIC_KEY="your-public-key"
export PAYMOB_INTEGRATION_ID="your-integration-id"

# WeChat Configuration
export WXCORP_AGENT_ID="your-agent-id"
export WXCORP_ID="your-corp-id"
export WXCORP_SECRET="your-secret"
export WXOPEN_APP_ID="your-app-id"
export WXOPEN_APP_SECRET="your-app-secret"

# Email Configuration
export MAIL_FROM="noreply@yourdomain.com"
export MAIL_PASSWORD="your-email-password"
export MAIL_SMTP="smtp.gmail.com"
```

### Docker Environment Variables

In `docker-compose.yml`:
```yaml
environment:
  - ASPNETCORE_ENVIRONMENT=Docker
  - ASPNETCORE_URLS=http://+:8887
  - DOTNET_SYSTEM_NET_DISABLEIPV6=true
  - ConnectionStrings__DefaultConnection=${DB_CONNECTION}
```

## CORS Configuration

### Development
```json
"corsUrls": [
  "http://localhost:3000",
  "http://localhost:8080",
  "http://localhost:5173",
  "http://localhost:8887"
]
```

### Docker
```json
"corsUrls": ["*"]  // Allow all for testing
```

### Staging
```json
"corsUrls": [
  "https://staging.yourdomain.com",
  "http://localhost:3000"  // For testing
]
```

### Production
```json
"corsUrls": [
  "https://yourdomain.com",
  "https://www.yourdomain.com",
  "https://app.yourdomain.com"
]
```

## Logging Configuration

### Development
```json
{
  "LogLevel": {
    "Default": "Debug",
    "Microsoft": "Information",
    "Microsoft.EntityFrameworkCore.Database.Command": "Information"
  }
}
```

### Production
```json
{
  "LogLevel": {
    "Default": "Warning",
    "Microsoft": "Warning",
    "System": "Warning"
  }
}
```

## Quick Commands

### Run with specific environment
```bash
# Development
dotnet run --environment Development

# Docker
docker-compose up -d

# Staging
dotnet run --environment Staging

# Production
dotnet run --environment Production
```

### Build for specific environment
```bash
# Development
dotnet build -c Debug

# Production
dotnet build -c Release

# Publish for Production
dotnet publish -c Release -o ./publish
```

## Testing Environment Configuration

### Check current environment
```csharp
// In Program.cs or Startup.cs
var environment = app.Environment.EnvironmentName;
Console.WriteLine($"Running in: {environment}");
```

### Environment-specific code
```csharp
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
}
else if (app.Environment.IsStaging())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}
else if (app.Environment.IsProduction())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
    app.UseHttpsRedirection();
}
```

## Configuration Priority

Settings are loaded in this order (later overrides earlier):
1. `appsettings.json` (base configuration)
2. `appsettings.{Environment}.json` (environment-specific)
3. Environment variables
4. Command-line arguments

## Best Practices

### 1. Never commit secrets
- Use environment variables for production secrets
- Use user-secrets for development
```bash
dotnet user-secrets init
dotnet user-secrets set "JwtSettings:SecretKey" "dev-secret-key"
```

### 2. Environment-specific connection strings
- Development: Local database
- Docker: Container network
- Staging: Test database
- Production: Production database with pooling

### 3. Logging levels
- Development: Debug/Information
- Staging: Information
- Production: Warning/Error

### 4. Security settings
- Development: Relaxed CORS, extended tokens
- Staging: Moderate security
- Production: Strict CORS, short token lifetime

## Troubleshooting

### Environment not loading
```bash
# Check current environment
echo $ASPNETCORE_ENVIRONMENT  # Linux/Mac
echo %ASPNETCORE_ENVIRONMENT%  # Windows CMD
echo $env:ASPNETCORE_ENVIRONMENT  # PowerShell
```

### Wrong configuration loaded
1. Check file naming: `appsettings.{Environment}.json`
2. Verify environment name (case-sensitive)
3. Check build action: "Content" and "Copy Always"

### Docker environment issues
```bash
# Check environment in container
docker exec maham_backend printenv | grep ASPNETCORE

# Override in docker-compose
docker-compose run -e ASPNETCORE_ENVIRONMENT=Docker backend
```

## Summary

Each environment serves a specific purpose:
- **Development**: Local development with debugging
- **Docker**: Containerized testing and deployment
- **Staging**: Pre-production validation
- **Production**: Live environment with optimizations

Choose the appropriate environment based on your current needs and ensure proper configuration for security and performance.