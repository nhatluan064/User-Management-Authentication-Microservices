@echo off
echo Stopping Authentication Service...
docker-compose down
echo.
echo Containers stopped.
echo.
echo To remove volumes (delete database data):
echo   docker-compose down -v
echo.
pause

