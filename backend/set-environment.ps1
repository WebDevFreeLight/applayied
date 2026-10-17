# PowerShell Script to Set and Run Application in Different Environments
# Usage: .\set-environment.ps1 -Environment <Development|Docker|Staging|Production>

param(
    [Parameter(Mandatory=$false)]
    [ValidateSet("Development", "Docker", "Staging", "Production", "List")]
    [string]$Environment = "List",
    
    [Parameter(Mandatory=$false)]
    [switch]$Build,
    
    [Parameter(Mandatory=$false)]
    [switch]$Run,
    
    [Parameter(Mandatory=$false)]
    [switch]$Publish
)

$ErrorActionPreference = "Stop"

# Colors
function Write-ColorOutput($ForegroundColor) {
    $fc = $host.UI.RawUI.ForegroundColor
    $host.UI.RawUI.ForegroundColor = $ForegroundColor
    if ($args) {
        Write-Output $args
    }
    $host.UI.RawUI.ForegroundColor = $fc
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Environment Configuration Manager" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Check if appsettings files exist
function Check-AppSettings {
    $files = @(
        "appsettings.json",
        "appsettings.Development.json",
        "appsettings.Docker.json",
        "appsettings.Staging.json",
        "appsettings.Production.json"
    )
    
    Write-Host "Checking configuration files..." -ForegroundColor Yellow
    foreach ($file in $files) {
        $path = "ZR.Admin.WebApi\$file"
        if (Test-Path $path) {
            Write-Host "  ✓ $file" -ForegroundColor Green
        } else {
            Write-Host "  ✗ $file (missing)" -ForegroundColor Red
        }
    }
    Write-Host ""
}

# List available environments
function List-Environments {
    Write-Host "Available Environments:" -ForegroundColor Cyan
    Write-Host ""
    
    Write-Host "1. Development" -ForegroundColor Green
    Write-Host "   - Local development with detailed logging" -ForegroundColor Gray
    Write-Host "   - Database initialization enabled" -ForegroundColor Gray
    Write-Host "   - Ports: 8887 (HTTP), 8888 (HTTPS)" -ForegroundColor Gray
    Write-Host ""
    
    Write-Host "2. Docker" -ForegroundColor Blue
    Write-Host "   - Containerized environment" -ForegroundColor Gray
    Write-Host "   - Supabase pooler connection" -ForegroundColor Gray
    Write-Host "   - Port: 8887" -ForegroundColor Gray
    Write-Host ""
    
    Write-Host "3. Staging" -ForegroundColor Yellow
    Write-Host "   - Pre-production testing" -ForegroundColor Gray
    Write-Host "   - Production-like settings" -ForegroundColor Gray
    Write-Host "   - Port: 8887" -ForegroundColor Gray
    Write-Host ""
    
    Write-Host "4. Production" -ForegroundColor Red
    Write-Host "   - Live environment" -ForegroundColor Gray
    Write-Host "   - Minimal logging" -ForegroundColor Gray
    Write-Host "   - Ports: 80 (HTTP), 443 (HTTPS)" -ForegroundColor Gray
    Write-Host ""
}

# Set environment
function Set-Environment($env) {
    Write-Host "Setting environment to: $env" -ForegroundColor Yellow
    
    # Set environment variable
    $env:ASPNETCORE_ENVIRONMENT = $env
    [System.Environment]::SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", $env, "User")
    
    Write-Host "✓ Environment variable set" -ForegroundColor Green
    
    # Display current settings
    Write-Host ""
    Write-Host "Current Configuration:" -ForegroundColor Cyan
    Write-Host "  ASPNETCORE_ENVIRONMENT: $env" -ForegroundColor White
    
    # Show connection string based on environment
    $configFile = "ZR.Admin.WebApi\appsettings.$env.json"
    if (Test-Path $configFile) {
        $config = Get-Content $configFile | ConvertFrom-Json
        
        if ($config.dbConfigs -and $config.dbConfigs.Count -gt 0) {
            Write-Host "  Database: $($config.dbConfigs[0].Comment)" -ForegroundColor White
        }
        
        if ($config.urls) {
            Write-Host "  URLs: $($config.urls)" -ForegroundColor White
        }
        
        if ($config.corsUrls) {
            Write-Host "  CORS: $($config.corsUrls -join ', ')" -ForegroundColor White
        }
    }
    Write-Host ""
}

# Build application
function Build-Application($env, $configuration) {
    Write-Host "Building application..." -ForegroundColor Yellow
    Write-Host "  Environment: $env" -ForegroundColor Gray
    Write-Host "  Configuration: $configuration" -ForegroundColor Gray
    
    $buildCmd = "dotnet build ZR.Admin.WebApi\ZR.Admin.WebApi.csproj -c $configuration"
    Write-Host "  Command: $buildCmd" -ForegroundColor Gray
    Write-Host ""
    
    & dotnet build ZR.Admin.WebApi\ZR.Admin.WebApi.csproj -c $configuration
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✓ Build successful" -ForegroundColor Green
    } else {
        Write-Host "✗ Build failed" -ForegroundColor Red
        exit 1
    }
}

