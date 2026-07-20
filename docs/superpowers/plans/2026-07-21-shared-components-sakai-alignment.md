# Shared Components Sakai Alignment — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Refine 15 existing shared components with tailwindcss-primeui tokens and Sakai-lean patterns, add 3 new high-value compositions, and migrate ~100 duplicated inline patterns across 12 features to shared components.

**Architecture:** Sakai-lean wrappers — no base input wrappers, only high-value compositions. Semantic tokens (`text-muted-color`, `bg-primary`, `border-surface`, `surface-0`, `dark:*`) replace hardcoded CSS throughout `shared/components/`. Three new components (ConfirmDialog, ModalDialog, CompactTable) added to cover the biggest duplication gaps. Six adoption patterns migrate feature-level inline code to shared components.

**Tech Stack:** Vue 3 + TypeScript, PrimeVue 4 (auto-import), tailwindcss-primeui 0.6.1, Vitest, vue-tsc

## Global Constraints

- No new npm dependencies (tailwindcss-primeui already installed)
- No base input wrappers (no BaseButton, BaseInput, BaseSelect)
- No router, store, API, or CSS layer changes
- Tests colocated in `__tests__/` directories
- Every task passes `pnpm run type-check` and `pnpm run test:unit` before commit
- Commit after each task with conventional commit format

---

### Task 1: Scaffold overlays/ + barrel exports + file moves

**Files:**
- Create: `app/Admin/src/shared/components/overlays/` (empty directory)
- Create: `app/Admin/src/shared/components/index.ts`
- Move: `app/Admin/src/shared/components/base/ConfirmButton.vue` → `app/Admin/src/shared/components/overlays/ConfirmDialog.vue`
- Move: `app/Admin/src/shared/components/base/__tests__/ConfirmButton.test.ts` → `app/Admin/src/shared/components/overlays/__tests__/ConfirmDialog.test.ts`
- Move: `app/Admin/src/shared/components/navigation/ManagerWelcome.vue` → `app/Admin/src/shared/components/feedback/ManagerWelcome.vue`
- Delete: `app/Admin/src/shared/components/base/` directory
- Modify: `app/Admin/eslint.config.ts` — add overlays/ to shared layer boundary

**Interfaces:**
- Produces: `overlays/` directory tree, `shared/components/index.ts` barrel, eslint boundary updated

