<script setup lang="ts">
import { onMounted } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { useI18n } from 'vue-i18n'
import PageHeader from '@/shared/components/layout/PageHeader.vue'
import { ListLayout } from '@/shared/components'
import DataTable from '@/shared/components/data/DataTable.vue'
import TableToolbar from '@/shared/components/layout/TableToolbar.vue'
import Column from 'primevue/column'
import LoadingSkeleton from '@/shared/components/feedback/LoadingSkeleton.vue'
import ErrorState from '@/shared/components/feedback/ErrorState.vue'
import Button from 'primevue/button'
import Tag from 'primevue/tag'
import { usePermissionStore } from '../store/permission.store'
import { ROUTE } from '../routes'

const router = useRouter()
const route = useRoute()
const { t } = useI18n()
const store = usePermissionStore()

function goToView(id: string) { router.push({ name: ROUTE.PERMISSIONS.VIEW, params: { id } }) }
function onPageChange(e: { page: number; rows: number }) { store.setPage(e.page + 1) }

onMounted(() => store.fetchMany())
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
    <TableToolbar v-model:query="store.searchQuery" search-placeholder="Search permissions..." />
    <LoadingSkeleton v-if="store.loading && store.items.length === 0" :rows="5" :columns="4" />
    <ErrorState v-else-if="store.error" :description="store.error" @retry="store.fetchMany" />
    <DataTable
      v-else
      :rows="[...store.items]"
      :loading="store.loading"
      :total-records="store.totalRecords"
      :page-size="store.query.pageSize"
      :first="(store.query.page - 1) * store.query.pageSize"
      @page="onPageChange"
    >
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
