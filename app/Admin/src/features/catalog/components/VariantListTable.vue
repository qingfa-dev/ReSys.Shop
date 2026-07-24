<script setup lang="ts">
import { onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
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
import { useVariantStore } from '../store/variant.store'
import { VariantApi } from '../api'
import { ROUTE } from '../routes'

const router = useRouter()
const route = useRoute()
const { confirmDelete } = useConfirm()
const toast = useToast()
const { t } = useI18n()
const store = useVariantStore()
const productId = route.params.productId as string

onMounted(() => store.fetchMany(productId))

function goToCreate() { router.push({ name: ROUTE.VARIANTS.CREATE, params: { productId } }) }
function goToView(id: string) { router.push({ name: ROUTE.VARIANTS.VIEW, params: { productId, id } }) }
function goToEdit(id: string) { router.push({ name: ROUTE.VARIANTS.EDIT, params: { productId, id } }) }

async function onDelete(id: string) {
  confirmDelete({
    target: 'this variant',
    onAccept: async () => {
      const result = await VariantApi.delete(id)
      if (result.isSuccess) { toast.success(t('catalog.variants.messages.delete_success')); await store.fetchMany(productId) }
      else { toast.error(result.message ?? 'Failed to delete') }
    },
  })
}

function onSearch(value: string) { store.setSearch(value); store.fetchMany(productId) }
function onPageChange(e: { page: number; rows: number }) { store.setPage(e.page + 1); store.fetchMany(productId) }
</script>

<template>
  <div>
    <TableToolbar
      search-placeholder="Search variants..."
      :create-label="t('catalog.variants.actions.create')"
      @search="onSearch"
      @create="goToCreate"
    />
    <LoadingSkeleton v-if="store.loading && store.items.length === 0" :rows="5" :columns="5" />
    <ErrorState v-else-if="store.error" :description="store.error" @retry="store.fetchMany(productId)" />
    <EmptyState v-else-if="store.items.length === 0" :title="t('catalog.variants.messages.empty_list')" description="Create your first variant." />
    <DataTable
      v-else
      :rows="[...store.items]"
      :loading="store.loading"
      :total-records="store.totalRecords"
      :page-size="store.query.pageSize"
      :first="(store.query.page - 1) * store.query.pageSize"
      @page="onPageChange"
    >
      <Column field="sku" header="SKU" sortable />
      <Column field="position" header="Position" />
      <Column field="isMaster" header="Master">
        <template #body="{ data }">
          <i v-if="data.isMaster" class="pi pi-check" style="color: var(--p-green-500)" />
          <i v-else class="pi pi-times" style="color: var(--p-red-400)" />
        </template>
      </Column>
      <Column field="trackInventory" header="Track Inv.">
        <template #body="{ data }">
          <i v-if="data.trackInventory" class="pi pi-check" style="color: var(--p-green-500)" />
          <i v-else class="pi pi-times" style="color: var(--p-red-400)" />
        </template>
      </Column>
      <Column field="price" header="Price" />
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
