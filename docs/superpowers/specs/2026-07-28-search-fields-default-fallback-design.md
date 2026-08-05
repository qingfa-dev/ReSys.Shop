# Search Fields Default Fallback — Design

**Date:** 2026-07-28
**Status:** Approved
**Approach:** A (one-line fix in `ResolveFields`)

## Problem

When a feature handler passes `allowedSearchFields` to `ParseAll` and the client omits `?searchFields=`, the search becomes a no-op (returns all records). The `AllowedFields` whitelist is stored for validation but never consulted as a fallback field set.

## Change

File: `service/Api/src/Shared/Operational/Persistence/Specifications/Searching/Search.Model.cs`

**`ResolveFields` resolution order changes** from:

```
Fields > passed defaultFields > []
```

To:

```
Fields > passed defaultFields > AllowedFields > []
```

When both `model.Fields` (user-specified) and the caller-supplied `defaultFields` are empty, fall back to `model.AllowedFields` (the whitelist set). If even `AllowedFields` is null, return `[]` (preserving current no-op behavior for unconfigured features).

### Before

```csharp
public IReadOnlyList<string> ResolveFields(IReadOnlyList<string> defaultFields)
    => Fields.Count > 0 ? Fields : defaultFields;
```

### After

```csharp
public IReadOnlyList<string> ResolveFields(IReadOnlyList<string> defaultFields)
{
    if (Fields.Count > 0) return Fields;
    if (defaultFields.Count > 0) return defaultFields;
    return AllowedFields?.ToList().AsReadOnly() ?? [];
}
```

## Impact

- **No callers change.** All existing feature handlers keep their current `ParseAll(allowedSearchFields: X).ApplyQuerying(model)` pattern unchanged.
- **No new model properties.** `AllowedFields` already exists and is already set from the `allowedSearchFields` parameter.
- **Existing tests pass.** `ResolveFields_ShouldHandleEmptyAndNonEmpty` already tests the explicit-`defaultFields`-param case and is unaffected.
- **New behavior:** When a feature passes e.g. `StateConstant.Query.AllowedSearchFields.ToHashSet(...)` to `ParseAll`, and the client sends just `?search=hello` (no `searchFields`), the search will now scan across all whitelisted fields instead of returning everything.

## What is NOT changed

- The `string[]` → join → `Split` round trip in `ParseAll`/`FromQueryString`/`Parser` is left as-is (separate concern).
- `AllowedFields` retains its primary meaning as a validation whitelist — the fallback is a secondary use that happens to match the same set in practice for all current features.

## Tests to add

- `ResolveFields_EmptyFieldsEmptyDefaults_WithAllowedFields_ShouldFallBackToAllowedFields`
