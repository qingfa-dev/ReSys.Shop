# Component Extraction Plan — Admin SPA

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Extract 18 reusable e-commerce admin components from `src/views/` into `shared/components/`, preserving Sakai Vue styles, then delete `src/views/`.

**Architecture:** Each component is a single-file Vue component (`.vue`) with `<script setup lang="ts">`, typed props via `defineProps`, and Sakai CSS class conventions. Components live in 5 subdirectories under `shared/components/` with barrel `index.ts` files. All use PrimeVue components and follow existing `shared/components/` patterns.

**Tech Stack:** Vue 3.5 + TypeScript, PrimeVue 5, Tailwind CSS, Sakai theme

## Global Constraints

- All components use `<script setup lang="ts">`
- Props typed via `interface Props` + `defineProps<Props>()`
- Source Sakai CSS classes preserved exactly (`.card`, `font-semibold text-xl mb-4`, flex utilities, gap classes)
- No default exports — named exports for barrel compatibility
- Vue files at `app/Admin/src/shared/components/{folder}/{ComponentName}.vue`
- Barrel files at `app/Admin/src/shared/components/{folder}/index.ts`

---

### Task 1: feedback/ — ErrorPageShell + GradientCard

**Files:**
- Create: `src/shared/components/feedback/ErrorPageShell.vue`
- Create: `src/shared/components/feedback/GradientCard.vue`

**Interfaces:**
- Produces: `ErrorPageShell` — props `{ statusCode: string, title: string, description: string, gradientColor?: string, icon: string, iconBgClass?: string, image?: string, buttonLabel?: string, buttonTo?: string, links?: Array<{icon:string, title:string, description:string, to:string}> }`
- Produces: `GradientCard` — props `{ gradient: string, outerRadius: string, innerRadius: string }` + default slot

- [ ] **Step 1: Create GradientCard.vue**

Source: `NotFound.vue:44-59` (the gradient-border card pattern used in NotFound, Access, Error, Login). Template:

```vue
<script setup lang="ts">
interface Props {
  gradient?: string
  outerRadius?: string
  innerRadius?: string
}
const props = withDefaults(defineProps<Props>(), {
  gradient: 'linear-gradient(180deg, rgba(64,150,255,0.4) 0%, rgba(64,150,255,0) 100%)',
  outerRadius: '56px',
  innerRadius: '53px',
})
</script>

<template>
  <div class="border border-surface-200 dark:border-surface-700 rounded-border" :style="{ borderRadius: outerRadius, padding: '0.3rem', background: gradient }">
    <div class="w-full bg-surface-0 dark:bg-surface-900 py-20 px-8 sm:px-20 flex flex-col items-center" :style="{ borderRadius: innerRadius }">
      <slot />
    </div>
  </div>
</template>
```

- [ ] **Step 2: Create ErrorPageShell.vue**

Source: Combined from `NotFound.vue` (full file), `Access.vue`, `Error.vue` — they share identical structure differing only in icon, colors, message, gradient. Template extracts the common layout:

```vue
<script setup lang="ts">
import { RouterLink } from 'vue-router'
import GradientCard from './GradientCard.vue'
import FloatingConfigurator from '../ui/FloatingConfigurator.vue'

interface ResourceLink {
  icon: string
  title: string
  description: string
  to: string
}

interface Props {
  statusCode?: string | number
  title: string
  description: string
  gradientColor?: string
  icon: string
  iconBgClass?: string
  image?: string
  buttonLabel?: string
  buttonTo?: string
  links?: ResourceLink[]
}

const props = withDefaults(defineProps<Props>(), {
  statusCode: '',
  gradientColor: 'rgba(64,150,255,0.4)',
  buttonLabel: 'Go to Dashboard',
  buttonTo: '/',
  links: () => [],
  image: '',
})
</script>

<template>
  <FloatingConfigurator />
  <div class="flex items-center justify-center min-h-screen overflow-hidden">
    <div class="flex flex-col items-center justify-center">
      <svg viewBox="0 0 54 40" fill="none" xmlns="http://www.w3.org/2000/svg" class="mb-8" width="80">
        <!-- Sakai logo SVG path from NotFound.vue:10-28 -->
        <path fill-rule="evenodd" clip-rule="evenodd" d="M..." fill="var(--primary-color)" />
      </svg>
      <GradientCard :gradient="`linear-gradient(180deg, ${gradientColor} 0%, rgba(64,150,255,0) 100%)`">
        <div v-if="statusCode" class="text-surface-500 dark:text-surface-200 font-bold text-8xl mb-6 leading-none">{{ statusCode }}</div>
        <div v-if="image" class="mb-8">
          <img :src="image" :alt="title" class="w-full max-w-md" />
        </div>
        <div :class="['flex justify-center items-center rounded-full mb-8', iconBgClass || 'bg-orange-500']" style="width:3.2rem; height:3.2rem">
          <i :class="icon" class="text-white text-5xl" />
        </div>
        <h1 class="text-surface-900 dark:text-surface-0 font-bold text-4xl lg:text-5xl mb-2">{{ title }}</h1>
        <span class="text-muted-color font-medium mb-8">{{ description }}</span>
        <router-link v-if="links.length" :to="buttonTo" class="mb-8">
          <Button :label="buttonLabel" />
        </router-link>
        <div v-if="links.length" class="w-full sm:w-80 mt-2">
          <router-link v-for="link in links" :key="link.to" :to="link.to" class="w-full flex items-center py-8 border-b border-surface">
            <span class="flex justify-center items-center rounded-full" style="width:2.5rem; height:2.5rem">
              <i :class="link.icon" class="text-xl" />
            </span>
            <span class="ml-6 flex flex-col">
              <span class="text-surface-900 dark:text-surface-0 lg:text-xl font-medium">{{ link.title }}</span>
              <span class="text-surface-600 dark:text-muted-color lg:text-lg">{{ link.description }}</span>
            </span>
          </router-link>
        </div>
      </GradientCard>
    </div>
  </div>
</template>
```

