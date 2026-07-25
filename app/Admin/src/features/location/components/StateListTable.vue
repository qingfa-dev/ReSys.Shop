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
import { useConfirm } from '@/shared/composables/useConfirm'
import { useToast } from '@/shared/composables/useToast'
import { useStateStore } from '../store/state.store'
import { StateApi } from '../api'

const router = useRouter()
const { confirmDelete } = useConfirm()
const toast = useToast()
const store = useStateStore()

onMounted(() => store.fetchMany())

function goToCreate() { router.push({ name: 'location.states.create' }) }
function goToView(id: string) { router.push({ name: 'location.states.view', params: { id } }) }
function goToEdit(id: string) { router.push({ name: 'location.states.edit', params: { id } }) }

async function onDelete(id: string) {
  confirmDelete({
    target: 'this state/province',
    onAccept: async () => {
      const result = await StateApi.delete(id)
      if (result.isSuccess) { toast.success('State deleted successfully'); await store.fetchMany() }
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
      search-placeholder="Search states..."
      create-label="Create State/Province"
      @search="onSearch"
      @create="goToCreate"
    />
    <LoadingSkeleton v-if="store.loading && store.items.length === 0" :rows="5" :columns="5" />
    <ErrorState v-else-if="store.error" :description="store.error" @retry="store.fetchMany" />
    <EmptyState v-else-if="store.items.length === 0" title="No states found" description="Create your first state/province." />
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
      <Column field="isoCode" header="ISO Code" />
      <Column field="countryName" header="Country" />
      <Column field="isActive" header="Active">
        <template #body="{ data }">
          <i v-if="data.isActive" class="pi pi-check" style="color: var(--p-green-500)" />
          <i v-else class="pi pi-times" style="color: var(--p-red-400)" />
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
