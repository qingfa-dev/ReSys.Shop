<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useI18n } from 'vue-i18n'
import { taxonRepository } from '../api/taxon.api'
import { useApiErrorHandler } from '@/common/composables/api-error-handler.use'
import { useFormatter } from '@/common/composables/formatter.use'
import type { DataTablePageEvent } from 'primevue/datatable'
import type { ProductSummary } from '@/features/catalog/products/types/product.response'
import StatusBadge from '@/shared/components/feedback/StatusBadge.vue'

const productStatusMap: Record<string, { label: string; severity: string }> = {
  Active: { label: 'Active', severity: 'success' },
  Inactive: { label: 'Inactive', severity: 'secondary' },
  Draft: { label: 'Draft', severity: 'warning' },
}

const props = defineProps<{
  taxonId: string
}>()

const { handleApiResult } = useApiErrorHandler()
const { formatCurrency } = useFormatter()
const { t } = useI18n()

const products = ref<ProductSummary[]>([])
const loading = ref(false)
const totalRecords = ref(0)
const page = ref(1)
const pageSize = ref(10)

const loadPreview = async () => {
  loading.value = true
  const result = await taxonRepository.getProductPreview(props.taxonId, {
    page: page.value,
    pageSize: pageSize.value
  })
  
  if (result.isSuccess && result.value) {
    products.value = result.value.items
    totalRecords.value = result.value.totalCount
  } else {
    handleApiResult(result)
  }
  loading.value = false
}

const onPage = (event: DataTablePageEvent) => {
  page.value = (event.page || 0) + 1
  pageSize.value = event.rows
  loadPreview()
}

onMounted(() => {
  loadPreview()
})

defineExpose({
    refresh: loadPreview
})
</script>

<template>
  <div class="flex flex-col gap-4">
    <div class="flex items-center justify-between">
        <div>
            <h3 class="text-lg font-bold text-surface-900 dark:text-surface-0 m-0">Matching Products</h3>
            <p class="text-sm text-surface-500 m-0">Live preview of products that satisfy the current automation rules.</p>
        </div>
        <Button icon="pi pi-refresh" text rounded @click="loadPreview" :loading="loading" />
    </div>

    <div class="overflow-hidden border border-surface-100 dark:border-surface-800 rounded-2xl bg-surface-0 dark:bg-surface-900">
        <DataTable
            :value="products"
            :loading="loading"
            lazy
            paginator
            :rows="pageSize"
            :totalRecords="totalRecords"
            @page="onPage"
            class="p-datatable-sm"
            rowHover
        >
            <template #empty>
                <div class="py-12 text-center text-surface-400 italic">
                    No products match the defined rules.
                </div>
            </template>

            <Column field="imageUrl" :header="t('catalog.products.table.preview')" class="w-20">
                <template #body="{ data }">
                    <div class="w-10 h-10 rounded-lg overflow-hidden border border-surface-100 dark:border-surface-700 bg-surface-50 flex items-center justify-center">
                        <Image v-if="data.imageUrl" :src="data.imageUrl" preview class="w-full h-full object-cover" />
                        <i v-else class="pi pi-image text-surface-300"></i>
                    </div>
                </template>
            </Column>

            <Column field="name" :header="t('catalog.products.table.name')">
                <template #body="{ data }">
                    <span class="font-bold text-surface-900 dark:text-surface-0">{{ data.name }}</span>
                </template>
            </Column>

            <Column field="sku" header="SKU" class="font-mono text-xs uppercase text-surface-500"></Column>

            <Column field="price" :header="t('catalog.products.table.price')" class="text-right">
                <template #body="{ data }">
                    <span class="font-black">{{ formatCurrency(data.price) }}</span>
                </template>
            </Column>

            <Column field="status" :header="t('catalog.products.table.status')" class="text-center w-24">
                <template #body="{ data }">
                    <StatusBadge :status="data.status" :statusMap="productStatusMap" />
                </template>
            </Column>
        </DataTable>
    </div>
  </div>
</template>
