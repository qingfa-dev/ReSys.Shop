@echo off
REM =================================================================
REM Master Diagram Generation Script
REM Regenerates ALL thesis diagrams at high resolution
REM =================================================================

echo ========================================
echo THESIS DIAGRAM GENERATION
echo ========================================
echo.
echo This will regenerate ALL diagrams:
echo   - 4 ML Model diagrams
echo   - 4 System Architecture diagrams
echo   - 3 Data Architecture diagrams
echo   - 20 Sequence diagrams
echo   - 24 Use Case diagrams
echo.
echo Total: 55 diagrams
echo.

set START_TIME=%TIME%
set TOTAL_GENERATED=0
set ERRORS=0

REM =================================================================
echo [1/5] Generating ML Models Diagrams...
echo =================================================================
cd diagrams\01-ml-models
call generate-ml-models.bat
if %ERRORLEVEL% NEQ 0 (
    echo [ERROR] ML Models generation failed
    set /a ERRORS+=1
) else (
    set /a TOTAL_GENERATED+=4
)
cd ..\..

echo.
REM =================================================================
echo [2/5] Generating System Architecture Diagrams...
echo =================================================================
cd diagrams\02-system-architecture
call generate-system-arch.bat
if %ERRORLEVEL% NEQ 0 (
    echo [ERROR] System Architecture generation failed
    set /a ERRORS+=1
) else (
    set /a TOTAL_GENERATED+=4
)
cd ..\..

echo.
REM =================================================================
echo [3/5] Generating Data Architecture Diagrams...
echo =================================================================
cd diagrams\03-data-architecture
call generate-data-arch.bat
if %ERRORLEVEL% NEQ 0 (
    echo [ERROR] Data Architecture generation failed
    set /a ERRORS+=1
) else (
    set /a TOTAL_GENERATED+=3
)
cd ..\..

echo.
REM =================================================================
echo [4/5] Generating Sequence Diagrams...
echo =================================================================
cd diagrams\sequences
call generate-sequences.bat
if %ERRORLEVEL% NEQ 0 (
    echo [ERROR] Sequence Diagrams generation failed
    set /a ERRORS+=1
) else (
    set /a TOTAL_GENERATED+=20
)
cd ..\..

echo.
REM =================================================================
echo [5/5] Generating Use Case Diagrams...
echo =================================================================
cd diagrams\usecases
call generate-usecases.bat
if %ERRORLEVEL% NEQ 0 (
    echo [ERROR] Use Case Diagrams generation failed
    set /a ERRORS+=1
) else (
    set /a TOTAL_GENERATED+=24
)
cd ..\..

echo.
REM =================================================================
echo [6/6] Generating Charts...
echo =================================================================
call diagrams\charts\generate-charts.bat
if %ERRORLEVEL% NEQ 0 (
    echo [ERROR] Chart generation failed
    set /a ERRORS+=1
) else (
    set /a TOTAL_GENERATED+=1
)

echo.
echo ========================================
echo GENERATION SUMMARY
echo ========================================
echo Start Time:        %START_TIME%
echo End Time:          %TIME%
echo Diagrams Generated: %TOTAL_GENERATED% of 55
echo Errors:            %ERRORS%

if %ERRORS% GTR 0 (
    echo.
    echo [FAILED] Generation completed with %ERRORS% error^(s^)
    exit /b 1
) else (
    echo.
    echo [SUCCESS] All diagrams generated successfully!
    exit /b 0
)
