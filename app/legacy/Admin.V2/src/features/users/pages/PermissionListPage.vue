<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { useI18n } from 'vue-i18n'
import PageHeader from '@/shared/components/layout/PageHeader.vue'
import { ListLayout } from '@/shared/components'
import DataTable from '@/shared/components/data/DataTable.vue'
import Column from 'primevue/column'
import LoadingSkeleton from '@/shared/components/feedback/LoadingSkeleton.vue'
import ErrorState from '@/shared/components/feedback/ErrorState.vue'
import Button from 'primevue/button'
import Tag from 'primevue/tag'
import { PermissionApi } from '../api'
import type { PermissionResponse } from '../types'
import { ROUTE } from '../routes'

const router = useRouter()
const route = useRoute()
const { t } = useI18n()

const items = ref<PermissionResponse[]>([])
const loading = ref(false)
const error = ref<string | null>(null)

async function fetchPermissions() {
  loading.value = true
  error.value = null
  try {
    const result = await PermissionApi.getMany({ page: 1, pageSize: 100 })
    if (result.isSuccess) {
      items.value = result.items ?? []
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
  <ListLayout>
    <template #header>
      <PageHeader
        :title="t('roles.permissions.titles.list')"
        :subtitle="t('roles.permissions.descriptions.list')"
        :icon="route.meta?.icon as string | undefined"
      />
    </template>
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
        <Button text size="small" @click="goToView(data.id)">
          <i class="pi pi-eye" /> View
        </Button>
      </template>
    </DataTable>
  </ListLayout>
</template>
