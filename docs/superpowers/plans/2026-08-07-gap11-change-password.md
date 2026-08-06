# Gap 11: Change Password Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** New `/account/change-password` view for changing password. Backend endpoint `POST /api/store/identity/passwords/change` exists.

**Architecture:** New Vue view with Zod validation, password strength meter (reuse pattern from RegisterView), API call to existing endpoint. Route added to profile routes. Sidebar link added to AccountLayout.

**Tech Stack:** Vue 3, Zod, PrimeVue InputPassword, existing `authApi.ts`

## Global Constraints

- Warnings-as-errors: any TypeScript/lint warning fails build
- Backend endpoint: `POST /api/store/identity/passwords/change` (requires auth)
- Request body: `{ currentPassword: string, newPassword: string }`
- Password strength meter pattern: reuse from `RegisterView.vue`

---

## File Structure

| File | Action | Purpose |
|------|--------|---------|
| `app/Store/src/features/profile/views/ChangePasswordView.vue` | CREATE | Change password form |
| `app/Store/src/features/identity/services/authApi.ts` | MODIFY | Add `changePassword` function |
| `app/Store/src/features/profile/routes/index.ts` | MODIFY | Add route |
| `app/Store/src/app/layouts/AccountLayout.vue` | MODIFY | Add sidebar link |

---

## Tasks

### Task 1: Add changePassword API function

**Files:**
- Modify: `app/Store/src/features/identity/services/authApi.ts`

**Interfaces:**
- Consumes: None
- Produces: `changePassword(currentPassword: string, newPassword: string): Promise<Result<void>>`

- [ ] **Step 1: Read authApi.ts**

Read `app/Store/src/features/identity/services/authApi.ts` to understand existing patterns (function signatures, imports, API client usage).

- [ ] **Step 2: Add changePassword function**

Append to `app/Store/src/features/identity/services/authApi.ts`:

```typescript
export async function changePassword(
  currentPassword: string,
  newPassword: string,
): Promise<Result<void>> {
  return post('api/store/identity/passwords/change', { currentPassword, newPassword })
}
```

- [ ] **Step 3: Run lint**

```bash
cd app/Store && pnpm run lint
```

Expected: PASS

### Task 2: Create ChangePasswordView.vue

**Files:**
- Create: `app/Store/src/features/profile/views/ChangePasswordView.vue`

**Interfaces:**
- Consumes: `changePassword` from `authApi.ts`
- Produces: No exports — page component only

- [ ] **Step 1: Create the view**

Create `app/Store/src/features/profile/views/ChangePasswordView.vue`:

