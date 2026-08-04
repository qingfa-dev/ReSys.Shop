@echo off
REM ML Models Diagram Generator (Mermaid)
echo ========================================
echo Mermaid ML Models Generator
echo ========================================

set MMDC_PATH=mmdc
set SOURCE_DIR=%~dp0sources
set OUTPUT_DIR=%~dp0..\..\images\diagrams\01-ml-models

if not exist "%OUTPUT_DIR%" mkdir "%OUTPUT_DIR%"

echo Cleaning old diagrams...
del /q "%OUTPUT_DIR%\*.png"

echo Generating ML Model diagrams at HIGH RESOLUTION...
for %%f in ("%SOURCE_DIR%\*.mmd") do (
    echo   Processing: %%~nxf
    call "%MMDC_PATH%" -i "%%f" -o "%OUTPUT_DIR%\%%~nf.png" -t default -b transparent -w 2400 -H 1800 -s 3
)

echo.
echo Done! Images saved to: %OUTPUT_DIR%
