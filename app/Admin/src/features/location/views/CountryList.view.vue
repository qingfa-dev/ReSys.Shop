<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { useCountryStore } from '../stores/country.store'
import { storeToRefs } from 'pinia'
import { useConfirm } from 'primevue/useconfirm'
import { useToast } from '@/shared/composables/toast.use'
import type { Country } from '../types/country.types'
import CountryForm from './CountryForm.view.vue'

const store = useCountryStore()
const { items, loading, totalRecords } = storeToRefs(store)
const confirm = useConfirm()
const { showToast } = useToast()

const dialogVisible = ref(false)
const editingItem = ref<Country | null>(null)
const isEdit = ref(false)

const openCreate = () => {
  isEdit.value = false
  editingItem.value = null
  dialogVisible.value = true
}

const openEdit = (item: Country) => {
  isEdit.value = true
  editingItem.value = { ...item }
  dialogVisible.value = true
}

const onDialogClose = () => {
  dialogVisible.value = false
  editingItem.value = null
}

const onSaved = async () => {
  dialogVisible.value = false
  editingItem.value = null
  await store.fetchCountries()
}

const confirmDelete = (item: Country) => {
  confirm.require({
    message: `Delete "${item.name}"?`,
    header: 'Confirm Deletion',
    icon: 'pi pi-exclamation-triangle',
    rejectLabel: 'Cancel',
    acceptLabel: 'Delete',
    acceptProps: { severity: 'danger' },
    accept: async () => {
      const result = await store.deleteCountry(item.id)
      if (result.success) {
        showToast('success', 'Deleted', 'Country removed.')
      }
    },
  })
}

onMounted(() => {
  store.fetchCountries()
})
</script>

<template>
  <Card>
    <template #content>
      <div class="flex flex-col items-start justify-between gap-4 mb-8 md:flex-row md:items-center">
      <div>
        <h2 class="text-3xl font-black tracking-tight text-surface-900 dark:text-surface-50">Countries</h2>
        <div class="flex items-center gap-2 mt-1">
          <span class="text-surface-500 dark:text-surface-400">Manage countries and regions</span>
          <Badge :value="totalRecords" severity="info" class="ml-2" />
        </div>
      </div>
      <Button label="New Country" icon="pi pi-plus" @click="openCreate" class="px-4 shadow-lg rounded-xl" />
    </div>

    <div class="overflow-hidden border shadow-sm bg-surface-0 dark:bg-surface-900 rounded-2xl border-surface-100 dark:border-surface-800">
      <DataTable
        :value="items"
        :loading="loading"
        lazy
        paginator
        :rows="10"
        removableSort
        scrollable
        rowHover
        stripedRows
        showGridlines
        dataKey="id"
      >
        <template #empty>
          <div class="flex flex-col items-center justify-center py-20 text-surface-400">
            <i class="mb-4 text-6xl pi pi-flag opacity-20"></i>
            <p class="text-xl font-medium">No countries found</p>
          </div>
        </template>

        <Column field="name" header="Name" sortable>
          <template #body="{ data }">
            <span class="font-bold text-surface-900 dark:text-surface-0">{{ data.name }}</span>
          </template>
        </Column>

        <Column field="isoCode2" header="ISO2" sortable class="text-center">
          <template #body="{ data }">
            <Tag :value="data.isoCode2" severity="info" />
          </template>
        </Column>

        <Column field="isoCode3" header="ISO3" sortable class="text-center">
          <template #body="{ data }">
            <Tag :value="data.isoCode3" severity="contrast" />
          </template>
        </Column>

        <Column field="phoneCode" header="Phone Code" sortable class="text-center">
          <template #body="{ data }">
            <span class="font-mono">{{ data.phoneCode || '-' }}</span>
          </template>
        </Column>

        <Column field="isActive" header="Active" dataType="boolean" class="w-24 text-center">
          <template #body="{ data }">
            <i class="pi" :class="{'pi-check-circle text-green-500': data.isActive, 'pi-times-circle text-surface-400': !data.isActive}"></i>
          </template>
        </Column>

        <Column class="w-32 text-right" frozen alignFrozen="right">
          <template #body="{ data }">
            <div class="flex justify-end gap-1">
              <Button icon="pi pi-pencil" severity="secondary" text rounded @click="openEdit(data)" />
              <Button icon="pi pi-trash" severity="danger" text rounded @click="confirmDelete(data)" />
            </div>
          </template>
        </Column>
      </DataTable>
    </div>

    <CountryForm
      v-model:visible="dialogVisible"
      :item="editingItem"
      :is-edit="isEdit"
      @close="onDialogClose"
      @saved="onSaved"
    />
  </template>
</Card>
</template>

<style scoped>
:deep(.p-datatable-header) {
  background: transparent;
  padding: 1rem;
}
:deep(.p-datatable-thead > tr > th) {
  background: var(--p-content-background);
  color: var(--p-text-color);
  font-size: 0.875rem;
  font-weight: 700;
  text-transform: uppercase;
  letter-spacing: 0.025em;
  padding: 1rem 1.5rem;
  border-bottom: 2px solid var(--p-primary-color);
}
:deep(.p-datatable-tbody > tr > td) {
  padding: 1rem 1.5rem;
  border-bottom: 1px solid var(--p-content-border-color);
}
</style>
