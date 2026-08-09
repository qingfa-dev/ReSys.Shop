---
goal: Rebuild the authenticated account experience with a consistent card-based layout.
version: 1.0
date_created: 2026-08-08
last_updated: 2026-08-08
owner: ReSys.Shop Frontend
status: 'Planned'
tags: ['frontend', 'storefront', 'account', 'profile', 'primevue', 'tailwind']
---

# Introduction
This plan implements the account experience spec for the storefront. It focuses on the account layout, profile and settings pages, and the order history area.

## 1. Requirements & Constraints
- REQ-001: Refactor the account shell into a more coherent navigation-driven layout.
- REQ-002: Rework profile, settings, and address-related pages into more structured card-based sections.
- REQ-003: Improve order history and detail presentation to match the rest of the storefront.
- CON-001: Preserve authentication and route guards.
- CON-002: Reuse the shared shell and design primitives established earlier.

## 2. Implementation Steps
1. Review the account layouts and current profile/ordering views.
2. Refactor the account shell and navigation structure.
3. Rebuild profile and settings pages around shared card and form primitives.
4. Refactor order list and detail views to align with the new visual system.
5. Verify the account experience across mobile and desktop.

## 3. Verification
- Run the storefront lint and unit test commands.
- Review the account pages in the browser.
