# Gap 11: Change Password

## Summary

New `/account/change-password` view for changing password. Endpoint `POST /api/store/identity/passwords/change` exists.

## Current State

- Backend endpoint exists and works
- No frontend view for password change
- `RegisterView.vue` has password strength meter (reusable)

## Design

### New View: `ChangePasswordView.vue`

**Location:** `app/Store/src/features/profile/views/ChangePasswordView.vue`

**Route:** `/account/change-password` (requires auth)

**Form fields:**
- Current Password (InputPassword, required)
- New Password (InputPassword, required, min 8 chars)
- Confirm New Password (InputPassword, must match new password)

**Password strength meter:** Reuse pattern from `RegisterView.vue` (lines 31-42, 98-107). Heuristic: length + uppercase + lowercase + numbers + special chars.

**Zod validation:**
```ts
z.object({
  currentPassword: z.string().min(1, 'Current password is required'),
  newPassword: z.string().min(8, 'Password must be at least 8 characters'),
  confirmPassword: z.string().min(1),
}).refine(data => data.newPassword === data.confirmPassword, {
  message: 'Passwords do not match',
  path: ['confirmPassword'],
})
```

**API call:** `POST /api/store/identity/passwords/change` with `{ currentPassword, newPassword }`

**Success:** Toast "Password changed successfully" → redirect to `/account/profile`

**Error:** Toast with error message from API

### UI Layout

```
┌─────────────────────────────┐
│ Change Password             │
├─────────────────────────────┤
│ Current Password            │
│ [________________] 👁       │
│                             │
│ New Password                │
│ [________________] 👁       │
│ ████████░░ Weak             │
│                             │
│ Confirm New Password        │
│ [________________] 👁       │
│                             │
│        [Change Password]    │
└─────────────────────────────┘
```

### Router Addition

**File:** `app/Store/src/features/profile/routes/index.ts`

Add route:
```ts
{
  path: 'change-password',
  name: 'change-password',
  component: () => import('../views/ChangePasswordView.vue'),
  meta: { requiresAuth: true },
}
```

### Sidebar Navigation

**File:** `app/Store/src/app/layouts/AccountLayout.vue`

Add to `navItems` array:
```ts
{ label: 'Change Password', to: '/account/change-password', icon: 'pi pi-key' },
```

## Files to Create/Modify

| File | Action |
|------|--------|
| `features/profile/views/ChangePasswordView.vue` | CREATE |
| `features/profile/routes/index.ts` | MODIFY — add route |
| `features/profile/services/authApi.ts` | MODIFY — add `changePassword` function |
| Account sidebar navigation | MODIFY — add link |

## Acceptance Criteria

- [ ] Form renders with current, new, confirm password fields
- [ ] Password strength meter shows on new password input
- [ ] Validation prevents submit if passwords don't match
- [ ] API call succeeds and shows success toast
- [ ] Redirects to profile page after success
- [ ] Error messages displayed from API
- [ ] Form resets after successful submission
