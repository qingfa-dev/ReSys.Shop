@echo off
REM Data Architecture Diagram Generator (Mermaid)
echo ========================================
echo Mermaid Data Architecture Generator
echo ========================================

set MMDC_PATH=mmdc
set SOURCE_DIR=%~dp0sources
set OUTPUT_DIR=%~dp0..\..\images\diagrams\03-data-architecture

if not exist "%OUTPUT_DIR%" mkdir "%OUTPUT_DIR%"

echo Cleaning old diagrams...
del /q "%OUTPUT_DIR%\*.png"

echo Generating Data Architecture diagrams at HIGH RESOLUTION...
for %%f in ("%SOURCE_DIR%\*.mmd") do (
    echo   Processing: %%~nxf
    call "%MMDC_PATH%" -i "%%f" -o "%OUTPUT_DIR%\%%~nf.png" -t default -b transparent -w 2400 -H 1800 -s 3
)

echo.
echo Generating Subsection ERDs (Catalog, Ordering, Inventory, Identity)...
call "%MMDC_PATH%" -i "%SOURCE_DIR%\data-01a-catalog-erd.mmd" -o "%OUTPUT_DIR%\data-01a-catalog-erd.png" -t default -b transparent -w 2400 -H 1800 -s 3
call "%MMDC_PATH%" -i "%SOURCE_DIR%\data-01b-ordering-erd.mmd" -o "%OUTPUT_DIR%\data-01b-ordering-erd.png" -t default -b transparent -w 2400 -H 1800 -s 3
call "%MMDC_PATH%" -i "%SOURCE_DIR%\data-01c-inventory-erd.mmd" -o "%OUTPUT_DIR%\data-01c-inventory-erd.png" -t default -b transparent -w 2400 -H 1800 -s 3
call "%MMDC_PATH%" -i "%SOURCE_DIR%\data-01d-identity-erd.mmd" -o "%OUTPUT_DIR%\data-01d-identity-erd.png" -t default -b transparent -w 2400 -H 1800 -s 3

echo.
echo Done! Images saved to: %OUTPUT_DIR%
