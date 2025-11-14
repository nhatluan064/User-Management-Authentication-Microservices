# PowerShell script để chạy Authentication Service với Docker

Write-Host "=== Authentication Service - Quick Start ===" -ForegroundColor Green
Write-Host ""

# Kiểm tra Docker
Write-Host "Checking Docker..." -ForegroundColor Yellow
try {
    docker --version | Out-Null
    docker-compose --version | Out-Null
    Write-Host "✓ Docker is installed" -ForegroundColor Green
} catch {
    Write-Host "✗ Docker is not installed. Please install Docker Desktop first." -ForegroundColor Red
    exit 1
}

# Kiểm tra xem containers đã chạy chưa
$running = docker ps --filter "name=auth" --format "{{.Names}}"
if ($running) {
    Write-Host ""
    Write-Host "Containers are already running:" -ForegroundColor Yellow
    $running | ForEach-Object { Write-Host "  - $_" -ForegroundColor Cyan }
    Write-Host ""
    $choice = Read-Host "Do you want to restart? (y/n)"
    if ($choice -eq "y" -or $choice -eq "Y") {
        Write-Host "Stopping containers..." -ForegroundColor Yellow
        docker-compose down
    } else {
        Write-Host "Using existing containers." -ForegroundColor Green
        Write-Host ""
        Write-Host "Access the application at:" -ForegroundColor Green
        Write-Host "  Frontend: http://localhost:3000" -ForegroundColor Cyan
        Write-Host "  Backend:  http://localhost:5000" -ForegroundColor Cyan
        Write-Host "  Swagger:  http://localhost:5000/swagger" -ForegroundColor Cyan
        exit 0
    }
}

# Build và chạy containers
Write-Host ""
Write-Host "Building and starting containers..." -ForegroundColor Yellow
docker-compose up -d

if ($LASTEXITCODE -ne 0) {
    Write-Host "✗ Failed to start containers" -ForegroundColor Red
    exit 1
}

Write-Host "✓ Containers started successfully" -ForegroundColor Green
Write-Host ""

# Chờ SQL Server khởi động
Write-Host "Waiting for SQL Server to be ready..." -ForegroundColor Yellow
$maxAttempts = 30
$attempt = 0
$ready = $false

while ($attempt -lt $maxAttempts -and -not $ready) {
    Start-Sleep -Seconds 2
    $attempt++
    try {
        $result = docker exec auth-db /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P YourStrong@Passw0rd -Q "SELECT 1" 2>&1
        if ($LASTEXITCODE -eq 0) {
            $ready = $true
        }
    } catch {
        # Continue waiting
    }
    Write-Progress -Activity "Waiting for SQL Server" -PercentComplete (($attempt / $maxAttempts) * 100)
}

Write-Progress -Activity "Waiting for SQL Server" -Completed
Write-Host "✓ SQL Server is ready" -ForegroundColor Green
Write-Host ""

# Kiểm tra xem đã có migration chưa
Write-Host "Checking database migrations..." -ForegroundColor Yellow
$migrationsExist = docker exec auth-api test -d /src/Migrations 2>&1

if ($LASTEXITCODE -ne 0) {
    Write-Host "Creating database migrations..." -ForegroundColor Yellow
    
    # Cài đặt EF Core tools nếu chưa có
    Write-Host "Installing EF Core tools..." -ForegroundColor Yellow
    docker exec auth-api dotnet tool install --global dotnet-ef 2>&1 | Out-Null
    
    # Tạo migration
    Write-Host "Creating initial migration..." -ForegroundColor Yellow
    docker exec auth-api dotnet ef migrations add InitialCreate --project /src 2>&1 | Out-Null
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "⚠ Migration creation had issues, but continuing..." -ForegroundColor Yellow
    }
}

# Apply migrations
Write-Host "Applying database migrations..." -ForegroundColor Yellow
docker exec auth-api dotnet ef database update --project /src 2>&1 | Out-Null

if ($LASTEXITCODE -eq 0) {
    Write-Host "✓ Database initialized successfully" -ForegroundColor Green
} else {
    Write-Host "⚠ Database migration had issues. You may need to run manually:" -ForegroundColor Yellow
    Write-Host "  docker exec -it auth-api dotnet ef database update --project /src" -ForegroundColor Cyan
}

Write-Host ""
Write-Host "=== Application is ready! ===" -ForegroundColor Green
Write-Host ""
Write-Host "Access the application at:" -ForegroundColor Green
Write-Host "  Frontend: http://localhost:3000" -ForegroundColor Cyan
Write-Host "  Backend:  http://localhost:5000" -ForegroundColor Cyan
Write-Host "  Swagger:  http://localhost:5000/swagger" -ForegroundColor Cyan
Write-Host ""
Write-Host "To view logs:" -ForegroundColor Yellow
Write-Host "  docker-compose logs -f" -ForegroundColor Cyan
Write-Host ""
Write-Host "To stop:" -ForegroundColor Yellow
Write-Host "  docker-compose down" -ForegroundColor Cyan
Write-Host ""

