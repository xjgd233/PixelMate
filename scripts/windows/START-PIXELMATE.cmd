@echo off
setlocal
cd /d "%~dp0"

if not exist "PixelMate.exe" (
    echo.
    echo PixelMate.exe was not found in this folder.
    echo Extract the complete ZIP before starting PixelMate.
    echo.
    pause
    exit /b 1
)

"%~dp0PixelMate.exe"
set "exit_code=%errorlevel%"

if not "%exit_code%"=="0" (
    echo.
    echo PixelMate exited with code %exit_code%.
    pause
)

exit /b %exit_code%
