# Store SPA — Agent Guide

Vue 3 Store SPA (PrimeVue, Aura theme, pnpm, Vite 8). Lives in the ReSys.Shop
monorepo. This file provisions the Code Commenting Standard v3.0 for view files
under `app/Store/src/features/*/views/` (`.vue` and `.ts`).

## Code Commenting Standard (effective rules)

The repository's Code Commenting Standard v3.0 governs all comments in this app:

- `guide/code-commenting/README.md` — human-readable overview
- `guide/code-commenting/SKILL.md` — applied workflow (decision tree, label table)
- `guide/code-commenting/CommentingRules.xml` — authoritative machine-readable spec

Every comment in a view must follow the rules below; where they conflict, the
XML standard wins.

## Inline label format (script logic)

Format every `<script setup>` comment as a single-line `//` label on its own
line, indented with the block it describes:

```
// Label: Capitalised sentence.
```

- **F2** — Begin the body with a capitalised word; treat it as a sentence.
- **F8** — One label, one action. Never join two actions with "and".
- **F10** — Use imperative-mood verbs: "Filter expired sessions", not
  "Expired sessions are filtered".
- **F3** — Keep every line under 100 characters.
- Choose labels via the CAT-1..CAT-10 decision tree in the standard; do not
  invent labels. Do not use multi-line `/* ... */` blocks in `<script setup>`
  (warnings-as-errors lint).

## Template section format

Add a template section comment above each major block in every `<template>`:

```
<!-- Section: <Title> — <purpose> -->
```

On its own line, indented to match its block, under 100 characters (F3).

### List-view section order (canonical)

1. `Page Header`
2. `Scrollable Content`
3. `Error State`
4. `Data Table`
5. `Search & Filters` (table `#header`)
6. `Table Columns`
7. `Row Actions`
8. `Empty State`

### Detail-view section order (canonical)

1. `Page Header`
2. `Content Card`
3. `Tabs`
4. `Form Fields`
5. `Action Footer`

## "Full but necessary" quality gate

Commentation must be full but necessary (GUD-005): comment every major
`<template>` section and every non-obvious `<script setup>` block, but comment
only where the WHY is non-obvious. Never restate what the code already says
(AP-1 redundancy); never comment trivial lines (AP-3 over-commenting). Comments
must match actual current behaviour (AP-5 stale comments) and never contain
secrets, credentials, or PII.
