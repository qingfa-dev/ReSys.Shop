<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useConfirm } from 'primevue/useconfirm'
import DataTable from 'primevue/datatable'
import Column from 'primevue/column'
import Tag from 'primevue/tag'
import { useNotify } from '@/shared/composables/useNotify'
import { VariantApi } from '../services/variantApi'
import type { Variant } from '../types/variant'

const route = useRoute()
const router = useRouter()
const confirm = useConfirm()
const notify = useNotify()

const productId = computed(() => route.query.productId as string | undefined)
const items = ref<Variant[]>([])
const loading = ref(false)
const searchTerm = ref('')

async function loadVariants() {
  if (!productId.value) {
    items.value = []
    return
  }
  loading.value = true
  const result = await VariantApi.getVariants(productId.value)
  if (result.isSuccess && result.value) {
    items.value = result.value.items
  }
  loading.value = false
}

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
}

const filteredItems = computed(() => {
  if (!searchTerm.value) return items.value
  const q = searchTerm.value.toLowerCase()
  return items.value.filter(
    (v) => v.sku.toLowerCase().includes(q),
  )
})

function confirmDelete(variant: Variant) {
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
        await loadVariants()
      } else {
        notify.error('Delete failed', result.errors?.[0]?.message ?? 'Could not delete variant.')
      }
    },
  })
}

function refresh() {
  loadVariants()
}

function clearSearch() {
  searchTerm.value = ''
}

onMounted(() => {
  loadVariants()
})
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

      <DataTable
        v-else
        :value="filteredItems"
        :loading="loading"
        scrollable
        data-key="id"
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
              <Button label="Reload" icon="pi pi-sync" severity="secondary" @click="refresh" />
            </div>
          </div>
        </template>
        <Column field="isMaster" header="Master" body-style="text-align: center">
          <template #body="{ data }">
            <Tag v-if="data.isMaster" value="Master" severity="info" />
            <span v-else class="text-muted-color">—</span>
          </template>
        </Column>
        <Column field="sku" header="SKU">
          <template #body="{ data }">
            <span :class="{ 'text-muted-color': !data.sku }">{{ data.sku || '—' }}</span>
          </template>
        </Column>
        <Column field="position" header="Position" body-style="text-align: center" />
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
