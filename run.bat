@echo off
REM Batch script để chạy Authentication Service với Docker

echo === Authentication Service - Quick Start ===
echo.

REM Kiểm tra Docker
echo Checking Docker...
docker --version >nul 2>&1
if errorlevel 1 (
    echo [ERROR] Docker is not installed. Please install Docker Desktop first.
    pause
    exit /b 1
)

docker-compose --version >nul 2>&1
if errorlevel 1 (
    echo [ERROR] Docker Compose is not installed.
    pause
    exit /b 1
)

echo [OK] Docker is installed
echo.

REM Build và chạy containers
echo Building and starting containers...
docker-compose up -d

if errorlevel 1 (
    echo [ERROR] Failed to start containers
    pause
    exit /b 1
)

echo [OK] Containers started successfully
echo.

REM Chờ SQL Server khởi động
echo Waiting for SQL Server to be ready...
timeout /t 30 /nobreak >nul

echo [OK] SQL Server should be ready
echo.

REM Kiểm tra và tạo migration
echo Checking database migrations...
docker exec auth-api dotnet tool install --global dotnet-ef >nul 2>&1

echo Creating database migrations...
docker exec auth-api dotnet ef migrations add InitialCreate --project /src >nul 2>&1

echo Applying database migrations...
docker exec auth-api dotnet ef database update --project /src

if errorlevel 1 (
    echo [WARNING] Database migration had issues. You may need to run manually:
    echo   docker exec -it auth-api dotnet ef database update --project /src
) else (
    echo [OK] Database initialized successfully
)

echo.
echo === Application is ready! ===
echo.
echo Access the application at:
echo   Frontend: http://localhost:3000
echo   Backend:  http://localhost:5000
echo   Swagger:  http://localhost:5000/swagger
echo.
echo To view logs:
echo   docker-compose logs -f
echo.
echo To stop:
echo   docker-compose down
echo.

pause

