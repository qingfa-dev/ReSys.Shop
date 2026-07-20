<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { useStateStore } from '../stores/state.store'
import { useCountryStore } from '../../countries/stores/country.store'
import { storeToRefs } from 'pinia'
import { useConfirm } from 'primevue/useconfirm'
import { useToast } from '@/common/composables/toast.use'
import type { State } from '../types/state.response.type'
import StateForm from './StateForm.View.vue'
import { useI18n } from 'vue-i18n'
import PageShell from '@/shared/components/navigation/PageShell.vue'
import PageHeader from '@/shared/components/navigation/PageHeader.vue'

const stateStore = useStateStore()
const countryStore = useCountryStore()
const { t } = useI18n()
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
      if (result.isSuccess) {
        showToast('success', t('common.deleted'), t('location.messages.state_delete_success'))
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
  <PageShell maxWidth="7xl">
    <PageHeader title="States / Provinces" description="Manage states and provinces within countries">
      <template #badge>
        <Badge :value="totalRecords" severity="info" class="ml-2" />
      </template>
      <template #actions>
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
        <Button :label="t('location.actions.new_state')" icon="pi pi-plus" @click="openCreate" class="px-4 shadow-lg rounded-xl" />
      </template>
    </PageHeader>

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
          <i class="mb-4 text-6xl pi pi-map opacity-20"></i>
          <p class="text-xl font-medium">No states found</p>
        </div>
      </template>

      <Column field="name" :header="t('location.labels.name')" sortable>
        <template #body="{ data }">
          <span class="font-bold text-surface-900 dark:text-surface-0">{{ data.name }}</span>
        </template>
      </Column>

      <Column field="abbreviation" :header="t('location.labels.abbreviation')" sortable class="text-center">
        <template #body="{ data }">
          <Tag :value="data.abbreviation" severity="info" />
        </template>
      </Column>

      <Column field="countryId" :header="t('location.labels.country')" sortable>
        <template #body="{ data }">
          <span>{{ countries.find(c => c.id === data.countryId)?.name || data.countryId }}</span>
        </template>
      </Column>

      <Column field="isActive" :header="t('location.labels.active')" dataType="boolean" class="w-24 text-center">
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

    <StateForm
      v-model:visible="dialogVisible"
      :item="editingItem"
      :is-edit="isEdit"
      @close="onDialogClose"
      @saved="onSaved"
    />
  </PageShell>
</template>
