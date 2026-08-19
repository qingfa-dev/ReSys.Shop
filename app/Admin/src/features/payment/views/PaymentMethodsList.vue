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
import { PaymentMethodApi } from '../services/paymentMethodApi'
import { usePaymentMethodList } from '../composables/usePaymentMethodList'
import type { PaymentMethodListItem } from '../types/paymentMethod'

const router = useRouter()
const confirm = useConfirm()
const notify = useNotify()
const { dt, exportCSV } = useDataTableExport()
const selectedItems = ref<PaymentMethodListItem[]>([])
const search = ref('')

const { items, loading, setSearch, refresh } = usePaymentMethodList({
  defaultSearchFields: ['name', 'code', 'description'],
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
  router.push('/payment/payment-methods/new')
}

function navigateToEdit(id: string) {
  router.push(`/payment/payment-methods/${id}`)
}

function confirmDelete(item: PaymentMethodListItem) {
  // Trigger: Confirm before deleting the payment method.
  confirm.require({
    message: `Delete payment method "${item.name}"? This action cannot be undone.`,
    header: 'Confirm Delete',
    icon: 'pi pi-exclamation-triangle',
    rejectLabel: 'Cancel',
    acceptLabel: 'Delete',
    acceptClass: 'p-button-danger',
    accept: async () => {
      const result = await PaymentMethodApi.deletePaymentMethod(item.id)
      if (result.isSuccess) {
        notify.success('Deleted', item.name)
      } else {
        notify.error('Failed', `${item.name}: ${result.message}`)
      }
      refresh()
    },
  })
}

function bulkConfirmDelete() {
  if (selectedItems.value.length === 0) return

  confirm.require({
    message: `Are you sure you want to delete ${selectedItems.value.length > 1 ? 'these payment methods' : 'this payment method'}?`,
    header: 'Confirm Delete',
    icon: 'pi pi-exclamation-triangle',
    rejectLabel: 'Cancel',
    acceptLabel: 'Delete',
    acceptClass: 'p-button-danger',
    accept: async () => {
      const ids = selectedItems.value.map(i => i.id)
      const names = selectedItems.value.map(i => i.name)
      let failed = 0
      for (const id of ids) {
        const result = await PaymentMethodApi.deletePaymentMethod(id)
        if (!result.isSuccess) failed++
      }
      selectedItems.value = []
      refresh()
      if (failed === 0) {
        notify.success(
          ids.length > 1 ? 'Payment methods deleted' : 'Payment method deleted',
          ids.length > 1
            ? `${ids.length} payment methods have been removed.`
            : `${names[0]} has been removed.`,
        )
      } else {
        notify.error(
          'Delete failed',
          `${failed} of ${ids.length} could not be deleted.`,
        )
      }
    },
  })
}
</script>

<template>
  <div class="flex flex-col h-full">
    <!-- Section: Page Header — title and one-line description -->
    <div class="mb-6">
      <h1 class="text-2xl font-semibold mb-1">Payment Methods</h1>
      <p class="text-muted-color">Manage payment methods</p>
    </div>

    <!-- Section: Search & Filters — search box and list-level actions -->
    <div class="flex items-center gap-3 mb-4">
      <IconField>
        <InputIcon class="pi pi-search" />
        <InputText
          :model-value="search"
          placeholder="Search payment methods..."
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
        v-if="selectedItems.length > 0"
        label="Delete"
        icon="pi pi-trash"
        severity="danger"
        @click="bulkConfirmDelete"
      />
      <Button
        label="New"
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

    <!-- Section: Data Table — payment method grid -->
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
      <!-- Section: Table Columns — method identity and configuration fields -->
      <Column field="name" header="Name" :sortable="true" />
      <Column field="code" header="Code">
        <template #body="{ data }">
          {{ data.code ?? '—' }}
        </template>
      </Column>
      <Column field="providerKey" header="Provider" />
      <Column field="active" header="Active" :sortable="true">
        <template #body="{ data }">
          <Tag :value="data.active ? 'Yes' : 'No'" :severity="data.active ? 'success' : 'warn'" />
        </template>
      </Column>
      <Column field="autoCapture" header="Auto Capture">
        <template #body="{ data }">
          <Tag :value="data.autoCapture ? 'Yes' : 'No'" :severity="data.autoCapture ? 'success' : 'warn'" />
        </template>
      </Column>
      <Column field="displayOn" header="Display On" />
      <!-- Section: Row Actions — edit and delete per method -->
      <Column header="Actions" header-style="width:8rem">
        <template #body="{ data }">
          <Button icon="pi pi-pencil" severity="secondary" text rounded @click="navigateToEdit(data.id)" />
          <Button
            icon="pi pi-trash"
            severity="danger"
            text
            rounded
            @click="confirmDelete(data)"
          />
        </template>
      </Column>
      <!-- Section: Empty State — shown when no methods match -->
      <template #empty>No payment methods found.</template>
    </DataTable>
  </div>
</template>
