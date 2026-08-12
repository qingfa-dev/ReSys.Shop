# Visual Search UI — Search Parameters & Image Actions

**Date:** 2026-08-12
**Status:** Approved
**Scope:** Store frontend only (no backend API changes)

## Goal

Add model selection, search tuning controls, image preview, and action buttons to the Visual Search page. The composable (`useVisualSearch`) already has the state and methods — this design wires them into the UI.

## Design

### Layout: Two-Card Structure

```
+--------------------------------------------------+
| Breadcrumb: [Home] > [Visual Search]             |
|                                                   |
| # Visual Search                                   |
| Find visually similar products by uploading       |
|                                                   |
| +----------------------------------------------+ |
| | CARD: Upload Panel                            | |
| |                                               | |
| | [Image Preview]  [Choose File]  [Clear btn]   | |
| | JPEG, PNG, WebP — max 10 MB                   | |
| |                                               | |
| +----------------------------------------------+ |
|                                                   |
| +----------------------------------------------+ |
| | CARD: Search Parameters                       | |
| |                                               | |
| | Model:        [Select dropdown ─────]         | |
| | Results:      [====o==========] [20]          | |
| | Min Match:    [=======o========] [40%]        | |
| | Score Weight: [====o==========] [1.0]         | |
| |                                               | |
| | [Search]  [Reset]                             | |
| |                                               | |
| +----------------------------------------------+ |
|                                                   |
| [Error Message]  (if validation/search error)     |
|                                                   |
| [Loading State]  (spinner while embedding)        |
|                                                   |
| RESULTS (N)                                       |
| +------+------+------+------+                   | |
| | Card | Card | Card | Card |                    | |
| +------+------+------+------+                   | |
+--------------------------------------------------+
```

### Component Inventory

| Component | Import | Purpose |
|-----------|--------|---------|
| Card | PrimeVue (auto) | Wrapper for upload and parameters |
| FileUpload | PrimeVue (auto) | Image picker with preview |
| Image | PrimeVue (auto) | Preview of selected image |
| Button | PrimeVue (auto) | Clear, Search, Reset actions |
| Select | PrimeVue (auto) | Model dropdown |
| Slider | PrimeVue (auto) | Continuous range for topK, threshold, weight |
| InputNumber | PrimeVue (auto) | Numeric display for slider values |
| Label | PrimeVue (explicit) | Form field labels (FloatLabel) |
| Message | PrimeVue (auto) | Error display |
| ProgressSpinner | PrimeVue (auto) | Loading state |
| ProductGridCard | Custom (import) | Result cards with similarity tags |

### Search Parameters

| Parameter | Control | Range | Default | Description |
|-----------|---------|-------|---------|-------------|
| Model | Select | API-provided list | `fashion-clip` | ML model for embedding |
| topK | Slider + InputNumber | 1–50 | 20 | Max results to return |
| Min Similarity | Slider + InputNumber | 0–100% | 0% | Filter results below threshold |
| Score Weight | Slider + InputNumber | 0.1–3.0 | 1.0 | Multiplier for similarity scores |

### Score Weight Behavior

- Applied client-side after API response
- Formula: `adjustedScore = originalScore * weight`
- Affects both display (Tag badge) and threshold filtering
- Range 0.1–3.0 covers under-weighting to aggressive amplification
- Reset returns weight to 1.0

### Image Actions

| Action | Trigger | Behavior |
|--------|---------|----------|
| Preview | File selected | Show thumbnail via `previewUrl` (blob URL) |
| Clear | "Clear" button or new file | Call `vs.reset()`, clear preview and results |
| Re-search | "Search" button | Re-run search with current params (no re-upload) |

### State Machine (unchanged)

1. `empty` — initial, no file selected
2. `upload` — file selected, preview shown
3. `loading` — search in progress
4. `results` — results displayed

### Data Flow

```
VisualSearchView
  → vs.loadModels()          [on mount]
  → FileUpload @select
    → vs.selectFile(file)    [validate + create preview URL]
    → state = 'upload'
  → Search button click
    → vs.search(topK, model) [API call]
    → state = 'loading' → 'results'
  → Results post-process
    → Filter by minSimilarity threshold
    → Apply scoreWeight multiplier to display
  → Reset button
    → vs.reset()             [clear everything]
    → state = 'empty'
```

### API Changes

None. The backend `SearchByImage.Request` already accepts `topK` and `model`. Score weight is frontend-only.

### Files to Modify

| File | Change |
|------|--------|
| `VisualSearchView.vue` | Add parameters card, image preview, actions |
| `useVisualSearch.ts` | Add `topK`, `minSimilarity`, `scoreWeight` refs; expose `filteredResults` computed |

### Files to Create

None.

## Verification

1. `pnpm run build-only` — no build errors
2. `pnpm run lint` — no lint errors
3. Manual: upload image → adjust params → search → verify filtering and score display
4. Manual: reset → verify state clears completely
