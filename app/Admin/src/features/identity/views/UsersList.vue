<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { useConfirm } from 'primevue/useconfirm'
import DataTable from 'primevue/datatable'
import Column from 'primevue/column'
import Tag from 'primevue/tag'
import Button from 'primevue/button'
import InputText from 'primevue/inputtext'
import IconField from 'primevue/iconfield'
import InputIcon from 'primevue/inputicon'
import { useDataTableExport } from '@/shared/composables/useDataTableExport'
import { usePagedQuery } from '@/shared/composables/usePagedQuery'
import { useNotify } from '@/shared/composables/useNotify'
import { IDENTITY } from '@/shared/constants/api'
import { UserApi } from '../services/userApi'
import type { UserListItem } from '../types/user'
import { USER_FILTER_FIELDS, USER_SORT_FIELDS, USER_SEARCH_FIELDS } from '../types/user'

const router = useRouter()
const confirm = useConfirm()
const notify = useNotify()
const { dt, exportCSV } = useDataTableExport()
const search = ref('')
const selectedItems = ref<UserListItem[]>([])

const { items, loading, setSearch, refresh } = usePagedQuery<UserListItem>(
  `${IDENTITY}/users`,
  {
    allowedFilterFields: USER_FILTER_FIELDS,
    allowedSortFields: USER_SORT_FIELDS,
    allowedSearchFields: USER_SEARCH_FIELDS,
    defaultSearchFields: ['email', 'userName', 'firstName', 'lastName'],
  },
)

function onSearch(value: string) {
  search.value = value
  setSearch(value)
}

function clearSearch() {
  search.value = ''
  setSearch('')
}

function navigateToNew() {
  router.push('/identity/users/new')
}

function navigateToEdit(id: string) {
  router.push(`/identity/users/${id}`)
}

function confirmDelete() {
  const names = selectedItems.value.map((u) => u.email).join(', ')
  confirm.require({
    message: `Delete ${names}? This action cannot be undone.`,
    header: 'Confirm Delete',
    icon: 'pi pi-exclamation-triangle',
    rejectLabel: 'Cancel',
    acceptLabel: 'Delete',
    acceptClass: 'p-button-danger',
    accept: async () => {
      for (const item of selectedItems.value) {
        const result = await UserApi.deleteUser(item.id)
        if (result.isSuccess) {
          notify.success('Deleted', item.email)
        } else {
          notify.error('Failed', `${item.email}: ${result.message}`)
        }
      }
      selectedItems.value = []
      refresh()
    },
  })
}
</script>

<template>
  <div class="flex flex-col h-full">
    <div class="mb-6">
      <h1 class="text-2xl font-semibold mb-1">Users</h1>
      <p class="text-muted-color">Manage registered users</p>
    </div>

    <div class="flex items-center gap-3 mb-4">
      <IconField>
        <InputIcon class="pi pi-search" />
        <InputText
          :model-value="search"
          placeholder="Search users..."
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
        label="New User"
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

    <DataTable
      ref="dt"
      v-model:selection="selectedItems"
      :value="items"
      :loading="loading"
      scrollable
      paginator
      :rows="20"
      :rows-per-page-options="[10, 20, 50]"
      data-key="id"
    >
      <Column selection-mode="multiple" header-style="width:3rem" />
      <Column field="email" header="Email" :sortable="true" />
      <Column field="userName" header="Username" :sortable="true" />
      <Column field="firstName" header="First Name" :sortable="true" />
      <Column field="lastName" header="Last Name" :sortable="true" />
      <Column field="phoneNumber" header="Phone" />
      <Column field="emailConfirmed" header="Confirmed" :sortable="true">
        <template #body="{ data }">
          <Tag :value="data.emailConfirmed ? 'Yes' : 'No'" :severity="data.emailConfirmed ? 'success' : 'warn'" />
        </template>
      </Column>
      <Column header="Actions" header-style="width:8rem">
        <template #body="{ data }">
          <Button icon="pi pi-pencil" severity="secondary" text rounded @click="navigateToEdit(data.id)" />
          <Button
            icon="pi pi-trash"
            severity="danger"
            text
            rounded
            @click="selectedItems = [data]; confirmDelete()"
          />
        </template>
      </Column>
      <template #empty>No users found.</template>
    </DataTable>
  </div>
</template>
