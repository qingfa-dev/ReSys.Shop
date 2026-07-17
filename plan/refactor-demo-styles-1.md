---
goal: Remove optional demo styles from main.scss to fix build failures caused by flags.css
version: 1.0
date_created: 2026-07-17
owner: feat/admin-app
status: 'Completed'
tags: refactor, css, build, asset
---

# Introduction

![Status: Completed](https://img.shields.io/badge/status-Completed-brightgreen)

Remove the `@use 'demo/demo'` import from both `main.scss` entry points (`scss/` and `sekai/`) and delete the corresponding `demo/` directories. The demo styles are marked "Optional" and contain a `flags/flags.css` file with a single ~10KB line that causes postcss build failures.

## 1. Requirements & Constraints

- **REQ-001**: Remove `@use 'demo/demo'` from both `scss/main.scss` and `sekai/main.scss`
- **REQ-002**: Delete the `demo/` directories under both `scss/` and `sekai/`
- **REQ-003**: Update section 5 comment to reflect removal
- **CON-001**: The `code.scss` (pre.app-code styling) is only used by demo pages; not needed for core app
- **CON-002**: The `flags/flags.css` is a single-line ~10KB sprite sheet that breaks postcss-import parsing

## 2. Implementation Steps

### Implementation Phase 1 — Remove demo import from scss/main.scss

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-001 | Remove `@use 'demo/demo';` line 24 and update section 5 comment in `src/assets/scss/main.scss` | ✅ | 2026-07-17 |
| TASK-002 | Remove `@use 'demo/demo';` line 32 and update section 5 comment in `src/assets/sekai/main.scss` | ✅ | 2026-07-17 |
| TASK-003 | Delete `src/assets/scss/demo/` directory | ✅ | 2026-07-17 |
| TASK-004 | Delete `src/assets/sekai/demo/` directory | ✅ | 2026-07-17 |
| TASK-005 | Verify `npx vite build` passes (or matches expected pre-existing errors only) | ✅ | 2026-07-17 |

## 3. Alternatives

- **ALT-001**: Keep `code.scss` as a direct import without flags.css — unnecessary; code block styles are demo-only and not referenced by any production component.
- **ALT-002**: Fix `flags.css` by splitting into multiple lines — the file is a CDN sprite sheet maintained externally; maintaining a local fork is high friction for zero value.

## 4. Dependencies

- **DEP-001**: None — self-contained styling change with no runtime impact.

## 5. Files

| File | Action |
|------|--------|
| `src/assets/scss/main.scss` | Edit — remove demo import |
| `src/assets/sekai/main.scss` | Edit — remove demo import |
| `src/assets/scss/demo/` | Delete — directory |
| `src/assets/sekai/demo/` | Delete — directory |

## 6. Testing

- **TEST-001**: `npx vite build` in `app/Admin/` must no longer report the `flags.css` CSS error (only pre-existing Vue template errors should remain)

## 7. Risks & Assumptions

- **RISK-001**: None — demo styles are explicitly labeled "Optional" and are not referenced by any production route or component.
- **ASSUMPTION-001**: No production view or component depends on `.flag` or `.app-code` CSS classes.

## 8. Related Specifications / Further Reading

- N/A