**Note:** The SVG path above is abbreviated — copy the full logo SVG from `NotFound.vue:10-28`.

- [ ] **Step 3: Build check**

```bash
pnpm run build
```

Expected: No TypeScript errors. The two new files compile.

- [ ] **Step 4: Commit**

```bash
git add src/shared/components/feedback/ErrorPageShell.vue src/shared/components/feedback/GradientCard.vue
git commit -m "feat: add ErrorPageShell and GradientCard components"
```

---

### Task 2: feedback/ — ConfirmDialog + EmptyState

**Files:**
- Create: `src/shared/components/feedback/ConfirmDialog.vue`
- Create: `src/shared/components/feedback/EmptyState.vue`

**Interfaces:**
- Produces: `ConfirmDialog` — props `{ visible: boolean, message: string, header?: string, icon?: string, confirmLabel?: string, cancelLabel?: string }` + emits `confirm`, `cancel`, `update:visible`
- Produces: `EmptyState` — props `{ title: string, description?: string, icon?: string, actionLabel?: string }` + emit `action` + default slot

- [ ] **Step 1: Create ConfirmDialog.vue**

Source: `Crud.vue:199-219` (delete confirmation dialog pattern used 3 times):

```vue
<script setup lang="ts">
interface Props {
  visible: boolean
  message: string
  header?: string
  icon?: string
  confirmLabel?: string
  cancelLabel?: string
}

const props = withDefaults(defineProps<Props>(), {
  header: 'Confirm',
  icon: 'pi pi-exclamation-triangle',
  confirmLabel: 'Yes',
  cancelLabel: 'No',
})

const emit = defineEmits<{
  (e: 'update:visible', value: boolean): void
  (e: 'confirm'): void
  (e: 'cancel'): void
}>()
</script>

<template>
  <Dialog
    v-model:visible="visible"
    :header="header"
    :modal="true"
    :style="{ width: '450px' }"
    @update:visible="emit('update:visible', $event)"
  >
    <div class="flex items-center gap-4">
      <i :class="icon" class="text-8xl" style="color: var(--p-amber-500)" />
      <span class="text-surface-600 dark:text-surface-0 text-lg">{{ message }}</span>
    </div>
    <template #footer>
      <Button :label="cancelLabel" icon="pi pi-times" text @click="emit('cancel')" />
      <Button :label="confirmLabel" icon="pi pi-check" @click="emit('confirm')" />
    </template>
  </Dialog>
</template>
```

- [ ] **Step 2: Create EmptyState.vue**

Source: New component — pattern from empty data states in TableDoc.vue `#empty` template and `Empty.vue`:

```vue
<script setup lang="ts">
interface Props {
  title: string
  description?: string
  icon?: string
  actionLabel?: string
}

withDefaults(defineProps<Props>(), {
  description: '',
  icon: 'pi pi-inbox',
})

const emit = defineEmits<{
  (e: 'action'): void
}>()
</script>

<template>
  <div class="card flex flex-col items-center justify-center py-12 gap-4">
    <i :class="icon" class="text-6xl text-surface-300 dark:text-surface-600" />
    <div class="text-xl font-semibold text-surface-600 dark:text-surface-300">{{ title }}</div>
    <p v-if="description" class="text-muted-color text-center max-w-md">{{ description }}</p>
    <Button v-if="actionLabel" :label="actionLabel" @click="emit('action')" />
    <slot />
  </div>
</template>
```

- [ ] **Step 3: Build check**

