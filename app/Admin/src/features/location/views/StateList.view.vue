<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { useStateStore } from '../stores/state.store'
import { useCountryStore } from '../stores/country.store'
import { storeToRefs } from 'pinia'
import { useConfirm } from 'primevue/useconfirm'
import { useToast } from '@/shared/composables/toast.use'
import type { State } from '../types/state.types'
import StateForm from './StateForm.view.vue'

const stateStore = useStateStore()
const countryStore = useCountryStore()
const { items, loading, totalRecords } = storeToRefs(stateStore)
const { items: countries } = storeToRefs(countryStore)
const confirm = useConfirm()
const { showToast } = useToast()

const dialogVisible = ref(false)
const editingItem = ref<State | null>(null)
const isEdit = ref(false)

const selectedCountryId = ref<string | null>(null)

const openCreate = () => {
  isEdit.value = false
  editingItem.value = null
  dialogVisible.value = true
}

const openEdit = (item: State) => {
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
  await stateStore.fetchStates()
}

const confirmDelete = (item: State) => {
  confirm.require({
    message: `Delete "${item.name}"?`,
    header: 'Confirm Deletion',
    icon: 'pi pi-exclamation-triangle',
    rejectLabel: 'Cancel',
    acceptLabel: 'Delete',
    acceptProps: { severity: 'danger' },
    accept: async () => {
      const result = await stateStore.deleteState(item.id)
      if (result.success) {
        showToast('success', 'Deleted', 'State removed.')
      }
    },
  })
}

const filterByCountry = () => {
  const params: Record<string, unknown> = {}
  if (selectedCountryId.value) {
    params.countryId = selectedCountryId.value
  }
  stateStore.fetchStates(params)
}

onMounted(async () => {
  await countryStore.fetchCountries()
  await stateStore.fetchStates()
})
</script>

<template>
  <div class="p-6">
    <div class="flex flex-col items-start justify-between gap-4 mb-8 md:flex-row md:items-center">
      <div>
        <h2 class="text-3xl font-black tracking-tight text-surface-900 dark:text-surface-50">States / Provinces</h2>
        <div class="flex items-center gap-2 mt-1">
          <span class="text-surface-500 dark:text-surface-400">Manage states and provinces within countries</span>
          <Badge :value="totalRecords" severity="info" class="ml-2" />
        </div>
      </div>
      <div class="flex gap-2">
        <Select
          v-model="selectedCountryId"
          :options="countries"
          optionLabel="name"
          optionValue="id"
          placeholder="Filter by country"
          class="rounded-xl w-56"
          :showClear="true"
          @change="filterByCountry"
        />
        <Button label="New State" icon="pi pi-plus" @click="openCreate" class="px-4 shadow-lg rounded-xl" />
      </div>
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
        dataKey="id"
      >
        <template #empty>
          <div class="flex flex-col items-center justify-center py-20 text-surface-400">
            <i class="mb-4 text-6xl pi pi-map opacity-20"></i>
            <p class="text-xl font-medium">No states found</p>
          </div>
        </template>

        <Column field="name" header="Name" sortable>
          <template #body="{ data }">
            <span class="font-bold text-surface-900 dark:text-surface-0">{{ data.name }}</span>
          </template>
        </Column>

        <Column field="abbreviation" header="Abbreviation" sortable class="text-center">
          <template #body="{ data }">
            <Tag :value="data.abbreviation" severity="info" />
          </template>
        </Column>

        <Column field="countryId" header="Country" sortable>
          <template #body="{ data }">
            <span>{{ countries.find(c => c.id === data.countryId)?.name || data.countryId }}</span>
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

    <StateForm
      v-model:visible="dialogVisible"
      :item="editingItem"
      :is-edit="isEdit"
      @close="onDialogClose"
      @saved="onSaved"
    />
  </div>
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
