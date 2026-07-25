<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useI18n } from 'vue-i18n'
import PageHeader from '@/shared/components/layout/PageHeader.vue'
import { DetailLayout, AppCard } from '@/shared/components'
import LoadingSkeleton from '@/shared/components/feedback/LoadingSkeleton.vue'
import ErrorState from '@/shared/components/feedback/ErrorState.vue'
import Button from 'primevue/button'
import Tag from 'primevue/tag'
import { PermissionApi } from '../api'
import type { PermissionResponse } from '../types'
import { ROUTE } from '../routes'

const route = useRoute()
const router = useRouter()
const { t } = useI18n()

const permission = ref<PermissionResponse | null>(null)
const loading = ref(false)
const error = ref<string | null>(null)

const id = computed(() => route.params.id as string)
const allPermissions = ref<PermissionResponse[]>([])

async function load() {
  loading.value = true
  error.value = null
  const result = await PermissionApi.getMany()
  if (result.isSuccess) {
    allPermissions.value = result.value ?? []
    permission.value = allPermissions.value.find(p => p.id === id.value) ?? null
    if (!permission.value) error.value = 'Permission not found'
  } else {
    error.value = result.message ?? 'Failed to load permission'
  }
  loading.value = false
}

onMounted(load)
</script>

<template>
  <DetailLayout>
    <PageHeader
      :title="permission?.name ?? 'Permission'"
      :subtitle="t('roles.permissions.descriptions.detail')"
      :icon="route.meta?.icon as string | undefined"
    />
    <LoadingSkeleton v-if="loading" :rows="4" :columns="2" />
    <ErrorState v-else-if="error" :description="error" @retry="load" />
    <AppCard v-else-if="permission">
      <div class="grid grid-cols-12 gap-4">
        <div class="col-span-full sm:col-span-6">
          <div class="flex flex-col gap-1">
            <label class="text-sm font-medium text-surface-500">Name</label>
            <span>{{ permission.name }}</span>
          </div>
        </div>
        <div class="col-span-full sm:col-span-6">
          <div class="flex flex-col gap-1">
            <label class="text-sm font-medium text-surface-500">Module</label>
            <Tag :value="permission.module" severity="info" />
          </div>
        </div>
      </div>
      <div v-if="permission.description" class="mt-4">
        <div class="flex flex-col gap-1">
          <label class="text-sm font-medium text-surface-500">Description</label>
          <span>{{ permission.description }}</span>
        </div>
      </div>
      <div class="mt-4">
        <Button text @click="router.push({ name: ROUTE.PERMISSIONS.LIST })">
          <i class="pi pi-arrow-left" /> Back to Permissions
        </Button>
      </div>
    </AppCard>
  </DetailLayout>
</template>
