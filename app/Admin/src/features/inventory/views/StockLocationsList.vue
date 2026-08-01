<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { useConfirm } from 'primevue/useconfirm'
import DataTable from 'primevue/datatable'
import Column from 'primevue/column'
import Tag from 'primevue/tag'
import Button from 'primevue/button'
import InputText from 'primevue/inputtext'
import IconField from 'primevue/iconfield'
import InputIcon from 'primevue/inputicon'
import { useDataTableExport } from '@/shared/composables/useDataTableExport'
import { useNotify } from '@/shared/composables/useNotify'
import { StockLocationApi } from '../services/stockLocationApi'
import type { StockLocationListItem } from '../types/stockLocation'
import { useStockLocationList } from '../composables/useStockLocationList'

const router = useRouter()
const confirm = useConfirm()
const notify = useNotify()
const { dt, exportCSV } = useDataTableExport()
const search = ref('')
const selectedItems = ref<StockLocationListItem[]>([])

const { items, loading, setSearch, refresh } = useStockLocationList({
  defaultSearchFields: ['name', 'code', 'city'],
})

function onSearch(value: string) {
  search.value = value
  setSearch(value)
}

function clearSearch() {
  search.value = ''
  setSearch('')
}

function navigateToNew() {
  router.push('/inventory/stock-locations/new')
}

function navigateToEdit(id: string) {
  router.push(`/inventory/stock-locations/${id}`)
}

function confirmDelete() {
  const names = selectedItems.value.map((l) => l.name).join(', ')
  confirm.require({
    message: `Delete ${names}? This action cannot be undone.`,
    header: 'Confirm Delete',
    icon: 'pi pi-exclamation-triangle',
    rejectLabel: 'Cancel',
    acceptLabel: 'Delete',
    acceptClass: 'p-button-danger',
    accept: async () => {
      for (const item of selectedItems.value) {
        const result = await StockLocationApi.deleteStockLocation(item.id)
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
      <h1 class="text-2xl font-semibold mb-1">Stock Locations</h1>
      <p class="text-muted-color">Manage stock locations</p>
    </div>

    <div class="flex items-center gap-3 mb-4">
      <IconField>
        <InputIcon class="pi pi-search" />
        <InputText
          :model-value="search"
          placeholder="Search stock locations..."
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
        label="New Location"
        icon="pi pi-plus"
        @click="navigateToNew"
      />
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
      <Column field="code" header="Code" :sortable="true" />
      <Column field="city" header="City" />
      <Column field="active" header="Active" :sortable="true">
        <template #body="{ data }">
          <Tag :value="data.active ? 'Yes' : 'No'" :severity="data.active ? 'success' : 'warn'" />
        </template>
      </Column>
      <Column field="default" header="Default" :sortable="true">
        <template #body="{ data }">
          <Tag :value="data.default ? 'Yes' : 'No'" :severity="data.default ? 'success' : 'warn'" />
        </template>
      </Column>
      <Column header="Actions" header-style="width:8rem">
        <template #body="{ data }">
          <Button icon="pi pi-pencil" severity="secondary" text rounded @click="navigateToEdit(data.id)" />
          <Button
            icon="pi pi-trash"
            severity="danger"
            text
            rounded
            @click="selectedItems = [data]; confirmDelete()"
          />
        </template>
      </Column>
      <template #empty>No stock locations found.</template>
    </DataTable>
  </div>
</template>
