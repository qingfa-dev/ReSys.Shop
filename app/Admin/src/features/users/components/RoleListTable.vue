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
import Tag from 'primevue/tag'
import { useConfirm } from '@/shared/composables/useConfirm'
import { useToast } from '@/shared/composables/useToast'
import { useRoleStore } from '../store/role.store'
import { RoleApi } from '../api'
import { ROUTE } from '../routes'

const router = useRouter()
const { confirmDelete } = useConfirm()
const toast = useToast()
const store = useRoleStore()

onMounted(() => store.fetchMany())

function goToCreate() { router.push({ name: ROUTE.ROLES.CREATE }) }
function goToView(id: string) { router.push({ name: ROUTE.ROLES.VIEW, params: { id } }) }
function goToEdit(id: string) { router.push({ name: ROUTE.ROLES.EDIT, params: { id } }) }

async function onDelete(id: string) {
  confirmDelete({
    target: 'this role',
    onAccept: async () => {
      const result = await RoleApi.delete(id)
      if (result.isSuccess) { toast.success('Role deleted successfully'); await store.fetchMany() }
      else { toast.error(result.message ?? 'Failed to delete') }
    },
  })
}

function onPageChange(e: { page: number; rows: number }) { store.setPage(e.page + 1) }
</script>

<template>
  <div>
    <TableToolbar
      v-model:query="store.searchQuery"
      search-placeholder="Search roles..."
      create-label="Create Role"
      @create="goToCreate"
    />
    <LoadingSkeleton v-if="store.loading && store.items.length === 0" :rows="5" :columns="5" />
    <ErrorState v-else-if="store.error" :description="store.error" @retry="store.fetchMany" />
    <EmptyState v-else-if="store.items.length === 0" title="No roles found" description="Create your first role." />
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
      <Column field="description" header="Description" />
      <Column field="isSystem" header="System">
        <template #body="{ data }">
          <Tag :value="data.isSystem ? 'Yes' : 'No'" :severity="data.isSystem ? 'warn' : 'info'" />
        </template>
      </Column>
      <Column field="permissionCount" header="Permissions" sortable />
      <Column field="createdAt" header="Created" sortable>
        <template #body="{ data }">
          {{ new Date(data.createdAt).toLocaleDateString() }}
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