```bash
pnpm run build
```

Expected: No TypeScript errors.

- [ ] **Step 4: Commit**

```bash
git add src/shared/components/feedback/ConfirmDialog.vue src/shared/components/feedback/EmptyState.vue
git commit -m "feat: add ConfirmDialog and EmptyState components"
```

---

### Task 3: forms/ — AuthLayout + LoginForm

**Files:**
- Create: `src/shared/components/forms/AuthLayout.vue`
- Create: `src/shared/components/forms/LoginForm.vue`

**Interfaces:**
- Produces: `AuthLayout` — props `{ title: string, subtitle?: string, gradient?: string }` + default slot
- Produces: `LoginForm` — props `{ submitLabel?: string, loading?: boolean }` + emits `submit(data: { email: string, password: string, remember: boolean })`, `forgotPassword`

- [ ] **Step 1: Create AuthLayout.vue**

Source: `Login.vue:23-68` template structure — full-screen centering with GradientCard:

```vue
<script setup lang="ts">
import GradientCard from '../feedback/GradientCard.vue'

interface Props {
  title: string
  subtitle?: string
  gradient?: string
}

withDefaults(defineProps<Props>(), {
  subtitle: '',
  gradient: 'var(--p-primary-color)',
})
</script>

<template>
  <div class="flex items-center justify-center min-h-screen overflow-hidden">
    <div class="flex flex-col items-center justify-center">
      <svg viewBox="0 0 54 40" fill="none" xmlns="http://www.w3.org/2000/svg" class="mb-8" width="80">
        <path fill-rule="evenodd" clip-rule="evenodd" d="M..." fill="var(--primary-color)" />
      </svg>
      <GradientCard :gradient="`linear-gradient(180deg, ${gradient} 0%, rgba(64,150,255,0) 100%)`">
        <div class="flex flex-col items-center gap-2 mb-8">
          <h1 class="text-surface-900 dark:text-surface-0 font-bold text-4xl lg:text-5xl">{{ title }}</h1>
          <span v-if="subtitle" class="text-muted-color font-medium">{{ subtitle }}</span>
        </div>
        <slot />
      </GradientCard>
    </div>
  </div>
</template>
```

**Note:** Copy full SVG path from `Login.vue:27-37`.

- [ ] **Step 2: Create LoginForm.vue**

Source: `Login.vue:15-23` (script refs) + `Login.vue:38-64` (form fields):

```vue
<script setup lang="ts">
import { ref } from 'vue'

interface LoginData {
  email: string
  password: string
  remember: boolean
}

interface Props {
  submitLabel?: string
  loading?: boolean
}

withDefaults(defineProps<Props>(), {
  submitLabel: 'Sign In',
  loading: false,
})

const emit = defineEmits<{
  (e: 'submit', data: LoginData): void
  (e: 'forgotPassword'): void
}>()

const email = ref('')
const password = ref('')
const remember = ref(false)
</script>

<template>
  <form @submit.prevent="emit('submit', { email, password, remember })" class="flex flex-col gap-4 w-full md:w-[30rem]">
    <div class="flex flex-col gap-1">
      <label for="email" class="text-surface-900 dark:text-surface-0 font-medium">Email</label>
      <InputText id="email" v-model="email" class="w-full" type="email" placeholder="Email address" />
    </div>
    <div class="flex flex-col gap-1">
      <label for="password" class="text-surface-900 dark:text-surface-0 font-medium">Password</label>
      <Password id="password" v-model="password" class="w-full" :toggleMask="true" :feedback="false" placeholder="Password" />
    </div>
    <div class="flex items-center justify-between">
      <div class="flex items-center gap-2">
        <Checkbox v-model="remember" inputId="remember" binary />
        <label for="remember" class="text-surface-600 dark:text-surface-300">Remember me</label>
      </div>
      <a class="text-primary font-medium hover:underline cursor-pointer" @click="emit('forgotPassword')">Forgot password?</a>
    </div>
    <Button type="submit" :label="submitLabel" class="w-full" :loading="loading" />
  </form>
</template>
```

- [ ] **Step 3: Build check + tests**

```bash
pnpm run build && pnpm run test:unit -- run
```

Expected: No errors, all tests pass.

- [ ] **Step 4: Commit**

```bash
git add src/shared/components/forms/AuthLayout.vue src/shared/components/forms/LoginForm.vue
git commit -m "feat: add AuthLayout and LoginForm components"
```

---

### Task 4: forms/ — FormField + FormSection

**Files:**
- Create: `src/shared/components/forms/FormField.vue`
- Create: `src/shared/components/forms/FormSection.vue`

