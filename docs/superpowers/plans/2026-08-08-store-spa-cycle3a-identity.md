# Store SPA Cycle 3a: Identity — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Replace 5 identity skeleton views with vee-validate + Zod forms — Login, Register, ForgotPassword, ResetPassword, and Sessions.

**Architecture:** All form views use `useForm` from vee-validate with `toFormValidator` from `@vee-validate/zod`. AuthLayout wrapper from Cycle 1. Google social login on Login + Register. SessionsView uses AccountLayout from Cycle 1.

**Tech Stack:** Vue 3.5, PrimeVue 5, Tailwind CSS v4, vee-validate 4.15, `@vee-validate/zod` 4.15, Zod 4.4, Vitest + jsdom

## Global Constraints

- `TreatWarningsAsErrors=true` — no TypeScript warnings
- Neutral color palette only (`neutral-*`), teal primary (`#0d7377`) for CTAs only
- Inter body font, no serif in forms
- All forms: vee-validate `Field` + `ErrorMessage`, disabled submit until valid, spinner on submit
- Toast on server error, redirect on success
- Route meta: `guestOnly: true` for Login/Register/ForgotPassword/ResetPassword, `requiresAuth: true` for Sessions
- Google button: `Button`, outlined, full-width, icon="pi pi-google", label="Continue with Google"

---

### Task 1: LoginView — vee-validate form

**Files:**
- Modify: `app/Store/src/features/identity/views/LoginView.vue`

- [ ] **Step 1: Replace LoginView.vue**

```vue
<script setup lang="ts">
import { useForm } from 'vee-validate'
import { toFormValidator } from '@vee-validate/zod'
import { useRouter, useRoute } from 'vue-router'
import { usePageTitle } from '@/shared/composables/usePageTitle'
import { useAuthStore } from '../stores/authStore'
import { LoginFormSchema } from '../validations/auth'
import type { LoginForm } from '../validations/auth'
import { useNotify } from '@/shared/composables/useNotify'

usePageTitle('Sign In')
const auth = useAuthStore()
const router = useRouter()
const route = useRoute()
const notify = useNotify()

const { handleSubmit, isSubmitting, meta } = useForm<LoginForm>({
  validationSchema: toFormValidator(LoginFormSchema),
  initialValues: { credential: '', password: '' },
})

const onSubmit = handleSubmit(async (values) => {
  const ok = await auth.login(values.credential, values.password)
  if (ok) {
    const redirect = (route.query.redirect as string) || '/'
    router.push(redirect)
    notify.success(`Welcome back`)
  } else {
    notify.error(auth.error ?? 'Invalid email or password')
  }
})

function onGoogleLogin(): void {
  auth.loginWithGoogle()
}
</script>
<template>
  <div class="w-full max-w-md mx-auto">
    <h1 class="text-lg font-semibold text-neutral-900 mb-6">Sign In</h1>

    <form @submit="onSubmit" class="space-y-4">
      <div>
        <label class="block text-sm font-medium text-neutral-700 mb-1">Email</label>
        <Field name="credential" v-slot="{ field, errorMessage }">
          <InputText v-bind="field" type="email" placeholder="you@example.com" class="w-full" :class="{ 'p-invalid': errorMessage }" />
          <ErrorMessage name="credential" class="text-red-600 text-xs mt-1" />
        </Field>
      </div>

      <div>
        <div class="flex items-center justify-between mb-1">
          <label class="text-sm font-medium text-neutral-700">Password</label>
          <router-link to="/forgot-password" class="text-xs text-neutral-500 hover:text-neutral-900">Forgot password?</router-link>
        </div>
        <Field name="password" v-slot="{ field, errorMessage }">
          <Password v-bind="field" placeholder="Enter your password" class="w-full" :class="{ 'p-invalid': errorMessage }" :feedback="false" toggle-mask />
          <ErrorMessage name="password" class="text-red-600 text-xs mt-1" />
        </Field>
      </div>

      <Button type="submit" label="Sign In" severity="primary" class="w-full" :disabled="!meta.valid || isSubmitting" :loading="isSubmitting" />
    </form>

    <div class="flex items-center gap-3 my-6">
      <div class="flex-1 border-t border-neutral-200" />
      <span class="text-xs text-neutral-400">or</span>
      <div class="flex-1 border-t border-neutral-200" />
    </div>

    <Button label="Continue with Google" icon="pi pi-google" severity="secondary" outlined class="w-full" @click="onGoogleLogin" />

    <p class="text-center text-sm text-neutral-500 mt-6">
      Don't have an account?
      <router-link to="/register" class="font-medium text-neutral-900 hover:underline ml-1">Register</router-link>
    </p>
  </div>
</template>
```

