---
goal: Rebuild the storefront catalog experience with a new premium layout and reusable catalog components.
version: 1.0
date_created: 2026-08-08
last_updated: 2026-08-08
owner: ReSys.Shop Frontend
status: 'Planned'
tags: ['frontend', 'storefront', 'catalog', 'primevue', 'tailwind']
---

# Introduction
This plan implements the catalog experience spec for the storefront. It covers the home page, shop page, product detail page, and the shared catalog components that drive them.

## 1. Requirements & Constraints
- REQ-001: Rewrite the home page into a more polished editorial experience.
- REQ-002: Rebuild the shop page with cleaner filters, sort controls, and better product presentation.
- REQ-003: Rework the product detail page around a premium gallery, pricing section, variant selector, and action area.
- REQ-004: Replace repeated inline UI with shared catalog components.
- CON-001: Preserve product data flows and route behavior.
- CON-002: Keep PrimeVue 5 and Tailwind 4 as the implementation stack.

## 2. Implementation Steps
1. Review the current catalog views and their supporting components.
2. Create shared catalog components for product cards, filter surfaces, and state presentation.
3. Refactor the home view to use the new component model and visual system.
4. Refactor the shop view and mobile filter behavior.
5. Refactor the product detail view and related selectors.
6. Verify responsive layout and content hierarchy across the catalog experience.

## 3. Verification
- Run the storefront lint and unit test commands.
- Review the updated catalog pages in the browser.
