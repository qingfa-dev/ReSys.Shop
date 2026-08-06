# Gap 12: User Preferences Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** New `/account/preferences` page for currency/language/theme. Client-side only (localStorage). Default currency changed from VND to USD.

**Architecture:** New `usePreferences` composable following `useTheme.ts` pattern (raw localStorage). Rename `formatVnd` to `formatCurrency` with preference-based formatting. New view with theme/currency/language selectors.

**Tech Stack:** Vue 3, PrimeVue Select, localStorage, Intl.NumberFormat

## Global Constraints

- Warnings-as-errors: any TypeScript/lint warning fails build
- Follow `useTheme.ts` pattern for localStorage (raw get/set with try/catch, no VueUse)
- Default currency: USD (not VND)
- All existing `formatVnd` call sites must be updated

---

## File Structure

| File | Action | Purpose |
|------|--------|---------|
| `app/Store/src/shared/composables/usePreferences.ts` | CREATE | Preferences composable |
| `app/Store/src/shared/utils/currency.ts` | MODIFY | Rename formatVnd → formatCurrency |
| `app/Store/src/app/composables/useCurrency.ts` | MODIFY | Export formatCurrency |
| `app/Store/src/features/profile/views/PreferencesView.vue` | CREATE | Preferences page |
| `app/Store/src/features/profile/routes/index.ts` | MODIFY | Add route |
| `app/Store/src/app/layouts/AccountLayout.vue` | MODIFY | Add sidebar link |

---

## Tasks

### Task 1: Create usePreferences composable

**Files:**
- Create: `app/Store/src/shared/composables/usePreferences.ts`

**Interfaces:**
- Consumes: None
- Produces: `usePreferences()` → `{ preferences, formatCurrency }`

- [ ] **Step 1: Read useTheme.ts for pattern**

Read `app/Store/src/shared/composables/useTheme.ts` to match the localStorage pattern.

- [ ] **Step 2: Create composable**

Create `app/Store/src/shared/composables/usePreferences.ts`:

```typescript
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
  function formatCurrency(amount: number): string {
    switch (preferences.value.currency) {
      case 'USD': return new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' }).format(amount)
      case 'EUR': return new Intl.NumberFormat('de-DE', { style: 'currency', currency: 'EUR' }).format(amount)
      case 'VND': return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(amount)
    }
  }

  return { preferences, formatCurrency }
}
```

- [ ] **Step 3: Run lint**

```bash
cd app/Store && pnpm run lint
```

Expected: PASS

### Task 2: Rename formatVnd to formatCurrency

**Files:**
- Modify: `app/Store/src/shared/utils/currency.ts`
- Modify: `app/Store/src/app/composables/useCurrency.ts`

**Interfaces:**
- Consumes: `usePreferences().formatCurrency`
- Produces: `formatCurrency(amount: number)` function

- [ ] **Step 1: Update currency.ts**

Replace `app/Store/src/shared/utils/currency.ts`:

```typescript
import { usePreferences } from '@/shared/composables/usePreferences'

export function formatCurrency(amount: number): string {
  return usePreferences().formatCurrency(amount)
}
```

- [ ] **Step 2: Update useCurrency.ts**

Replace `app/Store/src/app/composables/useCurrency.ts`:

```typescript
import { formatCurrency } from '@/shared/utils/currency'

export function useCurrency(): {
  formatCurrency: (amount: number) => string
} {
  return { formatCurrency }
}
```

- [ ] **Step 3: Update all formatVnd call sites**

Run grep to find all files importing `formatVnd`:

```bash
cd app/Store && grep -r "formatVnd" src/ --include="*.vue" --include="*.ts" -l
```

Update each file to import `formatCurrency` instead of `formatVnd`. Known locations:
- `app/Store/src/features/catalog/components/ProductCard.vue` line 4

- [ ] **Step 4: Run lint**

```bash
cd app/Store && pnpm run lint
```

Expected: PASS

### Task 3: Create PreferencesView.vue

**Files:**
- Create: `app/Store/src/features/profile/views/PreferencesView.vue`

