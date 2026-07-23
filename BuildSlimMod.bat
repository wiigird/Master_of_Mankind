@echo off
setlocal

cd /d "%~dp0"

where py >nul 2>nul
if not errorlevel 1 (
    py -3 BuildSlimMod.py
    exit /b %errorlevel%
)

where python >nul 2>nul
if not errorlevel 1 (
    python BuildSlimMod.py
    exit /b %errorlevel%
)

echo ERROR: Python 3 was not found in PATH.
echo Install Python 3, then run this file again.
exit /b 1