- [ ] **Step 2: Verify build**

```bash
cd app/Store && npx tsc --noEmit
```

- [ ] **Step 3: Commit**

```bash
git add app/Store/src/features/identity/views/LoginView.vue
git commit -m "feat(store): implement LoginView with vee-validate, Zod, and Google login"
```

---

### Task 2: RegisterView — vee-validate form

**Files:**
- Modify: `app/Store/src/features/identity/views/RegisterView.vue`

- [ ] **Step 1: Replace RegisterView.vue**

```vue
<script setup lang="ts">
import { useForm } from 'vee-validate'
import { toFormValidator } from '@vee-validate/zod'
import { useRouter } from 'vue-router'
import { usePageTitle } from '@/shared/composables/usePageTitle'
import { useAuthStore } from '../stores/authStore'
import { RegisterFormSchema } from '../validations/auth'
import type { RegisterForm } from '../validations/auth'
import { useNotify } from '@/shared/composables/useNotify'

usePageTitle('Register')
const auth = useAuthStore()
const router = useRouter()
const notify = useNotify()

const { handleSubmit, isSubmitting, meta } = useForm<RegisterForm>({
  validationSchema: toFormValidator(RegisterFormSchema),
  initialValues: { fullName: '', email: '', password: '', confirmPassword: '' },
})

const onSubmit = handleSubmit(async (values) => {
  const ok = await auth.register({
    fullName: values.fullName,
    email: values.email,
    password: values.password,
  })
  if (ok) {
    await auth.login(values.email, values.password)
    router.push('/')
    notify.success('Account created')
  } else {
    notify.error(auth.error ?? 'Registration failed')
  }
})

function onGoogleLogin(): void {
  auth.loginWithGoogle()
}
</script>
<template>
  <div class="w-full max-w-md mx-auto">
    <h1 class="text-lg font-semibold text-neutral-900 mb-6">Create your account</h1>

    <form @submit="onSubmit" class="space-y-4">
      <div>
        <label class="block text-sm font-medium text-neutral-700 mb-1">Full name</label>
        <Field name="fullName" v-slot="{ field, errorMessage }">
          <InputText v-bind="field" placeholder="Jane Doe" class="w-full" :class="{ 'p-invalid': errorMessage }" />
          <ErrorMessage name="fullName" class="text-red-600 text-xs mt-1" />
        </Field>
      </div>

      <div>
        <label class="block text-sm font-medium text-neutral-700 mb-1">Email</label>
        <Field name="email" v-slot="{ field, errorMessage }">
          <InputText v-bind="field" type="email" placeholder="you@example.com" class="w-full" :class="{ 'p-invalid': errorMessage }" />
          <ErrorMessage name="email" class="text-red-600 text-xs mt-1" />
        </Field>
      </div>

      <div>
        <label class="block text-sm font-medium text-neutral-700 mb-1">Password</label>
        <Field name="password" v-slot="{ field, errorMessage }">
          <Password v-bind="field" placeholder="Min 8 characters" class="w-full" :class="{ 'p-invalid': errorMessage }" :feedback="false" toggle-mask />
          <ErrorMessage name="password" class="text-red-600 text-xs mt-1" />
        </Field>
      </div>

      <div>
        <label class="block text-sm font-medium text-neutral-700 mb-1">Confirm password</label>
        <Field name="confirmPassword" v-slot="{ field, errorMessage }">
          <Password v-bind="field" placeholder="Re-enter your password" class="w-full" :class="{ 'p-invalid': errorMessage }" :feedback="false" toggle-mask />
          <ErrorMessage name="confirmPassword" class="text-red-600 text-xs mt-1" />
        </Field>
      </div>

      <Button type="submit" label="Create Account" severity="primary" class="w-full" :disabled="!meta.valid || isSubmitting" :loading="isSubmitting" />
    </form>

    <div class="flex items-center gap-3 my-6">
      <div class="flex-1 border-t border-neutral-200" />
      <span class="text-xs text-neutral-400">or</span>
      <div class="flex-1 border-t border-neutral-200" />
    </div>

    <Button label="Continue with Google" icon="pi pi-google" severity="secondary" outlined class="w-full" @click="onGoogleLogin" />

    <p class="text-center text-sm text-neutral-500 mt-6">
      Already have an account?
      <router-link to="/login" class="font-medium text-neutral-900 hover:underline ml-1">Sign In</router-link>
    </p>
  </div>
</template>
```

- [ ] **Step 2: Verify build**

```bash
cd app/Store && npx tsc --noEmit
```

- [ ] **Step 3: Commit**

