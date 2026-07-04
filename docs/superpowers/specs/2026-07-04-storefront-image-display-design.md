# Storefront Image Display Endpoint

**Date:** 2026-07-04
**Status:** Approved

## Problem

Images cannot be displayed in the storefront. Three root causes:

1. **`VariantImage.Url` is empty** — the local storage provider returns `null` for `Uri`, so `thumbnailUrl` in product list/detail responses is always `""`.
2. **No static file middleware** — files stored in `./uploads` are never exposed via URL.
3. **The only image-serving endpoint forces download** — `GET /api/storefront/images/{id}/download` uses `Results.File(stream, contentType, fileName)` which sets `Content-Disposition: attachment`. Browsers treat this as a file download, not an inline image display.

## Design

Replace the download endpoint with a display endpoint that uses `TypedResults.PhysicalFile()` — the most efficient way to serve local files in ASP.NET Core. No `fileDownloadName` parameter means browsers display the image inline.

### Final Endpoint

| Method | Route | Purpose |
|--------|-------|---------|
| GET | `api/storefront/images/{id:guid}` | Display VariantImage inline (replaces download) |

### Handler Design

1. Look up `VariantImage` by ID from `IApplicationDbContext`
2. Return 404 if not found via `VariantImageResult.Failure.ById(id)`
3. Construct the physical file path: `Path.GetFullPath(Path.Combine(localPath, image.StoragePath))`
4. Verify the file exists with `File.Exists()`, return 404 if not
5. Return `TypedResults.PhysicalFile(fullPath, image.ContentType)` — serves inline, no forced download

### Dependencies

- `IApplicationDbContext` — DB lookup
- `IOptions<LocalStorageProviderSetting>` — provides `LocalPath` (e.g., `./uploads`)
- No `IStorageService` needed — `PhysicalFile` reads the file directly via the OS

### Directory Structure

```
Images/Get/Image/
├── GetImage.cs              # Query + handler
└── GetImage.Endpoint.cs     # Carter ICarterModule
```

Delete: `Images/Get/Download/` (entire directory)

### Route Constants

In `CatalogFeature.Storefront.cs`:
- Replace `Images.Get.Download` with `Images.Get.Image`
- Route: `$"{Storefront.Route}/images/{{id:guid}}"`

### Frontend Integration

No response model changes needed. Product list/detail already returns `VariantImage.Id`. The frontend constructs the display URL as:

```
<img src="/api/storefront/images/{imageId}" alt="{alt}" />
```

### Error Handling

- `VariantImage` not found → 404
- Physical file not found on disk → 404
- Unexpected I/O errors → bubble up to global exception handler

## Architecture Notes

- Follows existing vertical slice pattern (partial class + Carter endpoint + MediatR handler)
- Uses `TypedResults.PhysicalFile` for zero-copy file serving from the local filesystem
- Coupled to local storage by design — the user chose this path explicitly
- If S3/Azure are needed later, a separate serving strategy would be implemented (redirect to presigned URL or a provider-specific stream endpoint)