**Interfaces:**
- Produces: `FormField` — props `{ label: string, layout?: 'vertical'|'horizontal'|'inline', helpText?: string, invalid?: boolean, required?: boolean }` + default slot
- Produces: `FormSection` — props `{ title: string, description?: string }` + default slot

- [ ] **Step 1: Create FormField.vue**

Source: `FormLayout.vue` patterns (vertical: lines 10-14, horizontal: lines 24-30, inline: lines 34-40):

```vue
<script setup lang="ts">
type FormLayout = 'vertical' | 'horizontal' | 'inline'

interface Props {
  label: string
  layout?: FormLayout
  helpText?: string
  invalid?: boolean
  required?: boolean
}

withDefaults(defineProps<Props>(), {
  layout: 'vertical',
})
</script>

<template>
  <div v-if="layout === 'vertical'" class="flex flex-col gap-1">
    <label class="text-surface-900 dark:text-surface-0 font-medium">
      {{ label }}<span v-if="required" class="text-red-500 ml-1">*</span>
    </label>
    <slot />
    <small v-if="helpText" class="text-muted-color">{{ helpText }}</small>
    <small v-if="invalid" class="text-red-500">This field is required</small>
  </div>
  <div v-else-if="layout === 'horizontal'" class="grid grid-cols-12 gap-4 items-center">
    <label class="col-span-12 md:col-span-2 text-surface-900 dark:text-surface-0 font-medium">
      {{ label }}<span v-if="required" class="text-red-500 ml-1">*</span>
    </label>
    <div class="col-span-12 md:col-span-10">
      <slot />
      <small v-if="helpText" class="text-muted-color block mt-1">{{ helpText }}</small>
    </div>
  </div>
  <div v-else class="flex flex-wrap items-start gap-4">
    <label class="sr-only">{{ label }}</label>
    <slot />
  </div>
</template>
```

- [ ] **Step 2: Create FormSection.vue**

Source: `FormLayout.vue` section wrappers (each `<div class="card flex flex-col gap-4">` with heading):

```vue
<script setup lang="ts">
interface Props {
  title: string
  description?: string
}

withDefaults(defineProps<Props>(), {
  description: '',
})
</script>

<template>
  <div class="card flex flex-col gap-4">
    <div>
      <div class="font-semibold text-xl">{{ title }}</div>
      <p v-if="description" class="text-muted-color mt-1">{{ description }}</p>
    </div>
    <slot />
  </div>
</template>
```

- [ ] **Step 3: Build check**

```bash
pnpm run build
```

Expected: No errors.

- [ ] **Step 4: Commit**

```bash
git add src/shared/components/forms/FormField.vue src/shared/components/forms/FormSection.vue
git commit -m "feat: add FormField and FormSection components"
```

---

### Task 5: tables/ — CrudToolbar + DataTableCard

**Files:**
- Create: `src/shared/components/tables/CrudToolbar.vue`
- Create: `src/shared/components/tables/DataTableCard.vue`

**Interfaces:**
- Produces: `CrudToolbar` — props `{ newLabel?: string, deleteLabel?: string, exportLabel?: string, deleteDisabled?: boolean, searchPlaceholder?: string }` + emits `new`, `delete`, `export`, `update:search`
- Produces: `DataTableCard` — props `{ title: string }` + default slot

- [ ] **Step 1: Create CrudToolbar.vue**

Source: `Crud.vue:158-177` (Toolbar with start/end + search header):

```vue
<script setup lang="ts">
interface Props {
  newLabel?: string
  deleteLabel?: string
  exportLabel?: string
  deleteDisabled?: boolean
  searchPlaceholder?: string
}

withDefaults(defineProps<Props>(), {
  newLabel: 'New',
  deleteLabel: 'Delete',
  exportLabel: 'Export',
  deleteDisabled: false,
  searchPlaceholder: 'Search...',
})

const emit = defineEmits<{
  (e: 'new'): void
  (e: 'delete'): void
  (e: 'export'): void
  (e: 'update:search', value: string): void
}>()
</script>

<template>
  <div class="card mb-6">
    <Toolbar class="mb-4">
      <template #start>
        <Button :label="newLabel" icon="pi pi-plus" severity="secondary" class="mr-2" @click="emit('new')" />
        <Button :label="deleteLabel" icon="pi pi-trash" severity="secondary" :disabled="deleteDisabled" @click="emit('delete')" />
      </template>
      <template #end>
        <Button :label="exportLabel" icon="pi pi-upload" severity="secondary" @click="emit('export')" />
      </template>
    </Toolbar>
    <div class="flex justify-between items-center">
      <slot name="header-left" />
      <IconField>
        <InputIcon class="pi pi-search" />
        <InputText :placeholder="searchPlaceholder" @update:modelValue="emit('update:search', $event)" />
      </IconField>
    </div>
  </div>
</template>
```

