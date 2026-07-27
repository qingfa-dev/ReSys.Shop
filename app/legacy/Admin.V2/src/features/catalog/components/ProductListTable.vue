<script setup lang="ts">
import { onMounted, ref, computed } from 'vue'
import { useRouter } from 'vue-router'
import Column from 'primevue/column'
import DataTable from '@/shared/components/data/DataTable.vue'
import TableToolbar from '@/shared/components/layout/TableToolbar.vue'
import FilterPanel from '@/shared/components/layout/FilterPanel.vue'
import type { ColumnFilterDef, FilterConfig } from '@/shared/components/layout/FilterPanel.vue'
import ActionMenu from '@/shared/components/layout/ActionMenu.vue'
import StatusTag from '@/shared/components/data/StatusTag.vue'
import EmptyState from '@/shared/components/feedback/EmptyState.vue'
import LoadingSkeleton from '@/shared/components/feedback/LoadingSkeleton.vue'
import ErrorState from '@/shared/components/feedback/ErrorState.vue'
import { useI18n } from 'vue-i18n'
import { useConfirm } from '@/shared/composables/useConfirm'
import { useToast } from '@/shared/composables/useToast'
import { useProductStore } from '../store/product.store'
import { ProductApi } from '../api'
import { ROUTE } from '../routes'

const router = useRouter()
const { confirmDelete } = useConfirm()
const toast = useToast()
const { t } = useI18n()
const store = useProductStore()

const showFilters = ref(false)

const productFilterDefs: ColumnFilterDef[] = [
  { field: 'name', label: t('catalog.products.labels.name'), type: 'text' },
  { field: 'status', label: t('catalog.products.labels.status'), type: 'select', options: [
    { label: 'Active', value: 'Active' },
    { label: 'Draft', value: 'Draft' },
    { label: 'Discontinued', value: 'Discontinued' },
  ] },
  { field: 'price', label: t('catalog.products.labels.price'), type: 'number-range' },
]

onMounted(() => store.fetchMany())

function goToCreate() { router.push({ name: ROUTE.PRODUCTS.CREATE }) }
function goToView(id: string) { router.push({ name: ROUTE.PRODUCTS.VIEW, params: { id } }) }
function goToEdit(id: string) { router.push({ name: ROUTE.PRODUCTS.EDIT, params: { id } }) }

async function onDelete(id: string) {
  confirmDelete({
    target: 'this product',
    onAccept: async () => {
      const result = await ProductApi.delete(id)
      if (result.isSuccess) { toast.success(t('catalog.products.messages.delete_success')); await store.fetchMany() }
      else { toast.error(result.message ?? 'Failed to delete') }
    },
  })
}

function onPageChange(e: { page: number; rows: number }) { store.setPage(e.page + 1) }

const activeFilterChips = computed(() =>
  store.activeFilters.map(f => ({ field: f.field, value: String(f.value), label: f.label }))
)

function onFiltersApplied(filters: FilterConfig[]) {
  store.setFilters(filters)
}

function onClearFilters() {
  store.setFilters([])
}

function onFilterChipsChanged(chips: { field: string; value: string; label: string }[]) {
  const remainingFields = new Set(chips.map(c => c.field))
  store.setFilters(store.activeFilters.filter(f => remainingFields.has(f.field)))
}
</script>

<template>
  <div>
    <TableToolbar
      v-model:query="store.searchQuery"
      :filters="activeFilterChips"
      :search-placeholder="t('catalog.products.placeholders.search')"
      :create-label="t('catalog.products.actions.new')"
      :show-filter="true"
      @create="goToCreate"
      @toggle-filter="showFilters = !showFilters"
      @update:filters="onFilterChipsChanged"
    >
      <template #secondary-actions>
        <slot name="secondary-actions" />
      </template>
    </TableToolbar>

    <FilterPanel
      v-model:visible="showFilters"
      :definitions="productFilterDefs"
      :active-filters="store.activeFilters"
      @apply="onFiltersApplied"
      @clear="onClearFilters"
    />

    <LoadingSkeleton v-if="store.loading && store.items.length === 0" :rows="5" :columns="4" />
    <ErrorState v-else-if="store.error" :description="store.error" @retry="store.fetchMany" />
    <EmptyState v-else-if="store.items.length === 0" :title="t('catalog.products.messages.empty_list')" description="Create your first product." />
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
