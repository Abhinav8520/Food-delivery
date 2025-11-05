# Food Delivery App - Unified Startup Script
# This script starts all services: C# Recommendation Service, Node.js Backend, Frontend, and Admin Panel

param(
    [switch]$SkipChecks = $false
)

$ErrorActionPreference = "Continue"

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Food Delivery App - Startup Script   " -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Get the root directory
$rootDir = $PSScriptRoot
$services = @()

# Function to check if a port is in use
function Test-Port {
    param([int]$Port)
    $connection = Test-NetConnection -ComputerName localhost -Port $Port -WarningAction SilentlyContinue -InformationLevel Quiet
    return $connection
}

# Function to wait for a service to be ready
function Wait-ForService {
    param(
        [string]$Url,
        [int]$TimeoutSeconds = 30,
        [string]$ServiceName
    )
    Write-Host "  Waiting for $ServiceName..." -ForegroundColor Yellow -NoNewline
    $waited = 0
    while ($waited -lt $TimeoutSeconds) {
        try {
            $response = Invoke-WebRequest -Uri $Url -TimeoutSec 2 -UseBasicParsing -ErrorAction Stop
            if ($response.StatusCode -eq 200) {
                Write-Host " [OK]" -ForegroundColor Green
                return $true
            }
        } catch {
            # Service not ready yet
        }
        Start-Sleep -Seconds 1
        Write-Host "." -ForegroundColor Yellow -NoNewline
        $waited++
    }
    Write-Host " [TIMEOUT]" -ForegroundColor Red
    return $false
}

# Check prerequisites
if (-not $SkipChecks) {
    Write-Host "Checking prerequisites..." -ForegroundColor Yellow
    
    # Check Node.js
    try {
        $nodeVersion = node --version
        Write-Host "  [OK] Node.js: $nodeVersion" -ForegroundColor Green
    } catch {
        Write-Host "  [FAIL] Node.js not found. Please install Node.js." -ForegroundColor Red
        exit 1
    }
    
    # Check .NET SDK
    try {
        $dotnetVersion = dotnet --version
        Write-Host "  [OK] .NET SDK: $dotnetVersion" -ForegroundColor Green
    } catch {
        Write-Host "  [FAIL] .NET SDK not found. Please install .NET SDK 7.0 or higher." -ForegroundColor Red
        exit 1
    }
    
    # Check .env file
    $envPath = Join-Path $rootDir "backend\.env"
    if (Test-Path $envPath) {
        Write-Host "  [OK] Backend .env file found" -ForegroundColor Green
    } else {
        Write-Host "  [WARN] Backend .env file not found. Make sure to configure it." -ForegroundColor Yellow
    }
    
    Write-Host ""
}

# Check if ports are already in use
Write-Host "Checking ports..." -ForegroundColor Yellow
$ports = @{4000 = "Backend"; 5001 = "Recommendation Service"; 5173 = "Frontend"; 5174 = "Admin"}
$portsInUse = @()

foreach ($port in $ports.Keys) {
    if (Test-Port -Port $port) {
        Write-Host "  [WARN] Port $port ($($ports[$port])) is already in use" -ForegroundColor Yellow
        $portsInUse += $port
    } else {
        Write-Host "  [OK] Port $port ($($ports[$port])) is available" -ForegroundColor Green
    }
}

if ($portsInUse.Count -gt 0) {
    Write-Host ""
    $response = Read-Host "Some ports are in use. Continue anyway? (y/n)"
    if ($response -ne "y" -and $response -ne "Y") {
        exit 0
    }
}

Write-Host ""

# Cleanup function
function Stop-AllServices {
    Write-Host ""
    Write-Host "Stopping all services..." -ForegroundColor Yellow
    foreach ($service in $services) {
        if ($service.Process -and -not $service.Process.HasExited) {
            Write-Host "  Stopping $($service.Name)..." -ForegroundColor Gray
            try {
                $service.Process.Kill()
            } catch {
                # Ignore errors
            }
        }
    }
    Write-Host "  [OK] All services stopped" -ForegroundColor Green
}

# Register cleanup on exit
Register-EngineEvent PowerShell.Exiting -Action { Stop-AllServices } | Out-Null

# 1. Start C# Recommendation Service
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "1. Starting C# Recommendation Service" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