```bash
git add app/Store/src/features/identity/views/RegisterView.vue
git commit -m "feat(store): implement RegisterView with vee-validate, Zod, and Google login"
```

---

### Task 3: ForgotPasswordView + ResetPasswordView

**Files:**
- Modify: `app/Store/src/features/identity/views/ForgotPasswordView.vue`
- Modify: `app/Store/src/features/identity/views/ResetPasswordView.vue`

- [ ] **Step 1: Replace ForgotPasswordView.vue**

```vue
<script setup lang="ts">
import { ref } from 'vue'
import { useForm } from 'vee-validate'
import { toFormValidator } from '@vee-validate/zod'
import { usePageTitle } from '@/shared/composables/usePageTitle'
import { useAuthStore } from '../stores/authStore'
import { ForgotPasswordSchema } from '../validations/auth'
import { useNotify } from '@/shared/composables/useNotify'

usePageTitle('Reset Password')
const auth = useAuthStore()
const notify = useNotify()
const submitted = ref(false)
const submittedEmail = ref('')
const cooldown = ref(0)

const { handleSubmit, isSubmitting, meta } = useForm({
  validationSchema: toFormValidator(ForgotPasswordSchema),
  initialValues: { email: '' },
})

const onSubmit = handleSubmit(async (values) => {
  await auth.forgotPassword(values.email)
  submitted.value = true
  submittedEmail.value = values.email
  notify.success('Check your email')
})

async function onResend(): Promise<void> {
  if (cooldown.value > 0) return
  cooldown.value = 30
  await auth.forgotPassword(submittedEmail.value)
  notify.success('Email sent')
  const timer = setInterval(() => {
    cooldown.value--
    if (cooldown.value <= 0) clearInterval(timer)
  }, 1000)
}
</script>
<template>
  <div class="w-full max-w-md mx-auto">
    <h1 class="text-lg font-semibold text-neutral-900 mb-3">Reset your password</h1>

    <!-- Form state -->
    <form v-if="!submitted" @submit="onSubmit" class="space-y-4">
      <p class="text-sm text-neutral-500">Enter your email and we'll send you a link to reset your password.</p>
      <div>
        <label class="block text-sm font-medium text-neutral-700 mb-1">Email</label>
        <Field name="email" v-slot="{ field, errorMessage }">
          <InputText v-bind="field" type="email" placeholder="you@example.com" class="w-full" :class="{ 'p-invalid': errorMessage }" />
          <ErrorMessage name="email" class="text-red-600 text-xs mt-1" />
        </Field>
      </div>
      <Button type="submit" label="Send Reset Link" severity="primary" class="w-full" :disabled="!meta.valid || isSubmitting" :loading="isSubmitting" />
    </form>

    <!-- Success state -->
    <div v-else class="text-center py-8">
      <i class="pi pi-check-circle text-4xl text-green-500 mb-4 block" />
      <p class="text-sm font-medium text-neutral-900 mb-1">Check your email</p>
      <p class="text-sm text-neutral-500 mb-4">We sent a reset link to {{ submittedEmail }}</p>
      <button class="text-sm font-medium text-neutral-900 hover:underline" :disabled="cooldown > 0" @click="onResend">
        {{ cooldown > 0 ? `Resend in ${cooldown}s` : 'Resend' }}
      </button>
    </div>

    <p class="text-center text-sm mt-6">
      <router-link to="/login" class="text-neutral-500 hover:text-neutral-900">&larr; Back to Sign In</router-link>
    </p>
  </div>
</template>
```

- [ ] **Step 2: Replace ResetPasswordView.vue**

