---
goal: Downgrade PrimeVue to v4, Rename scss → sekai, Add Code Comments
version: 2.0
date_created: 2026-07-01
status: 'Planned'
tags: refactor, primevue, downgrade, sekai, documentation
---

# Introduction

![Status: Planned](https://img.shields.io/badge/status-Planned-blue)

## 1. Requirements & Constraints

- **REQ-001**: Downgrade PrimeVue from v5 RC (`5.0.0-rc.1`) to v4 stable (`^4.5.5`) — v5 requires a commercial license.
- **REQ-002**: Downgrade `@primeuix/themes@3.0.0-rc.1` to `@primevue/themes@^4.5.4` — v4 uses a different npm package for theme presets.
- **REQ-003**: Rename `src/assets/scss/` to `src/assets/sekai/` — naming after the Sakai template origin, avoids ambiguity with Sass file extension.
- **REQ-004**: Add block-comment headers to every setup file explaining its purpose, origin (Sakai Vue), and how it integrates with PrimeVue v4 + Tailwind v4.
- **REQ-005**: All `--p-*` variable references in the SCSS must remain compatible with PrimeVue v4's design-token output (same token names as v5).
- **REQ-006**: Dark mode selector must remain `.p-dark` — supported by both PrimeVue v4 and v5.
- **CON-001**: License compliance — PrimeVue v4 `^4.5.5` is MIT-licensed; no commercial license needed.
- **CON-002**: The `@primevue/auto-import-resolver@^4.5.5` is already v4-compatible — no change needed.
- **CON-003**: `@primevue/forms@^4.5.5` is already v4 — no change needed.
- **CON-004**: The Aura preset in `@primevue/themes@^4.5.4` provides the same `--p-*` design tokens as the v5 `@primeuix/themes` — minimal visual regression.
- **GUD-001**: Every `.ts`, `.vue`, `.scss`, `.html` file touched must have a JSDoc/SCDoc comment block at the top explaining purpose, origin, and dependencies.
- **GUD-002**: Folder rename must preserve all relative `@use` and `import` paths — update every reference.
- **PAT-001**: Comment format — use `// --- [filename] ---` header blocks in SCSS; use `/* --- [filename] --- */` JSDoc blocks in TypeScript/Vue.

## 2. Implementation Steps

### Implementation Phase 1: Downgrade PrimeVue v5 RC → v4 Stable

- GOAL-001: Replace PrimeVue v5 RC packages with PrimeVue v4 stable equivalents and update all imports.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-001 | Update `package.json`: change `"primevue": "5.0.0-rc.1"` → `"primevue": "^4.5.5"` | | |
| TASK-002 | Update `package.json`: change `"@primeuix/themes": "3.0.0-rc.1"` → `"@primevue/themes": "^4.5.4"` | | |
| TASK-003 | Update `src/main.ts`: change import `from '@primeuix/themes/aura'` → `from '@primevue/themes/aura'` | | |
| TASK-004 | Run `pnpm install` to remove stale v5 packages and install v4 equivalents | | |
| TASK-005 | Run `npx vite build` and confirm zero compilation errors | | |
| TASK-006 | Run `npx vue-tsc --build` and confirm zero type-check errors | | |

### Implementation Phase 2: Rename `scss` → `sekai` Folder

- GOAL-002: Rename `src/assets/scss/` to `src/assets/sekai/` and update every import path across the project.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-007 | `mv src/assets/scss src/assets/sekai` — rename the directory | | |
| TASK-008 | Update `src/main.ts`: change `'./assets/scss/main.scss'` → `'./assets/sekai/main.scss'` | | |
| TASK-009 | Audit all SCSS `@use` paths inside `sekai/` — they use relative paths like `'abstracts/variables/common'` which are relative to the file location, not the folder name. These should NOT need changes since the internal file structure is preserved. Verify none use an absolute `scss/` segment. | | |
| TASK-010 | Run `npx vite build` — confirm all SCSS imports resolve correctly with the new folder name | | |

### Implementation Phase 3: Add Code Comments to All Setup Files

- GOAL-003: Add explanatory header comments to every touched file documenting purpose, origin, and PrimeVue/Tailwind integration.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-011 | Add header comment to `src/main.ts` — document PrimeVue v4 + Aura preset + dark mode + Tailwind + SCSS imports | | |
| TASK-012 | Add header comment to `src/vite.config.ts` — document plugin order (Tailwind → Vue → auto-import resolver) | | |
| TASK-013 | Add header comment to `src/assets/sekai/main.scss` — document architecture layers (abstracts → vendors → base → layout) | | |
| TASK-014 | Add header comment to `src/assets/sekai/abstracts/variables/_common.scss` — document that variables map PrimeVue v4 `--p-*` design tokens | | |
| TASK-015 | Add header comment to `src/assets/sekai/base/_core.scss` — document global resets and 14px font-size intent | | |
| TASK-016 | Add header comment to `src/assets/sekai/base/_typography.scss` — document heading scale and that it supplements Tailwind prose | | |
| TASK-017 | Add header comment to `src/assets/sekai/layout/_topbar.scss` — document fixed topbar, responsive dropdown at lg breakpoint | | |
| TASK-018 | Add header comment to `src/assets/sekai/layout/_menu.scss` — document sidebar nav, active-route, submenu transitions | | |
| TASK-019 | Add header comment to `src/assets/sekai/layout/_footer.scss` — document centered footer with top border | | |
| TASK-020 | Add header comment to `src/assets/sekai/layout/_main.scss` — document flex-based main content area | | |
| TASK-021 | Add header comment to `src/assets/sekai/layout/_responsive.scss` — document breakpoint alignment with Tailwind (lg: 1024px, 2xl: 1536px) and sidebar overlay/static/mobile modes | | |
| TASK-022 | Add header comment to `src/assets/sekai/layout/_utils.scss` — document `.card` utility class | | |
| TASK-023 | Add header comment to `src/index.html` — document Tailwind `antialiased` on body | | |

### Implementation Phase 4: Final Verification

- GOAL-004: Verify everything compiles and runs correctly after all changes.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-024 | Run `npx vite build` — confirm zero errors | | |
| TASK-025 | Run `npx vue-tsc --build` — confirm zero type errors | | |
| TASK-026 | Verify `package.json` no longer references any `@primeuix/*` or `primevue@5` packages | | |
| TASK-027 | Verify `src/assets/sekai/` exists and `src/assets/scss/` no longer exists | | |

## 3. Alternatives

- **ALT-001**: Stay on PrimeVue v5 RC and buy a commercial license. Rejected — v5 RC is not production-ready and the commercial license cost is not justified for this project.
- **ALT-002**: Keep the folder named `scss` for clarity. Rejected — `sekai` (derived from Sakai) better describes the template origin and avoids confusion with the `.scss` file extension.
- **ALT-003**: Skip code comments to keep files minimal. Rejected — comments are essential for AI-agent maintainability and onboarding new developers.

## 4. Dependencies

- **DEP-001**: `primevue@^4.5.5` — MIT-licensed stable release, replaces `primevue@5.0.0-rc.1`.
- **DEP-002**: `@primevue/themes@^4.5.4` — provides Aura preset for PrimeVue v4, replaces `@primeuix/themes@3.0.0-rc.1`.
- **DEP-003**: `@primevue/auto-import-resolver@^4.5.5` — already v4-compatible, no change.
- **DEP-004**: `@primevue/forms@^4.5.5` — already v4-compatible, no change.

## 5. Files

- **FILE-001**: `package.json` — update `primevue` and `@primeuix/themes` versions.
- **FILE-002**: `src/main.ts` — update theme import path + add header comment.
- **FILE-003**: `src/vite.config.ts` — add header comment.
- **FILE-004**: `src/index.html` — add header comment.
- **FILE-005**: `src/assets/scss/` → `src/assets/sekai/` — entire directory rename.
- **FILE-006**: `src/assets/sekai/main.scss` — rename path + add header comment.
- **FILE-007**: `src/assets/sekai/abstracts/variables/_common.scss` — add header comment.
- **FILE-008**: `src/assets/sekai/base/_core.scss` — add header comment.
- **FILE-009**: `src/assets/sekai/base/_typography.scss` — add header comment.
- **FILE-010**: `src/assets/sekai/layout/_topbar.scss` — add header comment.
- **FILE-011**: `src/assets/sekai/layout/_menu.scss` — add header comment.
- **FILE-012**: `src/assets/sekai/layout/_footer.scss` — add header comment.
- **FILE-013**: `src/assets/sekai/layout/_main.scss` — add header comment.
- **FILE-014**: `src/assets/sekai/layout/_responsive.scss` — add header comment.
- **FILE-015**: `src/assets/sekai/layout/_utils.scss` — add header comment.

## 6. Testing

- **TEST-001**: `npx vite build` exits with code 0.
- **TEST-002**: `npx vue-tsc --build` exits with code 0.
- **TEST-003**: `grep -r 'primevue@5\|@primeuix/themes' package.json` returns empty.
- **TEST-004**: `ls src/assets/sekai/` succeeds, `ls src/assets/scss/` fails.
- **TEST-005**: Visual check that PrimeVue components (Button, InputText, etc.) render with Aura theme styles.
- **TEST-006**: Visual check that dark mode `.p-dark` toggle works.

## 7. Risks & Assumptions

- **RISK-001**: PrimeVue v4's `@primevue/themes/aura` may emit slightly different `--p-*` token values than v5's `@primeuix/themes/aura`, causing visual drift. Mitigation: compare rendered output side-by-side; adjust variable overrides in `_common.scss` if needed.
- **RISK-002**: The `@primevue/auto-import-resolver@^4.5.5` may not resolve all PrimeVue v4 component names identically to v5. Mitigation: test with a `<Button>` and `<InputText>` in any view.
- **ASSUMPTION-001**: Neither `@primevue/forms` nor `@primevue/auto-import-resolver` need version bumps beyond `^4.5.5`.
- **ASSUMPTION-002**: No third-party code imports `@primeuix/themes` or `primevue/dist/*` paths.

## 8. Related Specifications / Further Reading

- `plan/refactor-scss-architecture-1.md` — Previous plan (v1) that was already implemented for the SCSS cleanup.
- https://primevue.org/v4/theming/styled/ — PrimeVue v4 theming documentation (Aura preset).
- https://github.com/primefaces/primevue/releases/tag/4.5.5 — PrimeVue v4.5.5 release notes.
- `app/Admin/src/main.ts` — PrimeVue v4 + Aura theme configuration.
- `app/Admin/vite.config.ts` — Tailwind v4 + PrimeVue auto-import resolver config.
