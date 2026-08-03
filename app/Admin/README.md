# Admin

This template should help get you started developing with Vue 3 in Vite.

## Recommended IDE Setup

[VS Code](https://code.visualstudio.com/) + [Vue (Official)](https://marketplace.visualstudio.com/items?itemName=Vue.volar) (and disable Vetur).

## Recommended Browser Setup

- Chromium-based browsers (Chrome, Edge, Brave, etc.):
  - [Vue.js devtools](https://chromewebstore.google.com/detail/vuejs-devtools/nhdogjmejiglipccpnnnanhbledajbpd)
  - [Turn on Custom Object Formatter in Chrome DevTools](http://bit.ly/object-formatters)
- Firefox:
  - [Vue.js devtools](https://addons.mozilla.org/en-US/firefox/addon/vue-js-devtools/)
  - [Turn on Custom Object Formatter in Firefox DevTools](https://fxdx.dev/firefox-devtools-custom-object-formatters/)

## Type Support for `.vue` Imports in TS

TypeScript cannot handle type information for `.vue` imports by default, so we replace the `tsc` CLI with `vue-tsc` for type checking. In editors, we need [Volar](https://marketplace.visualstudio.com/items?itemName=Vue.volar) to make the TypeScript language service aware of `.vue` types.

## Customize configuration

See [Vite Configuration Reference](https://vite.dev/config/).

## Template Section Commenting Standard

Every view template must place a section comment above each major block, using
the format below. The comment sits on its own line, indented to match its
block, and stays under 100 characters (F3). Comment only where the WHY matters
(GUD-005) — never restate what the markup already shows.

```html
<!-- Section: <Title> — <purpose> -->
```

These markers live inside `<template>`, not in `<script setup>` (which uses
single-line `// Label:` inline labels instead). Follow the canonical section
order for each view type so every view reads consistently:

### List-view section order (PAT-002)

1. `Page Header`
2. `Scrollable Content`
3. `Error State`
4. `Data Table`
5. `Search & Filters` (table `#header`)
6. `Table Columns`
7. `Row Actions`
8. `Empty State`

### Detail-view section order (PAT-003)

1. `Page Header`
2. `Content Card`
3. `Tabs`
4. `Form Fields`
5. `Action Footer`

## View Code-Commenting Rules

`<script setup>` logic uses single-line `//` labels. Before writing a label,
traverse the Label Decision Tree (CAT-1..CAT-10) in the standard (GUD-001) and
pick the correct category — do not invent labels. Format each script comment as
a capitalised imperative sentence on its own line (GUD-002):

```ts
// Label: Capitalised imperative sentence.
```

Per view operation, use the mapping below (under 100 characters, F3):

- validate / guard → `Validate:`
- computed / derived value → `Compute:` (or `Transform:` when converting shape)
- API call → `Call:` (or `Load:` for fetch-on-mount data)
- confirm / flush / status change → `Trigger:` / `Handle:`
- data mapping → `Map:`

The template section markers (above) stay inside `<template>`; these script
labels live in `<script setup>` only. Never use multi-line `/* ... */` blocks in
script — single-line `//` labels only, to avoid warnings-as-errors lint
failures.

## Project Setup

```sh
pnpm install
```

### Compile and Hot-Reload for Development

```sh
pnpm dev
```

### Type-Check, Compile and Minify for Production

```sh
pnpm build
```

### Run Unit Tests with [Vitest](https://vitest.dev/)

```sh
pnpm test:unit
```

### Lint with [ESLint](https://eslint.org/)

```sh
pnpm lint
```