```vue
<script setup lang="ts">
import { useForm } from 'vee-validate'
import { toFormValidator } from '@vee-validate/zod'
import { useRouter, useRoute } from 'vue-router'
import { usePageTitle } from '@/shared/composables/usePageTitle'
import { useAuthStore } from '../stores/authStore'
import { ResetPasswordSchema } from '../validations/auth'
import { useNotify } from '@/shared/composables/useNotify'

usePageTitle('Set New Password')
const auth = useAuthStore()
const router = useRouter()
const route = useRoute()
const notify = useNotify()

const token = (route.query.token as string) || ''
const hasToken = token.length > 0

const { handleSubmit, isSubmitting, meta } = useForm({
  validationSchema: toFormValidator(ResetPasswordSchema),
  initialValues: { token, newPassword: '' },
})

const onSubmit = handleSubmit(async (values) => {
  const ok = await auth.resetPassword(values.token, values.newPassword)
  if (ok) {
    notify.success('Password reset successfully. Please sign in.')
    router.push('/login')
  } else {
    notify.error('Invalid or expired reset token')
  }
})
</script>
<template>
  <div class="w-full max-w-md mx-auto">
    <h1 class="text-lg font-semibold text-neutral-900 mb-6">Set new password</h1>

    <div v-if="!hasToken" class="text-center py-8">
      <p class="text-sm text-neutral-500 mb-4">Invalid reset link.</p>
      <router-link to="/forgot-password" class="text-sm font-medium text-neutral-900 hover:underline">Request a new one</router-link>
    </div>

    <form v-else @submit="onSubmit" class="space-y-4">
      <div>
        <label class="block text-sm font-medium text-neutral-700 mb-1">New password</label>
        <Field name="newPassword" v-slot="{ field, errorMessage }">
          <Password v-bind="field" placeholder="Min 8 characters" class="w-full" :class="{ 'p-invalid': errorMessage }" :feedback="false" toggle-mask />
          <ErrorMessage name="newPassword" class="text-red-600 text-xs mt-1" />
        </Field>
      </div>
      <Button type="submit" label="Set New Password" severity="primary" class="w-full" :disabled="!meta.valid || isSubmitting" :loading="isSubmitting" />
    </form>
  </div>
</template>
```

- [ ] **Step 3: Verify build**

```bash
cd app/Store && npx tsc --noEmit
```

- [ ] **Step 4: Commit**

```bash
git add app/Store/src/features/identity/views/ForgotPasswordView.vue app/Store/src/features/identity/views/ResetPasswordView.vue
git commit -m "feat(store): implement ForgotPassword and ResetPassword views with vee-validate"
```

---

### Task 4: SessionsView — session list

**Files:**
- Modify: `app/Store/src/features/identity/views/SessionsView.vue`

- [ ] **Step 1: Replace SessionsView.vue**

```vue
<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { usePageTitle } from '@/shared/composables/usePageTitle'
import { SessionApi } from '../services'
import { useNotify } from '@/shared/composables/useNotify'
import type { SessionInfo } from '../types'

usePageTitle('Sessions')
const notify = useNotify()
const sessions = ref<SessionInfo[]>([])
const loading = ref(true)
const error = ref<string | null>(null)

onMounted(async () => {
  loading.value = true
  const result = await SessionApi.getSessions()
  if (result.isSuccess && result.value) {
    sessions.value = result.value
  } else {
    error.value = 'Failed to load sessions'
  }
  loading.value = false
})

async function onRevokeAll(): Promise<void> {
  const result = await SessionApi.revokeAll()
  if (result.isSuccess) {
    sessions.value = sessions.value.filter(s => s.isCurrent)
    notify.success('All other sessions revoked')
  } else {
    notify.error('Failed to revoke sessions')
  }
}

function deviceIcon(session: SessionInfo): string {
  const name = session.deviceName.toLowerCase()
  if (name.includes('phone') || name.includes('iphone')) return 'pi pi-mobile'
  if (name.includes('tablet') || name.includes('ipad')) return 'pi pi-tablet'
  return 'pi pi-desktop'
}

function formatDate(iso: string): string {
  const d = new Date(iso)
  return d.toLocaleDateString('en-US', { month: 'short', day: 'numeric', hour: '2-digit', minute: '2-digit' })
}
</script>
<template>
  <div>
    <h1 class="text-2xl font-bold text-neutral-900 mb-6">Active Sessions</h1>

    <!-- Loading -->
    <div v-if="loading" class="space-y-3">
      <Skeleton v-for="i in 3" :key="i" height="4rem" />
    </div>

    <!-- Error -->
    <div v-else-if="error" class="text-center py-8">
      <p class="text-neutral-500 mb-4">{{ error }}</p>
      <Button label="Retry" severity="secondary" outlined @click="loading = true; error = null; onMounted(() => {})" />
    </div>

    <!-- Session List -->
    <div v-else class="space-y-3">
      <div v-for="session in sessions" :key="session.id" class="flex items-center justify-between p-4 bg-white rounded-lg border border-neutral-200">
        <div class="flex items-center gap-3">
          <i :class="deviceIcon(session)" class="text-xl text-neutral-500" />
          <div>
            <p class="text-sm font-medium text-neutral-900">{{ session.deviceName }}</p>
            <p class="text-xs text-neutral-500">{{ session.ipAddress }} &middot; {{ formatDate(session.lastActivityAt) }}</p>
          </div>
        </div>
        <Tag v-if="session.isCurrent" value="Current" severity="info" />
        <Button v-else label="Revoke" severity="danger" outlined size="small" @click="notify.info('Revoke single session not yet available')" />
      </div>
    </div>

    <!-- Revoke All -->
    <div v-if="sessions.length > 1 && !loading" class="mt-6">
      <Button label="Revoke All Other Sessions" severity="danger" text size="small" @click="onRevokeAll" />
    </div>
  </div>
</template>
```

