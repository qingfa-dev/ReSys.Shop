<script setup lang="ts">
import { ref } from 'vue'
import DataTable from 'primevue/datatable'
import Column from 'primevue/column'
import Button from 'primevue/button'
import InputText from 'primevue/inputtext'
import IconField from 'primevue/iconfield'
import InputIcon from 'primevue/inputicon'
import { useDataTableExport } from '@/shared/composables/useDataTableExport'
import { formatDate } from '@/shared/utils/date'
import { useStockMovementList } from '../composables/useStockMovementList'
import type { StockMovementListItem } from '../types/stockMovement'

const { dt, exportCSV } = useDataTableExport()
const search = ref('')

const { items, loading, setSearch, refresh } = useStockMovementList({
  defaultSearchFields: ['reason'],
})

function onSearch(value: string) {
  search.value = value
  setSearch(value)
}

function clearSearch() {
  search.value = ''
  setSearch('')
}

function quantitySign(quantity: number): string {
  if (quantity > 0) return `+${quantity}`
  return `${quantity}`
}

function quantityClass(quantity: number): string {
  if (quantity > 0) return 'text-green-600'
  if (quantity < 0) return 'text-red-500'
  return ''
}

function originatorLabel(item: StockMovementListItem): string {
  if (!item.originatorType) return '—'
  return item.originatorType
}
</script>

<template>
  <div class="flex flex-col h-full">
    <div class="mb-6">
      <h1 class="text-2xl font-semibold mb-1">Stock Movements</h1>
      <p class="text-muted-color">Audit log of stock quantity changes</p>
    </div>

    <div class="flex items-center gap-3 mb-4">
      <IconField>
        <InputIcon class="pi pi-search" />
        <InputText
          :model-value="search"
          placeholder="Search by reason..."
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

    <DataTable
      ref="dt"
      :value="items"
      :loading="loading"
      scrollable
      paginator
      :rows="20"
      :rows-per-page-options="[10, 20, 50]"
      data-key="id"
    >
      <Column field="stockItemId" header="Stock Item (ID)" />
      <Column field="action" header="Action">
        <template #body="{ data }">
          {{ data.action ?? '—' }}
        </template>
      </Column>
      <Column field="quantity" header="Quantity" :sortable="true">
        <template #body="{ data }">
          <span :class="quantityClass(data.quantity)">{{ quantitySign(data.quantity) }}</span>
        </template>
      </Column>
      <Column field="previousCountOnHand" header="Previous Count" />
      <Column field="originatorType" header="Originator">
        <template #body="{ data }">
          {{ originatorLabel(data) }}
        </template>
      </Column>
      <Column field="reason" header="Reason">
        <template #body="{ data }">
          {{ data.reason ?? '—' }}
        </template>
      </Column>
      <Column field="createdAtUtc" header="Created" :sortable="true">
        <template #body="{ data }">
          {{ formatDate(data.createdAtUtc) }}
        </template>
      </Column>
      <template #empty>No stock movements found.</template>
    </DataTable>
  </div>
</template>