- [ ] **Step 2: Create DataTableCard.vue**

Source: Pattern from all uikit table panels (`card` + heading + DataTable):

```vue
<script setup lang="ts">
interface Props {
  title: string
}

defineProps<Props>()
</script>

<template>
  <div class="card">
    <div class="font-semibold text-xl mb-4">{{ title }}</div>
    <slot />
  </div>
</template>
```

- [ ] **Step 3: Build check**

```bash
pnpm run build
```

Expected: No errors.

- [ ] **Step 4: Commit**

```bash
git add src/shared/components/tables/CrudToolbar.vue src/shared/components/tables/DataTableCard.vue
git commit -m "feat: add CrudToolbar and DataTableCard components"
```

---

### Task 6: tables/ — FilterableDataTable

**Files:**
- Create: `src/shared/components/tables/FilterableDataTable.vue`

**Interfaces:**
- Produces: `FilterableDataTable` — props `{ columns: ColumnDef[], data: any[], filters: Record<string, any>, loading?: boolean, rows?: number, paginator?: boolean, globalFilterFields?: string[] }` + slots for column body templates

- [ ] **Step 1: Create FilterableDataTable.vue**

Source: `TableDoc.vue:101-246` (filtering DataTable with menu filter display, 7 filter columns, formatted cells). Extract the general-purpose DataTable with `#header` search and column templates:

```vue
<script setup lang="ts">
import { ref } from 'vue'
import { FilterMatchMode } from '@primevue/core/api'
import type { DataTableFilterMeta } from 'primevue'

interface ColumnDef {
  field: string
  header: string
  sortable?: boolean
  filter?: boolean
  filterField?: string
  bodyStyle?: string
  style?: string
}

interface Props {
  columns: ColumnDef[]
  data: any[]
  filters: DataTableFilterMeta
  loading?: boolean
  rows?: number
  paginator?: boolean
  globalFilterFields?: string[]
}

const props = withDefaults(defineProps<Props>(), {
  loading: false,
  rows: 10,
  paginator: true,
  globalFilterFields: () => [],
})

const emit = defineEmits<{
  (e: 'update:filters', value: DataTableFilterMeta): void
}>()

const globalFilterValue = ref('')
const dt = ref()

const onGlobalFilterChange = (value: string) => {
  globalFilterValue.value = value
}

const clearFilter = () => {
  emit('update:filters', { global: { value: null } })
}

const exportCSV = () => {
  dt.value?.exportCSV()
}
</script>

<template>
  <DataTable
    ref="dt"
    :value="data"
    :paginator="paginator"
    :rows="rows"
    :filters="filters"
    :loading="loading"
    :globalFilterFields="globalFilterFields"
    filterDisplay="menu"
    :globalFilterFields="globalFilterFields"
    dataKey="id"
    paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink CurrentPageReport RowsPerPageDropdown"
    :rowsPerPageOptions="[5, 10, 25]"
    currentPageReportTemplate="Showing {first} to {last} of {totalRecords} products"
  >
    <template #header>
      <div class="flex justify-between items-center">
        <Button type="button" icon="pi pi-filter-slash" label="Clear" outlined @click="clearFilter" />
        <IconField>
          <InputIcon class="pi pi-search" />
          <InputText v-model="globalFilterValue" placeholder="Search..." @update:modelValue="onGlobalFilterChange" />
        </IconField>
      </div>
    </template>
    <Column v-for="col in columns" :key="col.field" :field="col.field" :header="col.header" :sortable="col.sortable" :filter="col.filter" :filterField="col.filterField || col.field" :bodyStyle="col.bodyStyle" :style="col.style">
      <template v-if="col.field" #body="slotProps">
        <slot :name="`body-${col.field}`" :data="slotProps.data" :field="col.field">
          {{ slotProps.data[col.field] }}
        </slot>
      </template>
    </Column>
    <template #empty>
      <slot name="empty">
        <div class="text-center py-8 text-muted-color">No records found.</div>
      </slot>
    </template>
    <template #loading>
      <slot name="loading">
        <div class="text-center py-8 text-muted-color">Loading...</div>
      </slot>
    </template>
  </DataTable>
</template>
```

- [ ] **Step 2: Build check + tests**

```bash
pnpm run build && pnpm run test:unit -- run
```

Expected: No errors, all tests pass.

- [ ] **Step 3: Commit**

```bash
git add src/shared/components/tables/FilterableDataTable.vue
git commit -m "feat: add FilterableDataTable component"
```

---

### Task 7: ui/ — StatCard + PageShell

**Files:**
- Create: `src/shared/components/ui/StatCard.vue`
- Create: `src/shared/components/ui/PageShell.vue`

