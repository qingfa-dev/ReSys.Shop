<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import DataTable from 'primevue/datatable'
import Column from 'primevue/column'
import Button from 'primevue/button'
import InputText from 'primevue/inputtext'
import IconField from 'primevue/iconfield'
import InputIcon from 'primevue/inputicon'
import { useDataTableExport } from '@/shared/composables/useDataTableExport'
import { useProfileList } from '../composables/useProfileList'
import type { ProfileListItem } from '../types/profile'
import { CUSTOMER_SEARCH_FIELDS } from '../types/profile'

const router = useRouter()
const { dt, exportCSV } = useDataTableExport()
const selectedItems = ref<ProfileListItem[]>([])
const search = ref('')

const { items, loading, setSearch, refresh } = useProfileList({
  defaultSearchFields: CUSTOMER_SEARCH_FIELDS,
})

function onSearch(value: string) {
  search.value = value
  setSearch(value)
}

function clearSearch() {
  search.value = ''
  setSearch('')
}

function navigateToDetail(id: string) {
  router.push(`/customer/profiles/${id}`)
}
</script>

<template>
  <div class="flex flex-col h-full">
    <!-- Section: Page Header — title and one-line description -->
    <div class="mb-6">
      <h1 class="text-2xl font-semibold mb-1">Profiles</h1>
      <p class="text-muted-color">View customer profiles</p>
    </div>

    <!-- Section: Search & Filters — search box and list-level actions -->
    <div class="flex items-center gap-3 mb-4 flex-wrap">
      <IconField>
        <InputIcon class="pi pi-search" />
        <InputText
          :model-value="search"
          placeholder="Search profiles..."
          @update:model-value="onSearch($event ?? '')"
        />
      </IconField>
      <Button
        v-if="search"
        label="Clear"
        severity="secondary"
        icon="pi pi-times"
        @click="clearSearch"
      />
      <div class="flex-1" />
      <Button
        label="Reload"
        icon="pi pi-refresh"
        severity="secondary"
        @click="refresh"
      />
      <Button
        label="Export"
        icon="pi pi-download"
        severity="secondary"
        @click="exportCSV"
      />
    </div>

    <!-- Section: Data Table — read-only customer profile grid -->
    <DataTable
      ref="dt"
      v-model:selection="selectedItems"
      :value="items"
      :loading="loading"
      scrollable
      paginator
      :rows="20"
      :rows-per-page-options="[10, 20, 50]"
      data-key="id"
    >
      <Column selection-mode="multiple" header-style="width: 3rem" />
      <!-- Section: Table Columns — profile identity and contact fields -->
      <Column field="fullName" header="Name" />
      <Column field="firstName" header="First Name" :sortable="true" />
      <Column field="lastName" header="Last Name" :sortable="true" />
      <Column field="email" header="Email" />
      <Column field="phoneNumber" header="Phone">
        <template #body="{ data }">
          {{ data.phoneNumber ?? '—' }}
        </template>
      </Column>
      <!-- Section: Row Actions — view profile detail -->
      <Column header="Actions" header-style="width:5rem">
        <template #body="{ data }">
          <Button icon="pi pi-eye" severity="secondary" text rounded @click="navigateToDetail(data.userId)" />
        </template>
      </Column>
      <!-- Section: Empty State — shown when no profiles match -->
      <template #empty>No profiles found.</template>
    </DataTable>
  </div>
</template>
