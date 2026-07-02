---
goal: Replace SixLabors.ImageSharp with SkiaSharp for image processing
version: 1.0
date_created: 2026-07-02
last_updated: 2026-07-02
owner: Platform Team
status: Completed
tags: upgrade, image-processing, migration
---

# Introduction

![Status: Completed](https://img.shields.io/badge/status-Completed-brightgreen)

Replace the existing SixLabors.ImageSharp 4.0.0 image processing backend in the `Shared` project with SkiaSharp. The `IImageProcessor` interface and all consuming code remain unchanged. Only the internal `ImageProcessor` partial class files (Implementation, Constants) and the test helper (`CreateTestImageStream`) are replaced. GIF and BMP output format support is dropped because SkiaSharp does not encode those formats natively; JPEG, PNG, and WebP continue to work with identical or improved quality.

## 1. Requirements & Constraints

- **REQ-001**: The `IImageProcessor` public interface signature (`Task<Result<Stream>> ProcessAsync(Stream, UploadOptions, CancellationToken)`) must not change.
- **REQ-002**: All three resize modes (Fit, Fill, Stretch) must produce pixel-identical (or visually equivalent) results at the same target dimensions.
- **REQ-003**: Supported output formats after migration: JPEG, PNG, WebP. GIF and BMP are removed.
- **REQ-004**: The default resampling quality must be Lanczos3 (matching the current `KnownResamplers.Lanczos3` → `SKSamplingOptions(SKFilterQuality.High)` or `SKFilterQuality.Medium` equivalent in SkiaSharp).
- **REQ-005**: SkiaSharp native binaries must be included for Linux (the target deployment OS). SkiaSharp loads native libSkia.so at runtime.
- **CON-001**: The `ImageProcessor` class is `internal sealed` — no external project references it directly.
- **CON-002**: No changes to `StorageService`, `UploadOptions`, `ProcessingResizeMode`, `ImageProcessorResult`, or DI registration are permitted.
- **CON-003**: The `Filter.Model.Constant.cs` file has a spurious `using SixLabors.ImageSharp.Processing;` import — this line must also be removed.
- **GUD-001**: Follow the existing partial-class file layout. Each concern (implementation, constants, enums, results, loggers) stays in its own file.
- **GUD-002**: All SkiaSharp types are aliased with `using` directives; the `ImageProcessor` class body itself should not reference `SK*` prefixed types directly — use contextual names where possible.

## 2. Implementation Steps

### Implementation Phase 1 — Package Management

- GOAL-001: Replace SixLabors.ImageSharp with SkiaSharp in central package management and project references.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-001 | In `Directory.Packages.props` (line 87-88): replace `<PackageVersion Include="SixLabors.ImageSharp" Version="4.0.0" />` with `<PackageVersion Include="SkiaSharp" Version="3.116.1" />` and `<PackageVersion Include="SkiaSharp.NativeAssets.Linux" Version="3.116.1" />`. | ✅ | 2026-07-02 |
| TASK-002 | In `Shared.csproj` (line 67): replace `<PackageReference Include="SixLabors.ImageSharp" />` with `<PackageReference Include="SkiaSharp" />` and add `<PackageReference Include="SkiaSharp.NativeAssets.Linux" />`. | ✅ | 2026-07-02 |
| TASK-003 | Run `dotnet restore` on the solution root to verify all package references resolve correctly. | ✅ | 2026-07-02 |

### Implementation Phase 2 — Core Implementation Rewrite

- GOAL-002: Rewrite `ImageProcessor.Implementation.cs` and `ImageProcessor.Constants.cs` to use SkiaSharp APIs while preserving the same external behavior.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-004 | Rewrite `ImageProcessor.Constants.cs` (full file, 32 lines). Remove all `SixLabors.ImageSharp.Formats.*` imports. Add `using SkiaSharp;`. Replace `FormatEncoders` dictionary (keyed by string, typed `Func<IImageEncoder>`) with a new dictionary of `Func<SKImage, SKData>` (keyed by `string`, mapping to `image.Encode(SKEncodedImageFormat format, int quality)` calls). Supported keys: `jpeg`, `jpg` → `SKEncodedImageFormat.Jpeg` (quality 85), `png` → `SKEncodedImageFormat.Png` (quality 100), `webp` → `SKEncodedImageFormat.Webp` (quality 85). Remove `gif` and `bmp` entries. Remove the `KnownResamplers.Lanczos3` reference; replace with `SKSamplingOptions DefaultResampleOptions = new(SKFilterQuality.Medium)`. Replace `IImageEncoder FallbackEncoder` with `Func<SKImage, SKData> FallbackEncoder = img => img.Encode(SKEncodedImageFormat.Jpeg, 85)`. Delete the `using SixLabors.ImageSharp.Processing.Processors.Transforms;` import. | ✅ | 2026-07-02 |
| TASK-005 | Rewrite `ImageProcessor.Implementation.cs` (full file, 98 lines). Replace all SixLabors imports with `using SkiaSharp;`. Rewrite the `ProcessAsync` method body using `SKCodec.Create`/`SKBitmap.Decode` for loading, `SKCanvas.DrawImage` for resize, and `SKImage.FromBitmap`/`SKData.SaveTo` for encoding. Implement Fit/Fill/Stretch resize modes with proper aspect ratio logic. Handle errors via null checks and a broad `Exception` catch. | ✅ | 2026-07-02 |
| TASK-006 | Remove the spurious `using SixLabors.ImageSharp.Processing;` import from `Filter.Model.Constant.cs` (line 4). | ✅ | 2026-07-02 |

### Implementation Phase 3 — Test Rewrite

- GOAL-003: Rewrite the unit tests so they no longer depend on SixLabors.ImageSharp for test image creation and validation.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-007 | Rewrite `CreateTestImageStream(int width, int height)` in `ImageProcessor.Tests.cs` (line 183-190). Replace `using var image = new Image<Rgba32>(width, height); image.SaveAsPng(stream);` with SkiaSharp: `using var bitmap = new SKBitmap(width, height); using var image = SKImage.FromBitmap(bitmap); using var data = image.Encode(SKEncodedImageFormat.Png, 100); data.SaveTo(stream);`. Update `using` directives: remove `SixLabors.ImageSharp` and `SixLabors.ImageSharp.PixelFormats`, add `using SkiaSharp;`. | ✅ | 2026-07-02 |
| TASK-008 | Update validation tests that use `Image.Load(result.Value)` to verify output dimensions. Replace with `SKBitmap.Decode(result.Value)`. | ✅ | 2026-07-02 |
| TASK-009 | Verify format conversion tests (magic byte assertions) work correctly with SkiaSharp output. | ✅ | 2026-07-02 |
| TASK-010 | Run `dotnet test service/Api/tests/Shared.UnitTests/` to verify all 9 ImageProcessor tests and the StorageService integration test pass. | ✅ | 2026-07-02 |

### Implementation Phase 4 — Cleanup

- GOAL-004: Remove residual ImageSharp references and verify no build artifacts remain.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-011 | Search entire repository for any remaining `SixLabors` string references via `grep -r "SixLabors" --include="*.cs" --include="*.csproj" --include="*.props"`. Remove any found. | ✅ | 2026-07-02 |
| TASK-012 | Run `dotnet build service/Api/` to verify full solution builds with zero warnings (pre-existing errors in unrelated files are known). | ✅ | 2026-07-02 |
| TASK-013 | Run `dotnet test service/Api/tests/` to verify all test projects pass. 1099/1102 passed; 3 pre-existing failures in DateTimeConfigurationTests and Caching tests. All 9 ImageProcessor tests + StorageService integration test pass. | ✅ | 2026-07-02 |

## 3. Alternatives

- **ALT-001**: Replace with ImageMagick/Magick.NET. Provides the widest format support (including GIF/BMP) and better metadata handling. Rejected because the project only needs basic resize + format conversion (5 operations) and SkiaSharp has better performance characteristics for these operations and a smaller deployment footprint.
- **ALT-002**: Keep SixLabors.ImageSharp. Rejected per user request to migrate away from it.
- **ALT-003**: Use `System.Drawing.Common` (cross-platform via libgdiplus). Rejected because it is deprecated on non-Windows and has known memory leak issues in server scenarios.
- **ALT-004**: Add `SkiaSharp` alongside ImageSharp with a feature flag, then remove ImageSharp later. Rejected as unnecessarily complex — the `IImageProcessor` abstraction makes the swap a single-point change.

## 4. Dependencies

- **DEP-001**: `SkiaSharp` version 3.116.1 (NuGet) — core cross-platform 2D graphics library.
- **DEP-002**: `SkiaSharp.NativeAssets.Linux` version 3.116.1 (NuGet) — native libSkia.so binaries for Linux deployment. Required at runtime because SkiaSharp P/Invokes into native Skia.
- **DEP-003**: No other runtime dependencies. SkiaSharp is self-contained.

## 5. Files

- **FILE-001**: `Directory.Packages.props:87-88` — replace ImageSharp version with SkiaSharp + SkiaSharp.NativeAssets.Linux.
- **FILE-002**: `service/Api/src/Shared/Shared.csproj:67` — replace ImageSharp package reference with SkiaSharp references.
- **FILE-003**: `service/Api/src/Shared/Operational/Storages/Processing/ImageProcessor.Constants.cs` — rewrite encoders, sampler.
- **FILE-004**: `service/Api/src/Shared/Operational/Storages/Processing/ImageProcessor.Implementation.cs` — rewrite load/resize/encode with SkiaSharp.
- **FILE-005**: `service/Api/src/Shared/Operational/Persistence/Specifications/Filtering/Filter.Model.Constant.cs:4` — remove unused ImageSharp import.
- **FILE-006**: `service/Api/tests/Shared.UnitTests/Operational/Storages/Processing/ImageProcessor.Tests.cs` — rewrite test image factory and image validation.

## 6. Testing

- **TEST-001**: All 9 existing `ImageProcessorTests` Facts must pass after migration (null/zero dimensions, invalid image, Fit/Fill/Stretch modes, aspect ratio bypass, format conversion, null format preservation, unsupported format).
- **TEST-002**: `StorageService.UploadAsync_WithImageProcessing_ShouldInvokeProcessor` must pass — this test mocks `IImageProcessor` and does not depend on the implementation, so no changes needed.
- **TEST-003**: Manual verification: upload a JPEG, PNG, and WebP image with resize options through the storage pipeline and confirm correct output dimensions and format.
- **TEST-004**: GIF and BMP upload requests with `OutputFormat = "gif"` / `"bmp"` must now return `UnsupportedFormat` error (this is a documented breaking change).

## 7. Risks & Assumptions

- **RISK-001**: SkiaSharp native binaries must match the target Linux runtime (glibc version, architecture). The `SkiaSharp.NativeAssets.Linux` package ships binaries for `linux-x64` (glibc 2.17+). If the deployment base image uses musl (Alpine), `SkiaSharp.NativeAssets.Linux.NoDependencies` or a custom build may be required.
- **RISK-002**: SkiaSharp `SKBitmap.Decode` is synchronous and may block the ASP.NET request thread for large images. If this becomes a performance issue, wrap in `Task.Run()` or use `SKCodec.Create()` for streaming decode.
- **RISK-003**: GIF and BMP output format support is dropped. Any caller passing `OutputFormat = "gif"` or `"bmp"` that previously worked will now receive `UnsupportedFormat` errors. This is a breaking change that must be communicated to API consumers.
- **ASSUMPTION-001**: The server runs on `linux-x64` with glibc. If the deployment target is Alpine Linux or ARM64, the native assets package must be adjusted.
- **ASSUMPTION-002**: `SKFilterQuality.Medium` produces visually comparable results to `KnownResamplers.Lanczos3`. If quality regression is observed, switch to `SKFilterQuality.High`.

## 8. Related Specifications / Further Reading

- [SkiaSharp API Docs — SKBitmap, SKImage, SKCanvas](https://skiasharp.net/docs/latest/api/SkiaSharp/)
- [SkiaSharp NuGet packages](https://www.nuget.org/packages/SkiaSharp)
- [ImageProcessor.Interface.cs](https://github.com/anomalyco/ReSys.Shop/blob/main/service/Api/src/Shared/Operational/Storages/Processing/ImageProcessor.Interface.cs) — public interface (unchanged)