- [ ] **Step 1: Create overlays directory, move files, remove base/**

```bash
mkdir -p app/Admin/src/shared/components/overlays/__tests__
git mv app/Admin/src/shared/components/base/ConfirmButton.vue app/Admin/src/shared/components/overlays/ConfirmDialog.vue
git mv app/Admin/src/shared/components/base/__tests__/ConfirmButton.test.ts app/Admin/src/shared/components/overlays/__tests__/ConfirmDialog.test.ts
git mv app/Admin/src/shared/components/navigation/ManagerWelcome.vue app/Admin/src/shared/components/feedback/ManagerWelcome.vue
rmdir app/Admin/src/shared/components/base/__tests__
rmdir app/Admin/src/shared/components/base/
```

- [ ] **Step 2: Update all imports referencing the old paths**

```bash
# Update imports for ConfirmButton → ConfirmDialog (in features)
rg -l "shared/components/base/ConfirmButton" app/Admin/src/features/ | xargs sed -i "s|@/shared/components/base/ConfirmButton|@/shared/components/overlays/ConfirmDialog|g"
# Update imports for ManagerWelcome (in features)
rg -l "shared/components/navigation/ManagerWelcome" app/Admin/src/features/ | xargs sed -i "s|@/shared/components/navigation/ManagerWelcome|@/shared/components/feedback/ManagerWelcome|g"
```

- [ ] **Step 3: Create barrel export**

```ts
// shared/components/index.ts
export { default as Breadcrumb } from './navigation/Breadcrumb.vue'
export { default as CompactTable } from './tables/CompactTable.vue'
export { default as ConfirmDialog } from './overlays/ConfirmDialog.vue'
export { default as DataTableShell } from './tables/DataTableShell.vue'
export { default as DetailField } from './data-display/DetailField.vue'
export { default as Drawer } from './feedback/Drawer.vue'
export { default as EmptyState } from './feedback/EmptyState.vue'
export { default as ErrorState } from './feedback/ErrorState.vue'
export { default as FormField } from './form/FormField.vue'
export { default as ManagerWelcome } from './feedback/ManagerWelcome.vue'
export { default as MetadataManager } from './data-display/MetadataManager.vue'
export { default as ModalDialog } from './overlays/ModalDialog.vue'
export { default as PageHeader } from './navigation/PageHeader.vue'
export { default as PageShell } from './navigation/PageShell.vue'
export { default as StatCard } from './data-display/StatCard.vue'
export { default as StatusBadge } from './feedback/StatusBadge.vue'
export { default as TabbedDetail } from './data-display/TabbedDetail.vue'
export type { ColumnDef } from './tables/DataTableShell.vue'
```

- [ ] **Step 4: Add overlays boundary to ESLint config**

Read `app/Admin/eslint.config.ts`, find the shared layer block that references `base/`, and replace `'src/shared/components/base/**'` with `'src/shared/components/overlays/**'`.

- [ ] **Step 5: Verify type-check + tests**

```bash
pnpm run type-check
pnpm run test:unit
```

- [ ] **Step 6: Commit**

```bash
git add -A app/Admin/
git commit -m "refactor(admin): scaffold overlays/, barrel exports, move ConfirmDialog and ManagerWelcome"
```

---

### Task 2: Rename ConfirmButton internals to ConfirmDialog + enhance with slot trigger

**Files:**
- Modify: `app/Admin/src/shared/components/overlays/ConfirmDialog.vue`
- Modify: `app/Admin/src/shared/components/overlays/__tests__/ConfirmDialog.test.ts`

**Interfaces:**
- Consumes: moved file from Task 1
- Produces: `ConfirmDialog` component with slot-based trigger, `confirm` + `cancel` emits

- [ ] **Step 1: Rewrite test for slot-based ConfirmDialog**

```ts
// overlays/__tests__/ConfirmDialog.test.ts
import { describe, it, expect, vi } from 'vitest'
import { mount } from '@vue/test-utils'
import ConfirmDialog from '../ConfirmDialog.vue'
import ConfirmDialogPrime from 'primevue/confirmdialog'
import { useConfirm } from 'primevue/useconfirm'

vi.mock('primevue/useconfirm', () => ({
  useConfirm: vi.fn(() => ({
    require: vi.fn((opts: any) => {
      // last call will be used for assertions
      ;(useConfirm as any)._lastOpts = opts
      // call accept callback to simulate confirmation
      opts.accept?.()
    }),
  })),
}))
;(useConfirm as any)._lastOpts = null

describe('ConfirmDialog', () => {
  it('renders default slot as trigger', () => {
    const wrapper = mount(ConfirmDialog, {
      global: { stubs: { ConfirmDialogPrime: true, Button: true } },
      props: { header: 'Delete', message: 'Sure?' },
      slots: { default: '<button class="my-trigger">Delete</button>' },
    })
    expect(wrapper.html()).toContain('my-trigger')
  })

  it('emits confirm when accept clicked', async () => {
    const wrapper = mount(ConfirmDialog, {
      global: { stubs: { ConfirmDialogPrime: true, Button: true } },
      props: { header: 'Delete', message: 'Sure?' },
      slots: { default: '<button>X</button>' },
    })
    await wrapper.find('button').trigger('click')
    await wrapper.vm.$nextTick()
    expect((useConfirm as any)._lastOpts.header).toBe('Delete')
    expect(wrapper.emitted('confirm')).toBeTruthy()
  })

  it('uses default severity and icon when not provided', () => {
    const wrapper = mount(ConfirmDialog, {
      global: { stubs: { ConfirmDialogPrime: true, Button: true } },
      props: { header: 'Title', message: 'Msg' },
      slots: { default: '<button>X</button>' },
    })
    // defaults: severity='danger', icon='pi pi-trash', acceptLabel='Confirm'
    expect(wrapper.props('severity')).toBe('danger')
  })
})
```

Run: `pnpm run test:unit -- ConfirmDialog`
Expected: 3 FAIL (component not yet updated)

- [ ] **Step 2: Rewrite ConfirmDialog.vue with slot-based trigger**

```vue
<!-- overlays/ConfirmDialog.vue -->
<script setup lang="ts">
import { useConfirm } from 'primevue/useconfirm'

const confirm = useConfirm()

const props = withDefaults(defineProps<{
  icon?: string
  severity?: string
  header: string
  message: string
  acceptLabel?: string
  rejectLabel?: string
  loading?: boolean
}>(), {
  icon: 'pi pi-trash',
  severity: 'danger',
  acceptLabel: 'Confirm',
  rejectLabel: 'Cancel',
  loading: false,
})

const emit = defineEmits<{
  confirm: []
  cancel: []
}>()

function open() {
  confirm.require({
    message: props.message,
    header: props.header,
    icon: 'pi pi-exclamation-triangle',
    rejectLabel: props.rejectLabel,
    acceptProps: {
      label: props.acceptLabel,
      severity: props.severity,
    },
    rejectProps: {
      label: props.rejectLabel,
      severity: 'secondary',
      outlined: true,
    },
    accept: () => emit('confirm'),
    reject: () => emit('cancel'),
  })
}
</script>

<template>
  <ConfirmDialogPrime />
  <Button :icon="icon" :severity="severity" :loading="loading" rounded text @click="open">
    <slot />
  </Button>
</template>
```

- [ ] **Step 3: Run tests**

```bash
pnpm run test:unit -- ConfirmDialog
```
Expected: 3 PASS

- [ ] **Step 4: Verify type-check**

```bash
pnpm run type-check
```

- [ ] **Step 5: Commit**

```bash
git add app/Admin/src/shared/components/overlays/
git commit -m "refactor(admin): rename ConfirmButton to ConfirmDialog with slot-based trigger"
```

---

### Task 3: Token-migrate + refine Feedback group

**Files:**
- Modify: `app/Admin/src/shared/components/feedback/EmptyState.vue`
- Modify: `app/Admin/src/shared/components/feedback/ErrorState.vue`
- Modify: `app/Admin/src/shared/components/feedback/Drawer.vue`
- Modify: `app/Admin/src/shared/components/feedback/StatusBadge.vue`
- Modify: `app/Admin/src/shared/components/feedback/ManagerWelcome.vue`
- Modify: `app/Admin/src/shared/components/feedback/__tests__/Drawer.test.ts`

**Interfaces:**
- Produces: All 5 feedback components with tailwindcss-primeui tokens

- [ ] **Step 1: Token-migrate EmptyState.vue**

Replace in `EmptyState.vue`:

```
class="flex flex-col items-center justify-center py-20 text-surface-400"
```
→
```
class="flex flex-col items-center justify-center py-20 text-muted-color"
```

Replace `actionRoute?: any` → `actionRoute?: RouteLocationRaw`. Add import: `import type { RouteLocationRaw } from 'vue-router'`.

- [ ] **Step 2: Token-migrate ErrorState.vue**

Already uses `text-muted-color` and `text-surface-700 dark:text-surface-300`. Confirm no changes needed.

- [ ] **Step 3: Refine Drawer.vue — replace manual v-model with defineModel**

Replace the entire `<script setup>` block:

```vue
<script setup lang="ts">
const visible = defineModel<boolean>({ required: true })

withDefaults(defineProps<{
  header?: string
  position?: 'left' | 'right' | 'top' | 'bottom'
  width?: string
}>(), {
  position: 'right',
  width: '30rem',
})
</script>

<template>
  <Drawer v-model:visible="visible" :header="header" :position="position" :style="{ width }">
    <slot />
  </Drawer>
</template>
```

Drop the local `ref` / `watch` / emit blocks and the `import { ref, watch } from 'vue'` line.

- [ ] **Step 4: Refine StatusBadge.vue — add label/severity fallback, tokenize classes**

```vue
<!-- overlaps/StatusBadge.vue — tokenized version -->
<script setup lang="ts">
import { computed } from 'vue'

const props = withDefaults(defineProps<{
  status?: string | number
  statusMap?: Record<string | number, { label: string; severity: string }>
  label?: string
  severity?: string
  size?: 'small' | 'normal'
}>(), {
  size: 'normal',
  severity: 'info',
})

const resolved = computed(() => {
  if (props.label) return { label: props.label, severity: props.severity }
  if (props.status !== undefined && props.statusMap) {
    return props.statusMap[props.status] ?? { label: String(props.status), severity: 'secondary' }
  }
  return { label: props.label ?? '', severity: props.severity }
})
</script>

<template>
  <Tag
    :value="resolved.label"
    :severity="resolved.severity"
    :class="size === 'normal' ? 'px-4 py-2 font-bold rounded-border' : ''"
    rounded
  />
</template>
```

Changes:
- `status` prop → optional (`status?`)
- `statusMap` prop → optional
- Add `label?` and `severity?` props for label-only mode
- `rounded-xl` → `rounded-border`
- Remove `as any` cast on severity

- [ ] **Step 5: Token-migrate ManagerWelcome.vue**

Replace `text-surface-500` → `text-muted-color` in ManagerWelcome.vue.

- [ ] **Step 6: Update Drawer test for defineModel**

In `Drawer.test.ts`, update the test to use `props: { modelValue: true }` since `defineModel` uses `modelValue`/`update:modelValue`.

```ts
// Before: props: { visible: true }
// After:
props: { modelValue: true },
```

- [ ] **Step 7: Run tests**

```bash
pnpm run test:unit -- EmptyState ErrorState Drawer StatusBadge
```

- [ ] **Step 8: Type-check + commit**

```bash
pnpm run type-check
git add app/Admin/src/shared/components/feedback/
git commit -m "refactor(admin): token-migrate feedback components, refine StatusBadge and Drawer"
```

---

### Task 4: Token-migrate + refine Navigation group

**Files:**
- Modify: `app/Admin/src/shared/components/navigation/PageShell.vue`
- Modify: `app/Admin/src/shared/components/navigation/PageHeader.vue`
- Modify: `app/Admin/src/shared/components/navigation/Breadcrumb.vue`

**Interfaces:**
- Produces: All 3 navigation components with tailwindcss-primeui tokens

- [ ] **Step 1: Token-migrate PageShell.vue**

```vue
<!-- PageShell.vue — after -->
<script setup lang="ts">
withDefaults(defineProps<{
  maxWidth?: '2xl' | '4xl' | '6xl' | '7xl' | 'none'
  card?: boolean
  gap?: boolean
}>(), {
  maxWidth: 'none',
  card: true,
  gap: false,
})

const MAX_WIDTH_CLASS: Record<string, string> = {
  '2xl': 'max-w-2xl mx-auto',
  '4xl': 'max-w-4xl mx-auto',
  '6xl': 'max-w-6xl mx-auto',
  '7xl': 'max-w-7xl mx-auto',
}
</script>

<template>
  <div
    class="p-6"
    :class="[
      MAX_WIDTH_CLASS[maxWidth] || '',
      !card && gap ? 'flex flex-col gap-6 bg-surface-0 dark:bg-surface-950' : '',
    ]"
  >
    <Card v-if="card" class="!mb-0">
      <template #content>
        <slot />
      </template>
    </Card>
    <slot v-else />
  </div>
</template>
```

Changes:
- Add `!mb-0` to Card (Sakai pattern)
- Add `bg-surface-0 dark:bg-surface-950` for `card=false` + `gap=true` mode for dark mode

- [ ] **Step 2: Token-migrate PageHeader.vue**

PageHeader already uses `text-surface-900 dark:text-surface-50`, `text-surface-500 dark:text-surface-400`, `bg-surface-100 dark:bg-surface-800`. Replace `text-surface-500` → `text-muted-color`:

```
<span v-if="description" class="text-surface-500 dark:text-surface-400">{{ description }}</span>
```
→
```
<span v-if="description" class="text-muted-color">{{ description }}</span>
```

- [ ] **Step 3: Token-migrate Breadcrumb.vue**

Read Breadcrumb.vue, replace any `text-surface-*` → `text-muted-color` / `text-color-emphasis` as appropriate.

- [ ] **Step 4: Verify**

```bash
pnpm run type-check
pnpm run test:unit
```

- [ ] **Step 5: Commit**

```bash
git add app/Admin/src/shared/components/navigation/
git commit -m "refactor(admin): token-migrate navigation components"
```

---

### Task 5: Token-migrate + refine Form + Tables group

**Files:**
- Modify: `app/Admin/src/shared/components/form/FormField.vue`
- Modify: `app/Admin/src/shared/components/tables/DataTableShell.vue`
- Modify: `app/Admin/src/shared/components/tables/__tests__/DataTableShell.test.ts`

**Interfaces:**
- Produces: FormField with tokens + description slot, DataTableShell with EmptyState reuse + rounded-border tokens

- [ ] **Step 1: Token-migrate + refine FormField.vue**

```vue
<!-- FormField.vue — after -->
<script setup lang="ts">
withDefaults(defineProps<{
  label: string
  name: string
  error?: string
  required?: boolean
  hint?: string
}>(), {
  required: false,
})

defineSlots<{
  default(): any
  description(): any
}>()
</script>

<template>
  <div class="flex flex-col gap-2">
    <div class="flex items-center gap-2">
      <label
        :for="name"
        class="font-bold text-xs uppercase tracking-wider text-muted-color ml-1"
      >
        {{ label }}
        <span v-if="required" class="text-red-500">*</span>
      </label>
      <slot name="description" />
    </div>
    <slot />
    <small v-if="error" class="p-error">{{ error }}</small>
    <small v-else-if="hint" class="text-muted-color">{{ hint }}</small>
  </div>
</template>
```

Changes:
- `text-surface-500` → `text-muted-color` on label
- `text-surface-400` → `text-muted-color` on hint
- Add flex row wrapper around label + description slot
- Add `#description` slot

- [ ] **Step 2: Token-migrate DataTableShell.vue**

Replace inline classes in DataTableShell:

```
class="rounded-xl"
```
→
```
class="rounded-border"
```
(found 4 times on Button components in the toolbar)

Replace empty state (lines 148-153) with reuse of EmptyState component:

```vue
<template #empty>
  <slot name="empty">
    <EmptyState
      :icon="emptyIcon"
      :title="emptyTitle"
      :description="emptyDescription"
    />
  </slot>
</template>
```

Add import: `import EmptyState from '@/shared/components/feedback/EmptyState.vue'`

Replace loading skeleton (lines 157-159):
```
<div class="p-4">
  <Skeleton v-for="i in skeletonRows.length" :key="i" class="mb-3" height="2.5rem" />
</div>
```
→
```
<div class="p-4 space-y-3">
  <Skeleton v-for="i in skeletonRows.length" :key="i" height="2.5rem" class="bg-surface-100 dark:bg-surface-800" />
</div>
```

- [ ] **Step 3: Verify**

```bash
pnpm run type-check
pnpm run test:unit -- DataTableShell FormField
```

- [ ] **Step 4: Commit**

```bash
git add app/Admin/src/shared/components/form/ app/Admin/src/shared/components/tables/
git commit -m "refactor(admin): token-migrate FormField and DataTableShell, EmptyState reuse"
```

---

### Task 6: Token-migrate Data-Display group + barrels

**Files:**
- Modify: `app/Admin/src/shared/components/data-display/DetailField.vue`
- Modify: `app/Admin/src/shared/components/data-display/StatCard.vue`
- Modify: `app/Admin/src/shared/components/data-display/MetadataManager.vue`
- Modify: `app/Admin/src/shared/components/data-display/TabbedDetail.vue`

**Interfaces:**
- Produces: All 4 data-display components with tokens

- [ ] **Step 1: Token-migrate DetailField.vue**

```vue
<!-- DetailField.vue — after -->
<script setup lang="ts">
withDefaults(defineProps<{
  label: string
  value?: string | number | null
  emptyText?: string
}>(), {
  emptyText: '\u2014',
})
</script>

<template>
  <div class="flex flex-col">
    <span class="text-xs text-muted-color uppercase font-bold mb-1">{{ label }}</span>
    <span v-if="value !== null && value !== undefined && value !== ''" class="text-lg font-medium text-color dark:text-surface-0">
      {{ value }}
    </span>
    <span v-else class="text-lg text-muted-color">{{ emptyText }}</span>
  </div>
</template>
```

Changes:
- `text-surface-400` → `text-muted-color`
- `text-surface-900 dark:text-surface-0` → `text-color dark:text-surface-0`
- `text-surface-300 dark:text-surface-600` → `text-muted-color`

- [ ] **Step 2: Token-migrate StatCard.vue**

In StatCard.vue, make these replacements:

| Line | Old | New |
|---|---|---|
| Title span | `text-sm text-surface-500` | `text-sm text-muted-color` |
| Value span (already good) | `text-2xl font-black text-surface-900 dark:text-surface-0` | keep |
| Card wrapper | `card !mb-0 flex flex-col gap-4 p-6` | keep (already uses `!mb-0`) |
| Skeleton bg | n/a | add `bg-surface-100 dark:bg-surface-800` class |

Trend colors (`text-green-500`, `text-red-500`) — keep these as intent colors.

- [ ] **Step 3: Token-migrate MetadataManager.vue**

Replace in MetadataManager.vue:
- `text-surface-400` → `text-muted-color` (empty message, descriptions)
- `rounded-lg` / `rounded-xl` → `rounded-border` (input fields, buttons)

- [ ] **Step 4: Token-migrate TabbedDetail.vue**

Replace in TabbedDetail.vue:
- `text-surface-500` → `text-muted-color`
- `rounded-lg` on tab panels → `rounded-border`

- [ ] **Step 5: Verify**

```bash
pnpm run type-check
pnpm run test:unit -- DetailField StatCard
```

- [ ] **Step 6: Commit**

```bash
git add app/Admin/src/shared/components/data-display/
git commit -m "refactor(admin): token-migrate data-display components"
```

---

### Task 7: New components — ModalDialog + CompactTable

**Files:**
- Create: `app/Admin/src/shared/components/overlays/ModalDialog.vue`
- Create: `app/Admin/src/shared/components/overlays/__tests__/ModalDialog.test.ts`
- Create: `app/Admin/src/shared/components/tables/CompactTable.vue`
- Create: `app/Admin/src/shared/components/tables/__tests__/CompactTable.test.ts`
- Modify: `app/Admin/src/shared/components/index.ts` (add exports for ModalDialog, CompactTable)

**Interfaces:**
- Produces: ModalDialog (defineModel-based Dialog wrapper), CompactTable (lightweight DataTable)

- [ ] **Step 1: Write failing ModalDialog test**

```ts
// overlays/__tests__/ModalDialog.test.ts
import { describe, it, expect } from 'vitest'
import { mount } from '@vue/test-utils'
import ModalDialog from '../ModalDialog.vue'

describe('ModalDialog', () => {
  it('renders header and content', () => {
    const wrapper = mount(ModalDialog, {
      global: { stubs: { Dialog: true } },
      props: { modelValue: true, header: 'My Title' },
      slots: { default: '<p>Body content</p>' },
    })
    expect(wrapper.html()).toContain('Body content')
  })

  it('toggles visibility via v-model', async () => {
    const wrapper = mount(ModalDialog, {
      global: { stubs: { Dialog: true } },
      props: { modelValue: false, header: 'Test' },
    })
    await wrapper.setProps({ modelValue: true })
    expect(wrapper.emitted('update:modelValue')?.[0]).toEqual([true])
  })

  it('renders footer slot', () => {
    const wrapper = mount(ModalDialog, {
      global: { stubs: { Dialog: true } },
      props: { modelValue: true, header: 'Title' },
      slots: { footer: '<button>Save</button>' },
    })
    expect(wrapper.html()).toContain('Save')
  })
})
```

Run: `pnpm run test:unit -- ModalDialog`
Expected: 3 FAIL

- [ ] **Step 2: Implement ModalDialog.vue**

```vue
<!-- overlays/ModalDialog.vue -->
<script setup lang="ts">
const visible = defineModel<boolean>({ required: true })

withDefaults(defineProps<{
  header: string
  maxWidth?: string
  closable?: boolean
  dismissableMask?: boolean
}>(), {
  maxWidth: 'max-w-lg',
  closable: true,
  dismissableMask: true,
})
</script>

<template>
  <Dialog
    v-model:visible="visible"
    :header="header"
    :closable="closable"
    :dismissableMask="dismissableMask"
    modal
    :class="maxWidth"
  >
    <slot />
    <template v-if="$slots.footer" #footer>
      <slot name="footer" />
    </template>
  </Dialog>
</template>
```

- [ ] **Step 3: Write failing CompactTable test**

```ts
// tables/__tests__/CompactTable.test.ts
import { describe, it, expect } from 'vitest'
import { mount } from '@vue/test-utils'
import CompactTable from '../CompactTable.vue'

const columns = [
  { field: 'name', header: 'Name' },
  { field: 'status', header: 'Status' },
]
const rows = [{ id: '1', name: 'Item A', status: 'Active' }, { id: '2', name: 'Item B', status: 'Inactive' }]

describe('CompactTable', () => {
  it('renders rows from value prop', () => {
    const wrapper = mount(CompactTable, {
      global: { stubs: { DataTable: true, Column: true } },
      props: { value: rows, columns },
    })
    expect(wrapper.props('value')).toEqual(rows)
  })

  it('accepts loading prop', () => {
    const wrapper = mount(CompactTable, {
      global: { stubs: { DataTable: true, Column: true } },
      props: { value: [], columns, loading: true },
    })
    expect(wrapper.props('loading')).toBe(true)
  })

  it('defaults rows to 5', () => {
    const wrapper = mount(CompactTable, {
      global: { stubs: { DataTable: true, Column: true } },
      props: { value: [], columns },
    })
    expect(wrapper.props('rows')).toBe(5)
  })
})
```

Run: `pnpm run test:unit -- CompactTable`
Expected: 3 FAIL

- [ ] **Step 4: Implement CompactTable.vue**

```vue
<!-- tables/CompactTable.vue -->
<script setup lang="ts">
import type { ColumnDef } from './DataTableShell.vue'

withDefaults(defineProps<{
  value: any[]
  columns: ColumnDef[]
  rows?: number
  dataKey?: string
  loading?: boolean
  scrollable?: boolean
  stripedRows?: boolean
}>(), {
  rows: 5,
  dataKey: 'id',
  loading: false,
  scrollable: true,
  stripedRows: false,
})
</script>

<template>
  <DataTable
    :value="value"
    :loading="loading"
    :rows="rows"
    :dataKey="dataKey"
    :scrollable="scrollable"
    :stripedRows="stripedRows"
    rowHover
    showGridlines
    class="rounded-border"
  >
    <Column
      v-for="col in columns"
      :key="col.field"
      :field="col.field"
      :header="col.header"
    >
      <template v-if="col.body" #body="{ data }">
        {{ col.body(data) }}
      </template>
    </Column>
  </DataTable>
</template>
```

- [ ] **Step 5: Add barrel exports**

Add to `shared/components/index.ts`:
```ts
export { default as ModalDialog } from './overlays/ModalDialog.vue'
export { default as CompactTable } from './tables/CompactTable.vue'
```

- [ ] **Step 6: Verify**

```bash
pnpm run type-check
pnpm run test:unit -- ModalDialog CompactTable
```

- [ ] **Step 7: Commit**

```bash
git add app/Admin/src/shared/components/overlays/ModalDialog.vue app/Admin/src/shared/components/overlays/__tests__/ModalDialog.test.ts
git add app/Admin/src/shared/components/tables/CompactTable.vue app/Admin/src/shared/components/tables/__tests__/CompactTable.test.ts
git add app/Admin/src/shared/components/index.ts
git commit -m "feat(admin): add ModalDialog and CompactTable shared components"
```

---

### Task 8: Adopt ConfirmDialog across features (delete confirmation pattern)

**Files:**
Modify ~20 files across 8 features. Each migration replaces:

```ts
// OLD pattern (remove)
import { useConfirm } from 'primevue/useconfirm'
const confirm = useConfirm()

function confirmDelete(item: Item) {
  confirm.require({
    message: `Delete ${item.name}?`,
    header: 'Confirm',
    icon: 'pi pi-exclamation-triangle',
    rejectLabel: 'Cancel',
    acceptProps: { label: 'Delete', severity: 'danger' },
    accept: () => deleteItem(item.id),
  })
}
```

```html
<!-- OLD template (remove) -->
<Button icon="pi pi-trash" severity="danger" text rounded @click="confirmDelete(item)" />
```

→

```ts
// NEW (add none, just import)
import ConfirmDialog from '@/shared/components/overlays/ConfirmDialog.vue'
```

```html
<!-- NEW template -->
<ConfirmDialog
  severity="danger"
  :header="`Delete ${item.name}`"
  message="Are you sure? This action cannot be undone."
  @confirm="deleteItem(item.id)"
/>
```

**Target features and files** (each file needs the same transformation):

| Feature | Files |
|---|---|
| **catalog** | `features/catalog/products/pages/ProductListPage.vue`, `features/catalog/products/components/ProductImageList.vue`, `features/catalog/products/components/OptionTypeForm.vue`, `features/catalog/products/components/OptionValueForm.vue`, `features/catalog/products/components/TaxonRuleList.vue`, `features/catalog/products/components/VariantList.vue` |
| **users** | `features/users/customers/pages/CustomerListPage.vue`, `features/users/customers/components/CustomerDetailPage.vue` |
| **ordering** | `features/ordering/orders/pages/OrderListPage.vue`, `features/ordering/orders/components/LineItemForm.vue` |
| **inventories** | `features/inventories/stock-items/pages/InventoryUnitListPage.vue`, `features/inventories/stock-transfers/pages/StockTransferListPage.vue`, `features/inventories/stock-transfers/components/StockTransferDetailPage.vue` |
| **location** | `features/location/addresses/pages/AddressListPage.vue` |
| **payment** | `features/payment/payment-methods/pages/PaymentMethodListPage.vue` |
| **shipping** | `features/shipping/methods/pages/ShippingMethodListPage.vue` (already uses ConfirmButton — update import to ConfirmDialog + slot) |

- [ ] **Step 1: Migrate catalog (6 files)**

For each file:
```bash
# Remove useConfirm import + confirmDelete function + old Button
# Replace with ConfirmDialog import + component usage
```
Use the pattern transformation above. After each file edit, ensure the `confirmDelete` function is removed and `import ConfirmDialog` is added.

- [ ] **Step 2: Migrate remaining features using the same pattern**

users (2), ordering (2), inventories (3), location (1), payment (1), shipping (1)

- [ ] **Step 3: Verify**

```bash
pnpm run type-check
pnpm run test:unit
```

- [ ] **Step 4: Commit**

```bash
git add app/Admin/src/features/
git commit -m "refactor(admin): adopt ConfirmDialog across 8 features (20 sites)"
```

---

### Task 9: Adopt StatusBadge across features (status tag pattern)

**Files:**
Modify ~33 inline `<Tag>` usages across 7 features. Pattern transformation:

```html
<!-- OLD -->
<Tag :value="row.active ? 'Active' : 'Inactive'" :severity="row.active ? 'success' : 'secondary'" rounded class="font-bold px-3" />

<!-- NEW — per-feature status map -->
<StatusBadge :status="row.active" :statusMap="activeStatusMap" />
```

Each feature defines its status maps as a local constant (or in a shared status utils file):

```ts
const activeStatusMap = {
  true: { label: 'Active', severity: 'success' as const },
  false: { label: 'Inactive', severity: 'secondary' as const },
} as const
```

**Target features:**

| Feature | Files | Status values |
|---|---|---|
| **users** | `features/users/customers/pages/CustomerListPage.vue`, `features/users/customers/components/CustomerDetailPage.vue` | active (boolean) |
| **inventories** | `features/inventories/stock-items/pages/InventoryUnitListPage.vue`, `features/inventories/stock-transfers/pages/StockTransferListPage.vue`, `features/inventories/stock-transfers/components/StockTransferDetailPage.vue` | transfer status, stock status |
| **catalog** | `features/catalog/products/pages/ProductListPage.vue`, `features/catalog/products/components/VariantList.vue` | published status |
| **ordering** | `features/ordering/orders/pages/OrderListPage.vue`, `features/ordering/orders/components/OrderDetailPage.vue` | order status |
| **reports** | `features/reports/dashboard/pages/DashboardPage.vue` | order status |
| **location** | `features/location/addresses/pages/AddressListPage.vue` | default status |
| **payment** | `features/payment/payment-methods/pages/PaymentMethodListPage.vue`, `features/payment/payment-methods/components/PaymentDetailPage.vue` | payment status |

- [ ] **Step 1: Create shared status utils (optional, but helpful)**

```ts
// app/Admin/src/common/utils/status.util.ts
export type StatusDef = { label: string; severity: string }

export const booleanStatusMap: Record<boolean, StatusDef> = {
  true: { label: 'Active', severity: 'success' },
  false: { label: 'Inactive', severity: 'secondary' },
}

// Export to common/api/index.ts
```

- [ ] **Step 2: Migrate users feature**

Replace inline `<Tag>` with `<StatusBadge>`, define local status maps or import from shared utils.

- [ ] **Step 3: Migrate inventories (3 files), catalog (2 files)**

Same pattern.

- [ ] **Step 4: Migrate ordering (2 files), reports (1), location (1), payment (2)**

- [ ] **Step 5: Verify + commit**

```bash
pnpm run type-check
pnpm run test:unit
git add app/Admin/src/features/ app/Admin/src/common/utils/
git commit -m "refactor(admin): adopt StatusBadge across 7 features (33 sites)"
```

---

### Task 10: Adopt ModalDialog across features (dialog pattern)

**Files:**
Modify ~14 files across 3 features. Pattern transformation:

```html
<!-- OLD -->
<Dialog v-model:visible="showDialog" header="Edit" modal class="w-full max-w-lg" @hide="emit('close')">
  <div>...</div>
  <template #footer>
    <Button label="Cancel" text @click="showDialog = false" />
    <Button label="Save" @click="save" />
  </template>
</Dialog>
```

→

```html
<!-- NEW -->
<ModalDialog v-model="showDialog" header="Edit">
  <div>...</div>
  <template #footer>
    <Button label="Cancel" text @click="showDialog = false" />
    <Button label="Save" @click="save" />
  </template>
</ModalDialog>
```

Replace `import Dialog from 'primevue/dialog'` with `import ModalDialog from '@/shared/components/overlays/ModalDialog.vue'`.

**Target features:** ordering (5 dialogs), catalog (7 dialogs), inventories (2 dialogs)

List of files (same pattern for each):
- `features/ordering/orders/components/OrderFormPage.vue` (Dialog for form)
- `features/ordering/orders/components/ShipmentFormDialog.vue`, `RefundFormDialog.vue`, `OrderItemFormDialog.vue`, `AddressFormDialog.vue`
- `features/catalog/products/components/ProductImageUploader.vue`, `ProductImageList.vue`, `VariantForm.vue`, `VariantGeneration.vue`, `OptionTypeForm.vue`, `OptionValueForm.vue`, `TaxonRuleList.vue`
- `features/inventories/stock-transfers/components/StockAdjustmentDialog.vue`, `StockTransferDetailPage.vue`

- [ ] **Step 1: Migrate ordering dialogs (5 files)**
- [ ] **Step 2: Migrate catalog dialogs (7 files)**
- [ ] **Step 3: Migrate inventories dialogs (2 files)**
- [ ] **Step 4: Verify + commit**

```bash
pnpm run type-check && pnpm run test:unit
git add app/Admin/src/features/
git commit -m "refactor(admin): adopt ModalDialog across ordering, catalog, inventories (14 sites)"
```

---

### Task 11: Adopt CompactTable + FormField + DetailField across features

**Files:**
Modify ~43 files across 5 features. Two patterns.

**Pattern A — CompactTable (inline DataTable → CompactTable):**
~20 files in ordering, catalog, inventories, payment. Replace raw `<DataTable>` imports and usage with `<CompactTable>`.

**Pattern B — FormField (raw label+input → FormField):**
~15 files in inventories, ordering, auth, users. Wrap each `<label>` + `<InputText>` pair in `<FormField>`.

```html
<!-- OLD -->
<div>
  <label class="block text-sm font-bold mb-1">Name</label>
  <InputText v-model="form.name" />
</div>

<!-- NEW -->
<FormField label="Name" name="name" required>
  <InputText v-model="form.name" />
</FormField>
```

**Pattern C — DetailField (raw span+value → DetailField):**
~8 files across all detail pages.

```html
<!-- OLD -->
<div>
  <span class="text-xs text-surface-400">Price</span>
  <span class="text-lg font-medium">{{ item.price }}</span>
</div>

<!-- NEW -->
<DetailField label="Price" :value="item.price" />
```

- [ ] **Step 1: Migrate CompactTable — ordering (line items, 4 files)**

Replace `import { DataTable, Column } from 'primevue/...'` with `import CompactTable from '@/shared/components/tables/CompactTable.vue'` and adjust template.

- [ ] **Step 2: Migrate CompactTable — catalog (variants, 5+ files)**
- [ ] **Step 3: Migrate CompactTable — inventories, payment (remaining files)**
- [ ] **Step 4: Migrate FormField — inventories (form pages, 3 files)**
- [ ] **Step 5: Migrate FormField — ordering, auth, users (remaining files)**
- [ ] **Step 6: Migrate DetailField — all detail pages (8+ files)**
- [ ] **Step 7: Verify + commit**

```bash
pnpm run type-check && pnpm run test:unit
git add app/Admin/src/features/
git commit -m "refactor(admin): adopt CompactTable, FormField, DetailField across features"
```

---

### Task 12: Final verification and cleanup

**Files:**
- Modify: `app/Admin/src/shared/components/index.ts` (verify all exports present)
- Scan: stale imports referencing old paths

- [ ] **Step 1: Full verification**

```bash
pnpm run type-check
pnpm run test:unit
pnpm run lint:eslint
```

- [ ] **Step 2: Scan for stale imports**

```bash
# No file should reference the old base/ directory
rg -l "shared/components/base/" app/Admin/src/
# No file should reference old ConfirmButton name
rg -l "ConfirmButton" app/Admin/src/
# No file should reference old ManagerWelcome location
rg "shared/components/navigation/ManagerWelcome" app/Admin/src/
```

Expected: zero results for all three.

- [ ] **Step 3: Final commit**

```bash
git add -A app/Admin/
git commit -m "refactor(admin): final verification — type-check, tests, lint, stale import scan"
```
