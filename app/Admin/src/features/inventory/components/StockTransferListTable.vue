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
import { useStockTransferStore } from '../store/stock-transfer.store'
import { ROUTE } from '../routes'

const router = useRouter()
const { t } = useI18n()
const store = useStockTransferStore()

onMounted(() => store.fetchMany())

function goToCreate() { router.push({ name: 'inventory.transfers.create' }) }
function goToView(id: string) { router.push({ name: 'inventory.transfers.view', params: { id } }) }

function onSearch(value: string) { store.setSearch(value) }
function onPageChange(e: { page: number; rows: number }) { store.setPage(e.page + 1) }
</script>

<template>
  <div>
    <TableToolbar
      search-placeholder="Search transfers..."
      :create-label="t('inventory.transfers.actions.create')"
      @search="onSearch"
      @create="goToCreate"
    />
    <LoadingSkeleton v-if="store.loading && store.items.length === 0" :rows="5" :columns="6" />
    <ErrorState v-else-if="store.error" :description="store.error" @retry="store.fetchMany" />
    <EmptyState v-else-if="store.items.length === 0" title="No transfers found" description="Create your first stock transfer." />
    <DataTable
      v-else
      :rows="[...store.items]"
      :loading="store.loading"
      :total-records="store.totalRecords"
      :page-size="store.query.pageSize"
      :first="(store.query.page - 1) * store.query.pageSize"
      @page="onPageChange"
    >
      <Column field="reference" header="Reference" sortable />
      <Column field="sourceLocationName" header="From" />
      <Column field="destinationLocationName" header="To" />
      <Column field="status" header="Status">
        <template #body="{ data }">
          <Tag :severity="data.status === 'Completed' ? 'success' : data.status === 'Cancelled' ? 'danger' : 'warn'" :value="data.status" />
        </template>
      </Column>
      <Column field="createdAt" header="Created" sortable />
      <template #rowActions="{ data }">
        <ActionMenu
          :items="[
            { label: 'View', icon: 'pi pi-eye', command: () => goToView(data.id) },
          ]"
        />
      </template>
    </DataTable>
  </div>
</template>