**Interfaces:**
- Produces: `StatCard` — props `{ label: string, value: string|number, icon: string, iconBgClass?: string, subText?: string, trend?: string }`
- Produces: `PageShell` — props `{ title: string, description?: string }` + default slot

- [ ] **Step 1: Create StatCard.vue**

Source: `StatsWidget.vue:25-50` (4 stat cards pattern) + `Blocks.vue` stat cards:

```vue
<script setup lang="ts">
interface Props {
  label: string
  value: string | number
  icon: string
  iconBgClass?: string
  subText?: string
  trend?: string
}

withDefaults(defineProps<Props>(), {
  iconBgClass: 'bg-indigo-500',
})
</script>

<template>
  <div class="col-span-12 lg:col-span-6 xl:col-span-3">
    <div class="card mb-0">
      <div class="flex justify-between mb-4">
        <div>
          <span class="block text-muted-color font-medium mb-4">{{ label }}</span>
          <div class="text-surface-900 dark:text-surface-0 font-medium text-xl">{{ value }}</div>
        </div>
        <div :class="['flex justify-center items-center rounded-full', iconBgClass]" style="width: 2.5rem; height: 2.5rem">
          <i :class="icon" class="text-white text-xl" />
        </div>
      </div>
      <span v-if="subText" class="text-muted-color">{{ subText }}</span>
      <span v-if="trend" class="text-primary font-medium">{{ trend }}</span>
    </div>
  </div>
</template>
```

- [ ] **Step 2: Create PageShell.vue**

Source: `Empty.vue` + pattern from all uikit doc files (card wrapper with title):

```vue
<script setup lang="ts">
interface Props {
  title: string
  description?: string
}

withDefaults(defineProps<Props>(), {
  description: '',
})
</script>

<template>
  <div class="card">
    <div class="font-semibold text-xl mb-4">{{ title }}</div>
    <p v-if="description" class="text-muted-color mb-4">{{ description }}</p>
    <slot />
  </div>
</template>
```

- [ ] **Step 3: Build check**

```bash
pnpm run build
```

Expected: No errors.

- [ ] **Step 4: Commit**

```bash
git add src/shared/components/ui/StatCard.vue src/shared/components/ui/PageShell.vue
git commit -m "feat: add StatCard and PageShell components"
```

---

### Task 8: ui/ — StatusTag + RatingBadge + CountryFlag

**Files:**
- Create: `src/shared/components/ui/StatusTag.vue`
- Create: `src/shared/components/ui/RatingBadge.vue`
- Create: `src/shared/components/ui/CountryFlag.vue`

**Interfaces:**
- Produces: `StatusTag` — props `{ status: string, domain?: 'inventory'|'order'|'stock'|'default' }`
- Produces: `RatingBadge` — props `{ rating: number }`
- Produces: `CountryFlag` — props `{ country: { name: string, code: string } }`

- [ ] **Step 1: Create StatusTag.vue**

Source: `Crud.vue:221-240` (getStatusLabel switch) + `TableDoc.vue:61-78` (getSeverity stock) + `TableDoc.vue:51-58` (getOrderSeverity):

```vue
<script setup lang="ts">
type StatusDomain = 'inventory' | 'order' | 'stock' | 'default'

interface Props {
  status: string
  domain?: StatusDomain
}

withDefaults(defineProps<Props>(), {
  domain: 'default',
})

function getSeverity(status: string, domain: StatusDomain): string {
  const mappings: Record<StatusDomain, Record<string, string>> = {
    inventory: { INSTOCK: 'success', LOWSTOCK: 'warn', OUTOFSTOCK: 'danger' },
    order: { DELIVERED: 'success', CANCELLED: 'danger', PENDING: 'warn', RETURNED: 'info' },
    stock: { INSTOCK: 'success', LOWSTOCK: 'warn', OUTOFSTOCK: 'danger' },
    default: {},
  }
  return mappings[domain]?.[status] || 'info'
}
</script>

<template>
  <Tag :value="status" :severity="getSeverity(status, domain)" />
</template>
```

- [ ] **Step 2: Create RatingBadge.vue**

Source: `ListDoc.vue:52-54` (rating badge reused in both list and grid templates):

```vue
<script setup lang="ts">
interface Props {
  rating: number
}

defineProps<Props>()
</script>

<template>
  <div class="flex items-center gap-2">
    <div class="bg-surface-100 flex items-center gap-1 p-1" style="border-radius: 30px">
      <i class="pi pi-star-fill text-yellow-500 text-sm" />
      <span class="text-surface-900 dark:text-surface-0 font-medium text-sm">{{ rating }}</span>
    </div>
  </div>
</template>
```

- [ ] **Step 3: Create CountryFlag.vue**

Source: `TableDoc.vue:125-132` (country flag + name in filter/body columns) + `InputDoc.vue:96-103` (MultiSelect option template):

