<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import Column from 'primevue/column'
import DataTable from '@/shared/components/data/DataTable.vue'
import TableToolbar from '@/shared/components/layout/TableToolbar.vue'
import PageHeader from '@/shared/components/layout/PageHeader.vue'
import ActionMenu from '@/shared/components/layout/ActionMenu.vue'
import EmptyState from '@/shared/components/feedback/EmptyState.vue'
import LoadingSkeleton from '@/shared/components/feedback/LoadingSkeleton.vue'
import ErrorState from '@/shared/components/feedback/ErrorState.vue'
import { useConfirm } from '@/shared/composables/useConfirm'
import { useToast } from '@/shared/composables/useToast'
import { TaxonomyApi } from '../api'
import type { TaxonomyResponse } from '../types'
import { ROUTE } from '../routes'

const route = useRoute()
const router = useRouter()
const { confirmDelete } = useConfirm()
const toast = useToast()

const items = ref<TaxonomyResponse[]>([])
const loading = ref(true)
const error = ref<string | null>(null)
const search = ref('')
const page = ref(1)
const pageSize = ref(20)
const totalCount = ref(0)

async function fetchTaxonomies() {
  loading.value = true
  error.value = null
  const result = await TaxonomyApi.getMany({
    page: page.value,
    pageSize: pageSize.value,
    search: search.value || undefined,
  })
  if (result.isSuccess) {
    items.value = result.items
    totalCount.value = result.totalCount
  } else {
    error.value = result.message ?? 'Failed to load taxonomies'
  }
  loading.value = false
}

function goToCreate() { router.push({ name: ROUTE.TAXONOMIES.CREATE }) }
function goToView(id: string) { router.push({ name: ROUTE.TAXONOMIES.VIEW, params: { id } }) }
function goToEdit(id: string) { router.push({ name: ROUTE.TAXONOMIES.EDIT, params: { id } }) }

async function onDelete(id: string) {
  confirmDelete({
    target: 'this taxonomy',
    onAccept: async () => {
      const result = await TaxonomyApi.delete(id)
      if (result.isSuccess) { toast.success('Taxonomy deleted'); await fetchTaxonomies() }
      else { toast.error(result.message ?? 'Failed to delete') }
    },
  })
}

function onSearch(value: string) { search.value = value; page.value = 1; fetchTaxonomies() }
function onPageChange(e: { page: number; rows: number }) {
  page.value = e.page + 1
  pageSize.value = e.rows
  fetchTaxonomies()
}

onMounted(() => fetchTaxonomies())
</script>

<template>
  <div>
    <PageHeader title="Taxonomies" subtitle="Manage taxonomy groups" :icon="route.meta?.icon as string | undefined" />
    <TableToolbar
      search-placeholder="Search taxonomies..."
      create-label="Add Taxonomy"
      @search="onSearch"
      @create="goToCreate"
    />
    <LoadingSkeleton v-if="loading" :rows="5" :columns="4" />
    <ErrorState v-else-if="error" :description="error" @retry="fetchTaxonomies" />
    <EmptyState v-else-if="items.length === 0" title="No taxonomies" description="Create your first taxonomy." />
    <DataTable
      v-else
      :rows="items"
      :loading="loading"
      :total-records="totalCount"
      :page-size="pageSize"
      :first="(page - 1) * pageSize"
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
