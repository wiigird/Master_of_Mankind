@echo off
setlocal

title Build Master_of_Mankind Mod
cd /d "%~dp0"

set "PROJECT=Master_of_Mankind.csproj"
set "MOD_DIR=D:\SteamLibrary\steamapps\common\Slay the Spire 2\mods\Master_of_Mankind"

echo ========================================
echo   Master_of_Mankind Mod Builder
echo ========================================
echo.

if not exist "%PROJECT%" (
    echo ERROR: Could not find %PROJECT%.
    goto :fail
)

where dotnet >nul 2>nul
if errorlevel 1 (
    echo ERROR: .NET SDK was not found in PATH.
    echo Install .NET 9 SDK and try again.
    goto :fail
)

tasklist /FI "IMAGENAME eq SlayTheSpire2.exe" 2>nul | find /I "SlayTheSpire2.exe" >nul
if not errorlevel 1 (
    echo ERROR: Slay the Spire 2 is currently running.
    echo Close the game before building so the mod files are not locked.
    goto :fail
)

echo Building DLL and exporting PCK...
echo.
dotnet publish "%PROJECT%" -c Release -v:minimal
if errorlevel 1 goto :fail

if not exist "%MOD_DIR%\Master_of_Mankind.pck" (
    echo.
    echo ERROR: Build finished without producing the expected PCK.
    goto :fail
)

echo.
echo ========================================
echo BUILD SUCCEEDED
echo Output: %MOD_DIR%
echo ========================================
goto :end

:fail
echo.
echo ========================================
echo BUILD FAILED
echo Read the error messages above.
echo ========================================
set "BUILD_FAILED=1"

:end
if /I not "%~1"=="--no-pause" pause
if defined BUILD_FAILED exit /b 1
exit /b 0
