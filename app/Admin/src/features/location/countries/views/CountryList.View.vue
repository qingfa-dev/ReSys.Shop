<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { useCountryStore } from '../stores/country.store'
import { storeToRefs } from 'pinia'
import { useConfirm } from 'primevue/useconfirm'
import { useToast } from '@/common/composables/toast.use'
import type { Country } from '../types/country.response.type'
import CountryForm from './CountryForm.View.vue'
import { useI18n } from 'vue-i18n'
import PageShell from '@/shared/components/navigation/PageShell.vue'
import PageHeader from '@/shared/components/navigation/PageHeader.vue'

const store = useCountryStore()
const { t } = useI18n()
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
      if (result.isSuccess) {
        showToast('success', t('common.deleted'), t('location.messages.delete_success'))
      }
    },
  })
}

onMounted(() => {
  store.fetchCountries()
})
</script>

<template>
  <PageShell maxWidth="7xl">
    <PageHeader :title="t('location.titles.countries')" description="Manage countries and regions">
      <template #badge>
        <Badge :value="totalRecords" severity="info" class="ml-2" />
      </template>
      <template #actions>
        <Button :label="t('location.actions.new_country')" icon="pi pi-plus" @click="openCreate" class="px-4 shadow-lg rounded-xl" />
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
          <i class="mb-4 text-6xl pi pi-flag opacity-20"></i>
          <p class="text-xl font-medium">No countries found</p>
        </div>
      </template>

      <Column field="name" :header="t('location.labels.name')" sortable>
        <template #body="{ data }">
          <span class="font-bold text-surface-900 dark:text-surface-0">{{ data.name }}</span>
        </template>
      </Column>

      <Column field="isoCode" header="ISO Code" sortable class="text-center">
        <template #body="{ data }">
          <Tag :value="data.isoCode" severity="info" />
        </template>
      </Column>

      <Column field="callingCode" :header="t('location.labels.calling_code')" sortable class="text-center">
        <template #body="{ data }">
          <span class="font-mono">{{ data.callingCode || '-' }}</span>
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

    <CountryForm
      v-model:visible="dialogVisible"
      :item="editingItem"
      :is-edit="isEdit"
      @close="onDialogClose"
      @saved="onSaved"
    />
  </PageShell>
</template>
