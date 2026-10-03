@echo off
REM =================================================================
REM Complete Thesis Compilation Script
REM Verifies, generates diagrams, and compiles PDF
REM =================================================================

echo ========================================
echo THESIS COMPILATION WORKFLOW
echo ========================================
echo.
echo This script will:
echo  1. Verify all source files exist
echo  2. Generate all diagrams at high resolution
echo  3. Verify generated images
echo  4. Compile Typst document to PDF
echo.

set START_TIME=%TIME%

REM =================================================================
echo.
echo [STEP 1/4] Verifying Source Files...
echo =================================================================
call verify-all-files.bat
if %ERRORLEVEL% NEQ 0 (
    echo.
    echo [FAILED] Source file verification failed
    echo Please fix errors and try again.
    exit /b 1
)

REM =================================================================
echo.
echo [STEP 2/4] Generating All Diagrams...
echo =================================================================
call generate-all-diagrams.bat
if %ERRORLEVEL% NEQ 0 (
    echo.
    echo [WARNING] Some diagrams failed to generate
    echo Continuing with compilation...
)

REM =================================================================
echo.
echo [STEP 3/4] Verifying Generated Images...
echo =================================================================
call verify-generated-images.bat
if %ERRORLEVEL% NEQ 0 (
    echo.
    echo [WARNING] Some images are missing
    echo Continuing with compilation anyway...
)

REM =================================================================
echo.
echo [STEP 4/4] Compiling Thesis PDF...
echo =================================================================

echo Running: typst compile main.typ thesis.pdf
typst compile main.typ thesis.pdf

if %ERRORLEVEL% NEQ 0 (
    echo.
    echo [FAILED] PDF compilation failed
    echo Check the errors above
    exit /b 1
)

if not exist "thesis.pdf" (
    echo.
    echo [FAILED] PDF was not generated
    exit /b 1
)

REM Get PDF file size
for %%I in ("thesis.pdf") do set PDF_SIZE=%%~zI

echo.
echo ========================================
echo COMPILATION COMPLETE
echo ========================================
echo Start Time: %START_TIME%
echo End Time:   %TIME%
echo PDF Size:   %PDF_SIZE% bytes
echo Location:   %CD%\thesis.pdf
echo.
echo [SUCCESS] Thesis compiled successfully!
echo.
echo Next steps:
echo  1. Open thesis.pdf to review
echo  2. Check all diagrams render correctly
echo  3. Verify no missing references
echo.

exit /b 0
