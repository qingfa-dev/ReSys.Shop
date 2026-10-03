<script setup lang="ts">
import { ref } from 'vue'
import DataTable from 'primevue/datatable'
import Column from 'primevue/column'
import Button from 'primevue/button'
import InputText from 'primevue/inputtext'
import IconField from 'primevue/iconfield'
import InputIcon from 'primevue/inputicon'
import { useDataTableExport } from '@/shared/composables/useDataTableExport'
import { usePagedQuery } from '@/shared/composables/usePagedQuery'

import type { PermissionMetadata } from '../types/permission'

const { dt, exportCSV } = useDataTableExport()
const search = ref('')
const selectedItems = ref<PermissionMetadata[]>([])

const { items, loading, setSearch, refresh } = usePagedQuery<PermissionMetadata>(
  `/api/admin/identity/permissions`,
  {
    defaultPageSize: 100,
    allowedSearchFields: ['name', 'category', 'description'],
    defaultSearchFields: ['name', 'category'],
  },
)

function onSearch(value: string) {
  search.value = value
  setSearch(value)
}

function clearSearch() {
  search.value = ''
  setSearch('')
}
</script>

<template>
  <div class="flex flex-col h-full">
    <!-- Section: Page Header — title and one-line description -->
    <div class="mb-6">
      <h1 class="text-2xl font-semibold mb-1">Permissions</h1>
      <p class="text-muted-color">System permissions reference</p>
    </div>

    <!-- Section: Search & Filters — search box, clear, reload and export actions -->
    <div class="flex items-center gap-3 mb-4">
      <IconField>
        <InputIcon class="pi pi-search" />
        <InputText :model-value="search" placeholder="Search permissions..." @update:model-value="onSearch($event ?? '')" />
      </IconField>
      <Button v-if="search" label="Clear" severity="secondary" icon="pi pi-times" @click="clearSearch" />
      <div class="flex-1" />
      <Button label="Reload" icon="pi pi-refresh" severity="secondary" @click="refresh" />
      <Button label="Export" icon="pi pi-download" severity="secondary" @click="exportCSV" />
    </div>

    <!-- Section: Data Table — scrollable grid of reference permissions -->
    <DataTable
      ref="dt"
      :value="items"
      :loading="loading"
      v-model:selection="selectedItems"
      scrollable
      paginator
      :rows="50"
      data-key="name"
    >
      <Column selection-mode="multiple" header-style="width: 3rem" />
      <!-- Section: Table Columns — permission identity and descriptive fields -->
      <Column field="name" header="Name" :sortable="true" />
      <Column field="category" header="Category" :sortable="true" />
      <Column field="description" header="Description" />
      <!-- Section: Empty State — shown when the query returns no permissions -->
      <template #empty>No permissions found.</template>
    </DataTable>
  </div>
</template>
