# Implementation Plan: Gap 3 — Image Zoom / Lightbox

**Spec:** `docs/superpowers/specs/2026-08-07-gap3-image-zoom-design.md`
**Estimated effort:** Trivial (30 min)
**Dependencies:** None

## Tasks

### T1: Replace img with PrimeVue Image
- [ ] Edit `app/Store/src/features/catalog/components/ProductGallery.vue`
- [ ] Replace `<img :src="..." />` with `<Image :src="..." :preview="true" />`
- [ ] Ensure PrimeVue Image auto-imports via PrimeVueResolver
- [ ] Keep thumbnail strip unchanged

### T2: Style adjustments
- [ ] Ensure Image container matches current `aspect-square bg-stone-100 rounded-xl overflow-hidden`
- [ ] Test lightbox overlay styling

### T3: Verify
- [ ] Clicking main image opens lightbox
- [ ] Escape key closes lightbox
- [ ] Arrow keys navigate between images
- [ ] Swipe works on mobile
- [ ] Thumbnail selection still works

## Verification

```bash
cd app/Store && pnpm run lint && pnpm run test:unit
```
