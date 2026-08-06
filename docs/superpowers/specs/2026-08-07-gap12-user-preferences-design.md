# Gap 12: User Preferences

## Summary

New `/account/preferences` page for user preferences. Client-side only (localStorage). Default currency changed from VND to USD.

## Current State

- `useTheme.ts`: handles dark/light/system theme with localStorage
- No currency/language preference system
- `formatVnd()` hardcoded to VND formatting
- No preferences view in new Store

## Design

### New View: `PreferencesView.vue`

**Location:** `app/Store/src/features/profile/views/PreferencesView.vue`

**Route:** `/account/preferences` (requires auth)

**Sections:**

#### Appearance
- Theme: Light / Dark / System (reuses `useTheme.ts`)

#### Regional
- Currency: VND / USD / EUR
- Language: English / Vietnamese

### UI Layout

```
┌─────────────────────────────┐
│ Preferences                  │
├─────────────────────────────┤
│ Appearance                   │
│ Theme: [Light ▾]            │
│   Light / Dark / System     │
├─────────────────────────────┤
│ Regional                     │
│ Currency: [USD ▾]           │
│   VND / USD / EUR           │
│                             │
│ Language: [English ▾]       │
│   English / Vietnamese      │
├─────────────────────────────┤
│        [Save Preferences]   │
└─────────────────────────────┘
```

### localStorage Keys

```ts
'resys-preferences' = {
  theme: 'light' | 'dark' | 'system',
  currency: 'VND' | 'USD' | 'EUR',
  language: 'en' | 'vi'
}
```

### New Composable: `usePreferences.ts`

**Location:** `app/Store/src/shared/composables/usePreferences.ts`

Follows same pattern as `useTheme.ts` — raw localStorage with try/catch, reactive ref.

```ts
import { ref, watch } from 'vue'

const STORAGE_KEY = 'resys-preferences'

interface UserPreferences {
  currency: 'VND' | 'USD' | 'EUR'
  language: 'en' | 'vi'
}

const defaults: UserPreferences = { currency: 'USD', language: 'en' }

function load(): UserPreferences {
  try {
    const raw = localStorage.getItem(STORAGE_KEY)
    return raw ? { ...defaults, ...JSON.parse(raw) } : defaults
  } catch { return defaults }
}

const preferences = ref<UserPreferences>(load())

watch(preferences, (val) => {
  try { localStorage.setItem(STORAGE_KEY, JSON.stringify(val)) } catch { /* ignore */ }
}, { deep: true })

export function usePreferences() {
  const formatCurrency = (amount: number) => {
    switch (preferences.value.currency) {
      case 'USD': return new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' }).format(amount)
      case 'EUR': return new Intl.NumberFormat('de-DE', { style: 'currency', currency: 'EUR' }).format(amount)
      case 'VND': return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(amount)
    }
  }

  return { preferences, formatCurrency }
}
  }

  return { preferences, formatCurrency }
}
```

### Currency Default Change

**File:** `app/Store/src/shared/utils/currency.ts`

- Rename `formatVnd` to `formatCurrency`
- Accept optional currency parameter, default to `usePreferences().preferences.value.currency`
- Update `app/Store/src/app/composables/useCurrency.ts` to export `formatCurrency`
- Update all call sites (grep for `formatVnd`)

### Router Addition

**File:** `app/Store/src/features/profile/routes/index.ts`

```ts
{
  path: 'preferences',
  name: 'preferences',
  component: () => import('../views/PreferencesView.vue'),
  meta: { requiresAuth: true },
}
```

## Files to Create/Modify

| File | Action |
|------|--------|
| `features/profile/views/PreferencesView.vue` | CREATE |
| `shared/composables/usePreferences.ts` | CREATE |
| `features/profile/routes/index.ts` | MODIFY — add route |
| `shared/utils/format.ts` | MODIFY — rename formatVnd to formatCurrency |
| All files using formatVnd | MODIFY — update imports |
| `app/composables/useCurrency.ts` | MODIFY — export formatCurrency |
| All files using formatVnd | MODIFY — update imports |
| `app/layouts/AccountLayout.vue` | MODIFY — add sidebar link |

## Acceptance Criteria

- [ ] Preferences page renders with theme/currency/language selectors
- [ ] Changes persist to localStorage
- [ ] Theme changes apply immediately (reuses useTheme)
- [ ] Currency format changes affect price display
- [ ] Default currency is USD (not VND)
- [ ] Preferences persist across page reloads
- [ ] Works without authentication (localStorage only)
