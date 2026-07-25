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
import { useAddressStore } from '../store/address.store'
import { AddressApi } from '../api'

const router = useRouter()
const { confirmDelete } = useConfirm()
const toast = useToast()
const store = useAddressStore()

onMounted(() => store.fetchMany())

function goToCreate() { router.push({ name: 'profile.addresses.create' }) }
function goToView(id: string) { router.push({ name: 'profile.addresses.view', params: { id } }) }
function goToEdit(id: string) { router.push({ name: 'profile.addresses.edit', params: { id } }) }

async function onDelete(id: string) {
  confirmDelete({
    target: 'this address',
    onAccept: async () => {
      const result = await AddressApi.delete(id)
      if (result.isSuccess) { toast.success('Address deleted successfully'); await store.fetchMany() }
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
      search-placeholder="Search addresses..."
      create-label="Create Address"
      @search="onSearch"
      @create="goToCreate"
    />
    <LoadingSkeleton v-if="store.loading && store.items.length === 0" :rows="5" :columns="6" />
    <ErrorState v-else-if="store.error" :description="store.error" @retry="store.fetchMany" />
    <EmptyState v-else-if="store.items.length === 0" title="No addresses found" description="Create your first address." />
    <DataTable
      v-else
      :rows="[...store.items]"
      :loading="store.loading"
      :total-records="store.totalRecords"
      :page-size="store.query.pageSize"
      :first="(store.query.page - 1) * store.query.pageSize"
      @page="onPageChange"
    >
      <Column field="firstName" header="First Name" />
      <Column field="lastName" header="Last Name" />
      <Column field="address1" header="Address" />
      <Column field="city" header="City" />
      <Column field="country" header="Country" />
      <Column field="isDefault" header="Default">
        <template #body="{ data }">
          <i v-if="data.isDefault" class="pi pi-check" style="color: var(--p-green-500)" />
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
