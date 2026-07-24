<script setup lang="ts">
import { onMounted } from 'vue'
import { useRouter } from 'vue-router'
import Column from 'primevue/column'
import Tag from 'primevue/tag'
import DataTable from '@/shared/components/data/DataTable.vue'
import TableToolbar from '@/shared/components/layout/TableToolbar.vue'
import ActionMenu from '@/shared/components/layout/ActionMenu.vue'
import EmptyState from '@/shared/components/feedback/EmptyState.vue'
import LoadingSkeleton from '@/shared/components/feedback/LoadingSkeleton.vue'
import ErrorState from '@/shared/components/feedback/ErrorState.vue'
import { useI18n } from 'vue-i18n'
import { useConfirm } from '@/shared/composables/useConfirm'
import { useToast } from '@/shared/composables/useToast'
import { useStockItemStore } from '../store/stock-item.store'
import { StockItemApi } from '../api'
import { ROUTE } from '../routes'

const router = useRouter()
const { confirmDelete } = useConfirm()
const toast = useToast()
const { t } = useI18n()
const store = useStockItemStore()

onMounted(() => store.fetchMany())

function goToCreate() { router.push({ name: 'inventory.stocks.create' }) }
function goToView(id: string) { router.push({ name: 'inventory.stocks.view', params: { id } }) }
function goToEdit(id: string) { router.push({ name: 'inventory.stocks.edit', params: { id } }) }

async function onDelete(id: string) {
  confirmDelete({
    target: 'this stock item',
    onAccept: async () => {
      const result = await StockItemApi.delete(id)
      if (result.isSuccess) { toast.success('Stock item deleted'); await store.fetchMany() }
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
      search-placeholder="Search stock items..."
      :create-label="t('inventory.stock_items.actions.create')"
      @search="onSearch"
      @create="goToCreate"
    />
    <LoadingSkeleton v-if="store.loading && store.items.length === 0" :rows="5" :columns="8" />
    <ErrorState v-else-if="store.error" :description="store.error" @retry="store.fetchMany" />
    <EmptyState v-else-if="store.items.length === 0" title="No stock items found" description="Create your first stock item." />
    <DataTable
      v-else
      :rows="[...store.items]"
      :loading="store.loading"
      :total-records="store.totalRecords"
      :page-size="store.query.pageSize"
      :first="(store.query.page - 1) * store.query.pageSize"
      @page="onPageChange"
    >
      <Column field="variantSku" header="SKU" sortable />
      <Column field="variantName" header="Variant" sortable />
      <Column field="locationName" header="Location" />
      <Column field="quantity" header="On Hand" sortable />
      <Column field="reservedQuantity" header="Reserved" sortable />
      <Column field="availableQuantity" header="Available" sortable />
      <Column field="lowStockThreshold" header="Threshold" />
      <Column field="isLowStock" header="Low Stock">
        <template #body="{ data }">
          <Tag v-if="data.isLowStock" severity="warn" value="Low" />
          <Tag v-else severity="success" value="OK" />
        </template>
      </Column>
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