```vue
<script setup lang="ts">
import { ref, computed } from 'vue'
import { useRouter } from 'vue-router'
import { z } from 'zod'
import { changePassword } from '@/features/identity/services/authApi'
import { useNotify } from '@/shared/composables/useNotify'

const router = useRouter()
const notify = useNotify()

const form = ref({
  currentPassword: '',
  newPassword: '',
  confirmPassword: '',
})
const loading = ref(false)
const errors = ref<Record<string, string>>({})

const schema = z.object({
  currentPassword: z.string().min(1, 'Current password is required'),
  newPassword: z.string().min(8, 'Password must be at least 8 characters'),
  confirmPassword: z.string().min(1, 'Please confirm your password'),
}).refine(data => data.newPassword === data.confirmPassword, {
  message: 'Passwords do not match',
  path: ['confirmPassword'],
})

// Derive: Password strength heuristic (0-4)
const strength = computed(() => {
  const p = form.value.newPassword
  let s = 0
  if (p.length >= 8) s++
  if (/[A-Z]/.test(p)) s++
  if (/[0-9]/.test(p)) s++
  if (/[^A-Za-z0-9]/.test(p)) s++
  return s
})

const strengthLabel = computed(() => ['Very weak', 'Weak', 'Fair', 'Strong', 'Very strong'][strength.value])
const strengthColor = computed(() => ['bg-red-500', 'bg-orange-500', 'bg-yellow-500', 'bg-green-500', 'bg-emerald-500'][strength.value])

async function submit(): Promise<void> {
  errors.value = {}
  const result = schema.safeParse(form.value)
  if (!result.success) {
    for (const issue of result.error.issues) {
      errors.value[issue.path[0] as string] = issue.message
    }
    return
  }
  loading.value = true
  try {
    const res = await changePassword(form.value.currentPassword, form.value.newPassword)
    if (res.isSuccess) {
      notify.success('Password changed', 'Your password has been updated')
      router.push('/account/profile')
    } else {
      notify.error('Change failed', res.message ?? 'Current password may be incorrect')
    }
  } finally {
    loading.value = false
  }
}
</script>
<template>
  <!-- Section: Change Password Page -->
  <div class="max-w-md">
    <h1 class="text-2xl font-bold text-stone-900 mb-6">Change Password</h1>
    <form class="space-y-4" @submit.prevent="submit">
      <!-- Section: Current Password -->
      <div>
        <label class="block text-sm font-medium text-stone-700 mb-1">Current Password</label>
        <InputText v-model="form.currentPassword" type="password" class="w-full" :invalid="!!errors.currentPassword" />
        <small v-if="errors.currentPassword" class="text-red-500">{{ errors.currentPassword }}</small>
      </div>
      <!-- Section: New Password -->
      <div>
        <label class="block text-sm font-medium text-stone-700 mb-1">New Password</label>
        <InputText v-model="form.newPassword" type="password" class="w-full" :invalid="!!errors.newPassword" />
        <div v-if="form.newPassword" class="mt-1 flex items-center gap-2">
          <div class="flex-1 h-1.5 bg-stone-200 rounded-full overflow-hidden">
            <div class="h-full rounded-full transition-all" :class="strengthColor" :style="{ width: `${(strength + 1) * 25}%` }" />
          </div>
          <span class="text-xs text-stone-500">{{ strengthLabel }}</span>
        </div>
        <small v-if="errors.newPassword" class="text-red-500">{{ errors.newPassword }}</small>
      </div>
      <!-- Section: Confirm Password -->
      <div>
        <label class="block text-sm font-medium text-stone-700 mb-1">Confirm New Password</label>
        <InputText v-model="form.confirmPassword" type="password" class="w-full" :invalid="!!errors.confirmPassword" />
        <small v-if="errors.confirmPassword" class="text-red-500">{{ errors.confirmPassword }}</small>
      </div>
      <!-- Section: Submit -->
      <Button type="submit" label="Change Password" class="w-full" :loading="loading" />
    </form>
  </div>
</template>
```

- [ ] **Step 2: Run lint**

```bash
cd app/Store && pnpm run lint
```

Expected: PASS

### Task 3: Add route

**Files:**
- Modify: `app/Store/src/features/profile/routes/index.ts`

**Interfaces:**
- Consumes: None
- Produces: New route `/account/change-password`

- [ ] **Step 1: Read current routes**

Read `app/Store/src/features/profile/routes/index.ts` to see existing route structure.

- [ ] **Step 2: Add change-password route**

Add to the routes array:

```typescript
{
  path: 'change-password',
  name: 'change-password',
  component: () => import('../views/ChangePasswordView.vue'),
},
```

- [ ] **Step 3: Run lint**

```bash
cd app/Store && pnpm run lint
```

Expected: PASS

### Task 4: Add sidebar link

**Files:**
- Modify: `app/Store/src/app/layouts/AccountLayout.vue:8-15`

**Interfaces:**
- Consumes: None
- Produces: New nav item in sidebar

- [ ] **Step 1: Add nav item**

Edit `app/Store/src/app/layouts/AccountLayout.vue`. In the `navItems` array (lines 8-15), add after the Notifications item:

```typescript
{ label: 'Change Password', to: '/account/change-password', icon: 'pi pi-key' },
```

- [ ] **Step 2: Run lint**

```bash
cd app/Store && pnpm run lint
```

Expected: PASS

- [ ] **Step 3: Run unit tests**

```bash
cd app/Store && pnpm run test:unit
```

Expected: PASS

- [ ] **Step 4: Commit**

```bash
cd app/Store && git add src/features/profile/views/ChangePasswordView.vue src/features/identity/services/authApi.ts src/features/profile/routes/index.ts src/app/layouts/AccountLayout.vue
git commit -m "feat(profile): add change password view"
```