```vue
<script setup lang="ts">
interface Props {
  country: {
    name: string
    code: string
  }
}

defineProps<Props>()
</script>

<template>
  <div class="flex items-center gap-2">
    <img
      :alt="country.name"
      src="https://primefaces.org/cdn/primevue/images/flag/flag_placeholder.png"
      :class="`fi fi-${country.code.toLowerCase()}`"
      style="width: 18px"
    />
    <span>{{ country.name }}</span>
  </div>
</template>
```

- [ ] **Step 4: Build check**

```bash
pnpm run build
```

Expected: No errors.

- [ ] **Step 5: Commit**

```bash
git add src/shared/components/ui/StatusTag.vue src/shared/components/ui/RatingBadge.vue src/shared/components/ui/CountryFlag.vue
git commit -m "feat: add StatusTag, RatingBadge, and CountryFlag components"
```

---

### Task 9: ui/ — ProductCard + PageHeading

**Files:**
- Create: `src/shared/components/ui/ProductCard.vue`
- Create: `src/shared/components/ui/PageHeading.vue`

**Interfaces:**
- Produces: `ProductCard` — props `{ product: { id, name, category, price, rating, inventoryStatus, image }, layout?: 'list'|'grid' }`
- Produces: `PageHeading` — props `{ breadcrumbs: Array<{label,to?}>, title: string, stats?: Array<{icon,text}>, actions?: Array<{label,icon?,severity?}> }` + emits `action(index)`

- [ ] **Step 1: Create ProductCard.vue**

Source: `ListDoc.vue:55-92` (list template) + `ListDoc.vue:93-115` (grid template):

```vue
<script setup lang="ts">
import RatingBadge from './RatingBadge.vue'
import StatusTag from './StatusTag.vue'

interface Product {
  id: string
  name: string
  category: string
  price: number
  rating: number
  inventoryStatus: string
  image: string
}

interface Props {
  product: Product
  layout?: 'list' | 'grid'
}

withDefaults(defineProps<Props>(), {
  layout: 'list',
})
</script>

<template>
  <div v-if="layout === 'list'" class="flex flex-col sm:flex-row items-center p-4 gap-4 border-b border-surface">
    <div class="relative">
      <img :src="product.image" :alt="product.name" class="w-48 rounded-border" />
      <StatusTag :status="product.inventoryStatus" domain="inventory" class="absolute top-3 left-3" />
    </div>
    <div class="flex flex-col gap-2 flex-1">
      <span class="text-muted-color text-sm">{{ product.category }}</span>
      <span class="text-surface-900 dark:text-surface-0 font-medium text-lg">{{ product.name }}</span>
      <RatingBadge :rating="product.rating" />
      <div class="flex items-center justify-between">
        <span class="text-surface-900 dark:text-surface-0 font-semibold text-xl">{{ product.price }}</span>
        <slot name="actions" />
      </div>
    </div>
  </div>
  <div v-else class="border border-surface rounded-border p-4 flex flex-col gap-4">
    <div class="relative">
      <img :src="product.image" :alt="product.name" class="w-full rounded-border" />
      <StatusTag :status="product.inventoryStatus" domain="inventory" class="absolute top-3 left-3" />
    </div>
    <div class="flex flex-col gap-2">
      <span class="text-muted-color text-sm">{{ product.category }}</span>
      <span class="text-surface-900 dark:text-surface-0 font-medium text-lg">{{ product.name }}</span>
      <RatingBadge :rating="product.rating" />
      <div class="flex items-center justify-between">
        <span class="text-surface-900 dark:text-surface-0 font-semibold text-xl">{{ product.price }}</span>
        <slot name="actions" />
      </div>
    </div>
  </div>
</template>
```

- [ ] **Step 2: Create PageHeading.vue**

Source: `Blocks.vue:328-372` (page heading block with breadcrumb, title, stats, actions):

