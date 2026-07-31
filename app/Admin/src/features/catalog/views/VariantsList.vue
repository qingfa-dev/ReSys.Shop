<script setup lang="ts">
import { ref, computed, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useConfirm } from 'primevue/useconfirm'
import DataTable from 'primevue/datatable'
import type { DataTablePageEvent, DataTableSortEvent } from 'primevue/datatable'
import Column from 'primevue/column'
import Tag from 'primevue/tag'
import Message from 'primevue/message'
import { usePagedQuery } from '@/shared/composables/usePagedQuery'
import { useNotify } from '@/shared/composables/useNotify'
import { VariantApi } from '../services/variantApi'
import type { VariantListItem } from '../types/variant'
import {
  VARIANT_FILTER_FIELDS,
  VARIANT_SORT_FIELDS,
  VARIANT_SEARCH_FIELDS,
} from '../types/variant'

const route = useRoute()
const router = useRouter()
const confirm = useConfirm()
const notify = useNotify()

const productId = computed(() => route.query.productId as string | undefined)
const searchTerm = ref('')

const {
  items,
  loading,
  error,
  totalCount,
  page,
  pageSize,
  totalPages,
  setPage,
  setPageSize,
  setSearch,
  setSort,
  refresh,
} = usePagedQuery<VariantListItem>(
  () => `api/catalog/variants?productId=${productId.value}`,
  {
    allowedFilterFields: VARIANT_FILTER_FIELDS,
    allowedSortFields: VARIANT_SORT_FIELDS,
    allowedSearchFields: VARIANT_SEARCH_FIELDS,
    defaultSearchFields: VARIANT_SEARCH_FIELDS,
    defaultSearchMode: 'any',
    defaultSort: ['position'],
    defaultPageSize: 20,
    immediate: false,
  },
)

const first = computed(() => (page.value - 1) * pageSize.value)

watch(productId, (id) => {
  if (id) {
    setSearch('')
    refresh()
  }
}, { immediate: true })

function navigateToNew() {
  if (!productId.value) return
  router.push(`/catalog/variants/new?productId=${productId.value}`)
}

function navigateToEdit(id: string) {
  router.push(`/catalog/variants/${id}`)
}

function navigateToProduct() {
  if (productId.value) {
    router.push(`/catalog/products/${productId.value}`)
  }
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

function confirmDelete(variant: VariantListItem) {
  confirm.require({
    message: `Are you sure you want to delete variant "${variant.sku}"?`,
    header: 'Confirm Delete',
    icon: 'pi pi-exclamation-triangle',
    rejectLabel: 'Cancel',
    acceptLabel: 'Delete',
    acceptClass: 'p-button-danger',
    accept: async () => {
      const result = await VariantApi.deleteVariant(variant.id)
      if (result.isSuccess) {
        notify.success('Variant deleted', `${variant.sku} has been removed.`)
        refresh()
      } else {
        notify.error('Delete failed', result.errors?.[0]?.message ?? 'Could not delete variant.')
      }
    },
  })
}

function refreshPage() {
  refresh()
}
</script>

<template>
  <div class="flex flex-col h-full p-4">
    <div class="flex-none flex flex-col gap-4">
      <div class="flex justify-between items-start">
        <div>
          <div class="font-semibold text-xl">Variants</div>
          <p class="text-muted-color mt-1">Manage product variants</p>
        </div>
        <Button
          label="Back to Product"
          icon="pi pi-arrow-left"
          severity="secondary"
          outlined
          @click="navigateToProduct"
        />
      </div>
    </div>

    <div class="flex-1 min-h-0 mt-4">
      <div v-if="!productId" class="flex items-center justify-center h-full">
        <div class="text-center text-muted-color">
          <i class="pi pi-info-circle text-4xl mb-3 block" />
          <p class="text-lg">Select a product to view its variants.</p>
          <p class="text-sm mt-1">Navigate from the Products list to manage variants.</p>
        </div>
      </div>

      <div v-else-if="error" class="flex items-center justify-center h-full">
        <Message severity="error" :closable="false" class="w-full max-w-lg">
          <div class="flex flex-col gap-2">
            <span>{{ error }}</span>
            <Button label="Reload" icon="pi pi-sync" severity="secondary" size="small" @click="refreshPage" />
          </div>
        </Message>
      </div>

      <DataTable
        v-else
        size="large"
        :value="items"
        :loading="loading"
        :total-records="totalCount"
        :first="first"
        :rows="pageSize"
        scrollable
        :paginator="true"
        data-key="id"
        :global-filter-fields="VARIANT_SEARCH_FIELDS"
        paginator-template="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink CurrentPageReport RowsPerPageDropdown"
        :rows-per-page-options="[5, 10, 25]"
        current-page-report-template="Showing {first} to {last} of {totalRecords}"
        @page="onPage"
        @update:rows="onRows"
        @sort="onSort"
        :pt="{ wrapper: { class: 'h-full' }, tableContainer: { class: 'h-full' } }"
      >
        <template #header>
          <div class="flex justify-between items-center">
            <div class="flex items-center gap-2">
              <FloatLabel variant="on">
                <IconField>
                  <InputIcon class="pi pi-search" />
                  <InputText
                    :model-value="searchTerm"
                    placeholder="Search variants..."
                    @update:model-value="onSearch($event ?? '')"
                  />
                </IconField>
                <label>Search</label>
              </FloatLabel>
              <Button label="Clear" outlined @click="clearSearch" />
            </div>
            <div class="flex items-center gap-2">
              <Button label="New Variant" icon="pi pi-plus" severity="primary" @click="navigateToNew" />
              <Button label="Reload" icon="pi pi-sync" severity="secondary" @click="refreshPage" />
            </div>
          </div>
        </template>
        <Column field="isMaster" header="Master" body-style="text-align: center">
          <template #body="{ data }">
            <Tag v-if="data.isMaster" value="Master" severity="info" />
            <span v-else class="text-muted-color">—</span>
          </template>
        </Column>
        <Column field="sku" header="SKU" :sortable="true">
          <template #body="{ data }">
            <span :class="{ 'text-muted-color': !data.sku }">{{ data.sku || '—' }}</span>
          </template>
        </Column>
        <Column field="position" header="Position" :sortable="true" body-style="text-align: center" />
        <Column field="price" header="Price">
          <template #body="{ data }">
            <span v-if="data.price != null">
              {{ data.price.toLocaleString() }} {{ data.costCurrency || '' }}
            </span>
            <span v-else class="text-muted-color">—</span>
          </template>
        </Column>
        <Column field="pricesCount" header="Prices" body-style="text-align: center" />
        <Column header="" body-style="text-align: right; width: 6rem">
          <template #body="{ data }">
            <div class="flex justify-end gap-2">
              <Button icon="pi pi-pencil" severity="secondary" text rounded aria-label="Edit" @click="navigateToEdit(data.id)" />
              <Button icon="pi pi-trash" severity="secondary" text rounded aria-label="Delete" @click="confirmDelete(data)" />
            </div>
          </template>
        </Column>
        <template #empty>
          <div class="text-center py-8 text-muted-color">No variants found for this product.</div>
        </template>
      </DataTable>
    </div>
  </div>
</template>
