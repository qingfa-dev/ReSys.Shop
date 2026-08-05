# Admin Topbar User Menu & Sidebar Logout — ReSys.Shop Admin SPA

## Problem

The admin topbar has three placeholder buttons (Calendar, Messages, Profile) that do nothing. There is no way to log out or see who is currently authenticated.

## Solution

Add a user menu in the topbar with avatar + name + dropdown, and a logout item at the bottom of the sidebar. Both consume the existing `useAuthStore`.

---

## 1. Topbar User Menu

### Component: `shared/components/navigation/UserMenu.vue`

A self-contained component for the topbar right side.

**Visual**:
- PrimeVue `Avatar` circle with user initials (from `authStore.currentUser?.userId`, or "?" as fallback)
- User identifier text next to the avatar
- Click toggles a PrimeVue `Popover` dropdown

**Dropdown content**:
1. User ID / name line (read-only, grey text)
2. "Profile" link — navigates to `/profile` (placeholder route, will 404 until profile page is built)
3. Divider
4. "Logout" button — red text, `pi pi-sign-out` icon, calls `authStore.logout()`

**Logout behavior**:
- Disable button while logout is in-flight (reactive `isLoggingOut` ref)
- On success: toast "Logged out" (severity: info), `router.replace('/auth/login')`
- On error: local state already cleared by store (fire-and-forget), no error shown

**Auth guard**: rendered only when `authStore.isAuthenticated` is `true`. Since `AppLayout` route has `meta.requiresAuth`, this is always true in practice, but the v-if handles edge cases.

### AppTopbar changes

- Remove the three placeholder buttons (Calendar, Messages, Profile)
- Remove the `pi-ellipsis-v` mobile menu button for the old placeholder menu
- Replace with `<UserMenu />` in `layout-topbar-actions`
- Keep dark mode toggle and configurator palette button unchanged

---

## 2. Sidebar Logout Item

### Implementation in `AppMenu.vue`

Add a separator + clickable logout item to the template (not via the menu model, to avoid modifying `AppMenuItem`):

```html
<li class="menu-separator"></li>
<li class="layout-root-menuitem">
  <div class="layout-menuitem-root-text">Account</div>
</li>
<li>
  <a class="flex align-items-center px-3 py-2 cursor-pointer logout-item"
     :class="{ 'opacity-50 pointer-events-none': isLoggingOut }"
     @click="handleLogout">
    <i class="pi pi-sign-out layout-menuitem-icon"></i>
    <span>Logout</span>
  </a>
</li>
```

`handleLogout` does the same flow as the topbar logout.

### Styling

Add to `_topbar.scss` (or a new menu helper style):
```scss
.logout-item {
  color: var(--red-500);
  &:hover { background-color: var(--surface-hover); }
}
```

---

## 3. Edge Cases

| Case | Handling |
|---|---|
| Double-click logout | `isLoggingOut` ref disables both topbar and sidebar logout UI |
| Auth store not initialized | `UserMenu` reads reactive `isAuthenticated` — renders nothing when idle/loading |
| Backend unreachable | Logout API call caught silently; local state always cleared |
| Already on `/auth/login` after logout | `router.replace` clears browser history |

---

## 4. Testing

### `UserMenu.spec.ts` (new, ~6 tests)
- Renders avatar and name when authenticated
- Does not render when unauthenticated
- Popover opens on avatar click
- Logout button calls `authStore.logout()` and shows toast
- Logout button disabled while `isLoggingOut` is true
- Profile link navigates to `/profile`

### `AppMenu.spec.ts` or `AppSidebar.spec.ts` (extend, ~2 tests)
- Logout item is rendered below a separator
- Clicking logout item calls `authStore.logout()` and shows toast

### Existing unchanged tests
- `authStore.spec.ts` — logout method already tested, no changes
- `LoginPage.spec.ts`, other auth tests — unchanged

---

## Files Changed

| File | Action |
|---|---|
| `shared/components/navigation/UserMenu.vue` | **New** |
| `shared/components/navigation/AppTopbar.vue` | Replace placeholder buttons with `<UserMenu />` |
| `shared/components/navigation/AppMenu.vue` | Add separator + logout item template + script |
| `shared/components/navigation/__tests__/UserMenu.spec.ts` | **New** — avatar, popover, logout flow |
| `shared/components/navigation/__tests__/AppMenu.spec.ts` | **New/Extend** — logout item rendered |
| `assets/layout/_topbar.scss` | Add `.logout-item` danger color |
| `features/auth/routes/index.ts` | Ensure `/auth/login` route has `name: 'login'` for redirect |

## Files Deleted

None

## Verification

- `pnpm run build` — zero TypeScript errors
- `pnpm run test:unit -- run` — all tests pass
- `pnpm run lint` — no warnings
