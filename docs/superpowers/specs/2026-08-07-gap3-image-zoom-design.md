# Gap 3: Image Zoom / Lightbox

## Summary

Replace raw `<img>` in `ProductGallery.vue` with PrimeVue `Image` component. Built-in zoom, lightbox, keyboard navigation, swipe on mobile. Zero custom code.

## Current State

- `ProductGallery.vue` (42 lines): raw `<img>` tags with click-to-select thumbnails
- No zoom, no lightbox, no swipe, no keyboard navigation
- PrimeVue 5 `Image` component available via auto-import

## Design

### ProductGallery Changes

**File:** `app/Store/src/features/catalog/components/ProductGallery.vue`

**Before:**
```html
<img :src="images[activeIndex]?.url" :alt="alt" class="..." />
```

**After:**
```html
<Image
  :src="images[activeIndex]?.url"
  :alt="alt"
  :preview="true"
  class="..."
  imageClass="..."
/>
```

### PrimeVue Image Features

- `preview: true` — enables lightbox on click
- Built-in zoom on hover (magnifier effect)
- Keyboard navigation (arrow keys, escape)
- Swipe support on mobile
- Fullscreen overlay with dark background

### Thumbnail Strip

Keep existing thumbnail strip unchanged. Only the main image gets PrimeVue `Image` treatment.

### Styling

- Ensure PrimeVue Image container matches current `aspect-square bg-stone-100 rounded-xl overflow-hidden`
- Lightbox overlay uses PrimeVue default styling (dark background, centered image)
- No custom CSS needed — PrimeVue handles all interactions

## Files to Modify

| File | Action |
|------|--------|
| `features/catalog/components/ProductGallery.vue` | MODIFY — replace img with Image |

## Acceptance Criteria

- [ ] Clicking main image opens lightbox
- [ ] Lightbox shows full-size image
- [ ] Escape key closes lightbox
- [ ] Arrow keys navigate between images in lightbox
- [ ] Swipe works on mobile
- [ ] Thumbnail selection still works
- [ ] No visual regressions on image display
