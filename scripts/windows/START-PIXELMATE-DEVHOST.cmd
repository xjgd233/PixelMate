@echo off
setlocal
cd /d "%~dp0"

where dotnet >nul 2>nul
if errorlevel 1 (
    echo.
    echo PixelMate developer acceptance package requires the .NET 10 Runtime x64.
    echo Install it with:
    echo.
    echo   winget install --id Microsoft.DotNet.Runtime.10 --exact
    echo.
    echo Then run this file again.
    echo.
    pause
    exit /b 1
)

dotnet PixelMate.dll
set "exit_code=%errorlevel%"

if not "%exit_code%"=="0" (
    echo.
    echo PixelMate exited with code %exit_code%.
    pause
)

exit /b %exit_code%
