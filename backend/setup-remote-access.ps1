# PowerShell Script to Enable Remote Access on Windows
# Run as Administrator

Write-Host "Setting up Remote Access for PostgreSQL and API..." -ForegroundColor Green

# Check if running as Administrator
if (-NOT ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] "Administrator"))
{
    Write-Host "This script requires Administrator privileges. Please run as Administrator." -ForegroundColor Red
    exit 1
}

# PostgreSQL Firewall Rules
Write-Host "`nConfiguring PostgreSQL Firewall Rules..." -ForegroundColor Yellow

# Remove existing rules if they exist
Remove-NetFirewallRule -DisplayName "PostgreSQL Inbound" -ErrorAction SilentlyContinue
Remove-NetFirewallRule -DisplayName "PostgreSQL Docker" -ErrorAction SilentlyContinue

# Add PostgreSQL rules
New-NetFirewallRule -DisplayName "PostgreSQL Inbound" `
    -Direction Inbound `
    -Protocol TCP `
    -LocalPort 5432 `
    -Action Allow `
    -Profile Any `
    -Description "Allow PostgreSQL connections on port 5432"

New-NetFirewallRule -DisplayName "PostgreSQL Docker" `
    -Direction Inbound `
    -Protocol TCP `
    -LocalPort 5433 `
    -Action Allow `
    -Profile Any `
    -Description "Allow PostgreSQL Docker connections on port 5433"

# API Firewall Rules
Write-Host "`nConfiguring API Firewall Rules..." -ForegroundColor Yellow

# Remove existing rule if it exists
Remove-NetFirewallRule -DisplayName "Maham API" -ErrorAction SilentlyContinue

# Add API rule
New-NetFirewallRule -DisplayName "Maham API" `
    -Direction Inbound `
    -Protocol TCP `
    -LocalPort 8888 `
    -Action Allow `
    -Profile Any `
    -Description "Allow Maham API connections on port 8888"

# Docker Desktop Rules (if Docker is installed)
$dockerInstalled = Get-Service -Name "com.docker.service" -ErrorAction SilentlyContinue
if ($dockerInstalled) {
    Write-Host "`nConfiguring Docker Firewall Rules..." -ForegroundColor Yellow
    
    # Allow Docker NAT
    New-NetFirewallRule -DisplayName "Docker NAT" `
        -Direction Inbound `
        -Protocol Any `
        -LocalAddress 10.0.0.0/8, 172.16.0.0/12, 192.168.0.0/16 `
        -Action Allow `
        -Profile Any `
        -Description "Allow Docker container communication" `
        -ErrorAction SilentlyContinue
}

# Display current rules
Write-Host "`nCurrent Firewall Rules:" -ForegroundColor Cyan
Get-NetFirewallRule -DisplayName "PostgreSQL*", "Maham API", "Docker NAT" -ErrorAction SilentlyContinue | 
    Format-Table DisplayName, Enabled, Direction, Action -AutoSize

# Get local IP addresses
Write-Host "`nYour IP Addresses:" -ForegroundColor Cyan
$ipAddresses = Get-NetIPAddress -AddressFamily IPv4 | 
    Where-Object { $_.InterfaceAlias -notlike "*Loopback*" } | 
    Select-Object IPAddress, InterfaceAlias

foreach ($ip in $ipAddresses) {
    Write-Host "  $($ip.InterfaceAlias): $($ip.IPAddress)" -ForegroundColor White
}

# Test port availability
Write-Host "`nTesting Port Availability..." -ForegroundColor Yellow

function Test-Port {
    param($Port)
    $listener = Get-NetTCPConnection -LocalPort $Port -ErrorAction SilentlyContinue
    if ($listener) {
        Write-Host "  Port $Port`: IN USE" -ForegroundColor Yellow
        return $false
    } else {
        Write-Host "  Port $Port`: AVAILABLE" -ForegroundColor Green
        return $true
    }
}

Test-Port 5432
Test-Port 5433
Test-Port 8888

# Docker commands
Write-Host "`nDocker Quick Commands:" -ForegroundColor Cyan
Write-Host "  Start all services:" -ForegroundColor White
Write-Host "    docker-compose up -d" -ForegroundColor Gray
Write-Host ""
Write-Host "  Start only PostgreSQL:" -ForegroundColor White
Write-Host "    docker-compose up -d postgres" -ForegroundColor Gray
Write-Host ""
Write-Host "  Check logs:" -ForegroundColor White
Write-Host "    docker-compose logs -f" -ForegroundColor Gray
Write-Host ""
Write-Host "  Stop all services:" -ForegroundColor White
Write-Host "    docker-compose down" -ForegroundColor Gray

# Connection strings
Write-Host "`nConnection Strings for Remote Access:" -ForegroundColor Cyan
$primaryIP = (Get-NetIPAddress -AddressFamily IPv4 | 
    Where-Object { $_.InterfaceAlias -notlike "*Loopback*" -and $_.InterfaceAlias -like "*Ethernet*" } | 
    Select-Object -First 1).IPAddress

if ($primaryIP) {
    Write-Host "  PostgreSQL (Direct):" -ForegroundColor White
    Write-Host "    Host=$primaryIP;Port=5432;Database=MahamDB;Username=postgres;Password=postgres123;" -ForegroundColor Gray
    Write-Host ""
    Write-Host "  PostgreSQL (Docker):" -ForegroundColor White
    Write-Host "    Host=$primaryIP;Port=5433;Database=MahamDB;Username=postgres;Password=postgres123;" -ForegroundColor Gray
    Write-Host ""
    Write-Host "  API Endpoint:" -ForegroundColor White
    Write-Host "    http://${primaryIP}:8888" -ForegroundColor Gray
}

Write-Host "`nSetup Complete!" -ForegroundColor Green
Write-Host "Remember to:" -ForegroundColor Yellow
Write-Host "  1. Use strong passwords in production"
Write-Host "  2. Restrict IP ranges for better security"
Write-Host "  3. Enable SSL/TLS for encrypted connections"
Write-Host "  4. Regularly update and patch your systems"