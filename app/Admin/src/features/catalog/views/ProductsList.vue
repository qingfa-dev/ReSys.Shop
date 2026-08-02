<script setup lang="ts">
import { ref, computed } from 'vue'
import { useRouter } from 'vue-router'
import { useConfirm } from 'primevue/useconfirm'
import DataTable from 'primevue/datatable'
import type { DataTablePageEvent, DataTableSortEvent } from 'primevue/datatable'
import Column from 'primevue/column'
import Tag from 'primevue/tag'
import Message from 'primevue/message'

import { useDataTableExport } from '@/shared/composables/useDataTableExport'
import { usePagedQuery } from '@/shared/composables/usePagedQuery'
import { useNotify } from '@/shared/composables/useNotify'
import { ProductApi } from '../services/productApi'
import type { ProductListItem } from '../types/product'
import { PRODUCT_FILTER_FIELDS, PRODUCT_SORT_FIELDS } from '../types/product'
import { statusAction } from '../utils/productStatus'

const router = useRouter()
const confirm = useConfirm()
const notify = useNotify()

const { dt, exportCSV } = useDataTableExport()
const selectedItems = ref<ProductListItem[]>([])
const searchTerm = ref('')
const allowedSearchFields = ['name', 'slug']

const {
  items,
  loading,
  error,
  totalCount,
  page,
  pageSize,
  setPage,
  setPageSize,
  setSearch,
  setSort,
  refresh,
} = usePagedQuery<ProductListItem>('api/catalog/products', {
  allowedFilterFields: PRODUCT_FILTER_FIELDS,
  allowedSortFields: PRODUCT_SORT_FIELDS,
  allowedSearchFields,
  defaultSearchFields: allowedSearchFields,
  defaultSearchMode: 'any',
  defaultSort: ['name'],
  defaultPageSize: 25,
})

const first = computed(() => (page.value - 1) * pageSize.value)

function navigateToNew() {
  router.push('/catalog/products/new')
}

function navigateToEdit(id: string) {
  router.push(`/catalog/products/${id}`)
}

function navigateToVariants(productId: string) {
  router.push(`/catalog/variants?productId=${productId}`)
}

function onSearch(value: string) {
  searchTerm.value = value
  setSearch(value)
}

function clearSearch() {
  searchTerm.value = ''
  setSearch('')
}

function onPage(event: DataTablePageEvent) {
  setPage(event.page + 1)
}

function onRows(rows: number) {
  setPageSize(rows)
}

function onSort(event: DataTableSortEvent) {
  const field = event.sortField
  if (typeof field !== 'string') return
  setSort(event.sortOrder === -1 ? [`-${field}`] : [field])
}

function confirmStatusChange(product: ProductListItem) {
  const action = statusAction(product.status)
  if (action.kind === 'none') return
  const isActivate = action.kind === 'activate'

  confirm.require({
    message: isActivate
      ? `Are you sure you want to activate "${product.name}"?`
      : `Are you sure you want to discontinue "${product.name}"?`,
    header: isActivate ? 'Confirm Activate' : 'Confirm Discontinue',
    icon: 'pi pi-exclamation-triangle',
    rejectLabel: 'Cancel',
    acceptLabel: isActivate ? 'Activate' : 'Discontinue',
    acceptClass: isActivate ? 'p-button-success' : 'p-button-warning',
    accept: async () => {
      const result = isActivate
        ? await ProductApi.activateProduct(product.id)
        : await ProductApi.discontinueProduct(product.id)
      if (result.isSuccess) {
        notify.success(
          isActivate ? 'Product activated' : 'Product discontinued',
          `${product.name} is now ${isActivate ? 'Active' : 'Discontinued'}.`,
        )
        refresh()
      } else {
        notify.error(
          isActivate ? 'Activate failed' : 'Discontinue failed',
          result.errors?.[0]?.message ?? 'Could not update status.',
        )
      }
    },
  })
}

function confirmDelete() {
  if (selectedItems.value.length === 0) return

  confirm.require({
    message: `Are you sure you want to delete ${selectedItems.value.length > 1 ? 'these products' : 'this product'}?`,
    header: 'Confirm Delete',
    icon: 'pi pi-exclamation-triangle',
    rejectLabel: 'Cancel',
    acceptLabel: 'Delete',
    acceptClass: 'p-button-danger',
    accept: async () => {
      const ids = selectedItems.value.map(i => i.id)
      const names = selectedItems.value.map(i => i.name)
      let failed = 0
      for (const id of ids) {
        const result = await ProductApi.deleteProduct(id)
        if (!result.isSuccess) failed++
      }
      selectedItems.value = []
      refresh()
      if (failed === 0) {
        notify.success(
          ids.length > 1 ? 'Products deleted' : 'Product deleted',
          ids.length > 1
            ? `${ids.length} products have been removed.`
            : `${names[0]} has been removed.`,
        )
      } else {
        notify.error(
          'Delete failed',
          `${failed} of ${ids.length} could not be deleted.`,
        )
      }
    },
  })
}
</script>