- [ ] **Step 2: Verify build**

```bash
cd app/Store && npx tsc --noEmit
```

- [ ] **Step 3: Commit**

```bash
git add app/Store/src/features/identity/views/SessionsView.vue
git commit -m "feat(store): implement SessionsView with session list and revoke actions"
```

---

### Task 5: Smoke tests

**Files:**
- Create: `app/Store/src/features/identity/views/__tests__/LoginView.spec.ts`
- Create: `app/Store/src/features/identity/views/__tests__/RegisterView.spec.ts`

- [ ] **Step 1: Write LoginView smoke test**

Create `app/Store/src/features/identity/views/__tests__/LoginView.spec.ts`:

```typescript
import { describe, it, expect, vi, beforeEach } from 'vitest'
import { mount } from '@vue/test-utils'
import { createRouter, createMemoryHistory } from 'vue-router'
import LoginView from '../LoginView.vue'

vi.mock('@/shared/composables/usePageTitle', () => ({ usePageTitle: vi.fn() }))
vi.mock('@/shared/composables/useNotify', () => ({ useNotify: () => ({ success: vi.fn(), error: vi.fn() }) }))
vi.mock('../../stores/authStore', () => ({
  useAuthStore: () => ({ login: vi.fn(), loginWithGoogle: vi.fn(), error: null }),
}))

const router = createRouter({
  history: createMemoryHistory(),
  routes: [
    { path: '/', component: { template: '<div />' } },
    { path: '/login', component: { template: '<div />' } },
  ],
})

describe('LoginView', () => {
  it('renders email and password fields', async () => {
    router.push('/login')
    await router.isReady()
    const wrapper = mount(LoginView, {
      global: { plugins: [router] },
    })
    expect(wrapper.html()).toContain('Sign In')
    expect(wrapper.html()).toContain('Forgot password')
  })

  it('renders Google login button', async () => {
    router.push('/login')
    await router.isReady()
    const wrapper = mount(LoginView, {
      global: { plugins: [router] },
    })
    expect(wrapper.html()).toContain('Continue with Google')
  })
})
```

- [ ] **Step 2: Write RegisterView smoke test**

Create `app/Store/src/features/identity/views/__tests__/RegisterView.spec.ts`:

```typescript
import { describe, it, expect, vi } from 'vitest'
import { mount } from '@vue/test-utils'
import { createRouter, createMemoryHistory } from 'vue-router'
import RegisterView from '../RegisterView.vue'

vi.mock('@/shared/composables/usePageTitle', () => ({ usePageTitle: vi.fn() }))
vi.mock('@/shared/composables/useNotify', () => ({ useNotify: () => ({ success: vi.fn(), error: vi.fn() }) }))
vi.mock('../../stores/authStore', () => ({
  useAuthStore: () => ({ register: vi.fn(), login: vi.fn(), loginWithGoogle: vi.fn(), error: null }),
}))

const router = createRouter({
  history: createMemoryHistory(),
  routes: [
    { path: '/', component: { template: '<div />' } },
    { path: '/register', component: { template: '<div />' } },
  ],
})

describe('RegisterView', () => {
  it('renders all form fields', async () => {
    router.push('/register')
    await router.isReady()
    const wrapper = mount(RegisterView, {
      global: { plugins: [router] },
    })
    expect(wrapper.html()).toContain('Create your account')
    expect(wrapper.html()).toContain('Full name')
    expect(wrapper.html()).toContain('Email')
  })

  it('renders Google login button', async () => {
    router.push('/register')
    await router.isReady()
    const wrapper = mount(RegisterView, {
      global: { plugins: [router] },
    })
    expect(wrapper.html()).toContain('Continue with Google')
  })
})
```

- [ ] **Step 3: Run tests**

```bash
cd app/Store && npx vitest run src/features/identity/views/__tests__/
```

- [ ] **Step 4: Commit**

```bash
git add app/Store/src/features/identity/views/__tests__/
git commit -m "test(store): add smoke tests for LoginView and RegisterView"
```

---

### Task 6: Full verification

- [ ] **Step 1: Run all tests**

```bash
cd app/Store && npx vitest run
```

- [ ] **Step 2: Run type check**

```bash
cd app/Store && npx tsc --noEmit
```

- [ ] **Step 3: Run build**

```bash
cd app/Store && pnpm run build-only
```

- [ ] **Step 4: Verify**

```bash
git status
```
