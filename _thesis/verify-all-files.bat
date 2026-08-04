@echo off
REM =================================================================
REM Master File Verification Script for Thesis
REM Verifies all source files, tools, and directories exist
REM =================================================================

echo ========================================
echo THESIS FILE VERIFICATION
echo ========================================
echo.

set ERROR_COUNT=0
set SUCCESS_COUNT=0

REM Check required tools
echo [1/6] Checking Required Tools...
echo.

where mmdc >nul 2>&1
if %ERRORLEVEL% EQU 0 (
    echo [OK] Mermaid CLI ^(mmdc^) found
    set /a SUCCESS_COUNT+=1
) else (
    echo [ERROR] Mermaid CLI ^(mmdc^) NOT found - Install: npm install -g @mermaid-js/mermaid-cli
    set /a ERROR_COUNT+=1
)

where plantuml >nul 2>&1
if %ERRORLEVEL% EQU 0 (
    echo [OK] PlantUML found
    set /a SUCCESS_COUNT+=1
) else (
    echo [ERROR] PlantUML NOT found - Install: choco install plantuml
    set /a ERROR_COUNT+=1
)

where typst >nul 2>&1
if %ERRORLEVEL% EQU 0 (
    echo [OK] Typst found
    set /a SUCCESS_COUNT+=1
) else (
    echo [ERROR] Typst NOT found - Install from https://github.com/typst/typst/releases
    set /a ERROR_COUNT+=1
)

echo.
echo [2/6] Checking Mermaid Source Files ^(11 expected^)...
echo.

set MMD_COUNT=0
for %%f in (
    "diagrams\01-ml-models\sources\ml-01-efficientnet-b0.mmd"
    "diagrams\01-ml-models\sources\ml-02-dinov2.mmd"
    "diagrams\01-ml-models\sources\ml-03-fashion-clip.mmd"
    "diagrams\01-ml-models\sources\ml-04-clip-vit-b16.mmd"
    "diagrams\02-system-architecture\sources\sys-01-overview.mmd"
    "diagrams\02-system-architecture\sources\sys-03-api-structure.mmd"
    "diagrams\02-system-architecture\sources\sys-04-domain-model.mmd"
    "diagrams\03-data-architecture\sources\data-01-backend-erd.mmd"
    "diagrams\03-data-architecture\sources\data-02-pgvector-hnsw.mmd"
    "diagrams\03-data-architecture\sources\data-03-ml-service-structure.mmd"
    "diagrams\sequences\sources\ml\sequence-visual-search.mmd"
) do (
    if exist "%%~f" (
        echo [OK] %%~f
        set /a MMD_COUNT+=1
        set /a SUCCESS_COUNT+=1
    ) else (
        echo [ERROR] MISSING: %%~f
        set /a ERROR_COUNT+=1
    )
)

echo.
echo Found %MMD_COUNT% of 11 Mermaid files

echo.
echo [3/6] Checking PlantUML Source Files ^(44 expected^)...
echo.

set PUML_COUNT=0

REM Sequence diagrams - Customer
for %%f in (
    "diagrams\sequences\sources\customer\sq-0001-browse-products.puml"
    "diagrams\sequences\sources\customer\sq-0002-checkout.puml"
    "diagrams\sequences\sources\customer\sq-0003-keyword-search.puml"
    "diagrams\sequences\sources\customer\sq-0004-visual-search.puml"
    "diagrams\sequences\sources\customer\sq-0005-cart.puml"
    "diagrams\sequences\sources\customer\sq-0006-track-order.puml"
    "diagrams\sequences\sources\customer\sq-0007-address-book.puml"
    "diagrams\sequences\sources\customer\sq-0008-recommendations.puml"
) do (
    if exist "%%~f" (
        set /a PUML_COUNT+=1
        set /a SUCCESS_COUNT+=1
    ) else (
        echo [ERROR] MISSING: %%~f
        set /a ERROR_COUNT+=1
    )
)

REM Sequence diagrams - Admin
for %%f in (
    "diagrams\sequences\sources\admin\sq-0009-manage-products.puml"
    "diagrams\sequences\sources\admin\sq-0010-upload-images.puml"
    "diagrams\sequences\sources\admin\sq-0011-taxonomy.puml"
    "diagrams\sequences\sources\admin\sq-0012-inventory.puml"
    "diagrams\sequences\sources\admin\sq-0013-analytics.puml"
    "diagrams\sequences\sources\admin\sq-0014-fulfillment.puml"
    "diagrams\sequences\sources\admin\sq-0015-user-management.puml"
) do (
    if exist "%%~f" (
        set /a PUML_COUNT+=1
        set /a SUCCESS_COUNT+=1
    ) else (
        echo [ERROR] MISSING: %%~f
        set /a ERROR_COUNT+=1
    )
)

