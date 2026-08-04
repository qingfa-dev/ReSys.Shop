@echo off
REM =================================================================
REM Chart Generator (Python + Matplotlib)
REM =================================================================

echo ========================================
echo Generating Charts...
echo ========================================

python --version >nul 2>&1
if %ERRORLEVEL% NEQ 0 (
    echo [ERROR] Python not found. Skipping charts.
    exit /b 1
)

python -c "import matplotlib" >nul 2>&1
if %ERRORLEVEL% NEQ 0 (
    echo [WARNING] Matplotlib not found. Installing...
    pip install matplotlib
)

python "%~dp0generate_charts.py"
if %ERRORLEVEL% NEQ 0 (
    echo [ERROR] Failed to generate charts
    exit /b 1
)

echo Done!
