# 📖 Swagger HTTPS Support Documentation

## Overview
Swagger UI has been fully configured to support both HTTP and HTTPS protocols, with automatic server detection and dynamic configuration.

## ✅ Features Implemented

### 1. **Dynamic Server Detection**
Swagger automatically detects and configures both HTTP and HTTPS servers based on:
- Current request protocol
- Environment configuration
- Available ports

### 2. **Multiple Server Support**
The Swagger UI now displays multiple server options:
- **Current Server** - Uses the current request's protocol
- **HTTP Server** - Port 8080 (configurable)
- **HTTPS Server** - Port 8443 (configurable)
- **Local Servers** - For development testing

### 3. **Automatic HTTPS Redirect**
When accessing Swagger over HTTPS, API calls are automatically upgraded to use HTTPS endpoints.

## 🔧 Configuration

### Environment Variables

```bash
# Enable/disable HTTPS
UseHttps=true

# Port configuration
HTTP_PORT=8080
HTTPS_PORT=8443

# Server hostname
SERVER_HOST=your-domain.com
```

### appsettings.json

```json
{
  "UseHttps": true,
  "Swagger": {
    "RoutePrefix": "",
    "DocumentTitle": "API Documentation"
  },
  "Kestrel": {
    "Endpoints": {
      "Http": {
        "Url": "http://+:8080"
      },
      "Https": {
        "Url": "https://+:8443"
      }
    }
  }
}
```

## 📋 Code Changes

### 1. **DynamicSwaggerExtension.cs**

#### Security Configuration (ConfigureSecurity method)
```csharp
// Adds both HTTP and HTTPS servers
options.AddServer(new OpenApiServer
{
    Url = $"http://{serverHost}:{httpPort}",
    Description = "HTTP Server"
});

options.AddServer(new OpenApiServer
{
    Url = $"https://{serverHost}:{httpsPort}",
    Description = "HTTPS Server (Secure)"
});
```

#### Dynamic Server Detection (UseDynamicSwagger method)
```csharp
// Dynamically adds servers based on request
var servers = new List<OpenApiServer>
{
    new OpenApiServer 
    { 
        Url = $"{httpReq.Scheme}://{httpReq.Host.Value}",
        Description = "Current Server"
    }
};

// Adds HTTP/HTTPS based on configuration
if (useHttps)
{
    servers.Add(new OpenApiServer
    {
        Url = $"https://{httpReq.Host.Host}:{httpsPort}",
        Description = "HTTPS Server (Secure)"
    });
}
```

#### UI Configuration (ConfigureSwaggerUI method)
```csharp
// Request interceptor to upgrade HTTP to HTTPS
options.UseRequestInterceptor(@"
    (request) => {
        if (window.location.protocol === 'https:' && request.url.startsWith('http://')) {
            request.url = request.url.replace('http://', 'https://');
            request.url = request.url.replace(':8080', ':8443');
        }
        return request;
    }
");
```

### 2. **Program.cs - Kestrel Configuration**
```csharp
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    // Automatic HTTPS configuration
    if (urls.Contains("https://"))
    {
        if (!string.IsNullOrEmpty(certPath) && File.Exists(certPath))
        {
            serverOptions.ConfigureHttpsDefaults(httpsOptions =>
            {
                httpsOptions.ServerCertificate = new X509Certificate2(certPath, certPassword);
            });
        }
        else
        {
            // Fallback to HTTP if no certificate
            Environment.SetEnvironmentVariable("ASPNETCORE_URLS", urls.Replace("https://", "http://"));
        }
    }
});
```

## 🚀 How It Works

### 1. **Server Selection in Swagger UI**

When you open Swagger UI, you'll see a "Servers" dropdown with options:
- **Current Server** - Uses your current protocol (HTTP/HTTPS)
- **HTTP Server** - http://your-server:8080
- **HTTPS Server** - https://your-server:8443

### 2. **Automatic Protocol Upgrade**

When accessing Swagger via HTTPS:
1. All API requests are automatically upgraded to HTTPS
2. Port 8080 is replaced with 8443
3. Ensures secure communication

### 3. **Try It Out Feature**

The "Try it out" button works with both protocols:
- Respects the selected server
- Handles self-signed certificates
- Shows proper error messages

## 🔍 Testing

### Access Swagger UI

**HTTP Access:**
```
http://your-server:8080/swagger
```

**HTTPS Access:**
```
https://your-server:8443/swagger
```

### Test API Calls

1. Open Swagger UI
2. Select a server from the dropdown
3. Choose an endpoint
4. Click "Try it out"
5. Execute the request

### Verify HTTPS

```bash
# Check if HTTPS is working
curl -k https://your-server:8443/swagger/v1/swagger.json

# Test an API endpoint
curl -k -X GET https://your-server:8443/api/health
```

## 🐳 Docker Support

### Docker Environment Variables

```yaml
environment:
  - ASPNETCORE_URLS=http://+:8080;https://+:8443
  - UseHttps=true
  - HTTP_PORT=8080
  - HTTPS_PORT=8443
  - SERVER_HOST=your-domain.com
```

### Docker Run Command

```bash
docker run -d \
  --name myapp \
  -p 8080:8080 \
  -p 8443:8443 \
  -e ASPNETCORE_URLS="http://+:8080;https://+:8443" \
  -e UseHttps=true \
  -v /certs:/certs:ro \
  myapp:latest
```

## 🛠️ Troubleshooting

### Issue: Swagger shows only HTTP servers
**Solution:** Check that `UseHttps=true` is set in environment variables

### Issue: HTTPS API calls fail
**Solution:** Ensure certificate is properly mounted and accessible

### Issue: Mixed content warnings
**Solution:** Access Swagger UI via HTTPS (https://...) instead of HTTP

### Issue: Server dropdown empty
**Solution:** Check that servers are being added in ConfigureSecurity method

## 📊 Benefits

1. **Security** - Supports encrypted communication
2. **Flexibility** - Works with both HTTP and HTTPS
3. **Development Friendly** - Easy switching between protocols
4. **Production Ready** - Properly configured for deployment
5. **User Friendly** - Clear server selection in UI

## 🔐 Security Considerations

1. **JWT Authentication** - Works over both HTTP and HTTPS
2. **CORS** - Properly configured for both protocols
3. **Certificate Validation** - Handles self-signed certificates
4. **Request Interception** - Automatic protocol upgrade

## 📝 Summary

Swagger UI now fully supports:
- ✅ HTTP and HTTPS protocols
- ✅ Dynamic server detection
- ✅ Automatic protocol upgrade
- ✅ Multiple server selection
- ✅ Secure API testing
- ✅ Certificate handling
- ✅ Development and production modes

The implementation ensures that your API documentation works seamlessly with both HTTP and HTTPS, providing flexibility for development and security for production.