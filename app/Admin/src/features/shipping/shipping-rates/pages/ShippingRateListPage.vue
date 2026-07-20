<script setup lang="ts">
import { onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useI18n } from 'vue-i18n'
import { useShippingRateStore } from '../store/shipping-rate.store'
import { storeToRefs } from 'pinia'
import { shippingRateRepository } from '../api/shipping-rate.api'
import { useToast } from '@/common/composables/toast.use'
import { useConfirm } from 'primevue/useconfirm'
import type { DataTablePageEvent, DataTableSortEvent } from 'primevue/datatable'
import PageShell from '@/shared/components/navigation/PageShell.vue'
import PageHeader from '@/shared/components/navigation/PageHeader.vue'
import DataTableShell from '@/shared/components/tables/DataTableShell.vue'
import ConfirmButton from '@/shared/components/base/ConfirmButton.vue'
import type { ColumnDef } from '@/shared/components/tables/DataTableShell.vue'

const { t } = useI18n()
const router = useRouter()
const store = useShippingRateStore()
const confirm = useConfirm()
const { showToast } = useToast()
const { items, loading, totalRecords } = storeToRefs(store)

const columns: ColumnDef[] = [
  { field: 'shippingMethodName', header: t('shipping.labels.shipping_method') || 'Shipping Method', sortable: true },
  { field: 'name', header: t('shipping.labels.name') || 'Name', sortable: true },
  { field: 'costDisplay', header: t('shipping.labels.rate') || 'Rate', sortable: true },
  { field: 'fromWeight', header: t('shipping.labels.from_weight') || 'Min Weight', sortable: false },
  { field: 'toWeight', header: t('shipping.labels.to_weight') || 'Max Weight', sortable: false },
]

function onPage(event: DataTablePageEvent) {
  store.fetchItems({ page: event.page + 1, pageSize: event.rows })
}
function onSort(event: DataTableSortEvent) {
  store.fetchItems({ sort: event.sortField ? [`${event.sortOrder === -1 ? '-' : ''}${event.sortField}`] : undefined })
}
function refresh() { store.fetchItems({}) }

function onEdit(id: string) {
  router.push({ name: 'shipping.rates.edit', params: { id } })
}

function onDelete(id: string) {
  confirm.require({
    message: t('shipping.messages.confirm_delete_rate') || 'Delete this shipping rate?',
    header: t('shipping.titles.confirm_delete') || 'Confirm Delete',
    icon: 'pi pi-exclamation-triangle',
    acceptClass: 'p-button-danger',
    accept: async () => {
      const result = await shippingRateRepository.delete(id)
      if (result.isSuccess) {
        showToast('success', t('common.success') || 'Success', t('shipping.messages.rate_deleted') || 'Shipping rate deleted')
        store.fetchItems({})
      } else {
        showToast('error', t('common.error') || 'Error', result.message || 'Delete failed')
      }
    },
  })
}

onMounted(() => store.fetchItems({}))
</script>

<template>
  <PageShell max-width="7xl">
    <PageHeader
      :title="t('shipping.titles.rate_list') || 'Shipping Rates'"
      :description="t('shipping.descriptions.rate_list') || 'Configure shipping rates for each method'"
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
      :search-placeholder="t('shipping.placeholders.search_rates') || 'Search rates...'"
      :empty-title="t('shipping.messages.empty_rates') || 'No shipping rates found'"
      :create-route="{ name: 'shipping.rates.create' }"
      :create-label="t('shipping.actions.create_rate') || 'Add Rate'"
      @page="onPage"
      @sort="onSort"
      @refresh="refresh"
    >
      <template #row-actions="{ data }">
        <Button icon="pi pi-pencil" severity="secondary" text rounded @click="onEdit(data.id)" />
        <ConfirmButton
          icon="pi pi-trash"
          severity="danger"
          :header="t('shipping.titles.confirm_delete') || 'Confirm Delete'"
          :message="t('shipping.messages.confirm_delete_rate') || 'Delete this shipping rate?'"
          @confirm="onDelete(data.id)"
        />
      </template>
    </DataTableShell>
  </PageShell>
</template>
