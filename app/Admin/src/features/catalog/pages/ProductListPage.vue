<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import Column from 'primevue/column'
import DataTable from '@/shared/components/data/DataTable.vue'
import TableToolbar from '@/shared/components/layout/TableToolbar.vue'
import PageHeader from '@/shared/components/layout/PageHeader.vue'
import ActionMenu from '@/shared/components/layout/ActionMenu.vue'
import StatusTag from '@/shared/components/data/StatusTag.vue'
import EmptyState from '@/shared/components/feedback/EmptyState.vue'
import LoadingSkeleton from '@/shared/components/feedback/LoadingSkeleton.vue'
import ErrorState from '@/shared/components/feedback/ErrorState.vue'
import { useConfirm } from '@/shared/composables/useConfirm'
import { useToast } from '@/shared/composables/useToast'
import { getProducts, deleteProduct } from '../api/products'
import type { ProductResponse } from '../models/Product'
import { ROUTE_CATALOG } from '../routers/route-names'

const route = useRoute()
const router = useRouter()
const { confirmDelete } = useConfirm()
const toast = useToast()

const items = ref<ProductResponse[]>([])
const loading = ref(true)
const error = ref<string | null>(null)
const search = ref('')
const page = ref(1)
const pageSize = ref(20)
const totalCount = ref(0)

async function fetchProducts() {
  loading.value = true
  error.value = null
  const result = await getProducts({
    page: page.value,
    pageSize: pageSize.value,
    search: search.value || undefined,
  })
  if (result.success) {
    items.value = result.data
    totalCount.value = result.meta?.totalCount ?? 0
  } else {
    error.value = result.error?.message ?? 'Failed to load products'
  }
  loading.value = false
}

function goToCreate() { router.push({ name: ROUTE_CATALOG.PRODUCTS.CREATE }) }
function goToView(id: string) { router.push({ name: ROUTE_CATALOG.PRODUCTS.VIEW, params: { id } }) }
function goToEdit(id: string) { router.push({ name: ROUTE_CATALOG.PRODUCTS.EDIT, params: { id } }) }

async function onDelete(id: string) {
  confirmDelete({
    target: 'this product',
    onAccept: async () => {
      const result = await deleteProduct(id)
      if (result.success) { toast.success('Product deleted'); await fetchProducts() }
      else { toast.error(result.error?.message ?? 'Failed to delete') }
    },
  })
}

function onSearch(value: string) { search.value = value; page.value = 1; fetchProducts() }
function onPageChange(e: { page: number; rows: number }) {
  page.value = e.page + 1
  pageSize.value = e.rows
  fetchProducts()
}

onMounted(() => fetchProducts())
</script>

<template>
  <div>
    <PageHeader title="Products" subtitle="Manage product catalog" :icon="route.meta?.icon as string | undefined" />
    <TableToolbar
      search-placeholder="Search products..."
      create-label="Add Product"
      @search="onSearch"
      @create="goToCreate"
    />
    <LoadingSkeleton v-if="loading" :rows="5" :columns="4" />
    <ErrorState v-else-if="error" :description="error" @retry="fetchProducts" />
    <EmptyState v-else-if="items.length === 0" title="No products" description="Create your first product." />
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
      <Column field="slug" header="Slug" />
      <Column field="status" header="Status">
        <template #body="slotProps">
          <StatusTag :status="slotProps.data.status" />
        </template>
      </Column>
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
