# Scalar API Documentation Setup

## Overview
The application now has comprehensive Scalar API documentation with advanced error handling and multiple API group support.

## Available Endpoints

### Scalar Documentation
- **Root**: `/scalar` - Redirects to `/scalar/v1`
- **Groups Page**: `/scalar/groups` - Shows all available API groups
- **Individual Groups**:
  - `/scalar/all` - All APIs combined
  - `/scalar/v1` - Business API (default)
  - `/scalar/sys` - System Management
  - `/scalar/hr` - Human Resources
  - `/scalar/shopping` - Shopping Mall
  - `/scalar/article` - Content Management
  - `/scalar/finance` - Finance Department
  - `/scalar/sales` - Sales Department
  - `/scalar/operations` - Operations Department
  - `/scalar/marketing` - Marketing Department
  - `/scalar/clients` - Clients & Vendors
  - `/scalar/projects` - Projects Management
  - `/scalar/supplychain` - Supply Chain Management
  - `/scalar/legal` - Legal Department
  - `/scalar/monitor` - System Monitor
  - `/scalar/tools` - Tools & Utilities
  - `/scalar/email` - Email Service
  - `/scalar/public` - Public APIs
  - `/scalar/business` - Business Logic

### Swagger Documentation
- **Root**: `/swagger` - Swagger UI with group selector
- **JSON Endpoints**:
  - `/swagger/{group}/swagger.json` - OpenAPI specification for each group
  - `/swagger/all/swagger.json` - All APIs combined
  - `/swagger/v1/swagger.json` - Default business APIs

## Features Implemented

### 1. Error Handling
- Comprehensive try-catch blocks around all Scalar configuration
- Fallback routes in case of configuration errors
- Helpful error pages with alternative links
- Console logging for debugging

### 2. Route Redirects
Fixed common incorrect URL patterns:
- `/swagger/swagger.json` → `/swagger/v1/swagger.json`
- `/swagger/index.html/swagger.json` → `/swagger/v1/swagger.json`
- `/scalar/swagger.json` → `/swagger/v1/swagger.json`
- `/scalar/index.html/swagger.json` → `/swagger/v1/swagger.json`

### 3. Custom Styling
Each Scalar endpoint includes custom CSS for improved appearance:
```css
.scalar-app { font-family: 'Inter', system-ui, sans-serif; }
.scalar-sidebar { background-color: #1a1a2e; }
.scalar-card { border-radius: 12px; }
```

### 4. API Groups
APIs are automatically grouped based on namespace:
- System namespace → sys group
- HR namespace → hr group
- Mall/Shopping namespace → shopping group
- etc.

The "all" group includes every API endpoint in one view.

### 5. Configuration
Configure in `appsettings.json`:
```json
{
  "ApiDocumentation": {
    "EnableScalar": true,
    "SwaggerRoutePrefix": "swagger",
    "ScalarRoutePrefix": "scalar",
    "DocumentTitle": "API Documentation"
  }
}
```

## Troubleshooting

### Issue: Scalar shows 404 for swagger.json
**Solution**: The redirect handlers now automatically redirect to the correct `/swagger/v1/swagger.json`

### Issue: "EndpointPathPrefix must define {documentName}"
**Solution**: Fixed by using individual MapScalarApiReference calls for each group

### Issue: CORS errors
**Solution**: All endpoints are marked with `.AllowAnonymous()` and proper CORS headers are configured

## Implementation Files

1. **ScalarProxyExtension.cs** - Main Scalar configuration with error handling
2. **DynamicSwaggerExtension.cs** - Dynamic Swagger group discovery
3. **ScalarApiDocumentationExtension.cs** - Additional Scalar features

## Testing

To test the implementation:
1. Run `dotnet build` to ensure compilation
2. Run `dotnet run` to start the application
3. Navigate to:
   - `https://localhost:8888/scalar` - Scalar documentation
   - `https://localhost:8888/scalar/groups` - Group selector
   - `https://localhost:8888/swagger` - Swagger UI

## Future Enhancements

Consider adding:
1. Authentication integration for protected endpoints
2. Custom themes per API group
3. Export functionality for offline documentation
4. Performance metrics for API endpoints
5. Version management for API documentation