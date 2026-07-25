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
import { useUserStore } from '../store/user.store'
import { UserApi } from '../api'
import { ROUTE } from '../routes'

const props = defineProps<{
  userType: 'staff' | 'customer'
}>()

const router = useRouter()
const { confirmDelete } = useConfirm()
const toast = useToast()
const store = useUserStore()

onMounted(() => store.fetchMany())

const routeMap = { staff: ROUTE.STAFF, customer: ROUTE.CUSTOMERS }
const routes = routeMap[props.userType]

function goToCreate() { router.push({ name: routes.CREATE }) }
function goToView(id: string) { router.push({ name: routes.VIEW, params: { id } }) }
function goToEdit(id: string) { router.push({ name: routes.EDIT, params: { id } }) }

async function onDelete(id: string) {
  confirmDelete({
    target: 'this user',
    onAccept: async () => {
      const result = await UserApi.delete(id)
      if (result.isSuccess) { toast.success('User deleted successfully'); await store.fetchMany() }
      else { toast.error(result.message ?? 'Failed to delete') }
    },
  })
}

async function onToggleStatus(id: string, isActive: boolean) {
  const result = await UserApi.toggleStatus(id, { isActive: !isActive })
  if (result.isSuccess) { toast.success('Status updated'); await store.fetchMany() }
  else { toast.error(result.message ?? 'Failed to update status') }
}

function onSearch(value: string) { store.setSearch(value) }
function onPageChange(e: { page: number; rows: number }) { store.setPage(e.page + 1) }
</script>

<template>
  <div>
    <TableToolbar
      search-placeholder="Search users..."
      :create-label="'Create ' + userType"
      @search="onSearch"
      @create="goToCreate"
    />
    <LoadingSkeleton v-if="store.loading && store.items.length === 0" :rows="5" :columns="6" />
    <ErrorState v-else-if="store.error" :description="store.error" @retry="store.fetchMany" />
    <EmptyState v-else-if="store.items.length === 0" title="No users found" description="Create your first user." />
    <DataTable
      v-else
      :rows="[...store.items]"
      :loading="store.loading"
      :total-records="store.totalRecords"
      :page-size="store.query.pageSize"
      :first="(store.query.page - 1) * store.query.pageSize"
      @page="onPageChange"
    >
      <Column field="email" header="Email" sortable />
      <Column field="userName" header="Username" sortable />
      <Column header="Name">
        <template #body="{ data }">
          {{ data.firstName }} {{ data.lastName }}
        </template>
      </Column>
      <Column field="isActive" header="Active">
        <template #body="{ data }">
          <Tag :value="data.isActive ? 'Active' : 'Inactive'" :severity="data.isActive ? 'success' : 'danger'" />
        </template>
      </Column>
      <Column header="Roles">
        <template #body="{ data }">
          <div class="flex gap-1 flex-wrap">
            <Tag v-for="role in data.roles" :key="role.id" :value="role.name" severity="info" />
          </div>
        </template>
      </Column>
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
            { label: data.isActive ? 'Deactivate' : 'Activate', icon: 'pi pi-power-off', command: () => onToggleStatus(data.id, data.isActive) },
            { label: 'Delete', icon: 'pi pi-trash', command: () => onDelete(data.id) },
          ]"
        />
      </template>
    </DataTable>
  </div>
</template>
