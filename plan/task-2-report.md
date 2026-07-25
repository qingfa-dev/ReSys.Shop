# Task 2 - FulfillmentWorkflow Fix Report

## Issue B2: Cancel path uses `confirmAction` instead of `confirmDelete`

**Finding date:** 2026-07-25

### Problem
`FulfillmentWorkflow.vue` used `confirmAction` with a Promise-wrapping `confirmPrompt` helper for the cancel confirmation path. The plan specified `confirmDelete` callback pattern (consistent with all other destructive action confirmations across the Admin SPA). The `confirmAction` dialog shows generic "Please confirm" / "Are you sure you want to proceed with..." messaging, whereas `confirmDelete` shows proper "Delete confirmation" wording.

### Fix applied
1. **FulfillmentWorkflow.vue** (line 18) — Changed `const { confirmAction }` to `const { confirmDelete }`.
2. **FulfillmentWorkflow.vue** (lines 42-51) — Removed `confirmPrompt` Promise wrapper function. Replaced with inline `confirmDelete({ target, onAccept })` callback pattern matching `OrderListTable.vue`, `OrderLineItemManager.vue`, and all other list-table components.
3. **FulfillmentWorkflow.vue** (lines 53-69) — Extracted transition logic into `executeTransition` helper so both confirmed (cancel) and direct (approve/complete) paths share the same API call + error handling.

### Files changed
- `app/Admin/src/features/ordering/components/FulfillmentWorkflow.vue`
