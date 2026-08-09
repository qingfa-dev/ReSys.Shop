---
goal: Rebuild the storefront shell and shared UI foundations using PrimeVue 5 and Tailwind.
version: 1.0
date_created: 2026-08-08
last_updated: 2026-08-08
owner: ReSys.Shop Frontend
status: 'Planned'
tags: ['frontend', 'storefront', 'primevue', 'tailwind', 'ui']
---

# Introduction
This plan implements the foundation spec for the storefront redesign. It focuses on the shared shell, navigation, layout primitives, and the visual system that will support the rest of the rewrite.

## 1. Requirements & Constraints
- REQ-001: Create a shared design-system foundation for spacing, typography, surface styling, and button/card treatment.
- REQ-002: Refactor the default shell layout and the shared header/footer so they share one consistent structure.
- REQ-003: Introduce reusable primitives for page containers, section headers, cards, loading states, empty states, and action rows.
- REQ-004: Preserve existing route structure, state stores, and API integration.
- CON-001: Keep PrimeVue 5 and Tailwind 4 as the primary UI stack.
- CON-002: Avoid introducing a separate component framework.

## 2. Implementation Steps
1. Review the existing shared layout files and current theme tokens.
2. Create or refactor shared primitives under the app shell area.
3. Update the default, auth, and account layouts to use the new shared structure.
4. Refactor the header and footer to follow the new visual language.
5. Apply the new shell styling and verify responsive behavior.

## 3. Verification
- Run the storefront lint and unit test commands.
- Inspect the updated shell and header/footer in the browser.
