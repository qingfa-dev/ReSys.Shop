# Task B5: Fix Report -- Notification Store read-only

## Issue
`useNotificationStore.ts` used `readonly()` on line 34 but did not import it from Vue.

## Fix
Added `readonly` to the Vue import:
```diff
- import { ref, computed } from 'vue'
+ import { ref, computed, readonly } from 'vue'
```

## Verification
- `pnpm run type-check` (`vue-tsc --build`) -- no errors in `useNotificationStore.ts`
- Other pre-existing errors in unrelated files remain unchanged

## File Changed
- `app/Admin/src/stores/useNotificationStore.ts`