# Run application
function Run-Application($env) {
    Write-Host "Starting application in $env environment..." -ForegroundColor Yellow
    Write-Host ""
    
    $runCmd = "dotnet run --project ZR.Admin.WebApi\ZR.Admin.WebApi.csproj"
    Write-Host "Command: $runCmd" -ForegroundColor Gray
    Write-Host "Press Ctrl+C to stop" -ForegroundColor Gray
    Write-Host ""
    
    & dotnet run --project ZR.Admin.WebApi\ZR.Admin.WebApi.csproj
}

# Publish application
function Publish-Application($env, $configuration) {
    Write-Host "Publishing application..." -ForegroundColor Yellow
    Write-Host "  Environment: $env" -ForegroundColor Gray
    Write-Host "  Configuration: $configuration" -ForegroundColor Gray
    
    $outputPath = "publish\$env"
    
    if (Test-Path $outputPath) {
        Remove-Item $outputPath -Recurse -Force
    }
    
    $publishCmd = "dotnet publish ZR.Admin.WebApi\ZR.Admin.WebApi.csproj -c $configuration -o $outputPath"
    Write-Host "  Command: $publishCmd" -ForegroundColor Gray
    Write-Host ""
    
    & dotnet publish ZR.Admin.WebApi\ZR.Admin.WebApi.csproj -c $configuration -o $outputPath
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✓ Published to: $outputPath" -ForegroundColor Green
        
        # Copy environment-specific appsettings
        $envConfig = "ZR.Admin.WebApi\appsettings.$env.json"
        if (Test-Path $envConfig) {
            Copy-Item $envConfig "$outputPath\appsettings.$env.json"
            Write-Host "✓ Copied environment configuration" -ForegroundColor Green
        }
    } else {
        Write-Host "✗ Publish failed" -ForegroundColor Red
        exit 1
    }
}

# Docker-specific commands
function Docker-Commands($action) {
    Write-Host "Docker Environment Commands:" -ForegroundColor Cyan
    Write-Host ""
    
    switch ($action) {
        "build" {
            Write-Host "Building Docker image..." -ForegroundColor Yellow
            docker-compose build
        }
        "run" {
            Write-Host "Starting Docker containers..." -ForegroundColor Yellow
            docker-compose up -d
            Write-Host ""
            Write-Host "View logs: docker-compose logs -f" -ForegroundColor Gray
        }
        "stop" {
            Write-Host "Stopping Docker containers..." -ForegroundColor Yellow
            docker-compose down
        }
        default {
            Write-Host "Available Docker commands:" -ForegroundColor White
            Write-Host "  docker-compose build     # Build image" -ForegroundColor Gray
            Write-Host "  docker-compose up -d     # Start containers" -ForegroundColor Gray
            Write-Host "  docker-compose logs -f   # View logs" -ForegroundColor Gray
            Write-Host "  docker-compose down      # Stop containers" -ForegroundColor Gray
        }
    }
}

# Main execution
Check-AppSettings

if ($Environment -eq "List") {
    List-Environments
    Write-Host "Usage Examples:" -ForegroundColor Cyan
    Write-Host "  .\set-environment.ps1 -Environment Development -Run" -ForegroundColor White
    Write-Host "  .\set-environment.ps1 -Environment Production -Build -Publish" -ForegroundColor White
    Write-Host "  .\set-environment.ps1 -Environment Docker" -ForegroundColor White
    Write-Host ""
    exit 0
}

# Set the environment
Set-Environment $Environment

# Determine configuration
$configuration = switch ($Environment) {
    "Development" { "Debug" }
    "Production" { "Release" }
    default { "Release" }
}

# Execute requested actions
if ($Build) {
    Build-Application $Environment $configuration
}

if ($Publish) {
    Publish-Application $Environment $configuration
}

if ($Run) {
    if ($Environment -eq "Docker") {
        Docker-Commands "run"
    } else {
        Run-Application $Environment
    }
} elseif ($Environment -eq "Docker" -and -not $Build -and -not $Publish) {
    Docker-Commands "list"
}

Write-Host ""
Write-Host "Environment setup complete!" -ForegroundColor Green
Write-Host ""