# Implementation Plan: Gap 12 — User Preferences

**Spec:** `docs/superpowers/specs/2026-08-07-gap12-user-preferences-design.md`
**Estimated effort:** Small (1-2 hours)
**Dependencies:** None

## Tasks

### T1: Create usePreferences composable
- [ ] Create `app/Store/src/shared/composables/usePreferences.ts`
- [ ] Follow useTheme.ts pattern (raw localStorage with try/catch)
- [ ] State: `{ currency: 'USD', language: 'en' }`
- [ ] `formatCurrency(amount)` function using Intl.NumberFormat
- [ ] Default currency: USD

### T2: Rename formatVnd to formatCurrency
- [ ] Edit `app/Store/src/shared/utils/currency.ts`
- [ ] Rename `formatVnd` to `formatCurrency`
- [ ] Accept optional currency parameter
- [ ] Use `usePreferences().preferences.value.currency` as default
- [ ] Edit `app/Store/src/app/composables/useCurrency.ts`
- [ ] Export `formatCurrency` instead of `formatVnd`
- [ ] Grep for all `formatVnd` call sites and update imports

### T3: Create PreferencesView.vue
- [ ] Create `app/Store/src/features/profile/views/PreferencesView.vue`
- [ ] Sections: Appearance (theme), Regional (currency, language)
- [ ] Theme: reuses useTheme.ts
- [ ] Currency: VND / USD / EUR dropdown
- [ ] Language: English / Vietnamese dropdown
- [ ] Save button persists to localStorage

### T4: Add route
- [ ] Edit `app/Store/src/features/profile/routes/index.ts`
- [ ] Add route: `{ path: 'preferences', name: 'preferences', component: PreferencesView, meta: { requiresAuth: true } }`

### T5: Add sidebar link
- [ ] Edit `app/Store/src/app/layouts/AccountLayout.vue`
- [ ] Add to navItems: `{ label: 'Preferences', to: '/account/preferences', icon: 'pi pi-cog' }`

### T6: Verify
- [ ] Preferences page renders
- [ ] Theme changes apply immediately
- [ ] Currency format changes affect price display
- [ ] Default currency is USD
- [ ] Preferences persist across reloads

## Verification

```bash
cd app/Store && pnpm run lint && pnpm run test:unit
```
