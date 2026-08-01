<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { useConfirm } from 'primevue/useconfirm'
import DataTable from 'primevue/datatable'
import Column from 'primevue/column'
import Button from 'primevue/button'
import InputText from 'primevue/inputtext'
import IconField from 'primevue/iconfield'
import InputIcon from 'primevue/inputicon'
import { useDataTableExport } from '@/shared/composables/useDataTableExport'
import { usePagedQuery } from '@/shared/composables/usePagedQuery'
import { useNotify } from '@/shared/composables/useNotify'
import { IDENTITY } from '@/shared/constants/api'
import { RoleApi } from '../services/roleApi'
import type { RoleListItem } from '../types/role'
import { ROLE_FILTER_FIELDS, ROLE_SORT_FIELDS, ROLE_SEARCH_FIELDS } from '../types/role'

const router = useRouter()
const confirm = useConfirm()
const notify = useNotify()
const { dt, exportCSV } = useDataTableExport()
const search = ref('')
const selectedItems = ref<RoleListItem[]>([])

const { items, loading, setSearch, refresh } = usePagedQuery<RoleListItem>(
  `${IDENTITY}/roles`,
  {
    allowedFilterFields: ROLE_FILTER_FIELDS,
    allowedSortFields: ROLE_SORT_FIELDS,
    allowedSearchFields: ROLE_SEARCH_FIELDS,
    defaultSearchFields: ['name'],
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

function navigateToNew() {
  router.push('/identity/roles/new')
}

function navigateToEdit(id: string) {
  router.push(`/identity/roles/${id}`)
}

function confirmDelete() {
  const names = selectedItems.value.map((r) => r.name).join(', ')
  confirm.require({
    message: `Delete role${selectedItems.value.length > 1 ? 's' : ''} "${names}"? This action cannot be undone.`,
    header: 'Confirm Delete',
    icon: 'pi pi-exclamation-triangle',
    rejectLabel: 'Cancel',
    acceptLabel: 'Delete',
    acceptClass: 'p-button-danger',
    accept: async () => {
      for (const item of selectedItems.value) {
        const result = await RoleApi.deleteRole(item.id)
        if (result.isSuccess) {
          notify.success('Deleted', item.name)
        } else {
          notify.error('Failed', `${item.name}: ${result.message}`)
        }
      }
      selectedItems.value = []
      refresh()
    },
  })
}
</script>

<template>
  <div class="flex flex-col h-full">
    <div class="mb-6">
      <h1 class="text-2xl font-semibold mb-1">Roles</h1>
      <p class="text-muted-color">Manage role definitions</p>
    </div>

    <div class="flex items-center gap-3 mb-4">
      <IconField>
        <InputIcon class="pi pi-search" />
        <InputText :model-value="search" placeholder="Search roles..." @update:model-value="onSearch($event ?? '')" />
      </IconField>
      <Button v-if="search" label="Clear" severity="secondary" icon="pi pi-times" @click="clearSearch" />
      <div class="flex-1" />
      <Button label="New Role" icon="pi pi-plus" @click="navigateToNew" />
      <Button label="Reload" icon="pi pi-refresh" severity="secondary" @click="refresh" />
      <Button label="Export" icon="pi pi-download" severity="secondary" @click="exportCSV" />
    </div>

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
      <Column selection-mode="multiple" header-style="width:3rem" />
      <Column field="name" header="Name" :sortable="true" />
      <Column header="Actions" header-style="width:8rem">
        <template #body="{ data }">
          <Button icon="pi pi-pencil" severity="secondary" text rounded @click="navigateToEdit(data.id)" />
          <Button icon="pi pi-trash" severity="danger" text rounded @click="selectedItems = [data]; confirmDelete()" />
        </template>
      </Column>
      <template #empty>No roles found.</template>
    </DataTable>
  </div>
</template>
