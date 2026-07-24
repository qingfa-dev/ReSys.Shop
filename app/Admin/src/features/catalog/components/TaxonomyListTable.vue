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
import { useTaxonomyStore } from '../store/taxonomy.store'
import { TaxonomyApi } from '../api'
import { ROUTE } from '../routes'

const router = useRouter()
const { confirmDelete } = useConfirm()
const toast = useToast()
const { t } = useI18n()
const store = useTaxonomyStore()

onMounted(() => store.fetchMany())

function goToCreate() { router.push({ name: ROUTE.TAXONOMIES.CREATE }) }
function goToView(id: string) { router.push({ name: ROUTE.TAXONOMIES.VIEW, params: { id } }) }
function goToEdit(id: string) { router.push({ name: ROUTE.TAXONOMIES.EDIT, params: { id } }) }

async function onDelete(id: string) {
  confirmDelete({
    target: 'this taxonomy',
    onAccept: async () => {
      const result = await TaxonomyApi.delete(id)
      if (result.isSuccess) { toast.success(t('catalog.taxonomies.messages.delete_success')); await store.fetchMany() }
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
      search-placeholder="Search taxonomies..."
      :create-label="t('catalog.taxonomies.actions.create')"
      @search="onSearch"
      @create="goToCreate"
    />
    <LoadingSkeleton v-if="store.loading && store.items.length === 0" :rows="5" :columns="4" />
    <ErrorState v-else-if="store.error" :description="store.error" @retry="store.fetchMany" />
    <EmptyState v-else-if="store.items.length === 0" :title="t('catalog.taxonomies.messages.empty_list')" description="Create your first taxonomy." />
    <DataTable
      v-else
      :rows="store.items"
      :loading="store.loading"
      :total-records="store.totalRecords"
      :page-size="store.query.pageSize"
      :first="(store.query.page - 1) * store.query.pageSize"
      @page="onPageChange"
    >
      <Column field="name" header="Name" sortable />
      <Column field="presentation" header="Presentation" />
      <Column field="position" header="Position" />
      <Column field="createdAt" header="Created" />
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
