# Plan: Remove PageHeading, Inline Toolbar in Detail Views

**Goal:** Delete `PageHeading.vue` and replace with inline PrimeVue `<Toolbar>` in all 6 detail/editor views.

**In each view, replace:**
```vue
<PageShell :title="pageTitle">
  <PageHeading
    title=""
    :breadcrumbs="[...]"
    :actions="[...Save, Cancel...]"
    @action="(i: number) => i === 0 ? onSave() : onCancel()"
  />
```

**With:**
```vue
<PageShell :title="pageTitle">
  <div class="flex items-center gap-2 text-muted-color mb-4">
    <template v-for="(item, i) in breadcrumbs" :key="i">
      <router-link v-if="item.to" :to="item.to" class="hover:text-primary">{{ item.label }}</router-link>
      <span v-else>{{ item.label }}</span>
      <i v-if="i < breadcrumbs.length - 1" class="pi pi-angle-right text-xs" />
    </template>
  </div>
  <Toolbar class="mb-8">
    <template #start>
      <h1 class="text-2xl font-bold">{{ pageTitle }}</h1>
    </template>
    <template #end>
      <Button label="Save" icon="pi pi-check" severity="primary" @click="onSave()" />
      <Button label="Cancel" icon="pi pi-times" severity="secondary" @click="onCancel()" />
    </template>
  </Toolbar>
```

**Additional changes per view:**
- Import: remove `PageHeading`, add `Toolbar`, `AngleRight` (if not already imported)
- Define `breadcrumbs` reactive array in `<script setup>` (3 items each)
- Drop `@action` emit handler

## Task Breakdown

### Task 1: Catalog detail views (4 files)
- `ProductDetail.vue`
- `OptionTypeDetail.vue`
- `TaxonomyDetail.vue`
- `TaxonDetail.vue`

### Task 2: Location detail views (2 files)
- `CountryDetail.vue`
- `StateDetail.vue`

### Task 3: Cleanup + verify
- Delete `PageHeading.vue`
- Remove from `panel/index.ts` barrel
- `pnpm run build-only && pnpm run test:unit -- run && pnpm run lint`
