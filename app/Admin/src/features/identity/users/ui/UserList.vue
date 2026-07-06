<template>
  <div>
    <AppPageHeader title="Users" subtitle="Manage administrators and staff">
      <template #actions>
        <AppButton icon="pi pi-plus" label="New user" @click="formOpen = true" />
      </template>
    </AppPageHeader>
    <UserFilters v-model="filters" />
    <AppDataTable :rows="rows" :total="total" :loading="query.isLoading.value" @page="onPage">
      <Column field="displayName" header="Name" sortable />
      <Column field="email" header="Email" sortable />
      <Column header="Status">
        <template #body="{ data }">
          <UserStatusBadge :status="data.status" />
        </template>
      </Column>
      <Column field="roleCount" header="Roles" />
      <Column header="">
        <template #body="{ data }">
          <div class="flex gap-1">
            <AppButton icon="pi pi-eye" variant="ghost" @click="openDetails(data)" />
            <AppButton icon="pi pi-pencil" variant="ghost" @click="openEdit(data)" />
            <AppButton icon="pi pi-trash" variant="danger" @click="confirmDelete(data)" />
          </div>
        </template>
      </Column>
    </AppDataTable>
    <UserFormDialog v-model:visible="formOpen" :user="editing" @saved="onSaved" />
    <UserDetailsDrawer v-model:visible="detailsOpen" :user="detailsUser" />
  </div>
</template>

<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import { useUsersList, useDeleteUser, useUser } from '../api'
import type { User, UserListItem } from '../model/user.types'
import { useConfirm } from '@/shared/composables/useConfirm'
import { useToast } from '@/shared/composables/useToast'
import UserFilters from './UserFilters.vue'
import UserFormDialog from './UserFormDialog.vue'
import UserDetailsDrawer from './UserDetailsDrawer.vue'
import UserStatusBadge from './UserStatusBadge.vue'

const filters = ref<{ search: string; status?: string }>({ search: '' })
const page = ref(1)
let pageSize = 20

const params = computed(() => ({ page: page.value, pageSize, search: filters.value.search }))
const query = useUsersList(params)

const rows = computed<UserListItem[]>(() => query.data.value?.items ?? [])
const total = computed(() => query.data.value?.totalCount ?? 0)

const formOpen = ref(false)
const editing = ref<User | null>(null)
const detailsOpen = ref(false)
const detailsUser = ref<User | null>(null)

const selectedId = ref<string | null>(null)
const user = useUser(selectedId)

watch(user.data, (data) => {
  if (data) {
    editing.value = data
    detailsUser.value = data
  } else {
    if (formOpen.value) formOpen.value = false
    if (detailsOpen.value) detailsOpen.value = false
  }
})

const remove = useDeleteUser()
const confirm = useConfirm()
const toast = useToast()

function onPage(e: { page: number; rows: number }) {
  page.value = e.page + 1
  pageSize = e.rows
}

function openEdit(item: UserListItem) {
  selectedId.value = item.id
  formOpen.value = true
}
function openDetails(item: UserListItem) {
  selectedId.value = item.id
  detailsOpen.value = true
}
async function confirmDelete(user: UserListItem) {
  confirm.require({
    message: `Delete ${user.displayName}?`,
    header: 'Confirm',
    icon: 'pi pi-exclamation-triangle',
    acceptClass: 'p-button-danger',
    accept: async () => {
      await remove.mutateAsync(user.id)
      toast.add({ severity: 'success', summary: 'Deleted', life: 3000 })
    },
  })
}
function onSaved() {
  formOpen.value = false
  editing.value = null
  query.refetch()
}
</script>
