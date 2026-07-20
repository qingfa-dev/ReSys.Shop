<script setup lang="ts">
import { onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useI18n } from 'vue-i18n'
import { useShippingMethodStore } from '../stores/shipping-method.store'
import { storeToRefs } from 'pinia'
import { shippingMethodService } from '../services/shipping-method.service'
import { useToast } from '@/shared/composables/toast.use'
import { useConfirm } from 'primevue/useconfirm'
import type { DataTablePageEvent, DataTableSortEvent } from 'primevue/datatable'
import PageShell from '@/shared/components/PageShell.Component.vue'
import PageHeader from '@/shared/components/PageHeader.Component.vue'
import DataTableShell from '@/shared/components/DataTableShell.Component.vue'
import ConfirmButton from '@/shared/components/ConfirmButton.Component.vue'
import type { ColumnDef } from '@/shared/components/DataTableShell.Component.vue'

const { t } = useI18n()
const router = useRouter()
const store = useShippingMethodStore()
const confirm = useConfirm()
const { showToast } = useToast()
const { items, loading, totalRecords } = storeToRefs(store)

const columns: ColumnDef[] = [
  { field: 'name', header: t('shipping.labels.name') || 'Name', sortable: true },
  { field: 'carrier', header: t('shipping.labels.carrier') || 'Carrier', sortable: true },
  { field: 'statusLabel', header: t('shipping.labels.is_active') || 'Status', sortable: true },
  { field: 'displayOrder', header: t('shipping.labels.display_order') || 'Order', sortable: true },
]

function onPage(event: DataTablePageEvent) {
  store.fetchItems({ page: event.page + 1, pageSize: event.rows })
}
function onSort(event: DataTableSortEvent) {
  store.fetchItems({ sort: event.sortField ? [`${event.sortOrder === -1 ? '-' : ''}${event.sortField}`] : undefined })
}
function refresh() { store.fetchItems({}) }

function onEdit(id: string) {
  router.push({ name: 'shipping.methods.edit', params: { id } })
}

function onDelete(id: string) {
  confirm.require({
    message: t('shipping.messages.confirm_delete') || 'Delete this shipping method?',
    header: t('shipping.titles.confirm_delete') || 'Confirm Delete',
    icon: 'pi pi-exclamation-triangle',
    acceptClass: 'p-button-danger',
    accept: async () => {
      const result = await shippingMethodService.delete(id)
      if (result.isSuccess) {
        showToast('success', t('common.success') || 'Success', t('shipping.messages.deleted') || 'Shipping method deleted')
        store.fetchItems({})
      } else {
        showToast('error', t('common.error') || 'Error', result.message || 'Delete failed')
      }
    },
  })
}

async function onToggleActive(id: string, currentActive: boolean) {
  const result = currentActive
    ? await shippingMethodService.deactivate(id)
    : await shippingMethodService.activate(id)
  if (result.isSuccess) {
    showToast('success', t('common.success') || 'Success', currentActive ? 'Deactivated' : 'Activated')
    store.fetchItems({})
  } else {
    showToast('error', t('common.error') || 'Error', result.message || 'Failed to update status')
  }
}

onMounted(() => store.fetchItems({}))
</script>

<template>
  <PageShell max-width="7xl">
    <PageHeader
      :title="t('shipping.titles.list') || 'Shipping Methods'"
      :description="t('shipping.descriptions.list') || 'Manage shipping methods and their availability'"
    >
      <template #badge>
        <Badge :value="totalRecords" severity="info" />
      </template>
    </PageHeader>
    <DataTableShell
      :columns="columns"
      :value="items"
      :loading="loading"
      :total-records="totalRecords"
      :search-placeholder="t('shipping.placeholders.search_methods') || 'Search methods...'"
      :empty-title="t('shipping.messages.empty_methods') || 'No shipping methods found'"
      :create-route="{ name: 'shipping.methods.create' }"
      :create-label="t('shipping.actions.create_method') || 'Add Method'"
      @page="onPage"
      @sort="onSort"
      @refresh="refresh"
    >
      <template #row-actions="{ data }">
        <Button icon="pi pi-power-off" :severity="data.isActive ? 'warn' : 'success'" text rounded @click="onToggleActive(data.id, data.isActive)" />
        <Button icon="pi pi-pencil" severity="secondary" text rounded @click="onEdit(data.id)" />
        <ConfirmButton
          icon="pi pi-trash"
          severity="danger"
          :header="t('shipping.titles.confirm_delete') || 'Confirm Delete'"
          :message="t('shipping.messages.confirm_delete') || 'Delete this shipping method?'"
          @confirm="onDelete(data.id)"
        />
      </template>
    </DataTableShell>
  </PageShell>
</template>