```vue
<script setup lang="ts">
interface BreadcrumbItem {
  label: string
  to?: string
}

interface StatItem {
  icon: string
  text: string
}

interface ActionItem {
  label: string
  icon?: string
  severity?: 'primary' | 'secondary' | 'info' | 'success' | 'warn' | 'danger' | 'help' | 'contrast'
}

interface Props {
  breadcrumbs?: BreadcrumbItem[]
  title: string
  stats?: StatItem[]
  actions?: ActionItem[]
}

withDefaults(defineProps<Props>(), {
  breadcrumbs: () => [],
  stats: () => [],
  actions: () => [],
})

const emit = defineEmits<{
  (e: 'action', index: number): void
}>()
</script>

<template>
  <div class="card mb-8">
    <div v-if="breadcrumbs.length" class="flex items-center gap-2 text-muted-color mb-4">
      <template v-for="(item, i) in breadcrumbs" :key="i">
        <router-link v-if="item.to" :to="item.to" class="hover:text-primary">{{ item.label }}</router-link>
        <span v-else>{{ item.label }}</span>
        <i v-if="i < breadcrumbs.length - 1" class="pi pi-angle-right text-xs" />
      </template>
    </div>
    <div class="flex flex-col lg:flex-row lg:items-center lg:justify-between gap-4">
      <h1 class="text-2xl font-bold text-surface-900 dark:text-surface-0">{{ title }}</h1>
      <div v-if="actions.length" class="flex gap-2">
        <Button
          v-for="(action, i) in actions"
          :key="i"
          :label="action.label"
          :icon="action.icon"
          :severity="action.severity || 'secondary'"
          @click="emit('action', i)"
        />
      </div>
    </div>
    <div v-if="stats.length" class="flex gap-6 mt-4">
      <div v-for="(stat, i) in stats" :key="i" class="flex items-center gap-2">
        <i :class="stat.icon" class="text-primary" />
        <span class="text-surface-600 dark:text-surface-300">{{ stat.text }}</span>
      </div>
    </div>
  </div>
</template>
```

- [ ] **Step 3: Build check + tests**

```bash
pnpm run build && pnpm run test:unit -- run
```

Expected: No errors, all tests pass.

- [ ] **Step 4: Commit**

```bash
git add src/shared/components/ui/ProductCard.vue src/shared/components/ui/PageHeading.vue
git commit -m "feat: add ProductCard and PageHeading components"
```

---

### Task 10: Update Barrel Files

**Files:**
- Modify: `src/shared/components/ui/index.ts`
- Modify: `src/shared/components/feedback/index.ts`
- Modify: `src/shared/components/forms/index.ts`
- Modify: `src/shared/components/tables/index.ts`

- [ ] **Step 1: Update ui/index.ts**

```typescript
export { default as AppConfigurator } from './AppConfigurator.vue'
export { default as AppFooter } from './AppFooter.vue'
export { default as BlockViewer } from './BlockViewer.vue'
export { default as FeaturesWidget } from './FeaturesWidget.vue'
export { default as FloatingConfigurator } from './FloatingConfigurator.vue'
export { default as FooterWidget } from './FooterWidget.vue'
export { default as HeroWidget } from './HeroWidget.vue'
export { default as HighlightsWidget } from './HighlightsWidget.vue'
export { default as PageHeading } from './PageHeading.vue'
export { default as PageShell } from './PageShell.vue'
export { default as PricingWidget } from './PricingWidget.vue'
export { default as ProductCard } from './ProductCard.vue'
export { default as RatingBadge } from './RatingBadge.vue'
export { default as StatCard } from './StatCard.vue'
export { default as StatsWidget } from './StatsWidget.vue'
export { default as StatusTag } from './StatusTag.vue'
export { default as CountryFlag } from './CountryFlag.vue'
export { default as TopbarWidget } from './TopbarWidget.vue'
```

- [ ] **Step 2: Update feedback/index.ts**

```typescript
export { default as NotificationsWidget } from './NotificationsWidget.vue'
export { default as ConfirmDialog } from './ConfirmDialog.vue'
export { default as EmptyState } from './EmptyState.vue'
export { default as ErrorPageShell } from './ErrorPageShell.vue'
export { default as GradientCard } from './GradientCard.vue'
```

- [ ] **Step 3: Update forms/index.ts**

```typescript
export { default as AuthLayout } from './AuthLayout.vue'
export { default as FormField } from './FormField.vue'
export { default as FormSection } from './FormSection.vue'
export { default as LoginForm } from './LoginForm.vue'
```

- [ ] **Step 4: Update tables/index.ts**

```typescript
export { default as CrudToolbar } from './CrudToolbar.vue'
export { default as DataTableCard } from './DataTableCard.vue'
export { default as FilterableDataTable } from './FilterableDataTable.vue'
```

- [ ] **Step 5: Build check**

```bash
pnpm run build
```

Expected: No errors.

- [ ] **Step 6: Commit**

```bash
git add src/shared/components/*/index.ts
git commit -m "feat: update barrel files with new component exports"
```

---

### Task 11: Delete src/views/

**Files:**
- Delete: `src/views/` (entire directory)
- Verify: `vite.config.ts` — no `@views` alias to remove

- [ ] **Step 1: Delete views directory**

```bash
rm -rf src/views
```

- [ ] **Step 2: Run full verification**

```bash
pnpm run build && pnpm run test:unit -- run
```

Expected: Build passes with zero errors. All 307 tests pass.

- [ ] **Step 3: Commit**

```bash
git add -A
git commit -m "feat: delete src/views after component extraction complete"
```
