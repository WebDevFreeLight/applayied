# PowerShell script to test all environment configurations

Write-Host "================================" -ForegroundColor Cyan
Write-Host "Testing Environment Configurations" -ForegroundColor Cyan
Write-Host "================================" -ForegroundColor Cyan
Write-Host ""

# Function to test environment
function Test-Environment {
    param(
        [string]$EnvironmentName
    )
    
    Write-Host "Testing $EnvironmentName Environment..." -ForegroundColor Yellow
    
    # Set environment variable
    $env:ASPNETCORE_ENVIRONMENT = $EnvironmentName
    
    # Check if configuration file exists
    $configFile = "ZR.Admin.WebApi\appsettings.$EnvironmentName.json"
    
    if (Test-Path $configFile) {
        Write-Host "  ✓ Configuration file exists: $configFile" -ForegroundColor Green
        
        # Read and validate JSON
        try {
            $config = Get-Content $configFile -Raw | ConvertFrom-Json
            Write-Host "  ✓ Valid JSON format" -ForegroundColor Green
            
            # Check key configurations
            if ($config.dbConfigs) {
                Write-Host "  ✓ Database configuration found" -ForegroundColor Green
                Write-Host "    - Connection: $($config.dbConfigs[0].Comment)" -ForegroundColor Gray
            }
            
            if ($config.urls) {
                Write-Host "  ✓ URLs configured: $($config.urls)" -ForegroundColor Green
            }
            
            if ($config.JwtSettings) {
                Write-Host "  ✓ JWT settings configured" -ForegroundColor Green
            }
            
        } catch {
            Write-Host "  ✗ Invalid JSON format!" -ForegroundColor Red
            Write-Host "    Error: $_" -ForegroundColor Red
        }
    } else {
        Write-Host "  ✗ Configuration file not found: $configFile" -ForegroundColor Red
    }
    
    Write-Host ""
}

# Test all environments
$environments = @("Development", "Docker", "Staging", "Production")

foreach ($env in $environments) {
    Test-Environment -EnvironmentName $env
}

# Test base configuration
Write-Host "Testing Base Configuration..." -ForegroundColor Yellow
if (Test-Path "ZR.Admin.WebApi\appsettings.json") {
    Write-Host "  ✓ Base configuration exists" -ForegroundColor Green
    
    try {
        $baseConfig = Get-Content "ZR.Admin.WebApi\appsettings.json" -Raw | ConvertFrom-Json
        Write-Host "  ✓ Valid JSON format" -ForegroundColor Green
    } catch {
        Write-Host "  ✗ Invalid JSON format in base config!" -ForegroundColor Red
    }
} else {
    Write-Host "  ✗ Base configuration not found!" -ForegroundColor Red
}

Write-Host ""
Write-Host "================================" -ForegroundColor Cyan
Write-Host "Configuration Test Summary" -ForegroundColor Cyan
Write-Host "================================" -ForegroundColor Cyan

# Count valid configurations
$validConfigs = 0
foreach ($env in $environments) {
    if (Test-Path "ZR.Admin.WebApi\appsettings.$env.json") {
        $validConfigs++
        Write-Host "  ✓ $env" -ForegroundColor Green
    } else {
        Write-Host "  ✗ $env" -ForegroundColor Red
    }
}

Write-Host ""
Write-Host "Valid configurations: $validConfigs / $($environments.Count)" -ForegroundColor $(if ($validConfigs -eq $environments.Count) { "Green" } else { "Yellow" })

# Show how to run with specific environment
Write-Host ""
Write-Host "To run with a specific environment:" -ForegroundColor Cyan
Write-Host '  $env:ASPNETCORE_ENVIRONMENT = "Development"; dotnet run' -ForegroundColor White
Write-Host '  $env:ASPNETCORE_ENVIRONMENT = "Docker"; dotnet run' -ForegroundColor White
Write-Host '  $env:ASPNETCORE_ENVIRONMENT = "Staging"; dotnet run' -ForegroundColor White
Write-Host '  $env:ASPNETCORE_ENVIRONMENT = "Production"; dotnet run' -ForegroundColor White

Write-Host ""
Write-Host "Current Environment: $($env:ASPNETCORE_ENVIRONMENT)" -ForegroundColor Yellow