$recommendationServicePath = Join-Path $rootDir "recommendation-service\RecommendationService"
if (Test-Path $recommendationServicePath) {
    $recStartInfo = New-Object System.Diagnostics.ProcessStartInfo
    $recStartInfo.FileName = "dotnet"
    $recStartInfo.Arguments = "run"
    $recStartInfo.WorkingDirectory = $recommendationServicePath
    $recStartInfo.UseShellExecute = $false
    $recStartInfo.RedirectStandardOutput = $true
    $recStartInfo.RedirectStandardError = $true
    $recStartInfo.CreateNoWindow = $false
    
    $recProcess = [System.Diagnostics.Process]::Start($recStartInfo)
    $services += @{Name = "Recommendation Service"; Process = $recProcess}
    
    Write-Host "  Started Recommendation Service (PID: $($recProcess.Id))" -ForegroundColor Green
    
    # Wait for service to be ready
    Start-Sleep -Seconds 3
    Wait-ForService -Url "http://localhost:5001/api/recommendation/health" -ServiceName "Recommendation Service" | Out-Null
} else {
    Write-Host "  [FAIL] Recommendation service directory not found!" -ForegroundColor Red
}

Write-Host ""

# 2. Start Node.js Backend
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "2. Starting Node.js Backend" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

$backendPath = Join-Path $rootDir "backend"
if (Test-Path $backendPath) {
$backendProcess = Start-Process -FilePath "cmd.exe" -ArgumentList "/c npm run server" -WorkingDirectory $backendPath -PassThru -NoNewWindow
    $services += @{Name = "Node.js Backend"; Process = $backendProcess}
    
    Write-Host "  Started Node.js Backend (PID: $($backendProcess.Id))" -ForegroundColor Green
    
    # Wait for backend to be ready
    Start-Sleep -Seconds 3
    $backendPort = 4000
    $backendReady = Wait-ForService -Url "http://localhost:$backendPort" -ServiceName "Node.js Backend"
    if (-not $backendReady) {
        Write-Host "  [WARN] Backend may still be starting..." -ForegroundColor Yellow
    }
} else {
    Write-Host "  [FAIL] Backend directory not found!" -ForegroundColor Red
}

Write-Host ""

# 3. Start Frontend
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "3. Starting Frontend (React)" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

$frontendPath = Join-Path $rootDir "frontend"
if (Test-Path $frontendPath) {
$frontendProcess = Start-Process -FilePath "cmd.exe" -ArgumentList "/c npm run dev" -WorkingDirectory $frontendPath -PassThru -NoNewWindow
    $services += @{Name = "Frontend"; Process = $frontendProcess}
    
    Write-Host "  Started Frontend (PID: $($frontendProcess.Id))" -ForegroundColor Green
    Write-Host "  Frontend will be available at: http://localhost:5173" -ForegroundColor Gray
} else {
    Write-Host "  [FAIL] Frontend directory not found!" -ForegroundColor Red
}

Write-Host ""

# 4. Start Admin Panel
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "4. Starting Admin Panel" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

$adminPath = Join-Path $rootDir "admin"
if (Test-Path $adminPath) {
$adminProcess = Start-Process -FilePath "cmd.exe" -ArgumentList "/c npm run dev" -WorkingDirectory $adminPath -PassThru -NoNewWindow
    $services += @{Name = "Admin Panel"; Process = $adminProcess}
    
    Write-Host "  Started Admin Panel (PID: $($adminProcess.Id))" -ForegroundColor Green
    Write-Host "  Admin Panel will be available at: http://localhost:5174" -ForegroundColor Gray
} else {
    Write-Host "  [FAIL] Admin directory not found!" -ForegroundColor Red
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "  All Services Started Successfully!   " -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
Write-Host "Services:" -ForegroundColor Cyan
Write-Host "  • Recommendation Service: http://localhost:5001" -ForegroundColor White
Write-Host "  • Backend API:            http://localhost:4000" -ForegroundColor White
Write-Host "  • Frontend (Customer):    http://localhost:5173" -ForegroundColor White
Write-Host "  • Admin Panel:            http://localhost:5174" -ForegroundColor White
Write-Host ""
Write-Host "Press Ctrl+C to stop all services" -ForegroundColor Yellow
Write-Host ""

# Keep script running and wait for Ctrl+C
try {
    while ($true) {
        Start-Sleep -Seconds 1
        # Check if any service has exited
        foreach ($service in $services) {
            if ($service.Process -and $service.Process.HasExited -and $service.Process.ExitCode -ne 0) {
                Write-Host ""
                Write-Host "[WARN] $($service.Name) has exited unexpectedly" -ForegroundColor Yellow
            }
        }
    }
} catch {
    # Handle Ctrl+C
} finally {
    Stop-AllServices
}

