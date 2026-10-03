@echo off
REM =================================================================
REM Image Verification Script
REM Verifies all generated diagram images exist and checks quality
REM =================================================================

echo ========================================
echo GENERATED IMAGE VERIFICATION
echo ========================================
echo.

set TOTAL_EXPECTED=62
set IMAGES_FOUND=0
set IMAGES_MISSING=0

echo [1/5] Checking ML Models Images ^(4 expected^)...
echo.

for %%f in (
    "ml-01-efficientnet-b0"
    "ml-02-dinov2"
    "ml-03-fashion-clip"
    "ml-04-clip-vit-b16"
    "ml-05-inference-pipeline"
    "ml-06-model-lifecycle"
) do (
    if exist "images\diagrams\01-ml-models\%%~f.png" (
        echo [OK] %%~f.png
        set /a IMAGES_FOUND+=1
    ) else (
        echo [MISSING] %%~f.png
        set /a IMAGES_MISSING+=1
    )
)

echo.
echo [2/5] Checking System Architecture Images ^(4 expected^)...
echo.

for %%f in (
    "sys-01-overview"
    "sys-03-api-structure"
    "sys-04-domain-model"
    "sys-05-request-pipeline"
) do (
    if exist "images\diagrams\02-system-architecture\%%~f.png" (
        echo [OK] %%~f.png
        set /a IMAGES_FOUND+=1
    ) else (
        echo [MISSING] %%~f.png
        set /a IMAGES_MISSING+=1
    )
)


echo.
echo [3/5] Checking Data Architecture Images ^(3 expected^)...
echo.

for %%f in (
    "data-01-backend-erd"
    "data-01a-catalog-erd"
    "data-01b-ordering-erd"
    "data-01c-inventory-erd"
    "data-01d-identity-erd"
    "data-02-pgvector-hnsw"
    "data-03-ml-service-structure"
) do (
    if exist "images\diagrams\03-data-architecture\%%~f.png" (
        echo [OK] %%~f.png
        set /a IMAGES_FOUND+=1
    ) else (
        echo [MISSING] %%~f.png
        set /a IMAGES_MISSING+=1
    )
)

echo.
echo [4/5] Checking Sequence Diagram Images ^(20 expected^)...
echo.

REM Customer sequences
for %%f in (
    "sq-0001-browse-products"
    "sq-0002-checkout"
    "sq-0003-keyword-search"
    "sq-0004-visual-search"
    "sq-0005-cart"
    "sq-0006-track-order"
    "sq-0007-address-book"
    "sq-0008-recommendations"
) do (
    if exist "images\diagrams\sequences\customer\%%~f.png" (
        set /a IMAGES_FOUND+=1
    ) else (
        echo [MISSING] customer\%%~f.png
        set /a IMAGES_MISSING+=1
    )
)

REM Admin sequences
for %%f in (
    "sq-0009-manage-products"
    "sq-0010-upload-images"
    "sq-0011-taxonomy"
    "sq-0012-inventory"
    "sq-0013-analytics"
    "sq-0014-fulfillment"
    "sq-0015-user-management"
) do (
    if exist "images\diagrams\sequences\admin\%%~f.png" (
        set /a IMAGES_FOUND+=1
    ) else (
        echo [MISSING] admin\%%~f.png
        set /a IMAGES_MISSING+=1
    )
)

REM System sequences
for %%f in (
    "sq-0016-embeddings"
    "sq-0017-reservations"
    "sq-0018-vector-index"
    "sq-0019-background-jobs"
    "sq-0020-payment-integration"
) do (
    if exist "images\diagrams\sequences\system\%%~f.png" (
        set /a IMAGES_FOUND+=1
    ) else (
        echo [MISSING] system\%%~f.png
        set /a IMAGES_MISSING+=1
    )
)

echo Found %IMAGES_FOUND% sequence diagrams

echo.
echo [5/5] Checking Use Case Diagram Images ^(24 expected^)...
echo.

REM Customer usecases
for %%f in (
    "uc-0001-browse-products"
    "uc-0002-checkout"
    "uc-0003-keyword-search"
    "uc-0004-visual-search"
    "uc-0005-cart"
    "uc-0006-track-order"
    "uc-0007-address-book"
    "uc-0008-recommendations"
) do (
    if exist "images\diagrams\usecases\customer\%%~f.png" (
        set /a IMAGES_FOUND+=1
    ) else (
        echo [MISSING] customer\%%~f.png
        set /a IMAGES_MISSING+=1
    )
)

REM Admin usecases
for %%f in (
    "uc-0009-manage-products"
    "uc-0010-upload-images"
    "uc-0011-taxonomy"
    "uc-0012-inventory"
    "uc-0013-analytics"
    "uc-0014-fulfillment"
    "uc-0015-user-management"
) do (
    if exist "images\diagrams\usecases\admin\%%~f.png" (
        set /a IMAGES_FOUND+=1
    ) else (
        echo [MISSING] admin\%%~f.png
        set /a IMAGES_MISSING+=1
    )
)

REM System usecases
for %%f in (
    "uc-0016-embeddings"
    "uc-0017-reservations"
    "uc-0018-vector-index"
    "uc-0019-background-jobs"
    "uc-0020-payment-integration"
) do (
    if exist "images\diagrams\usecases\system\%%~f.png" (
        set /a IMAGES_FOUND+=1
    ) else (
        echo [MISSING] system\%%~f.png
        set /a IMAGES_MISSING+=1
    )
)

echo Found %IMAGES_FOUND% use case diagrams

echo.
echo [6/6] Checking Charts (1 expected)...
echo.

if exist "images\diagrams\charts\latency_histogram.png" (
    echo [OK] latency_histogram.png
    set /a IMAGES_FOUND+=1
) else (
    echo [MISSING] latency_histogram.png
    set /a IMAGES_MISSING+=1
)

echo.
echo ========================================
echo IMAGE VERIFICATION SUMMARY
echo ========================================
echo Images Found:   %IMAGES_FOUND% of %TOTAL_EXPECTED%
echo Images Missing: %IMAGES_MISSING%

if %IMAGES_MISSING% GTR 0 (
    echo.
    echo [WARNING] %IMAGES_MISSING% image^(s^) missing
    echo Run generate-all-diagrams.bat to regenerate missing images
    exit /b 1
) else (
    echo.
    echo [SUCCESS] All expected images found!
    exit /b 0
)