REM Sequence diagrams - System
for %%f in (
    "diagrams\sequences\sources\system\sq-0016-embeddings.puml"
    "diagrams\sequences\sources\system\sq-0017-reservations.puml"
    "diagrams\sequences\sources\system\sq-0018-vector-index.puml"
    "diagrams\sequences\sources\system\sq-0019-background-jobs.puml"
    "diagrams\sequences\sources\system\sq-0020-payment-integration.puml"
) do (
    if exist "%%~f" (
        set /a PUML_COUNT+=1
        set /a SUCCESS_COUNT+=1
    ) else (
        echo [ERROR] MISSING: %%~f
        set /a ERROR_COUNT+=1
    )
)

REM Use case diagrams - Customer
for %%f in (
    "diagrams\usecases\sources\customer\uc-0001-browse-products.puml"
    "diagrams\usecases\sources\customer\uc-0002-checkout.puml"
    "diagrams\usecases\sources\customer\uc-0003-keyword-search.puml"
    "diagrams\usecases\sources\customer\uc-0004-visual-search.puml"
    "diagrams\usecases\sources\customer\uc-0005-cart.puml"
    "diagrams\usecases\sources\customer\uc-0006-track-order.puml"
    "diagrams\usecases\sources\customer\uc-0007-address-book.puml"
    "diagrams\usecases\sources\customer\uc-0008-recommendations.puml"
) do (
    if exist "%%~f" (
        set /a PUML_COUNT+=1
        set /a SUCCESS_COUNT+=1
    ) else (
        echo [ERROR] MISSING: %%~f
        set /a ERROR_COUNT+=1
    )
)

REM Use case diagrams - Admin
for %%f in (
    "diagrams\usecases\sources\admin\uc-0009-manage-products.puml"
    "diagrams\usecases\sources\admin\uc-0010-upload-images.puml"
    "diagrams\usecases\sources\admin\uc-0011-taxonomy.puml"
    "diagrams\usecases\sources\admin\uc-0012-inventory.puml"
    "diagrams\usecases\sources\admin\uc-0013-analytics.puml"
    "diagrams\usecases\sources\admin\uc-0014-fulfillment.puml"
    "diagrams\usecases\sources\admin\uc-0015-user-management.puml"
) do (
    if exist "%%~f" (
        set /a PUML_COUNT+=1
        set /a SUCCESS_COUNT+=1
    ) else (
        echo [ERROR] MISSING: %%~f
        set /a ERROR_COUNT+=1
    )
)

REM Use case diagrams - System
for %%f in (
    "diagrams\usecases\sources\system\uc-0016-embeddings.puml"
    "diagrams\usecases\sources\system\uc-0017-reservations.puml"
    "diagrams\usecases\sources\system\uc-0018-vector-index.puml"
    "diagrams\usecases\sources\system\uc-0019-background-jobs.puml"
    "diagrams\usecases\sources\system\uc-0020-payment-integration.puml"
) do (
    if exist "%%~f" (
        set /a PUML_COUNT+=1
        set /a SUCCESS_COUNT+=1
    ) else (
        echo [ERROR] MISSING: %%~f
        set /a ERROR_COUNT+=1
    )
)

echo Found %PUML_COUNT% of 44 PlantUML files

echo.
echo [4/6] Checking Core Thesis Files...
echo.

if exist "main.typ" (
    echo [OK] main.typ
    set /a SUCCESS_COUNT+=1
) else (
    echo [ERROR] MISSING: main.typ
    set /a ERROR_COUNT+=1
)

if exist "info.typ" (
    echo [OK] info.typ
    set /a SUCCESS_COUNT+=1
) else (
    echo [ERROR] MISSING: info.typ
    set /a ERROR_COUNT+=1
)

echo.
echo [5/6] Checking Directory Structure...
echo.

for %%d in (
    "chapters"
    "frontmatter"
    "backmatter"
    "images"
    "diagrams"
    "template"
) do (
    if exist "%%~d" (
        echo [OK] %%~d\
        set /a SUCCESS_COUNT+=1
    ) else (
        echo [ERROR] MISSING: %%~d\
        set /a ERROR_COUNT+=1
    )
)

echo.
echo [6/6] Checking Generation Scripts...
echo.

for %%s in (
    "diagrams\01-ml-models\generate-ml-models.bat"
    "diagrams\02-system-architecture\generate-system-arch.bat"
    "diagrams\03-data-architecture\generate-data-arch.bat"
    "diagrams\sequences\generate-sequences.bat"
    "diagrams\usecases\generate-usecases.bat"
) do (
    if exist "%%~s" (
        echo [OK] %%~s
        set /a SUCCESS_COUNT+=1
    ) else (
        echo [ERROR] MISSING: %%~s
        set /a ERROR_COUNT+=1
    )
)

echo.
echo ========================================
echo VERIFICATION SUMMARY
echo ========================================
echo Total Checks Passed: %SUCCESS_COUNT%
echo Total Errors Found:  %ERROR_COUNT%

if %ERROR_COUNT% GTR 0 (
    echo.
    echo [FAILED] Verification failed with %ERROR_COUNT% error^(s^)
    echo Please fix the errors above before continuing.
    exit /b 1
) else (
    echo.
    echo [SUCCESS] All verifications passed!
    exit /b 0
)
