# PowerShell script để dừng Authentication Service

Write-Host "Stopping Authentication Service..." -ForegroundColor Yellow
docker-compose down

Write-Host ""
Write-Host "Containers stopped." -ForegroundColor Green
Write-Host ""
Write-Host "To remove volumes (delete database data):" -ForegroundColor Yellow
Write-Host "  docker-compose down -v" -ForegroundColor Cyan
Write-Host ""

