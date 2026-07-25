<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useRoute } from 'vue-router'
import PageHeader from '@/shared/components/layout/PageHeader.vue'
import DataTable from '@/shared/components/data/DataTable.vue'
import Column from 'primevue/column'
import LoadingSkeleton from '@/shared/components/feedback/LoadingSkeleton.vue'
import ErrorState from '@/shared/components/feedback/ErrorState.vue'
import Tag from 'primevue/tag'
import { PermissionApi } from '../api'
import type { PermissionResponse } from '../types'
import { ROUTE } from '../routes'

const router = useRouter()
const route = useRoute()

const items = ref<PermissionResponse[]>([])
const loading = ref(false)
const error = ref<string | null>(null)

async function fetchPermissions() {
  loading.value = true
  error.value = null
  try {
    const result = await PermissionApi.getMany()
    if (result.isSuccess) {
      items.value = result.value ?? []
    } else {
      error.value = result.message ?? 'Failed to load permissions'
      items.value = []
    }
  } catch (err) {
    console.error(err)
    error.value = 'Failed to load permissions'
    items.value = []
  }
  loading.value = false
}

function goToView(id: string) { router.push({ name: ROUTE.PERMISSIONS.VIEW, params: { id } }) }

onMounted(fetchPermissions)
</script>

<template>
  <div>
    <PageHeader title="Permissions" :icon="route.meta?.icon as string | undefined" subtitle="View all system permissions" />
    <LoadingSkeleton v-if="loading && items.length === 0" :rows="5" :columns="4" />
    <ErrorState v-else-if="error" :description="error" @retry="fetchPermissions" />
    <DataTable v-else :rows="items" :loading="loading">
      <Column field="name" header="Name" sortable />
      <Column field="description" header="Description" />
      <Column field="module" header="Module">
        <template #body="{ data }">
          <Tag :value="data.module" severity="info" />
        </template>
      </Column>
      <template #rowActions="{ data }">
        <button class="p-button p-button-text p-button-sm" @click="goToView(data.id)">
          <i class="pi pi-eye" /> View
        </button>
      </template>
    </DataTable>
  </div>
</template>
