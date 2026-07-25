<script setup lang="ts">
import { onMounted } from 'vue'
import { useRouter } from 'vue-router'
import Column from 'primevue/column'
import DataTable from '@/shared/components/data/DataTable.vue'
import TableToolbar from '@/shared/components/layout/TableToolbar.vue'
import ActionMenu from '@/shared/components/layout/ActionMenu.vue'
import EmptyState from '@/shared/components/feedback/EmptyState.vue'
import LoadingSkeleton from '@/shared/components/feedback/LoadingSkeleton.vue'
import ErrorState from '@/shared/components/feedback/ErrorState.vue'
import { useI18n } from 'vue-i18n'
import { useConfirm } from '@/shared/composables/useConfirm'
import { useToast } from '@/shared/composables/useToast'
import { useShippingRateStore } from '../store/shipping-rate.store'
import { ShippingRateApi } from '../api'
import { ROUTE } from '../routes'

const router = useRouter()
const { confirmDelete } = useConfirm()
const toast = useToast()
const { t } = useI18n()
const store = useShippingRateStore()

onMounted(() => store.fetchMany())

function goToCreate() { router.push({ name: ROUTE.RATES.CREATE }) }
function goToView(id: string) { router.push({ name: ROUTE.RATES.VIEW, params: { id } }) }
function goToEdit(id: string) { router.push({ name: ROUTE.RATES.EDIT, params: { id } }) }

async function onDelete(id: string) {
  confirmDelete({
    target: 'this shipping rate',
    onAccept: async () => {
      const result = await ShippingRateApi.delete(id)
      if (result.isSuccess) { toast.success('Shipping rate deleted'); await store.fetchMany() }
      else { toast.error(result.message ?? 'Failed to delete') }
    },
  })
}

function onSearch(value: string) { store.setSearch(value) }
function onPageChange(e: { page: number; rows: number }) { store.setPage(e.page + 1) }
</script>

<template>
  <div>
    <TableToolbar
      search-placeholder="Search shipping rates..."
      :create-label="t('shipping.rates.actions.create')"
      @search="onSearch"
      @create="goToCreate"
    />
    <LoadingSkeleton v-if="store.loading && store.items.length === 0" :rows="5" :columns="6" />
    <ErrorState v-else-if="store.error" :description="store.error" @retry="store.fetchMany" />
    <EmptyState v-else-if="store.items.length === 0" title="No shipping rates found" description="Create your first shipping rate." />
    <DataTable
      v-else
      :rows="[...store.items]"
      :loading="store.loading"
      :total-records="store.totalRecords"
      :page-size="store.query.pageSize"
      :first="(store.query.page - 1) * store.query.pageSize"
      @page="onPageChange"
    >
      <Column field="name" header="Name" sortable />
      <Column field="shippingMethodName" header="Shipping Method" />
      <Column field="rate" header="Rate">
        <template #body="{ data }">
          {{ data.currency }} {{ data.rate?.toFixed(2) }}
        </template>
      </Column>
      <Column field="minOrderAmount" header="Min Order" />
      <Column field="maxOrderAmount" header="Max Order" />
      <Column field="createdAt" header="Created" sortable />
      <template #rowActions="{ data }">
        <ActionMenu
          :items="[
            { label: 'View', icon: 'pi pi-eye', command: () => goToView(data.id) },
            { label: 'Edit', icon: 'pi pi-pencil', command: () => goToEdit(data.id) },
            { label: 'Delete', icon: 'pi pi-trash', command: () => onDelete(data.id) },
          ]"
        />
      </template>
    </DataTable>
  </div>
</template>
