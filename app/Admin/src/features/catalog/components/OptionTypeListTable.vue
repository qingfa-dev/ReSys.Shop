<script setup lang="ts">
import { ref, onMounted } from 'vue'
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
import { OptionTypeApi } from '../api'
import type { OptionTypeResponse } from '../types'
import { ROUTE } from '../routes'

const router = useRouter()
const { confirmDelete } = useConfirm()
const toast = useToast()
const { t } = useI18n()

const items = ref<OptionTypeResponse[]>([])
const loading = ref(true)
const error = ref<string | null>(null)
const search = ref('')
const page = ref(1)
const pageSize = ref(20)
const totalCount = ref(0)

async function fetchOptionTypes() {
  loading.value = true
  error.value = null
  const result = await OptionTypeApi.getMany({
    page: page.value,
    pageSize: pageSize.value,
    search: search.value || undefined,
  })
  if (result.isSuccess) {
    items.value = result.items
    totalCount.value = result.totalCount
  } else {
    error.value = result.message ?? 'Failed to load option types'
  }
  loading.value = false
}

function goToCreate() { router.push({ name: ROUTE.OPTION_TYPES.CREATE }) }
function goToView(id: string) { router.push({ name: ROUTE.OPTION_TYPES.VIEW, params: { id } }) }
function goToEdit(id: string) { router.push({ name: ROUTE.OPTION_TYPES.EDIT, params: { id } }) }

async function onDelete(id: string) {
  confirmDelete({
    target: 'this option type',
    onAccept: async () => {
      const result = await OptionTypeApi.delete(id)
      if (result.isSuccess) { toast.success(t('catalog.option_types.messages.delete_success')); await fetchOptionTypes() }
      else { toast.error(result.message ?? 'Failed to delete') }
    },
  })
}

function onSearch(value: string) { search.value = value; page.value = 1; fetchOptionTypes() }
function onPageChange(e: { page: number; rows: number }) {
  page.value = e.page + 1
  pageSize.value = e.rows
  fetchOptionTypes()
}

onMounted(() => fetchOptionTypes())
</script>

<template>
  <div>
    <TableToolbar
      search-placeholder="Search option types..."
      :create-label="t('catalog.option_types.actions.create')"
      @search="onSearch"
      @create="goToCreate"
    />
    <LoadingSkeleton v-if="loading" :rows="5" :columns="4" />
    <ErrorState v-else-if="error" :description="error" @retry="fetchOptionTypes" />
    <EmptyState v-else-if="items.length === 0" :title="t('catalog.option_types.messages.empty_list')" description="Create your first option type." />
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
      <Column field="filterable" header="Filterable">
        <template #body="{ data }">
          <i v-if="data.filterable" class="pi pi-check" style="color: var(--p-green-500)" />
          <i v-else class="pi pi-times" style="color: var(--p-red-400)" />
        </template>
      </Column>
      <Column field="position" header="Position" />
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
