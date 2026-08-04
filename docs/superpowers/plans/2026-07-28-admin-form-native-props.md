# Auth Form Native Props — Implementation Plan

> **For agentic workers:** Execute inline. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Replace Tailwind sizing classes on InputText/InputPassword/Button with PrimeVue native `size="large"` and `fluid` props, and replace raw labels with `FloatLabel variant="on"` on all 3 auth pages.

**Architecture:** Each of the 3 auth pages gets the same mechanical edit: (1) add `import FloatLabel`, (2) swap `class="w-full p-4 text-lg"` → `fluid size="large"` on inputs/buttons, (3) remove `:pt` on InputPassword, (4) wrap labels + input in `<FloatLabel variant="on">`.

**Tech Stack:** Vue 3 + TypeScript, PrimeVue v5 (`FloatLabel` from `primevue/floatlabel`)

## Global Constraints

- `pnpm run build` must have zero TypeScript errors
- `pnpm run test:unit -- run` must pass all tests
- No logic or behavior changes
- Tailwind for layout spacing (`mb-*`, `gap-*`, `flex`) stays

---

### Task 1: LoginPage.vue

**Files:**
- Modify: `app/Admin/src/features/auth/views/LoginPage.vue`

- [ ] **Step 1: Add FloatLabel import**

```ts
import FloatLabel from 'primevue/floatlabel'
```

Insert after the existing `import InputPassword from 'primevue/inputpassword'` line.

- [ ] **Step 2: Replace email input block**

Current:
```html
    <label for="email1" class="block text-surface-900 dark:text-surface-0 text-2xl font-medium mb-2"
      >Email or Username</label
    >
    <IconField class="w-full md:w-[30rem] mb-8">
      <InputIcon> <User /> </InputIcon>
      <InputText
        id="email1"
        v-model="credential"
        type="text"
        placeholder="Email address"
        class="w-full p-4 text-lg"
        autocomplete="username"
        :invalid="!!fieldErrors.credential"
      />
    </IconField>
```

Replace with:
```html
    <FloatLabel variant="on" class="w-full md:w-[30rem] mb-8">
      <IconField>
        <InputIcon> <User /> </InputIcon>
        <InputText
          id="email1"
          v-model="credential"
          type="text"
          placeholder="Email address"
          fluid size="large"
          autocomplete="username"
          :invalid="!!fieldErrors.credential"
        />
      </IconField>
      <label>Email or Username</label>
    </FloatLabel>
```

- [ ] **Step 3: Replace password input block**

Current:
```html
    <label
      for="password1"
      class="block text-surface-900 dark:text-surface-0 font-medium text-2xl mb-2"
      >Password</label
    >
    <IconField class="mb-4 w-full">
      <InputIcon> <Lock /> </InputIcon>
      <InputPassword
        id="password1"
        v-model="password"
        placeholder="Password"
        :mask="mask"
        class="w-full"
        fluid
        :feedback="false"
        autocomplete="current-password"
        :invalid="!!fieldErrors.password"
        :pt="{ input: { class: 'p-4 text-lg' } }"
      />
      <InputIcon class="cursor-pointer" @click="mask = !mask">
        <Eye v-if="mask" :size="16" />
        <EyeSlash v-else :size="16" />
      </InputIcon>
    </IconField>
```

Replace with:
```html
    <FloatLabel variant="on" class="mb-4 w-full">
      <IconField>
        <InputIcon> <Lock /> </InputIcon>
        <InputPassword
          id="password1"
          v-model="password"
          placeholder="Password"
          :mask="mask"
          fluid size="large"
          :feedback="false"
          autocomplete="current-password"
          :invalid="!!fieldErrors.password"
        />
        <InputIcon class="cursor-pointer" @click="mask = !mask">
          <Eye v-if="mask" :size="16" />
          <EyeSlash v-else :size="16" />
        </InputIcon>
      </IconField>
      <label>Password</label>
    </FloatLabel>
```

- [ ] **Step 4: Replace button**

Current: `<Button label="Sign In" class="w-full p-4 text-lg" :loading="isLoading" @click="onSubmit" />`
Replace with: `<Button label="Sign In" fluid size="large" :loading="isLoading" @click="onSubmit" />`

