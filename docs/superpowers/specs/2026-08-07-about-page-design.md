# About Page Design Spec

## Summary

Add static About page to the Storefront. No backend needed — pure marketing content.

## Route

`/about` — public, no auth required.

## Content

### Page Structure

```
┌─────────────────────────────────────────┐
│ Hero: "About ReSys"                     │
│ Subtitle: Fashion meets technology      │
├─────────────────────────────────────────┤
│ Our Story                               │
│ Paragraph about the company             │
├─────────────────────────────────────────┤
│ Our Mission                             │
│ Paragraph about mission                 │
├─────────────────────────────────────────┤
│ Values                                  │
│ Grid of 3-4 value cards                 │
│ (Quality, Sustainability, Innovation,   │
│  Customer First)                        │
├─────────────────────────────────────────┤
│ Team                                    │
│ Optional: team member cards             │
├─────────────────────────────────────────┤
│ CTA                                     │
│ "Shop Now" button → /shop               │
└─────────────────────────────────────────┘
```

## Files

| File | Action |
|------|--------|
| `features/catalog/views/AboutView.vue` | CREATE |
| `features/catalog/routes/index.ts` | MODIFY — add route |
| `app/layouts/DefaultLayout.vue` | MODIFY — add "About" to footer links |

## Components Used

- `HeroSection` (reuse existing pattern)
- PrimeVue `Card` for value cards
- PrimeVue `Button` for CTA

## Verification

- [ ] `/about` route renders page
- [ ] Content displays correctly
- [ ] "Shop Now" links to `/shop`
- [ ] Footer has "About" link
- [ ] Responsive on mobile
