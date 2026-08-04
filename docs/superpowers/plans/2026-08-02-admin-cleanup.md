# Admin Cleanup — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Remove 3 unused feature directories (`reports/`, `users/`, `error/pages/`) per the spec.

**Architecture:** File-system cleanup — no code changes. These directories have empty files or no routes and are not used by any module.

**Tech Stack:** Bash (`rm`)

---

### Task 1: Remove unused directories

**Files:**
- Delete: `app/Admin/src/features/reports/`
- Delete: `app/Admin/src/features/users/`
- Delete: `app/Admin/src/features/error/pages/`
- Note: Keep `app/Admin/src/features/error/` if it has non-page files

- [ ] **Step 1: Remove the directories**

```bash
rm -rf app/Admin/src/features/reports
rm -rf app/Admin/src/features/users
rm -rf app/Admin/src/features/error/pages
```

- [ ] **Step 2: Verify nothing breaks**

```bash
cd app/Admin && pnpm run type-check && pnpm run lint
```

Expected: 0 errors (these directories were not imported by any file).

- [ ] **Step 3: Commit**

```bash
git add app/Admin/src/features/reports app/Admin/src/features/users app/Admin/src/features/error/pages
git commit -m "chore(admin): remove unused feature directories (reports, users, error/pages)"
```
