# Shared Component Library P0 — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Port and build 14 P0 shared components from legacy Admin into `app/Admin/src/shared/components/`, redesigned for PrimeVue 5 + Vue 3.5 Composition API + TypeScript, following Sakai design patterns.

**Architecture:** Components organized in 5 category subdirectories (layout, form, data-display, feedback, overlays) under `src/shared/components/`. Each category has a barrel `index.ts`. Root barrel re-exports all. Tests in `src/shared/components/__tests__/`. TDD: write test first, verify it fails, implement component, verify passes, commit.

**Tech Stack:** Vue 3.5, TypeScript 6, PrimeVue 5, Vitest + @vue/test-utils, jsdom

## Global Constraints

- `<script setup lang="ts">` only — no Options API
- Typed props via `defineProps<T>()` with `withDefaults()` for optional props
- Typed emits via `defineEmits<{ event: [payload] }>()`
- Typed slots via `defineSlots<{ slotName?(): any }>()`
- `defineModel<T>()` for v-model two-way binding
- Tailwind utilities for layout/spacing/sizing — no inline styles
- CSS token references for colors: `--p-primary-color`, `--p-text-muted-color`, `--p-surface-card`, `--p-content-border-color`
- No SCSS in shared components
- Vue component naming: `PascalCase.vue` (no `.Component` suffix)
- Path alias: `@/` → `./src/`
- Test environment: `jsdom`

---

### Task 1: Scaffold Category Directories and Barrels

**Files:**
- Create: `src/shared/components/layout/index.ts`
- Create: `src/shared/components/form/index.ts`
- Create: `src/shared/components/data-display/index.ts`
- Create: `src/shared/components/feedback/index.ts`
- Create: `src/shared/components/overlays/index.ts`
- Modify: `src/shared/components/index.ts`
- Create: `src/shared/components/__tests__/` (directory)

**Interfaces:**
- Consumes: nothing
- Produces: empty barrels that all subsequent tasks append to

- [ ] **Step 1: Create category barrel files**

```bash
mkdir -p src/shared/components/layout
mkdir -p src/shared/components/form
mkdir -p src/shared/components/data-display
mkdir -p src/shared/components/feedback
mkdir -p src/shared/components/overlays
mkdir -p src/shared/components/__tests__
```

- [ ] **Step 2: Write category barrel stubs**

Write `src/shared/components/layout/index.ts`:
```ts
export {}
```

Write `src/shared/components/form/index.ts`:
```ts
export {}
```

Write `src/shared/components/data-display/index.ts`:
```ts
export {}
```

Write `src/shared/components/feedback/index.ts`:
```ts
export {}
```

Write `src/shared/components/overlays/index.ts`:
```ts
export {}
```

- [ ] **Step 3: Update root barrel**

Write `src/shared/components/index.ts` (replacing existing):
```ts
export * from './layout'
export * from './form'
export * from './data-display'
export * from './feedback'
export * from './overlays'
```

- [ ] **Step 4: Verify TypeScript resolves barrels**

Run: `npx vue-tsc --noEmit 2>&1 | head -20`
Expected: no errors related to shared/components

- [ ] **Step 5: Commit**

```bash
git add src/shared/components/
git commit -m "feat: scaffold shared component category directories and barrels"
```

---

### Task 2: DetailField Component

**Files:**
- Create: `src/shared/components/__tests__/DetailField.spec.ts`
- Create: `src/shared/components/data-display/DetailField.vue`
- Modify: `src/shared/components/data-display/index.ts`

**Interfaces:**
- Consumes: nothing
- Produces: `DetailField` — props: `{ label: string; value?: string | number | null; emptyText?: string }`

- [ ] **Step 1: Write the failing test**

Write `src/shared/components/__tests__/DetailField.spec.ts`:
```ts
import { describe, it, expect } from 'vitest'
import { mount } from '@vue/test-utils'
import DetailField from '../data-display/DetailField.vue'

describe('DetailField', () => {
  it('renders label and value', () => {
    const wrapper = mount(DetailField, {
      props: { label: 'Name', value: 'John' },
    })
    expect(wrapper.text()).toContain('Name')
    expect(wrapper.text()).toContain('John')
  })

  it('shows em-dash fallback when value is null', () => {
    const wrapper = mount(DetailField, {
      props: { label: 'Name', value: null },
    })
    expect(wrapper.text()).toContain('\u2014')
  })

  it('shows em-dash fallback when value is undefined', () => {
    const wrapper = mount(DetailField, {
      props: { label: 'Name' },
    })
    expect(wrapper.text()).toContain('\u2014')
  })

  it('shows em-dash fallback when value is empty string', () => {
    const wrapper = mount(DetailField, {
      props: { label: 'Name', value: '' },
    })
    expect(wrapper.text()).toContain('\u2014')
  })

  it('uses custom emptyText when provided', () => {
    const wrapper = mount(DetailField, {
      props: { label: 'Name', value: null, emptyText: 'N/A' },
    })
    expect(wrapper.text()).toContain('N/A')
    expect(wrapper.text()).not.toContain('\u2014')
  })

  it('renders number zero as value not fallback', () => {
    const wrapper = mount(DetailField, {
      props: { label: 'Count', value: 0 },
    })
    expect(wrapper.text()).toContain('0')
    expect(wrapper.text()).not.toContain('\u2014')
  })

  it('renders custom value via default slot', () => {
    const wrapper = mount(DetailField, {
      props: { label: 'Status' },
      slots: { default: '<span class="custom">Active</span>' },
    })
    expect(wrapper.find('.custom').exists()).toBe(true)
    expect(wrapper.find('.custom').text()).toBe('Active')
  })
})
```

- [ ] **Step 2: Run test to verify it fails**

Run: `npx vitest run src/shared/components/__tests__/DetailField.spec.ts`
Expected: FAIL — cannot find module

- [ ] **Step 3: Write component**

Write `src/shared/components/data-display/DetailField.vue`:
```vue
<script setup lang="ts">
withDefaults(defineProps<{
  label: string
  value?: string | number | null
  emptyText?: string
}>(), {
  emptyText: '\u2014',
})

defineSlots<{
  default?(): any
}>()
</script>

<template>
  <div class="flex flex-col">
    <span class="text-xs uppercase font-bold mb-1" style="color: var(--p-text-muted-color)">{{ label }}</span>
    <slot>
      <span v-if="value !== null && value !== undefined && value !== ''" class="text-lg font-medium" style="color: var(--p-text-color)">
        {{ value }}
      </span>
      <span v-else class="text-lg" style="color: var(--p-text-muted-color)">{{ emptyText }}</span>
    </slot>
  </div>
</template>
```

- [ ] **Step 4: Run tests to verify they pass**

Run: `npx vitest run src/shared/components/__tests__/DetailField.spec.ts`
Expected: 7 tests PASS

- [ ] **Step 5: Update barrel**

Modify `src/shared/components/data-display/index.ts`:
```ts
export { default as DetailField } from './DetailField.vue'
```

- [ ] **Step 6: Verify barrel import works**

