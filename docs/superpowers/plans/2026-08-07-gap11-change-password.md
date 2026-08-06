# Implementation Plan: Gap 11 — Change Password

**Spec:** `docs/superpowers/specs/2026-08-07-gap11-change-password-design.md`
**Estimated effort:** Small (1-2 hours)
**Dependencies:** None

## Tasks

### T1: Add changePassword API function
- [ ] Edit `app/Store/src/features/identity/services/authApi.ts`
- [ ] Add `changePassword(currentPassword: string, newPassword: string)` function
- [ ] POST to `api/store/identity/passwords/change`

### T2: Create ChangePasswordView.vue
- [ ] Create `app/Store/src/features/profile/views/ChangePasswordView.vue`
- [ ] Form: current password, new password, confirm password
- [ ] Zod validation (current required, new min 8, confirm matches)
- [ ] Password strength meter (reuse pattern from RegisterView)
- [ ] On success: toast "Password changed" → redirect to /account/profile
- [ ] On error: toast with error message

### T3: Add route
- [ ] Edit `app/Store/src/features/profile/routes/index.ts`
- [ ] Add route: `{ path: 'change-password', name: 'change-password', component: ChangePasswordView, meta: { requiresAuth: true } }`

### T4: Add sidebar link
- [ ] Edit `app/Store/src/app/layouts/AccountLayout.vue`
- [ ] Add to navItems: `{ label: 'Change Password', to: '/account/change-password', icon: 'pi pi-key' }`

### T5: Verify
- [ ] Form renders with all fields
- [ ] Password strength meter works
- [ ] Validation prevents invalid submit
- [ ] API call succeeds
- [ ] Redirects after success

## Verification

```bash
cd app/Store && pnpm run lint && pnpm run test:unit
```