**Interfaces:**
- Consumes: `usePreferences()` from `usePreferences.ts`, `useTheme()` from `useTheme.ts`
- Produces: No exports — page component only

- [ ] **Step 1: Create the view**

Create `app/Store/src/features/profile/views/PreferencesView.vue`:

```vue
<script setup lang="ts">
import { usePreferences } from '@/shared/composables/usePreferences'
import { useTheme } from '@/shared/composables/useTheme'
import { useNotify } from '@/shared/composables/useNotify'

const { preferences } = usePreferences()
const { mode: themeMode, setMode } = useTheme()
const notify = useNotify()

const currencies = [
  { label: 'USD ($)', value: 'USD' },
  { label: 'EUR (€)', value: 'EUR' },
  { label: 'VND (₫)', value: 'VND' },
]

const languages = [
  { label: 'English', value: 'en' },
  { label: 'Vietnamese', value: 'vi' },
]

const themeOptions = [
  { label: 'Light', value: 'light' },
  { label: 'Dark', value: 'dark' },
  { label: 'System', value: 'system' },
]

function save(): void {
  setMode(themeMode.value)
  notify.success('Saved', 'Preferences updated')
}
</script>
<template>
  <!-- Section: Preferences Page -->
  <div class="max-w-md">
    <h1 class="text-2xl font-bold text-stone-900 mb-6">Preferences</h1>
    <div class="space-y-6">
      <!-- Section: Appearance -->
      <section class="space-y-3">
        <h2 class="text-sm font-semibold text-stone-900">Appearance</h2>
        <div>
          <label class="block text-sm text-stone-600 mb-1">Theme</label>
          <Select v-model="themeMode" :options="themeOptions" option-label="label" option-value="value" class="w-full" />
        </div>
      </section>
      <!-- Section: Regional -->
      <section class="space-y-3">
        <h2 class="text-sm font-semibold text-stone-900">Regional</h2>
        <div>
          <label class="block text-sm text-stone-600 mb-1">Currency</label>
          <Select v-model="preferences.currency" :options="currencies" option-label="label" option-value="value" class="w-full" />
        </div>
        <div>
          <label class="block text-sm text-stone-600 mb-1">Language</label>
          <Select v-model="preferences.language" :options="languages" option-label="label" option-value="value" class="w-full" />
        </div>
      </section>
      <!-- Section: Save -->
      <Button label="Save Preferences" class="w-full" @click="save" />
    </div>
  </div>
</template>
```

- [ ] **Step 2: Run lint**

```bash
cd app/Store && pnpm run lint
```

Expected: PASS

### Task 4: Add route and sidebar link

**Files:**
- Modify: `app/Store/src/features/profile/routes/index.ts`
- Modify: `app/Store/src/app/layouts/AccountLayout.vue`

**Interfaces:**
- Consumes: None
- Produces: New route + nav item

- [ ] **Step 1: Add route**

Edit `app/Store/src/features/profile/routes/index.ts`. Add to routes array:

```typescript
{
  path: 'preferences',
  name: 'preferences',
  component: () => import('../views/PreferencesView.vue'),
},
```

- [ ] **Step 2: Add sidebar link**

Edit `app/Store/src/app/layouts/AccountLayout.vue`. Add to `navItems` array:

```typescript
{ label: 'Preferences', to: '/account/preferences', icon: 'pi pi-cog' },
```

- [ ] **Step 3: Run lint**

```bash
cd app/Store && pnpm run lint
```

Expected: PASS

- [ ] **Step 4: Run unit tests**

```bash
cd app/Store && pnpm run test:unit
```

Expected: PASS

- [ ] **Step 5: Commit**

```bash
cd app/Store && git add src/shared/composables/usePreferences.ts src/shared/utils/currency.ts src/app/composables/useCurrency.ts src/features/profile/views/PreferencesView.vue src/features/profile/routes/index.ts src/app/layouts/AccountLayout.vue src/features/catalog/components/ProductCard.vue
git commit -m "feat(profile): add user preferences page with USD default"
```