Run: `npx vitest run src/shared/components/__tests__/DetailField.spec.ts`
Expected: 7 tests PASS (barrel import shouldn't break anything)

- [ ] **Step 7: Commit**

```bash
git add src/shared/components/__tests__/DetailField.spec.ts src/shared/components/data-display/
git commit -m "feat: add DetailField component with tests"
```

---

### Task 3: FormField Component

**Files:**
- Create: `src/shared/components/__tests__/FormField.spec.ts`
- Create: `src/shared/components/form/FormField.vue`
- Modify: `src/shared/components/form/index.ts`

**Interfaces:**
- Consumes: nothing
- Produces: `FormField` — props: `{ label: string; forId?: string; required?: boolean; error?: string; hint?: string }`, slots: `{ default() }`

- [ ] **Step 1: Write the failing test**

Write `src/shared/components/__tests__/FormField.spec.ts`:
```ts
import { describe, it, expect } from 'vitest'
import { mount } from '@vue/test-utils'
import FormField from '../form/FormField.vue'

describe('FormField', () => {
  it('renders label and slot content', () => {
    const wrapper = mount(FormField, {
      props: { label: 'Name', forId: 'name' },
      slots: { default: '<input id="name" />' },
    })
    expect(wrapper.find('label').text()).toContain('Name')
  })

  it('renders label with for attribute when forId provided', () => {
    const wrapper = mount(FormField, {
      props: { label: 'Email', forId: 'email' },
      slots: { default: '<input id="email" />' },
    })
    expect(wrapper.find('label').attributes('for')).toBe('email')
  })

  it('shows required asterisk', () => {
    const wrapper = mount(FormField, {
      props: { label: 'Name', forId: 'name', required: true },
      slots: { default: '<input id="name" />' },
    })
    const label = wrapper.find('label')
    expect(label.text()).toContain('*')
  })

  it('does not show asterisk when not required', () => {
    const wrapper = mount(FormField, {
      props: { label: 'Name', forId: 'name' },
      slots: { default: '<input id="name" />' },
    })
    expect(wrapper.find('label').text()).not.toContain('*')
  })

  it('shows error message when provided', () => {
    const wrapper = mount(FormField, {
      props: { label: 'Name', forId: 'name', error: 'Required field' },
      slots: { default: '<input id="name" />' },
    })
    expect(wrapper.text()).toContain('Required field')
  })

  it('shows hint when no error', () => {
    const wrapper = mount(FormField, {
      props: { label: 'Name', forId: 'name', hint: 'Enter your full name' },
      slots: { default: '<input id="name" />' },
    })
    expect(wrapper.text()).toContain('Enter your full name')
  })

  it('does not show hint when error is present', () => {
    const wrapper = mount(FormField, {
      props: { label: 'Name', forId: 'name', error: 'Required', hint: 'Enter name' },
      slots: { default: '<input id="name" />' },
    })
    expect(wrapper.text()).toContain('Required')
    expect(wrapper.text()).not.toContain('Enter name')
  })
})
```

- [ ] **Step 2: Run test to verify it fails**

Run: `npx vitest run src/shared/components/__tests__/FormField.spec.ts`
Expected: FAIL — cannot find module

- [ ] **Step 3: Write component**

Write `src/shared/components/form/FormField.vue`:
```vue
<script setup lang="ts">
withDefaults(defineProps<{
  label: string
  forId?: string
  required?: boolean
  error?: string
  hint?: string
}>(), {
  required: false,
})

defineSlots<{
  default(): any
}>()
</script>

<template>
  <div class="flex flex-col gap-2">
    <label
      v-if="forId"
      :for="forId"
      class="font-bold text-xs uppercase tracking-wider ml-1"
      style="color: var(--p-text-muted-color)"
    >
      {{ label }}
      <span v-if="required" style="color: var(--p-red-500)">*</span>
    </label>
    <label
      v-else
      class="font-bold text-xs uppercase tracking-wider ml-1"
      style="color: var(--p-text-muted-color)"
    >
      {{ label }}
      <span v-if="required" style="color: var(--p-red-500)">*</span>
    </label>
    <slot />
    <small v-if="error" class="p-error">{{ error }}</small>
    <small v-else-if="hint" style="color: var(--p-text-muted-color)">{{ hint }}</small>
  </div>
</template>
```

- [ ] **Step 4: Run tests to verify they pass**

Run: `npx vitest run src/shared/components/__tests__/FormField.spec.ts`
Expected: 7 tests PASS

- [ ] **Step 5: Update barrel**

Modify `src/shared/components/form/index.ts`:
```ts
export { default as FormField } from './FormField.vue'
```

- [ ] **Step 6: Commit**

```bash
git add src/shared/components/__tests__/FormField.spec.ts src/shared/components/form/
git commit -m "feat: add FormField component with tests"
```

---

### Task 4: EmptyState Component

**Files:**
- Create: `src/shared/components/__tests__/EmptyState.spec.ts`
- Create: `src/shared/components/feedback/EmptyState.vue`
- Modify: `src/shared/components/feedback/index.ts`

**Interfaces:**
- Consumes: nothing
- Produces: `EmptyState` — props: `{ icon?: string; title: string; description?: string; actionLabel?: string; actionTo?: string; actionIcon?: string }`, emits: `{ action: [] }`

- [ ] **Step 1: Write the failing test**

Write `src/shared/components/__tests__/EmptyState.spec.ts`:
```ts
import { describe, it, expect, vi } from 'vitest'
import { mount } from '@vue/test-utils'
import EmptyState from '../feedback/EmptyState.vue'

const pushMock = vi.fn()

vi.mock('vue-router', () => ({
  useRouter: () => ({ push: pushMock }),
}))

describe('EmptyState', () => {
  it('renders title', () => {
    const wrapper = mount(EmptyState, {
      props: { title: 'No items found' },
      global: { stubs: { Button: true } },
    })
    expect(wrapper.text()).toContain('No items found')
  })

  it('renders default icon', () => {
    const wrapper = mount(EmptyState, {
      props: { title: 'Empty' },
      global: { stubs: { Button: true } },
    })
    expect(wrapper.find('i.pi-inbox').exists()).toBe(true)
  })

  it('renders custom icon', () => {
    const wrapper = mount(EmptyState, {
      props: { title: 'Empty', icon: 'pi pi-search' },
      global: { stubs: { Button: true } },
    })
    expect(wrapper.find('i.pi-search').exists()).toBe(true)
  })

  it('renders description when provided', () => {
    const wrapper = mount(EmptyState, {
      props: { title: 'Empty', description: 'Try adding a new item' },
      global: { stubs: { Button: true } },
    })
    expect(wrapper.text()).toContain('Try adding a new item')
  })

  it('renders action button when actionLabel and actionTo provided', () => {
    const wrapper = mount(EmptyState, {
      props: { title: 'Empty', actionLabel: 'Add', actionTo: '/new' },
      global: { stubs: { RouterLink: { template: '<a><slot /></a>' }, Button: true } },
    })
    expect(wrapper.findComponent({ name: 'Button' }).exists()).toBe(true)
  })

  it('emits action when button clicked and no actionTo', async () => {
    const wrapper = mount(EmptyState, {
      props: { title: 'Empty', actionLabel: 'Add' },
      global: { stubs: { Button: { template: '<button @click="$emit(\'click\')"><slot /></button>' } } },
    })
    await wrapper.find('button').trigger('click')
    expect(wrapper.emitted('action')).toBeTruthy()
  })

  it('does not render action button without actionLabel', () => {
    const wrapper = mount(EmptyState, {
      props: { title: 'Empty' },
      global: { stubs: { Button: true } },
    })
    expect(wrapper.findComponent({ name: 'Button' }).exists()).toBe(false)
  })
})
```

- [ ] **Step 2: Run test to verify it fails**

Run: `npx vitest run src/shared/components/__tests__/EmptyState.spec.ts`
Expected: FAIL — cannot find module

- [ ] **Step 3: Write component**

Write `src/shared/components/feedback/EmptyState.vue`:
```vue
<script setup lang="ts">
import { useRouter } from 'vue-router'

const router = useRouter()

withDefaults(defineProps<{
  icon?: string
  title: string
  description?: string
  actionLabel?: string
  actionTo?: string
  actionIcon?: string
}>(), {
  icon: 'pi pi-inbox',
})

defineEmits<{
  action: []
}>()
</script>

<template>
  <div class="flex flex-col items-center justify-center py-20">
    <i :class="icon" class="mb-4 text-6xl opacity-20" style="color: var(--p-text-muted-color)" />
    <p class="text-xl font-medium">{{ title }}</p>
    <p v-if="description" class="text-sm mt-1 max-w-md text-center" style="color: var(--p-text-muted-color)">{{ description }}</p>
    <Button
      v-if="actionLabel"
      :label="actionLabel"
      :icon="actionIcon ?? 'pi pi-plus'"
      class="mt-6 rounded-xl"
      @click="actionTo ? router.push(actionTo) : $emit('action')"
    />
  </div>
</template>
```

- [ ] **Step 4: Run tests to verify they pass**

Run: `npx vitest run src/shared/components/__tests__/EmptyState.spec.ts`
Expected: 7 tests PASS

- [ ] **Step 5: Update barrel**

Modify `src/shared/components/feedback/index.ts`:
```ts
export { default as EmptyState } from './EmptyState.vue'
```

- [ ] **Step 6: Commit**

```bash
git add src/shared/components/__tests__/EmptyState.spec.ts src/shared/components/feedback/
git commit -m "feat: add EmptyState component with tests"
```

---

### Task 5: ConfirmButton Component

**Files:**
- Create: `src/shared/components/__tests__/ConfirmButton.spec.ts`
- Create: `src/shared/components/overlays/ConfirmButton.vue`
- Modify: `src/shared/components/overlays/index.ts`

**Interfaces:**
- Consumes: PrimeVue `useConfirm`
- Produces: `ConfirmButton` — props: `{ message: string; header?: string; icon?: string; severity?: 'danger' | 'warn' | 'info'; acceptLabel?: string; rejectLabel?: string; disabled?: boolean; loading?: boolean }`, emits: `{ confirm: []; cancel: [] }`

- [ ] **Step 1: Write the failing test**

Write `src/shared/components/__tests__/ConfirmButton.spec.ts`:
```ts
import { describe, it, expect, vi, beforeEach } from 'vitest'
import { mount } from '@vue/test-utils'
import { useConfirm } from 'primevue/useconfirm'

vi.mock('primevue/useconfirm')

import ConfirmButton from '../overlays/ConfirmButton.vue'

describe('ConfirmButton', () => {
  beforeEach(() => {
    vi.clearAllMocks()
    vi.mocked(useConfirm).mockReturnValue({
      require: vi.fn(),
    } as any)
  })

  it('renders trigger button', () => {
    const wrapper = mount(ConfirmButton, {
      props: { message: 'Are you sure?' },
      global: { stubs: { Button: true } },
    })
    expect(wrapper.findComponent({ name: 'Button' }).exists()).toBe(true)
  })

  it('calls confirm.require on click', async () => {
    const wrapper = mount(ConfirmButton, {
      props: { message: 'Sure?' },
      global: { stubs: { Button: { template: '<button><slot /></button>' } } },
    })
    await wrapper.find('button').trigger('click')
    expect(vi.mocked(useConfirm)().require).toHaveBeenCalledOnce()
  })

  it('passes message and header to confirm.require', async () => {
    const wrapper = mount(ConfirmButton, {
      props: { header: 'Delete item', message: 'This action cannot be undone.' },
      global: { stubs: { Button: { template: '<button><slot /></button>' } } },
    })
    await wrapper.find('button').trigger('click')
    expect(vi.mocked(useConfirm)().require).toHaveBeenCalledWith(
      expect.objectContaining({
        header: 'Delete item',
        message: 'This action cannot be undone.',
      }),
    )
  })

  it('emits confirm when accept callback fires', async () => {
    let acceptFn: (() => void) | null = null
    vi.mocked(useConfirm).mockReturnValue({
      require: vi.fn((opts: any) => {
        acceptFn = opts.accept
      }),
    } as any)

    const wrapper = mount(ConfirmButton, {
      props: { header: 'Delete', message: 'Are you sure?', severity: 'danger' },
      global: { stubs: { Button: { template: '<button><slot /></button>' } } },
    })

    await wrapper.find('button').trigger('click')
    expect(acceptFn).toBeDefined()
    acceptFn!()
    expect(wrapper.emitted('confirm')).toBeTruthy()
  })

  it('emits cancel when reject callback fires', async () => {
    let rejectFn: (() => void) | null = null
    vi.mocked(useConfirm).mockReturnValue({
      require: vi.fn((opts: any) => {
        rejectFn = opts.reject
      }),
    } as any)

    const wrapper = mount(ConfirmButton, {
      props: { message: 'Are you sure?' },
      global: { stubs: { Button: { template: '<button><slot /></button>' } } },
    })

    await wrapper.find('button').trigger('click')
    expect(rejectFn).toBeDefined()
    rejectFn!()
    expect(wrapper.emitted('cancel')).toBeTruthy()
  })
})
```

- [ ] **Step 2: Run test to verify it fails**

Run: `npx vitest run src/shared/components/__tests__/ConfirmButton.spec.ts`
Expected: FAIL — cannot find module

- [ ] **Step 3: Write component**

Write `src/shared/components/overlays/ConfirmButton.vue`:
```vue
<script setup lang="ts">
import { useConfirm } from 'primevue/useconfirm'

const confirm = useConfirm()

const props = withDefaults(defineProps<{
  message: string
  header?: string
  icon?: string
  severity?: 'danger' | 'warn' | 'info'
  acceptLabel?: string
  rejectLabel?: string
  disabled?: boolean
  loading?: boolean
}>(), {
  icon: 'pi pi-trash',
  severity: 'danger',
})

const emit = defineEmits<{
  confirm: []
  cancel: []
}>()

function onClick() {
  confirm.require({
    message: props.message,
    header: props.header ?? 'Confirm',
    icon: 'pi pi-exclamation-triangle',
    rejectLabel: props.rejectLabel ?? 'Cancel',
    acceptProps: {
      label: props.acceptLabel ?? (props.severity === 'danger' ? 'Delete' : 'Confirm'),
      severity: props.severity as any,
    },
    accept: () => emit('confirm'),
    reject: () => emit('cancel'),
  })
}
</script>

<template>
  <Button
    :icon="icon"
    :severity="severity"
    rounded
    text
    :disabled="disabled"
    :loading="loading"
    @click="onClick"
  />
</template>
```

- [ ] **Step 4: Run tests to verify they pass**

Run: `npx vitest run src/shared/components/__tests__/ConfirmButton.spec.ts`
Expected: 5 tests PASS

- [ ] **Step 5: Update barrel**

Modify `src/shared/components/overlays/index.ts`:
```ts
export { default as ConfirmButton } from './ConfirmButton.vue'
```

- [ ] **Step 6: Commit**

```bash
git add src/shared/components/__tests__/ConfirmButton.spec.ts src/shared/components/overlays/
git commit -m "feat: add ConfirmButton component with tests"
```

---

### Task 6: PageContainer Component

**Files:**
- Create: `src/shared/components/__tests__/PageContainer.spec.ts`
- Create: `src/shared/components/layout/PageContainer.vue`
- Modify: `src/shared/components/layout/index.ts`

**Interfaces:**
- Consumes: nothing
- Produces: `PageContainer` — props: `{ maxWidth?: string; card?: boolean }`, slots: `{ default() }`

- [ ] **Step 1: Write the failing test**

Write `src/shared/components/__tests__/PageContainer.spec.ts`:
```ts
import { describe, it, expect } from 'vitest'
import { mount } from '@vue/test-utils'
import PageContainer from '../layout/PageContainer.vue'

describe('PageContainer', () => {
  it('renders slot content', () => {
    const wrapper = mount(PageContainer, {
      slots: { default: '<p class="content">Hello</p>' },
    })
    expect(wrapper.find('.content').exists()).toBe(true)
    expect(wrapper.find('.content').text()).toBe('Hello')
  })

  it('applies default max-width', () => {
    const wrapper = mount(PageContainer, {
      slots: { default: '<p>test</p>' },
    })
    const container = wrapper.find('.page-container')
    expect(container.attributes('style')).toContain('max-width: 1504px')
  })

  it('applies custom maxWidth', () => {
    const wrapper = mount(PageContainer, {
      props: { maxWidth: '800px' },
      slots: { default: '<p>test</p>' },
    })
    const container = wrapper.find('.page-container')
    expect(container.attributes('style')).toContain('max-width: 800px')
  })

  it('wraps content in card when card is true', () => {
    const wrapper = mount(PageContainer, {
      props: { card: true },
      slots: { default: '<p>test</p>' },
    })
    expect(wrapper.find('.card').exists()).toBe(true)
  })

  it('does not wrap in card when card is false', () => {
    const wrapper = mount(PageContainer, {
      props: { card: false },
      slots: { default: '<p>test</p>' },
    })
    expect(wrapper.find('.card').exists()).toBe(false)
  })

  it('card defaults to true', () => {
    const wrapper = mount(PageContainer, {
      slots: { default: '<p>test</p>' },
    })
    expect(wrapper.find('.card').exists()).toBe(true)
  })
})
```

- [ ] **Step 2: Run test to verify it fails**

Run: `npx vitest run src/shared/components/__tests__/PageContainer.spec.ts`
Expected: FAIL — cannot find module

- [ ] **Step 3: Write component**

Write `src/shared/components/layout/PageContainer.vue`:
```vue
<script setup lang="ts">
withDefaults(defineProps<{
  maxWidth?: string
  card?: boolean
}>(), {
  maxWidth: '1504px',
  card: true,
})

defineSlots<{
  default(): any
}>()
</script>

<template>
  <div
    class="page-container p-6 mx-auto"
    :style="{ maxWidth }"
  >
    <div v-if="card" class="card">
      <slot />
    </div>
    <slot v-else />
  </div>
</template>
```

- [ ] **Step 4: Run tests to verify they pass**

Run: `npx vitest run src/shared/components/__tests__/PageContainer.spec.ts`
Expected: 6 tests PASS

- [ ] **Step 5: Update barrel**

Modify `src/shared/components/layout/index.ts`:
```ts
export { default as PageContainer } from './PageContainer.vue'
```

- [ ] **Step 6: Commit**

```bash
git add src/shared/components/__tests__/PageContainer.spec.ts src/shared/components/layout/
git commit -m "feat: add PageContainer component with tests"
```

---

### Task 7: PageHeader Component

**Files:**
- Create: `src/shared/components/__tests__/PageHeader.spec.ts`
- Create: `src/shared/components/layout/PageHeader.vue`
- Modify: `src/shared/components/layout/index.ts`

**Interfaces:**
- Consumes: `vue-router` (useRouter)
- Produces: `PageHeader` — props: `{ title: string; description?: string; backTo?: string; backLabel?: string }`, slots: `{ default?(); actions?() }`

- [ ] **Step 1: Write the failing test**

Write `src/shared/components/__tests__/PageHeader.spec.ts`:
```ts
import { describe, it, expect, vi } from 'vitest'
import { mount } from '@vue/test-utils'
import PageHeader from '../layout/PageHeader.vue'

const pushMock = vi.fn()
const backMock = vi.fn()

vi.mock('vue-router', () => ({
  useRouter: () => ({ push: pushMock, back: backMock }),
}))

describe('PageHeader', () => {
  it('renders title', () => {
    const wrapper = mount(PageHeader, {
      props: { title: 'Products' },
      global: { stubs: { Button: true } },
    })
    expect(wrapper.text()).toContain('Products')
  })

  it('renders description', () => {
    const wrapper = mount(PageHeader, {
      props: { title: 'Products', description: 'Manage your catalog' },
      global: { stubs: { Button: true } },
    })
    expect(wrapper.text()).toContain('Manage your catalog')
  })

  it('renders actions slot', () => {
    const wrapper = mount(PageHeader, {
      props: { title: 'Products' },
      slots: { actions: '<button class="add-btn">Add New</button>' },
      global: { stubs: { Button: true } },
    })
    expect(wrapper.find('.add-btn').exists()).toBe(true)
    expect(wrapper.find('.add-btn').text()).toBe('Add New')
  })

  it('renders back button when backTo is set', () => {
    const wrapper = mount(PageHeader, {
      props: { title: 'Detail', backTo: '/list' },
      global: { stubs: { Button: true } },
    })
    const backBtn = wrapper.findComponent({ name: 'Button' })
    expect(backBtn.exists()).toBe(true)
  })

  it('navigates to backTo when back button clicked', async () => {
    const wrapper = mount(PageHeader, {
      props: { title: 'Detail', backTo: '/products' },
      global: { stubs: { Button: { template: '<button @click="$emit(\'click\')"><slot /></button>' } } },
    })
    await wrapper.find('button').trigger('click')
    expect(pushMock).toHaveBeenCalledWith('/products')
  })

  it('calls router.back when backTo is empty string and back button clicked', async () => {
    const wrapper = mount(PageHeader, {
      props: { title: 'Detail', backTo: '' },
      global: { stubs: { Button: { template: '<button @click="$emit(\'click\')"><slot /></button>' } } },
    })
    await wrapper.find('button').trigger('click')
    expect(backMock).toHaveBeenCalled()
  })

  it('renders default slot content below title', () => {
    const wrapper = mount(PageHeader, {
      props: { title: 'Order' },
      slots: { default: '<span class="badge">#1234</span>' },
      global: { stubs: { Button: true } },
    })
    expect(wrapper.find('.badge').exists()).toBe(true)
  })

  it('does not render back button when backTo is undefined', () => {
    const wrapper = mount(PageHeader, {
      props: { title: 'Dashboard' },
      global: { stubs: { Button: true } },
    })
    expect(wrapper.findComponent({ name: 'Button' }).exists()).toBe(false)
  })
})
```

- [ ] **Step 2: Run test to verify it fails**

Run: `npx vitest run src/shared/components/__tests__/PageHeader.spec.ts`
Expected: FAIL — cannot find module

- [ ] **Step 3: Write component**

Write `src/shared/components/layout/PageHeader.vue`:
```vue
<script setup lang="ts">
import { useRouter } from 'vue-router'

const router = useRouter()

defineProps<{
  title: string
  description?: string
  backTo?: string
  backLabel?: string
}>()

defineSlots<{
  default?(): any
  actions?(): any
}>()

function goBack(backTo?: string) {
  if (backTo) {
    router.push(backTo)
  } else {
    router.back()
  }
}
</script>

<template>
  <div class="flex flex-col items-start justify-between gap-4 mb-8 md:flex-row md:items-center">
    <div class="flex items-center gap-4">
      <Button
        v-if="backTo !== undefined"
        :icon="backLabel ? undefined : 'pi pi-arrow-left'"
        :label="backLabel"
        text
        rounded
        severity="secondary"
        @click="goBack(backTo)"
        class="shrink-0"
        style="background: var(--p-surface-100)"
      />
      <div>
        <h2 class="text-3xl font-black tracking-tight m-0" style="color: var(--p-text-color)">
          {{ title }}
        </h2>
        <div v-if="description || $slots.default" class="flex items-center gap-2 mt-1">
          <span v-if="description" style="color: var(--p-text-muted-color)">{{ description }}</span>
          <slot />
        </div>
      </div>
    </div>
    <div v-if="$slots.actions" class="flex items-center gap-3 shrink-0">
      <slot name="actions" />
    </div>
  </div>
</template>
```

- [ ] **Step 4: Run tests to verify they pass**

Run: `npx vitest run src/shared/components/__tests__/PageHeader.spec.ts`
Expected: 8 tests PASS

- [ ] **Step 5: Update barrel**

Modify `src/shared/components/layout/index.ts`:
```ts
export { default as PageContainer } from './PageContainer.vue'
export { default as PageHeader } from './PageHeader.vue'
```

- [ ] **Step 6: Commit**

```bash
git add src/shared/components/__tests__/PageHeader.spec.ts src/shared/components/layout/
git commit -m "feat: add PageHeader component with tests"
```

---

### Task 8: Section Component

**Files:**
- Create: `src/shared/components/__tests__/Section.spec.ts`
- Create: `src/shared/components/layout/Section.vue`
- Modify: `src/shared/components/layout/index.ts`

**Interfaces:**
- Consumes: nothing
- Produces: `Section` — props: `{ title?: string; description?: string; collapsible?: boolean; collapsed?: boolean }`, emits: `{ 'update:collapsed': [value: boolean] }`, slots: `{ default(); actions?() }`

- [ ] **Step 1: Write the failing test**

Write `src/shared/components/__tests__/Section.spec.ts`:
```ts
import { describe, it, expect } from 'vitest'
import { mount } from '@vue/test-utils'
import Section from '../layout/Section.vue'

describe('Section', () => {
  it('renders title', () => {
    const wrapper = mount(Section, {
      props: { title: 'Basic Information' },
      slots: { default: '<p>content</p>' },
    })
    expect(wrapper.text()).toContain('Basic Information')
  })

  it('renders description', () => {
    const wrapper = mount(Section, {
      props: { title: 'Info', description: 'Core product details' },
      slots: { default: '<p>content</p>' },
    })
    expect(wrapper.text()).toContain('Core product details')
  })

  it('renders slot content', () => {
    const wrapper = mount(Section, {
      props: { title: 'Section' },
      slots: { default: '<p class="content">Hello World</p>' },
    })
    expect(wrapper.find('.content').exists()).toBe(true)
  })

  it('renders actions slot', () => {
    const wrapper = mount(Section, {
      props: { title: 'Section' },
      slots: { default: '<p>content</p>', actions: '<button class="edit-btn">Edit</button>' },
    })
    expect(wrapper.find('.edit-btn').exists()).toBe(true)
  })

  it('does not render header when no title or actions', () => {
    const wrapper = mount(Section, {
      props: {},
      slots: { default: '<p>content</p>' },
    })
    expect(wrapper.find('.section-header').exists()).toBe(false)
  })
})
```

- [ ] **Step 2: Run test to verify it fails**

Run: `npx vitest run src/shared/components/__tests__/Section.spec.ts`
Expected: FAIL — cannot find module

- [ ] **Step 3: Write component**

Write `src/shared/components/layout/Section.vue`:
```vue
<script setup lang="ts">
defineProps<{
  title?: string
  description?: string
  collapsible?: boolean
  collapsed?: boolean
}>()

defineEmits<{
  'update:collapsed': [value: boolean]
}>()

defineSlots<{
  default(): any
  actions?(): any
}>()
</script>

<template>
  <section class="mb-8">
    <div v-if="title || $slots.actions" class="section-header flex items-center justify-between mb-4 pb-3 border-b" style="border-color: var(--p-surface-200)">
      <div class="flex items-center gap-3">
        <Button
          v-if="collapsible"
          :icon="collapsed ? 'pi pi-chevron-right' : 'pi pi-chevron-down'"
          text
          rounded
          size="small"
          severity="secondary"
          @click="$emit('update:collapsed', !collapsed)"
        />
        <div>
          <h3 v-if="title" class="text-lg font-semibold m-0" style="color: var(--p-text-color)">{{ title }}</h3>
          <p v-if="description" class="text-sm mt-1" style="color: var(--p-text-muted-color)">{{ description }}</p>
        </div>
      </div>
      <div v-if="$slots.actions" class="flex items-center gap-2">
        <slot name="actions" />
      </div>
    </div>
    <div v-show="!collapsible || !collapsed">
      <slot />
    </div>
  </section>
</template>
```

- [ ] **Step 4: Run tests to verify they pass**

Run: `npx vitest run src/shared/components/__tests__/Section.spec.ts`
Expected: 5 tests PASS

- [ ] **Step 5: Update barrel**

Modify `src/shared/components/layout/index.ts`:
```ts
export { default as PageContainer } from './PageContainer.vue'
export { default as PageHeader } from './PageHeader.vue'
export { default as Section } from './Section.vue'
```

- [ ] **Step 6: Commit**

```bash
git add src/shared/components/__tests__/Section.spec.ts src/shared/components/layout/
git commit -m "feat: add Section component with tests"
```

---

### Task 9: SearchInput Component

**Files:**
- Create: `src/shared/components/__tests__/SearchInput.spec.ts`
- Create: `src/shared/components/form/SearchInput.vue`
- Modify: `src/shared/components/form/index.ts`

**Interfaces:**
- Consumes: nothing
- Produces: `SearchInput` — props: `{ placeholder?: string; debounce?: number }`, model: `string`, emits: `{ search: [value: string] }`

- [ ] **Step 1: Write the failing test**

Write `src/shared/components/__tests__/SearchInput.spec.ts`:
```ts
import { describe, it, expect, vi } from 'vitest'
import { mount } from '@vue/test-utils'
import SearchInput from '../form/SearchInput.vue'

vi.useFakeTimers()

describe('SearchInput', () => {
  it('renders with default placeholder', () => {
    const wrapper = mount(SearchInput, {
      global: { stubs: { InputText: true, IconField: true } },
    })
    expect(wrapper.findComponent({ name: 'InputText' }).exists()).toBe(true)
  })

  it('renders with custom placeholder', () => {
    const wrapper = mount(SearchInput, {
      props: { placeholder: 'Find products...' },
      global: { stubs: { InputText: true, IconField: true } },
    })
    expect(wrapper.findComponent({ name: 'InputText' }).attributes('placeholder')).toBe('Find products...')
  })

  it('emits search after debounce', async () => {
    const wrapper = mount(SearchInput, {
      props: { debounce: 300 },
      global: { stubs: { InputText: { template: '<input @input="$emit(\'update:modelValue\', $event.target.value)" />', props: ['modelValue'], emits: ['update:modelValue'] }, IconField: { template: '<div><slot /></div>' } } },
    })

    await wrapper.find('input').setValue('test')

    vi.advanceTimersByTime(300)

    expect(wrapper.emitted('search')).toBeTruthy()
    expect(wrapper.emitted('search')![0]).toEqual(['test'])
  })

  it('does not emit search before debounce', async () => {
    const wrapper = mount(SearchInput, {
      props: { debounce: 300 },
      global: { stubs: { InputText: { template: '<input @input="$emit(\'update:modelValue\', $event.target.value)" />', props: ['modelValue'], emits: ['update:modelValue'] }, IconField: { template: '<div><slot /></div>' } } },
    })

    await wrapper.find('input').setValue('test')
    vi.advanceTimersByTime(100)

    expect(wrapper.emitted('search')).toBeFalsy()
  })
})
```

- [ ] **Step 2: Run test to verify it fails**

Run: `npx vitest run src/shared/components/__tests__/SearchInput.spec.ts`
Expected: FAIL — cannot find module

- [ ] **Step 3: Write component**

Write `src/shared/components/form/SearchInput.vue`:
```vue
<script setup lang="ts">
import { ref, watch } from 'vue'

const props = withDefaults(defineProps<{
  placeholder?: string
  debounce?: number
}>(), {
  placeholder: 'Search...',
  debounce: 300,
})

const model = defineModel<string>({ default: '' })
const emit = defineEmits<{
  search: [value: string]
}>()

let timer: ReturnType<typeof setTimeout> | null = null

watch(model, (val) => {
  if (timer) clearTimeout(timer)
  timer = setTimeout(() => {
    emit('search', val)
  }, props.debounce)
})

function clear() {
  model.value = ''
  emit('search', '')
}
</script>

<template>
  <IconField>
    <InputIcon class="pi pi-search" />
    <InputText
      v-model="model"
      :placeholder="placeholder"
      class="w-full min-w-64"
    />
    <InputIcon
      v-if="model"
      class="pi pi-times cursor-pointer"
      @click="clear"
    />
  </IconField>
</template>
```

- [ ] **Step 4: Run tests to verify they pass**

Run: `npx vitest run src/shared/components/__tests__/SearchInput.spec.ts`
Expected: 4 tests PASS

- [ ] **Step 5: Update barrel**

Modify `src/shared/components/form/index.ts`:
```ts
export { default as FormField } from './FormField.vue'
export { default as SearchInput } from './SearchInput.vue'
```

- [ ] **Step 6: Commit**

```bash
git add src/shared/components/__tests__/SearchInput.spec.ts src/shared/components/form/
git commit -m "feat: add SearchInput component with tests"
```

---

### Task 10: DetailGroup Component

**Files:**
- Create: `src/shared/components/__tests__/DetailGroup.spec.ts`
- Create: `src/shared/components/data-display/DetailGroup.vue`
- Modify: `src/shared/components/data-display/index.ts`

**Interfaces:**
- Consumes: `DetailField` (no import dependency — slots-based composition)
- Produces: `DetailGroup` — props: `{ title: string; columns?: 1 | 2 | 3 | 4 }`, slots: `{ default() }`

- [ ] **Step 1: Write the failing test**

Write `src/shared/components/__tests__/DetailGroup.spec.ts`:
```ts
import { describe, it, expect } from 'vitest'
import { mount } from '@vue/test-utils'
import DetailGroup from '../data-display/DetailGroup.vue'

describe('DetailGroup', () => {
  it('renders title', () => {
    const wrapper = mount(DetailGroup, {
      props: { title: 'General Information' },
      slots: { default: '<div>content</div>' },
    })
    expect(wrapper.text()).toContain('General Information')
  })

  it('renders slot content', () => {
    const wrapper = mount(DetailGroup, {
      props: { title: 'Details' },
      slots: { default: '<p class="field">Name: John</p>' },
    })
    expect(wrapper.find('.field').exists()).toBe(true)
  })

  it('applies grid columns class for 2 columns (default)', () => {
    const wrapper = mount(DetailGroup, {
      props: { title: 'Test' },
      slots: { default: '<div>test</div>' },
    })
    expect(wrapper.find('.grid').exists()).toBe(true)
    expect(wrapper.find('.grid').classes()).toContain('grid-cols-2')
  })

  it('applies grid columns class for 3 columns', () => {
    const wrapper = mount(DetailGroup, {
      props: { title: 'Test', columns: 3 },
      slots: { default: '<div>test</div>' },
    })
    expect(wrapper.find('.grid').classes()).toContain('grid-cols-3')
  })

  it('applies responsive columns for 4 columns', () => {
    const wrapper = mount(DetailGroup, {
      props: { title: 'Test', columns: 4 },
      slots: { default: '<div>test</div>' },
    })
    expect(wrapper.find('.grid').classes()).toContain('md:grid-cols-2')
    expect(wrapper.find('.grid').classes()).toContain('xl:grid-cols-4')
  })
})
```

- [ ] **Step 2: Run test to verify it fails**

Run: `npx vitest run src/shared/components/__tests__/DetailGroup.spec.ts`
Expected: FAIL — cannot find module

- [ ] **Step 3: Write component**

Write `src/shared/components/data-display/DetailGroup.vue`:
```vue
<script setup lang="ts">
import { computed } from 'vue'

const props = withDefaults(defineProps<{
  title: string
  columns?: 1 | 2 | 3 | 4
}>(), {
  columns: 2,
})

defineSlots<{
  default(): any
}>()

const gridClass = computed(() => {
  const map: Record<number, string> = {
    1: 'grid-cols-1',
    2: 'grid-cols-1 md:grid-cols-2',
    3: 'grid-cols-1 md:grid-cols-2 lg:grid-cols-3',
    4: 'grid-cols-1 md:grid-cols-2 xl:grid-cols-4',
  }
  return map[props.columns] ?? map[2]
})
</script>

<template>
  <div class="mb-8">
    <h3 class="text-lg font-semibold mb-4 pb-3 border-b m-0" style="color: var(--p-text-color); border-color: var(--p-surface-200)">
      {{ title }}
    </h3>
    <div class="grid gap-6" :class="gridClass">
      <slot />
    </div>
  </div>
</template>
```

- [ ] **Step 4: Run tests to verify they pass**

Run: `npx vitest run src/shared/components/__tests__/DetailGroup.spec.ts`
Expected: 5 tests PASS

- [ ] **Step 5: Update barrel**

Modify `src/shared/components/data-display/index.ts`:
```ts
export { default as DetailField } from './DetailField.vue'
export { default as DetailGroup } from './DetailGroup.vue'
```

- [ ] **Step 6: Commit**

```bash
git add src/shared/components/__tests__/DetailGroup.spec.ts src/shared/components/data-display/
git commit -m "feat: add DetailGroup component with tests"
```

---

### Task 11: DescriptionList Component

**Files:**
- Create: `src/shared/components/__tests__/DescriptionList.spec.ts`
- Create: `src/shared/components/data-display/DescriptionList.vue`
- Modify: `src/shared/components/data-display/index.ts`

**Interfaces:**
- Consumes: nothing
- Produces: `DescriptionList` — props: `{ items: { label: string; value: string | number; emptyText?: string }[]; columns?: 1 | 2 | 3 }`

- [ ] **Step 1: Write the failing test**

Write `src/shared/components/__tests__/DescriptionList.spec.ts`:
```ts
import { describe, it, expect } from 'vitest'
import { mount } from '@vue/test-utils'
import DescriptionList from '../data-display/DescriptionList.vue'

describe('DescriptionList', () => {
  it('renders items with labels and values', () => {
    const wrapper = mount(DescriptionList, {
      props: { items: [{ label: 'Order ID', value: '#1234' }, { label: 'Total', value: '$99.00' }] },
    })
    expect(wrapper.text()).toContain('Order ID')
    expect(wrapper.text()).toContain('#1234')
    expect(wrapper.text()).toContain('Total')
    expect(wrapper.text()).toContain('$99.00')
  })

  it('shows emptyText when value is empty string', () => {
    const wrapper = mount(DescriptionList, {
      props: { items: [{ label: 'Notes', value: '' }] },
    })
    expect(wrapper.text()).toContain('\u2014')
  })

  it('uses custom emptyText', () => {
    const wrapper = mount(DescriptionList, {
      props: { items: [{ label: 'Notes', value: '', emptyText: 'None' }] },
    })
    expect(wrapper.text()).toContain('None')
    expect(wrapper.text()).not.toContain('\u2014')
  })

  it('renders number zero as value', () => {
    const wrapper = mount(DescriptionList, {
      props: { items: [{ label: 'Count', value: 0 }] },
    })
    expect(wrapper.text()).toContain('0')
  })

  it('applies columns class', () => {
    const wrapper = mount(DescriptionList, {
      props: { items: [{ label: 'A', value: '1' }], columns: 3 },
    })
    expect(wrapper.find('dl').classes()).toContain('lg:grid-cols-3')
  })

  it('defaults to 2 columns', () => {
    const wrapper = mount(DescriptionList, {
      props: { items: [{ label: 'A', value: '1' }] },
    })
    expect(wrapper.find('dl').classes()).toContain('md:grid-cols-2')
  })

  it('renders empty when items array is empty', () => {
    const wrapper = mount(DescriptionList, {
      props: { items: [] },
    })
    expect(wrapper.find('dl').exists()).toBe(true)
    expect(wrapper.findAll('div').length).toBe(0)
  })
})
```

- [ ] **Step 2: Run test to verify it fails**

Run: `npx vitest run src/shared/components/__tests__/DescriptionList.spec.ts`
Expected: FAIL — cannot find module

- [ ] **Step 3: Write component**

Write `src/shared/components/data-display/DescriptionList.vue`:
```vue
<script setup lang="ts">
import { computed } from 'vue'

const props = withDefaults(defineProps<{
  items: { label: string; value: string | number; emptyText?: string }[]
  columns?: 1 | 2 | 3
}>(), {
  columns: 2,
})

const isEmpty = (val: string | number): boolean => {
  return val === '' || val === null || val === undefined
}

const gridClass = computed(() => {
  const map: Record<number, string> = {
    1: 'grid-cols-1',
    2: 'grid-cols-1 md:grid-cols-2',
    3: 'grid-cols-1 md:grid-cols-2 lg:grid-cols-3',
  }
  return map[props.columns] ?? map[2]
})
</script>

<template>
  <dl class="grid gap-4" :class="gridClass">
    <div v-for="(item, index) in items" :key="index" class="flex flex-col">
      <dt class="text-xs uppercase font-bold mb-1" style="color: var(--p-text-muted-color)">{{ item.label }}</dt>
      <dd class="text-sm font-medium m-0" style="color: var(--p-text-color)">
        {{ isEmpty(item.value) ? (item.emptyText ?? '\u2014') : item.value }}
      </dd>
    </div>
  </dl>
</template>
```

- [ ] **Step 4: Run tests to verify they pass**

Run: `npx vitest run src/shared/components/__tests__/DescriptionList.spec.ts`
Expected: 7 tests PASS

- [ ] **Step 5: Update barrel**

Modify `src/shared/components/data-display/index.ts`:
```ts
export { default as DetailField } from './DetailField.vue'
export { default as DetailGroup } from './DetailGroup.vue'
export { default as DescriptionList } from './DescriptionList.vue'
```

- [ ] **Step 6: Commit**

```bash
git add src/shared/components/__tests__/DescriptionList.spec.ts src/shared/components/data-display/
git commit -m "feat: add DescriptionList component with tests"
```

---

### Task 12: CopyButton Component

**Files:**
- Create: `src/shared/components/__tests__/CopyButton.spec.ts`
- Create: `src/shared/components/data-display/CopyButton.vue`
- Modify: `src/shared/components/data-display/index.ts`

**Interfaces:**
- Consumes: `navigator.clipboard`
- Produces: `CopyButton` — props: `{ value: string; label?: string; icon?: string; variant?: 'button' | 'link' }`

- [ ] **Step 1: Write the failing test**

Write `src/shared/components/__tests__/CopyButton.spec.ts`:
```ts
import { describe, it, expect, vi } from 'vitest'
import { mount } from '@vue/test-utils'
import CopyButton from '../data-display/CopyButton.vue'

describe('CopyButton', () => {
  const writeText = vi.fn().mockResolvedValue(undefined)

  beforeAll(() => {
    Object.defineProperty(navigator, 'clipboard', {
      value: { writeText },
      writable: true,
    })
  })

  beforeEach(() => {
    writeText.mockClear()
  })

  it('renders link variant by default', () => {
    const wrapper = mount(CopyButton, {
      props: { value: 'copy-me' },
    })
    expect(wrapper.find('button').exists()).toBe(true)
  })

  it('copies value to clipboard on click', async () => {
    const wrapper = mount(CopyButton, {
      props: { value: 'SKU-12345' },
    })
    await wrapper.find('button').trigger('click')
    expect(writeText).toHaveBeenCalledWith('SKU-12345')
  })

  it('shows tooltip with label', () => {
    const wrapper = mount(CopyButton, {
      props: { value: 'test', label: 'Copy ID' },
    })
    expect(wrapper.find('button').attributes('title')).toBe('Copy ID')
  })

  it('renders default icon pi-copy', () => {
    const wrapper = mount(CopyButton, {
      props: { value: 'test' },
    })
    expect(wrapper.find('i').classes()).toContain('pi-copy')
  })

  it('renders custom icon', () => {
    const wrapper = mount(CopyButton, {
      props: { value: 'test', icon: 'pi pi-link' },
    })
    expect(wrapper.find('i').classes()).toContain('pi-link')
  })
})
```

- [ ] **Step 2: Run test to verify it fails**

Run: `npx vitest run src/shared/components/__tests__/CopyButton.spec.ts`
Expected: FAIL — cannot find module

- [ ] **Step 3: Write component**

Write `src/shared/components/data-display/CopyButton.vue`:
```vue
<script setup lang="ts">
import { ref } from 'vue'

const props = withDefaults(defineProps<{
  value: string
  label?: string
  icon?: string
  variant?: 'button' | 'link'
}>(), {
  icon: 'pi pi-copy',
  variant: 'link',
})

const copied = ref(false)

async function copy() {
  try {
    await navigator.clipboard.writeText(props.value)
    copied.value = true
    setTimeout(() => {
      copied.value = false
    }, 2000)
  } catch {
    // clipboard API unavailable
  }
}
</script>

<template>
  <button
    type="button"
    :title="label ?? 'Copy'"
    class="inline-flex items-center gap-1 border-none bg-transparent cursor-pointer transition-opacity hover:opacity-70"
    :class="{ 'p-button p-button-text p-button-sm p-button-rounded': variant === 'button' }"
    style="color: var(--p-text-muted-color)"
    @click="copy"
  >
    <i :class="copied ? 'pi pi-check' : icon" class="text-xs" :style="{ color: copied ? 'var(--p-green-500)' : '' }" />
  </button>
</template>
```

- [ ] **Step 4: Run tests to verify they pass**

Run: `npx vitest run src/shared/components/__tests__/CopyButton.spec.ts`
Expected: 5 tests PASS

- [ ] **Step 5: Update barrel**

Modify `src/shared/components/data-display/index.ts`:
```ts
export { default as DetailField } from './DetailField.vue'
export { default as DetailGroup } from './DetailGroup.vue'
export { default as DescriptionList } from './DescriptionList.vue'
export { default as CopyButton } from './CopyButton.vue'
```

- [ ] **Step 6: Commit**

```bash
git add src/shared/components/__tests__/CopyButton.spec.ts src/shared/components/data-display/
git commit -m "feat: add CopyButton component with tests"
```

---

### Task 13: SkeletonLoader Component

**Files:**
- Create: `src/shared/components/__tests__/SkeletonLoader.spec.ts`
- Create: `src/shared/components/feedback/SkeletonLoader.vue`
- Modify: `src/shared/components/feedback/index.ts`

**Interfaces:**
- Consumes: PrimeVue `Skeleton`
- Produces: `SkeletonLoader` — props: `{ variant: 'table' | 'card' | 'form' | 'detail' | 'list'; rows?: number }`

- [ ] **Step 1: Write the failing test**

Write `src/shared/components/__tests__/SkeletonLoader.spec.ts`:
```ts
import { describe, it, expect } from 'vitest'
import { mount } from '@vue/test-utils'
import SkeletonLoader from '../feedback/SkeletonLoader.vue'

describe('SkeletonLoader', () => {
  it('renders table variant skeleton rows', () => {
    const wrapper = mount(SkeletonLoader, {
      props: { variant: 'table', rows: 3 },
      global: { stubs: { Skeleton: true } },
    })
    const skeletons = wrapper.findAllComponents({ name: 'Skeleton' })
    expect(skeletons.length).toBeGreaterThan(0)
  })

  it('renders card variant', () => {
    const wrapper = mount(SkeletonLoader, {
      props: { variant: 'card' },
      global: { stubs: { Skeleton: true } },
    })
    const skeletons = wrapper.findAllComponents({ name: 'Skeleton' })
    expect(skeletons.length).toBeGreaterThan(0)
  })

  it('renders form variant', () => {
    const wrapper = mount(SkeletonLoader, {
      props: { variant: 'form' },
      global: { stubs: { Skeleton: true } },
    })
    const skeletons = wrapper.findAllComponents({ name: 'Skeleton' })
    expect(skeletons.length).toBeGreaterThan(0)
  })

  it('renders detail variant', () => {
    const wrapper = mount(SkeletonLoader, {
      props: { variant: 'detail' },
      global: { stubs: { Skeleton: true } },
    })
    const skeletons = wrapper.findAllComponents({ name: 'Skeleton' })
    expect(skeletons.length).toBeGreaterThan(0)
  })

  it('renders list variant', () => {
    const wrapper = mount(SkeletonLoader, {
      props: { variant: 'list' },
      global: { stubs: { Skeleton: true } },
    })
    const skeletons = wrapper.findAllComponents({ name: 'Skeleton' })
    expect(skeletons.length).toBeGreaterThan(0)
  })

  it('defaults variant to table', () => {
    const wrapper = mount(SkeletonLoader, {
      props: {},
      global: { stubs: { Skeleton: true } },
    })
    expect(wrapper.find('.card').exists()).toBe(true)
  })
})
```

- [ ] **Step 2: Run test to verify it fails**

Run: `npx vitest run src/shared/components/__tests__/SkeletonLoader.spec.ts`
Expected: FAIL — cannot find module

- [ ] **Step 3: Write component**

Write `src/shared/components/feedback/SkeletonLoader.vue`:
```vue
<script setup lang="ts">
import { computed } from 'vue'

const props = withDefaults(defineProps<{
  variant?: 'table' | 'card' | 'form' | 'detail' | 'list'
  rows?: number
}>(), {
  variant: 'table',
})

const effectiveRows = computed(() => {
  if (props.rows !== undefined) return props.rows
  if (props.variant === 'table') return 5
  if (props.variant === 'list') return 3
  return 0
})

const tableCols = 4
const cardCols = 2
const formFields = 4
const detailFields = 4
</script>

<template>
  <div class="card">
    <!-- Table variant -->
    <template v-if="variant === 'table'">
      <div class="flex items-center gap-2 mb-4">
        <Skeleton width="16rem" height="2.5rem" />
      </div>
      <div v-for="i in effectiveRows" :key="i" class="flex items-center gap-4 mb-3">
        <Skeleton v-for="j in tableCols" :key="j" height="1.5rem" class="flex-1" />
      </div>
      <div class="flex justify-between mt-4">
        <Skeleton width="10rem" height="2rem" />
        <Skeleton width="16rem" height="2rem" />
      </div>
    </template>

    <!-- Card variant -->
    <template v-if="variant === 'card'">
      <div class="grid grid-cols-2 gap-6">
        <div v-for="i in 4" :key="i">
          <Skeleton width="3rem" height="3rem" borderRadius="50%" class="mb-3" />
          <Skeleton width="60%" height="1.5rem" class="mb-2" />
          <Skeleton width="40%" height="1rem" />
        </div>
      </div>
    </template>

    <!-- Form variant -->
    <template v-if="variant === 'form'">
      <div v-for="i in formFields" :key="i" class="mb-4">
        <Skeleton width="6rem" height="1rem" class="mb-2" />
        <Skeleton height="2.5rem" />
      </div>
    </template>

    <!-- Detail variant -->
    <template v-if="variant === 'detail'">
      <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
        <div v-for="i in detailFields" :key="i">
          <Skeleton width="5rem" height="0.75rem" class="mb-2" />
          <Skeleton width="8rem" height="1.5rem" />
        </div>
      </div>
    </template>

    <!-- List variant -->
    <template v-if="variant === 'list'">
      <div v-for="i in effectiveRows" :key="i" class="flex items-center gap-3 mb-3 pb-3 border-b" style="border-color: var(--p-surface-100)">
        <Skeleton width="2.5rem" height="2.5rem" borderRadius="50%" />
        <div class="flex-1">
          <Skeleton width="60%" height="1rem" class="mb-1" />
          <Skeleton width="40%" height="0.75rem" />
        </div>
      </div>
    </template>
  </div>
</template>
```

- [ ] **Step 4: Run tests to verify they pass**

Run: `npx vitest run src/shared/components/__tests__/SkeletonLoader.spec.ts`
Expected: 6 tests PASS

- [ ] **Step 5: Update barrel**

Modify `src/shared/components/feedback/index.ts`:
```ts
export { default as EmptyState } from './EmptyState.vue'
export { default as SkeletonLoader } from './SkeletonLoader.vue'
```

- [ ] **Step 6: Commit**

```bash
git add src/shared/components/__tests__/SkeletonLoader.spec.ts src/shared/components/feedback/
git commit -m "feat: add SkeletonLoader component with tests"
```

---

### Task 14: LoadingOverlay Component

**Files:**
- Create: `src/shared/components/__tests__/LoadingOverlay.spec.ts`
- Create: `src/shared/components/feedback/LoadingOverlay.vue`
- Modify: `src/shared/components/feedback/index.ts`

**Interfaces:**
- Consumes: nothing
- Produces: `LoadingOverlay` — props: `{ loading: boolean; message?: string }`, slots: `{ default() }`

- [ ] **Step 1: Write the failing test**

Write `src/shared/components/__tests__/LoadingOverlay.spec.ts`:
```ts
import { describe, it, expect } from 'vitest'
import { mount } from '@vue/test-utils'
import LoadingOverlay from '../feedback/LoadingOverlay.vue'

describe('LoadingOverlay', () => {
  it('renders slot content', () => {
    const wrapper = mount(LoadingOverlay, {
      props: { loading: false },
      slots: { default: '<p class="content">Data loaded</p>' },
    })
    expect(wrapper.find('.content').exists()).toBe(true)
  })

  it('shows overlay when loading is true', () => {
    const wrapper = mount(LoadingOverlay, {
      props: { loading: true },
      slots: { default: '<p>content</p>' },
    })
    expect(wrapper.find('.loading-overlay').exists()).toBe(true)
  })

  it('does not show overlay when loading is false', () => {
    const wrapper = mount(LoadingOverlay, {
      props: { loading: false },
      slots: { default: '<p>content</p>' },
    })
    expect(wrapper.find('.loading-overlay').exists()).toBe(false)
  })

  it('shows default spinner', () => {
    const wrapper = mount(LoadingOverlay, {
      props: { loading: true },
      slots: { default: '<p>content</p>' },
    })
    expect(wrapper.find('i.pi-spin').exists()).toBe(true)
  })

  it('shows message when provided', () => {
    const wrapper = mount(LoadingOverlay, {
      props: { loading: true, message: 'Saving changes...' },
      slots: { default: '<p>content</p>' },
    })
    expect(wrapper.text()).toContain('Saving changes...')
  })
})
```

- [ ] **Step 2: Run test to verify it fails**

Run: `npx vitest run src/shared/components/__tests__/LoadingOverlay.spec.ts`
Expected: FAIL — cannot find module

- [ ] **Step 3: Write component**

Write `src/shared/components/feedback/LoadingOverlay.vue`:
```vue
<script setup lang="ts">
withDefaults(defineProps<{
  loading: boolean
  message?: string
}>(), {
  loading: false,
})

defineSlots<{
  default(): any
}>()
</script>

<template>
  <div class="relative">
    <slot />
    <Transition name="fade">
      <div
        v-if="loading"
        class="loading-overlay absolute inset-0 z-10 flex flex-col items-center justify-center rounded-xl"
        style="background: color-mix(in srgb, var(--p-surface-overlay) 70%, transparent)"
      >
        <i class="pi pi-spin pi-spinner text-3xl mb-3" style="color: var(--p-primary-color)" />
        <p v-if="message" class="text-sm font-medium" style="color: var(--p-text-muted-color)">{{ message }}</p>
      </div>
    </Transition>
  </div>
</template>

<style scoped>
.fade-enter-active,
.fade-leave-active {
  transition: opacity 0.2s ease;
}
.fade-enter-from,
.fade-leave-to {
  opacity: 0;
}
</style>
```

- [ ] **Step 4: Run tests to verify they pass**

Run: `npx vitest run src/shared/components/__tests__/LoadingOverlay.spec.ts`
Expected: 5 tests PASS

- [ ] **Step 5: Update barrel**

Modify `src/shared/components/feedback/index.ts`:
```ts
export { default as EmptyState } from './EmptyState.vue'
export { default as SkeletonLoader } from './SkeletonLoader.vue'
export { default as LoadingOverlay } from './LoadingOverlay.vue'
```

- [ ] **Step 6: Commit**

```bash
git add src/shared/components/__tests__/LoadingOverlay.spec.ts src/shared/components/feedback/
git commit -m "feat: add LoadingOverlay component with tests"
```

---

### Task 15: DeleteDialog Component

**Files:**
- Create: `src/shared/components/__tests__/DeleteDialog.spec.ts`
- Create: `src/shared/components/overlays/DeleteDialog.vue`
- Modify: `src/shared/components/overlays/index.ts`

**Interfaces:**
- Consumes: PrimeVue `Dialog`, `Button`
- Produces: `DeleteDialog` — props: `{ entityName: string; warningText?: string; loading?: boolean; visible: boolean }`, emits: `{ confirm: []; cancel: []; 'update:visible': [value: boolean] }`

- [ ] **Step 1: Write the failing test**

Write `src/shared/components/__tests__/DeleteDialog.spec.ts`:
```ts
import { describe, it, expect } from 'vitest'
import { mount } from '@vue/test-utils'
import DeleteDialog from '../overlays/DeleteDialog.vue'

describe('DeleteDialog', () => {
  it('renders entity name in warning', () => {
    const wrapper = mount(DeleteDialog, {
      props: { entityName: 'Order #1234', visible: true },
      global: { stubs: { Dialog: { template: '<div v-if="visible"><slot /><slot name="footer" /></div>', props: ['visible'] }, Button: true } },
    })
    expect(wrapper.text()).toContain('Order #1234')
  })

  it('shows default warning text', () => {
    const wrapper = mount(DeleteDialog, {
      props: { entityName: 'this item', visible: true },
      global: { stubs: { Dialog: { template: '<div v-if="visible"><slot /><slot name="footer" /></div>', props: ['visible'] }, Button: true } },
    })
    expect(wrapper.text()).toContain('cannot be undone')
  })

  it('shows custom warning text', () => {
    const wrapper = mount(DeleteDialog, {
      props: { entityName: 'Category', visible: true, warningText: 'All products in this category will be unlinked.' },
      global: { stubs: { Dialog: { template: '<div v-if="visible"><slot /><slot name="footer" /></div>', props: ['visible'] }, Button: true } },
    })
    expect(wrapper.text()).toContain('All products in this category will be unlinked.')
  })

  it('emits confirm when delete button clicked', async () => {
    const wrapper = mount(DeleteDialog, {
      props: { entityName: 'item', visible: true },
      global: { stubs: { Dialog: { template: '<div v-if="visible"><slot /><slot name="footer" :confirm="() => $emit(\'confirm\')" /></div>', props: ['visible'] }, Button: { template: '<button @click="$emit(\'click\')"><slot /></button>' } } },
    })

    const buttons = wrapper.findAll('button')
    const deleteBtn = buttons.find(b => b.text().includes('Delete'))
    expect(deleteBtn).toBeDefined()
    if (deleteBtn) {
      await deleteBtn.trigger('click')
      expect(wrapper.emitted('confirm')).toBeTruthy()
    }
  })

  it('emits cancel when cancel button clicked', async () => {
    const wrapper = mount(DeleteDialog, {
      props: { entityName: 'item', visible: true },
      global: { stubs: { Dialog: { template: '<div v-if="visible"><slot /><slot name="footer" /></div>', props: ['visible'] }, Button: { template: '<button @click="$emit(\'click\')"><slot /></button>' } } },
    })

    const buttons = wrapper.findAll('button')
    const cancelBtn = buttons.find(b => b.text().includes('Cancel'))
    expect(cancelBtn).toBeDefined()
    if (cancelBtn) {
      await cancelBtn.trigger('click')
      expect(wrapper.emitted('cancel')).toBeTruthy()
    }
  })

  it('shows loading state on delete button', () => {
    const wrapper = mount(DeleteDialog, {
      props: { entityName: 'item', visible: true, loading: true },
      global: { stubs: { Dialog: { template: '<div v-if="visible"><slot /><slot name="footer" /></div>', props: ['visible'] }, Button: true } },
    })
    expect(wrapper.text()).toContain('Deleting')
  })
})
```

- [ ] **Step 2: Run test to verify it fails**

Run: `npx vitest run src/shared/components/__tests__/DeleteDialog.spec.ts`
Expected: FAIL — cannot find module

- [ ] **Step 3: Write component**

Write `src/shared/components/overlays/DeleteDialog.vue`:
```vue
<script setup lang="ts">
withDefaults(defineProps<{
  entityName: string
  warningText?: string
  loading?: boolean
  visible: boolean
}>(), {
  warningText: 'This action cannot be undone.',
  loading: false,
})

const emit = defineEmits<{
  confirm: []
  cancel: []
  'update:visible': [value: boolean]
}>()

function onCancel() {
  emit('cancel')
  emit('update:visible', false)
}
</script>

<template>
  <Dialog
    :visible="visible"
    :modal="true"
    :closable="!loading"
    header="Delete Confirmation"
    :style="{ width: '450px' }"
    @update:visible="emit('update:visible', $event)"
    @hide="onCancel"
  >
    <div class="flex flex-col gap-4">
      <div class="flex items-center gap-3">
        <i class="pi pi-exclamation-triangle text-2xl" style="color: var(--p-yellow-500)" />
        <p class="m-0 text-sm" style="color: var(--p-text-color)">
          Are you sure you want to delete <strong>{{ entityName }}</strong>?
        </p>
      </div>
      <p class="m-0 text-sm" style="color: var(--p-text-muted-color)">{{ warningText }}</p>
    </div>
    <template #footer>
      <div class="flex gap-2 justify-end">
        <Button
          label="Cancel"
          severity="secondary"
          text
          :disabled="loading"
          @click="onCancel"
        />
        <Button
          :label="loading ? 'Deleting...' : 'Delete'"
          severity="danger"
          :loading="loading"
          :disabled="loading"
          @click="$emit('confirm')"
        />
      </div>
    </template>
  </Dialog>
</template>
```

- [ ] **Step 4: Run tests to verify they pass**

Run: `npx vitest run src/shared/components/__tests__/DeleteDialog.spec.ts`
Expected: 6 tests PASS

- [ ] **Step 5: Update barrel**

Modify `src/shared/components/overlays/index.ts`:
```ts
export { default as ConfirmButton } from './ConfirmButton.vue'
export { default as DeleteDialog } from './DeleteDialog.vue'
```

- [ ] **Step 6: Commit**

```bash
git add src/shared/components/__tests__/DeleteDialog.spec.ts src/shared/components/overlays/
git commit -m "feat: add DeleteDialog component with tests"
```

---

### Task 16: Final Verification

**Files:**
- Verify: all barrel exports resolve correctly
- Verify: all tests pass
- Verify: TypeScript type-checks

- [ ] **Step 1: Run all component tests**

Run: `npx vitest run src/shared/components/__tests__/`
Expected: all 70+ tests PASS across all 12 test files

- [ ] **Step 2: Verify barrel exports work end-to-end**

Create a temporary check file to test root barrel import:

Run: `echo "import { PageHeader, ConfirmButton, DetailField, FormField, EmptyState, SkeletonLoader, LoadingOverlay, SearchInput, PageContainer, Section, DetailGroup, DescriptionList, CopyButton, DeleteDialog } from '@/shared/components'" > /tmp/check-imports.ts && npx vue-tsc --noEmit 2>&1 | head -5`

- [ ] **Step 3: Run full TypeScript check**

Run: `npx vue-tsc --noEmit 2>&1 | tail -5`
Expected: no type errors

- [ ] **Step 4: Run build**

Run: `pnpm run build 2>&1 | tail -5`
Expected: build succeeds

- [ ] **Step 5: Commit final state**

```bash
git add src/shared/components/
git commit -m "feat: complete P0 shared component library (14 components, 12 test files)"
```
