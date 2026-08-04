<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useConfirm } from 'primevue/useconfirm'
import DataTable from 'primevue/datatable'
import Column from 'primevue/column'
import Tag from 'primevue/tag'
import Button from 'primevue/button'
import InputText from 'primevue/inputtext'
import IconField from 'primevue/iconfield'
import InputIcon from 'primevue/inputicon'
import Message from 'primevue/message'
import { useAuthStore } from '@/features/auth/stores/authStore'
import { useDataTableExport } from '@/shared/composables/useDataTableExport'
import { useNotify } from '@/shared/composables/useNotify'
import { useApiErrorHandler } from '@/shared/composables/useApiErrorHandler'
import { useAddressList } from '../composables/useAddressList'
import { AddressApi } from '../services/addressApi'
import { ADDRESS_FILTER_FIELDS, ADDRESS_SORT_FIELDS, ADDRESS_SEARCH_FIELDS } from '../types/address'
import type { AddressResponse } from '../types/address'

const route = useRoute()
const router = useRouter()
const confirm = useConfirm()
const notify = useNotify()
const { handleResult } = useApiErrorHandler()
const { dt, exportCSV } = useDataTableExport()
const authStore = useAuthStore()

const search = ref('')

const initialUserId = (() => {
  const requested = route.query.userId
  const requestedUserId = typeof requested === 'string' ? requested : undefined
  return requestedUserId ?? authStore.currentUser?.userId ?? ''
})()

const { items, loading, setSearch, refresh } = useAddressList(initialUserId, {
  allowedFilterFields: ADDRESS_FILTER_FIELDS,
  allowedSortFields: ADDRESS_SORT_FIELDS,
  allowedSearchFields: ADDRESS_SEARCH_FIELDS,
  defaultSearchFields: ADDRESS_SEARCH_FIELDS,
  immediate: false,
})

onMounted(() => {
  if (initialUserId) refresh()
})

function onSearch(value: string) {
  search.value = value
  setSearch(value)
}

function clearSearch() {
  search.value = ''
  setSearch('')
}

function navigateToNew() {
  router.push(initialUserId ? `/profile/addresses/new?userId=${encodeURIComponent(initialUserId)}` : '/profile/addresses/new')
}

function navigateToEdit(data: AddressResponse) {
  router.push(`/profile/addresses/${data.id}?userId=${encodeURIComponent(data.userId)}`)
}

function confirmDelete(data: AddressResponse) {
  const label = data.label ?? data.address1
  // Trigger: Confirm before deleting a single address.
  confirm.require({
    message: `Delete address "${label}"? This action cannot be undone.`,
    header: 'Confirm Delete',
    icon: 'pi pi-exclamation-triangle',
    rejectLabel: 'Cancel',
    acceptLabel: 'Delete',
    acceptClass: 'p-button-danger',
    accept: async () => {
      // Call: Delete the address.
      const result = await AddressApi.deleteAddress(data.userId, data.id)
      if (result.isSuccess) {
        notify.success('Deleted', label)
        refresh()
      } else {
        handleResult(result)
      }
    },
  })
}
</script>

<template>
  <div class="flex flex-col h-full">
    <!-- Section: Page Header — title and one-line description -->
    <div class="mb-6">
      <h1 class="text-2xl font-semibold mb-1">Addresses</h1>
      <p class="text-muted-color">Manage user addresses</p>
    </div>

    <!-- Section: Error State — warn when no user is selected -->
    <Message v-if="!initialUserId" severity="warn" variant="simple">
      No user is currently selected. Open this page with a userId query parameter or sign in to view addresses.
    </Message>

    <template v-else>
      <!-- Section: Search & Filters — search box and list-level actions -->
      <div class="flex items-center gap-3 mb-4">
        <IconField>
          <InputIcon class="pi pi-search" />
          <InputText
            :model-value="search"
            placeholder="Search addresses..."
            @update:model-value="onSearch($event ?? '')"
          />
        </IconField>
        <Button
          v-if="search"
          label="Clear"
          severity="secondary"
          icon="pi pi-times"
          @click="clearSearch"
        />
        <div class="flex-1" />
        <Button
          label="New Address"
          icon="pi pi-plus"
          @click="navigateToNew"
        />
        <Button
          label="Reload"
          icon="pi pi-refresh"
          severity="secondary"
          @click="refresh"
        />
        <Button
          label="Export"
          icon="pi pi-download"
          severity="secondary"
          @click="exportCSV"
        />
      </div>

      <!-- Section: Data Table — address grid -->
      <DataTable
        ref="dt"
        :value="items"
        :loading="loading"
        scrollable
        paginator
        :rows="20"
        :rows-per-page-options="[10, 20, 50]"
        data-key="id"
      >
        <!-- Section: Table Columns — address identity and default flag fields -->
        <Column field="addressType" header="Type" :sortable="true" />
        <Column field="firstName" header="First Name" :sortable="true" />
        <Column field="city" header="City" :sortable="true" />
        <Column field="countryName" header="Country" :sortable="true" />
        <Column field="zipCode" header="Zip Code" />
        <Column field="isDefault" header="Default">
          <template #body="{ data }">
            <Tag :value="data.isDefault ? 'Yes' : 'No'" :severity="data.isDefault ? 'success' : 'secondary'" />
          </template>
        </Column>
        <!-- Section: Row Actions — edit and delete per address -->
        <Column header="Actions" header-style="width:8rem">
          <template #body="{ data }">
            <Button icon="pi pi-pencil" severity="secondary" text rounded aria-label="Edit" @click="navigateToEdit(data)" />
            <Button icon="pi pi-trash" severity="danger" text rounded aria-label="Delete" @click="confirmDelete(data)" />
          </template>
        </Column>
        <!-- Section: Empty State — shown when no addresses match -->
        <template #empty>No addresses found.</template>
      </DataTable>
    </template>
  </div>
</template>
