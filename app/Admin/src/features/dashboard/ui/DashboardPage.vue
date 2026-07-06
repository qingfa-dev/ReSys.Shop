<template>
  <div>
    <AppPageHeader title="Dashboard" subtitle="Overview of your store" />
    <div v-if="!user" class="p-8">
      <AppLoadingState />
    </div>
    <div v-else class="grid grid-cols-1 gap-4 md:grid-cols-3">
      <div class="rounded border border-surface-200 bg-white p-4">
        <p class="text-sm text-color-secondary">Signed in as</p>
        <p class="text-lg font-semibold">{{ userName }}</p>
      </div>
      <div class="rounded border border-surface-200 bg-white p-4">
        <p class="text-sm text-color-secondary">Roles</p>
        <p class="text-lg font-semibold">{{ rolesCount }}</p>
      </div>
      <div class="rounded border border-surface-200 bg-white p-4">
        <p class="text-sm text-color-secondary">Permissions</p>
        <p class="text-lg font-semibold">{{ permissionsCount }}</p>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { useAuthStore } from '@/features/auth/stores/auth.store'
import { storeToRefs } from 'pinia'

const authStore = useAuthStore()
const { user } = storeToRefs(authStore)
const userData = computed(() => user.value as Record<string, unknown> | null)
const userName = computed(() => String(userData.value?.name ?? userData.value?.sub ?? '—'))
const rolesCount = computed(() => ((userData.value?.role_names ?? userData.value?.roles) as any[] | undefined)?.length ?? 0)
const permissionsCount = computed(() => ((userData.value?.permissions as any[] | undefined)?.length ?? 0))
</script>