- [ ] **Step 5: Build + test**

Run: `pnpm run build && pnpm run test:unit -- run`
Expected: zero errors, all tests pass

- [ ] **Step 6: Commit**

```bash
git add app/Admin/src/features/auth/views/LoginPage.vue
git commit -m "refactor(admin): use PrimeVue native props on LoginPage form"
```

---

### Task 2: ForgotPasswordPage.vue

**Files:**
- Modify: `app/Admin/src/features/auth/views/ForgotPasswordPage.vue`

- [ ] **Step 1: Add FloatLabel import**

```ts
import FloatLabel from 'primevue/floatlabel'
```

- [ ] **Step 2: Replace email + button**

Current email block:
```html
    <div class="flex flex-col gap-1">
      <label for="email" class="text-surface-900 dark:text-surface-0 font-medium text-2xl">Email</label>
      <IconField class="w-full">
        <InputIcon> <Envelope /> </InputIcon>
        <InputText id="email" v-model="email" v-bind="emailAttrs" class="w-full p-4 text-lg" type="email" placeholder="Email address" autocomplete="email" :invalid="!!errors.email" />
      </IconField>
      <small v-if="errors.email" class="text-red-500">{{ errors.email }}</small>
    </div>
```

Replace with:
```html
    <div class="flex flex-col gap-1">
      <FloatLabel variant="on">
        <IconField>
          <InputIcon> <Envelope /> </InputIcon>
          <InputText id="email" v-model="email" v-bind="emailAttrs" fluid size="large" type="email" placeholder="Email address" autocomplete="email" :invalid="!!errors.email" />
        </IconField>
        <label>Email</label>
      </FloatLabel>
      <small v-if="errors.email" class="text-red-500">{{ errors.email }}</small>
    </div>
```

Replace button: `<Button type="submit" label="Send Reset Link" fluid size="large" :loading="isSubmitting" />`

- [ ] **Step 3: Build + test**

```bash
pnpm run build && pnpm run test:unit -- run
```

- [ ] **Step 4: Commit**

```bash
git add app/Admin/src/features/auth/views/ForgotPasswordPage.vue
git commit -m "refactor(admin): use PrimeVue native props on ForgotPasswordPage form"
```

---

### Task 3: ResetPasswordPage.vue

**Files:**
- Modify: `app/Admin/src/features/auth/views/ResetPasswordPage.vue`

- [ ] **Step 1: Add FloatLabel import**

```ts
import FloatLabel from 'primevue/floatlabel'
```

- [ ] **Step 2: Replace all 4 input blocks**

For each of the 4 fields (email, userId, token, newPassword), replace the `<label>` + `<IconField><InputText/></IconField>` pattern with `<FloatLabel variant="on"><IconField><InputText fluid size="large"/></IconField><label>Text</label></FloatLabel>`.

Also: on InputPassword, remove `class="w-full"` and `:pt="{ input: { class: 'p-4 text-lg' } }"`, add `fluid size="large"`.

Replace button: `<Button type="submit" label="Reset Password" fluid size="large" :loading="isSubmitting" />`

- [ ] **Step 3: Build + test**

```bash
pnpm run build && pnpm run test:unit -- run
```

- [ ] **Step 4: Commit**

```bash
git add app/Admin/src/features/auth/views/ResetPasswordPage.vue
git commit -m "refactor(admin): use PrimeVue native props on ResetPasswordPage form"
```

---

### Task 4: Final verification

- [ ] **Step 1: Full build**

Run: `pnpm run build`
Expected: zero errors, zero warnings

- [ ] **Step 2: Full test suite**

Run: `pnpm run test:unit -- run`
Expected: all 357 tests pass

- [ ] **Step 3: Lint**

Run: `pnpm run lint`
Expected: no new warnings (pre-existing lint issues in parsers.spec.ts are unrelated)

---

## Self-Review

1. **Spec coverage:** All 3 files covered, all props mapping applied, FloatLabel on all labels, build+test verified.
2. **Placeholder scan:** No TBD/TODO, all code is concrete.
3. **Type consistency:** Single component type — FloatLabel. No cross-task dependencies.
