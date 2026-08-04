@echo off
REM Sequence Diagram Generator (PlantUML)
echo ========================================
echo PlantUML Sequence Generator
echo ========================================

set PLANTUML_PATH=C:\ProgramData\chocolatey\bin\plantuml.exe
set SOURCE_DIR=%~dp0sources
set OUTPUT_DIR=%~dp0..\..\images\diagrams\sequences
set PLANTUML_LIMIT_SIZE=16384

if not exist "%OUTPUT_DIR%\customer" mkdir "%OUTPUT_DIR%\customer"
if not exist "%OUTPUT_DIR%\admin" mkdir "%OUTPUT_DIR%\admin"
if not exist "%OUTPUT_DIR%\system" mkdir "%OUTPUT_DIR%\system"

echo Cleaning old diagrams...
del /q "%OUTPUT_DIR%\customer\*.png"
del /q "%OUTPUT_DIR%\admin\*.png"
del /q "%OUTPUT_DIR%\system\*.png"

echo Generating customer sequences at HIGH RESOLUTION (300 DPI)...
for %%f in ("%SOURCE_DIR%\customer\*.puml") do (
    echo   Processing: %%~nxf
    "%PLANTUML_PATH%" -DPLANTUML_DPI=300 -tpng "%%f" -o "%OUTPUT_DIR%\customer"
)

echo.
echo Generating system sequences at HIGH RESOLUTION (300 DPI)...
for %%f in ("%SOURCE_DIR%\system\*.puml") do (
    echo   Processing: %%~nxf
    "%PLANTUML_PATH%" -DPLANTUML_DPI=300 -tpng "%%f" -o "%OUTPUT_DIR%\system"
)

echo.
echo Generating admin sequences at HIGH RESOLUTION (300 DPI)...
for %%f in ("%SOURCE_DIR%\admin\*.puml") do (
    echo   Processing: %%~nxf
    "%PLANTUML_PATH%" -DPLANTUML_DPI=300 -tpng "%%f" -o "%OUTPUT_DIR%\admin"
)

echo.
echo Done! Images saved to: %OUTPUT_DIR%