<template>
  <div class="flex flex-col h-full p-4">
    <div class="flex-none flex flex-col gap-4">
      <div>
        <div class="font-semibold text-xl">Products</div>
        <p class="text-muted-color mt-1">Manage the product catalog</p>
      </div>
    </div>

    <div class="flex-1 min-h-0 mt-4">
      <div v-if="error" class="flex items-center justify-center h-full">
        <Message severity="error" :closable="false" class="w-full max-w-lg">
          <div class="flex flex-col gap-2">
            <span>{{ error }}</span>
            <Button label="Reload" icon="pi pi-sync" severity="secondary" size="small" @click="refresh" />
          </div>
        </Message>
      </div>

      <DataTable v-else size="large"
        ref="dt"
        v-model:selection="selectedItems"
        :value="items"
        :loading="loading"
        :total-records="totalCount"
        :first="first"
        :rows="pageSize"
        scrollable
        :paginator="true"
        filter-display="menu"
        data-key="id"
        :global-filter-fields="allowedSearchFields"
        paginator-template="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink CurrentPageReport RowsPerPageDropdown"
        :rows-per-page-options="[5, 10, 25]"
        current-page-report-template="Showing {first} to {last} of {totalRecords}"
        @page="onPage"
        @update:rows="onRows"
        @sort="onSort"
        :pt="{ wrapper: { class: 'h-full' }, tableContainer: { class: 'h-full' } }"
      >
        <Column selection-mode="multiple" header-style="width: 3rem" />
        <template #header>
          <div class="flex justify-between items-center">
            <div class="flex items-center gap-2">
              <FloatLabel variant="on">
                <IconField>
                  <InputIcon class="pi pi-search" />
                  <InputText
                    :model-value="searchTerm"
                    placeholder="Search products..."
                    @update:model-value="onSearch($event ?? '')"
                  />
                </IconField>
                <label>Search</label>
              </FloatLabel>
              <Button label="Clear" outlined @click="clearSearch" />
            </div>
            <div class="flex items-center gap-2">
              <Button label="New Product" icon="pi pi-plus" severity="primary" @click="navigateToNew" />
              <Button label="Reload" icon="pi pi-sync" severity="secondary" @click="refresh" />
              <Button label="Export" icon="pi pi-upload" severity="secondary" @click="exportCSV" />
            </div>
          </div>
        </template>
        <Column field="name" header="Name" :sortable="true" :filter="true" filter-field="name" />
        <Column field="slug" header="Slug" :sortable="true" />
        <Column field="status" header="Status" :sortable="true" :filter="true" filter-field="status" body-style="text-align: center">
          <template #body="{ data }">
            <Tag :value="data.status" :severity="data.status === 'Active' ? 'success' : data.status === 'Draft' ? 'info' : 'danger'" />
          </template>
        </Column>
        <Column field="department" header="Department" :sortable="true" />
        <Column field="seasonName" header="Season" :sortable="true" />
        <Column field="variantsCount" header="Variants" :sortable="true" body-style="text-align: center" />
        <Column field="createdAtUtc" header="Created" :sortable="true" />
        <Column header="" body-style="text-align: right; width: 12rem">
          <template #body="{ data }">
            <div class="flex justify-end gap-2">
              <Button
                v-if="statusAction(data.status).kind === 'activate'"
                icon="pi pi-check-circle"
                severity="success"
                text
                rounded
                aria-label="Activate"
                @click="confirmStatusChange(data)"
              />
              <Button
                v-else-if="statusAction(data.status).kind === 'discontinue'"
                icon="pi pi-pause-circle"
                severity="warning"
                text
                rounded
                aria-label="Discontinue"
                @click="confirmStatusChange(data)"
              />
              <Button icon="pi pi-box" severity="secondary" text rounded aria-label="Variants" @click="navigateToVariants(data.id)" />
              <Button icon="pi pi-pencil" severity="secondary" text rounded aria-label="Edit" @click="navigateToEdit(data.id)" />
              <Button icon="pi pi-trash" severity="secondary" text rounded aria-label="Delete" @click="selectedItems = [data]; confirmDelete()" />
            </div>
          </template>
        </Column>
        <template #empty>
          <div class="text-center py-8 text-muted-color">No products found.</div>
        </template>
      </DataTable>
    </div>
  </div>
</template>
